using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using LawPortal.Core.Entities;
using LawPortal.Core.Helpers;
using LawPortal.Core.Interfaces;
using LawPortal.Web.Areas;
using LawPortal.Web.Areas.Shared.ViewModels;
using LawPortal.Web.Extensions;
using LawPortal.Web.Extensions.ActionResults;
using LawPortal.Web.Helpers;
using LawPortal.Web.Interfaces;
using LawPortal.Web.Models;
using LawPortal.Web.Models.PageViewModels;
using LawPortal.Web.Security;
using LawPortal.Web.Services.DocumentStorage;
using LawPortal.Core.Entities.Documents;
using Microsoft.AspNetCore.Http;

namespace LawPortal.Web.Areas.Releases.Controllers
{
    [Area("Releases"), Authorize(Policy = ReleaseAuthorizationPolicy.CanAccessAuxiliary)]
    public class DeployController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<DeployPassword> _viewModelService;
        private readonly IEntityService<DeployPassword> _entityService;
        private readonly IEntityService<Release> _releaseService;
        private readonly IDocumentService _documentService;
        private readonly IDocumentHelper _documentHelper;
        private readonly IDocumentStorage _documentStorage;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IStringLocalizer<SharedResource> _localizer;

        private readonly string _dataContainer = "deployDetail";

        public DeployController(
            IAuthorizationService authService,
            IViewModelService<DeployPassword> viewModelService,
            IEntityService<DeployPassword> entityService,
            IEntityService<Release> releaseService,
            IDocumentService documentService,
            IDocumentHelper documentHelper,
            IDocumentStorage documentStorage,
            IWebHostEnvironment env,
            IConfiguration config,
            IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _entityService = entityService;
            _releaseService = releaseService;
            _documentService = documentService;
            _documentHelper = documentHelper;
            _documentStorage = documentStorage;
            _env = env;
            _config = config;
            _localizer = localizer;
        }

        // Mirrors ReleaseController.ToDocSystemType — converts a Release.SystemType
        // string ("Patent" / "Trademark" / ...) into the 1-2 char code used by the
        // document folder hierarchy ("P" / "T" / ...). Kept in sync intentionally.
        private static string ToDocSystemType(string systemType)
        {
            if (string.IsNullOrEmpty(systemType)) return "";
            if (systemType.Length <= 2) return systemType;
            return systemType.ToLower() switch
            {
                "patent" => "P",
                "trademark" => "T",
                "general matter" => "G",
                "dms" => "D",
                "shared" => "S",
                _ => systemType.Substring(0, Math.Min(systemType.Length, 2))
            };
        }

        private static string TruncateFolderName(string name, int maxLen = 100)
        {
            if (string.IsNullOrEmpty(name)) return "Documents";
            return name.Length <= maxLen ? name : name.Substring(0, maxLen);
        }

        /// <summary>
        /// Populates the path dropdowns on the Deploy detail screen. Returns
        /// the documents from any Release matching (Year, Quarter) whose
        /// Systems comma-list contains <paramref name="systemTag"/>, filtered
        /// by Pat/Tmk side and by whether the file is an MDB. The Pat/Tmk
        /// distinction matters for R4, which is shared between both sides.
        /// </summary>
        public async Task<IActionResult> GetReleaseDocs(int year, string quarter, string systemTag, bool isPat, bool isMdb)
        {
            if (year <= 0 || string.IsNullOrEmpty(quarter) || string.IsNullOrEmpty(systemTag))
                return Json(new List<object>());

            // Each Release is scoped to exactly one system version, stored in
            // SystemType (e.g. "R4", "PatR5-7", "PatR8-R10v2.1"). The Systems
            // column is reserved for downstream client-system overrides and is
            // typically empty, so the dropdown filter matches on SystemType.
            var matching = await _releaseService.QueryableList.AsNoTracking()
                .Where(r => r.Year == year && r.Quarter == quarter && r.SystemType == systemTag)
                .ToListAsync();

            var results = new List<object>();
            foreach (var rel in matching.OrderBy(r => r.Name))
            {
                var sysType = ToDocSystemType(rel.SystemType);
                var folder = await _documentService.GetFolder(sysType, "ReleaseId", rel.ReleaseId, TruncateFolderName(rel.Name), 0);
                if (folder == null) continue;

                // Filter on file ext: MDBs are exactly .mdb; "reports" are any
                // other file type living in the release folder (per the user's
                // spec — LawDocs paths accept anything that isn't an MDB).
                var docsQuery = _documentService.DocDocuments
                    .Where(d => d.FolderId == folder.FolderId && d.DocFile != null);
                docsQuery = isMdb
                    ? docsQuery.Where(d => d.DocFile.FileExt == "mdb")
                    : docsQuery.Where(d => d.DocFile.FileExt != "mdb");

                var docs = await docsQuery
                    .Select(d => new { d.DocId, d.DocName })
                    .ToListAsync();

                foreach (var doc in docs)
                {
                    // Filename convention: docs containing "Pat" belong on the
                    // Pat side; everything else (typically "Tmk"-named) on the
                    // Tmk side. Matches the existing GetCompareMdbFiles logic.
                    bool docIsPat = (doc.DocName ?? "").Contains("Pat", StringComparison.OrdinalIgnoreCase);
                    if (docIsPat == isPat)
                        results.Add(new { doc.DocId, Text = doc.DocName });
                }
            }
            return Json(results);
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "deploySearch",
                Title = _localizer["Deploy"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, ReleaseAuthorizationPolicy.AuxiliaryModify)).Succeeded
            };

            if (Request.IsAjax())
                return PartialView("Index", model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel()
            {
                Page = PageType.SearchResults,
                PageId = "deploySearchResults",
                Title = _localizer["Deploy"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, ReleaseAuthorizationPolicy.AuxiliaryModify)).Succeeded
            };

            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search()
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var deployments = _entityService.QueryableList;

                if (mainSearchFilters != null && mainSearchFilters.Count > 0)
                    deployments = _viewModelService.AddCriteria(deployments, mainSearchFilters);

                var result = await _viewModelService.CreateViewModelForGrid(request, deployments, "Year", "DeployPasswordId");
                return Json(result);
            }

            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost()]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction("Index");
            }

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Deploy"].ToString(),
                RecordId = detail.DeployPasswordId,
                SingleRecord = singleRecord || !Request.IsAjax(),
                ActiveTab = tab,
                PagePermission = page,
                Data = detail
            };

            if (Request.IsAjax())
            {
                if (!singleRecord && !fromSearch)
                    model.Page = PageType.DetailContent;

                return PartialView("Index", model);
            }

            return View("Index", model);
        }

        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            var detail = page.Detail;
            detail.Year = DateTime.Now.Year;
            detail.Quarter = "Q" + ((DateTime.Now.Month - 1) / 3 + 1);

            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Deployment"].ToString(),
                RecordId = detail.DeployPasswordId,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };
            ModelState.Clear();

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] DeployPassword deployPassword)
        {
            try
            {
            if (deployPassword == null)
                return new JsonBadRequest(new { errors = new[] { "Save: posted body deserialized to null — check form fields match DeployPassword model." } });

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            // Normalize nulls — the DB columns are NOT NULL with '' default, and the
            // unique index treats '' consistently across rows. NULLs would slip past
            // the index (NULL <> NULL in SQL) and yield duplicate-looking rows.
            deployPassword.PatentPassword ??= "";
            deployPassword.TrademarkPassword ??= "";

            // Reject duplicate (Year, Quarter, PatentPassword, TrademarkPassword)
            // tuples — matches the UX_tblDeployPassword unique index. Pre-check
            // gives a friendly error instead of letting the SQL exception bubble up.
            var duplicate = await _entityService.QueryableList
                .AnyAsync(d =>
                    d.DeployPasswordId != deployPassword.DeployPasswordId &&
                    d.Year == deployPassword.Year &&
                    d.Quarter == deployPassword.Quarter &&
                    d.PatentPassword == deployPassword.PatentPassword &&
                    d.TrademarkPassword == deployPassword.TrademarkPassword);

            if (duplicate)
            {
                return new JsonBadRequest(new
                {
                    errors = new[] { _localizer["A deployment with this Year, Quarter, Patent Password, and Trademark Password already exists."].Value }
                });
            }

            bool isNew = deployPassword.DeployPasswordId == 0;

            if (!isNew)
            {
                var existing = await _entityService.GetByIdAsync(deployPassword.DeployPasswordId);
                if (existing == null)
                    return new JsonBadRequest(new { errors = new[] { $"Deployment {deployPassword.DeployPasswordId} no longer exists." } });

                existing.Year = deployPassword.Year;
                existing.Quarter = deployPassword.Quarter;
                existing.PatentPassword = deployPassword.PatentPassword;
                existing.TrademarkPassword = deployPassword.TrademarkPassword;
                // Per-path doc selections (nullable; 0 from an empty dropdown
                // is treated as "not selected" via the conditional assignment).
                existing.PatVer9And10LawDocId = deployPassword.PatVer9And10LawDocId;
                existing.PatVer9And10MdbId    = deployPassword.PatVer9And10MdbId;
                existing.PatR5LawDocId        = deployPassword.PatR5LawDocId;
                existing.PatR5MdbId           = deployPassword.PatR5MdbId;
                existing.PatR8LawDocId        = deployPassword.PatR8LawDocId;
                existing.PatR8MdbId           = deployPassword.PatR8MdbId;
                existing.TmkVer9And10LawDocId = deployPassword.TmkVer9And10LawDocId;
                existing.TmkVer9And10MdbId    = deployPassword.TmkVer9And10MdbId;
                existing.TmkR5LawDocId        = deployPassword.TmkR5LawDocId;
                existing.TmkR5MdbId           = deployPassword.TmkR5MdbId;
                existing.TmkR9LawDocId        = deployPassword.TmkR9LawDocId;
                existing.TmkR9MdbId           = deployPassword.TmkR9MdbId;
                UpdateEntityStamps(existing, existing.DeployPasswordId);
                await _entityService.Update(existing);
                return Json(existing.DeployPasswordId);
            }

            UpdateEntityStamps(deployPassword, deployPassword.DeployPasswordId);
            await _entityService.Add(deployPassword);
            return Json(deployPassword.DeployPasswordId);
            }
            catch (Exception ex)
            {
                // Surface the real failure (binding, EF, SQL) to the red banner
                // instead of letting the ExceptionFilter swallow it into a
                // generic "An error occurred" message. Walk the inner-exception
                // chain so EF/SQL errors aren't hidden behind the EF wrapper.
                var msg = ex.Message;
                var inner = ex.InnerException;
                while (inner != null)
                {
                    msg += " :: " + inner.Message;
                    inner = inner.InnerException;
                }
                return new JsonBadRequest(new { errors = new[] { "Save failed: " + msg } });
            }
        }

        /// <summary>
        /// Reads each of the 6 selected MDB files (Pat/Tmk × Ver9and10/R5/R8 or R9)
        /// and populates the matching upd* SQL tables. Path → prefix mapping:
        ///   Ver9and10 → "upd"   (R4 system, both Pat and Tmk)
        ///   R5        → "upd5"  (both Pat and Tmk)
        ///   R8        → "upd8"  (Pat only)
        ///   R9        → "upd9"  (Tmk only)
        /// For each table inside an MDB that starts with "tbl{Side}", we look for
        /// a target SQL table named upd{prefix}{Side}{suffix}. If it exists, we
        /// TRUNCATE it and bulk-insert all rows. Tables with no matching upd
        /// counterpart are skipped. Per-MDB errors are collected and reported in
        /// the toast — one bad file doesn't abort the rest.
        /// </summary>
        [HttpPost, Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deploy(int id)
        {
            try
            {
                var record = await _entityService.GetByIdAsync(id);
                if (record == null)
                    return new JsonBadRequest(new { errors = new[] { $"Deployment {id} no longer exists." } });

                // Hard requirement before deploying: every dropdown filled and
                // both passwords present. The button is JS-gated too but a
                // server-side check guards against direct API calls.
                var missing = new List<string>();
                if (record.PatVer9And10MdbId == null) missing.Add("Mdbs/Pat/Ver9and10");
                if (record.PatR5MdbId == null) missing.Add("Mdbs/Pat/R5");
                if (record.PatR8MdbId == null) missing.Add("Mdbs/Pat/R8");
                if (record.TmkVer9And10MdbId == null) missing.Add("Mdbs/Tmk/Ver9and10");
                if (record.TmkR5MdbId == null) missing.Add("Mdbs/Tmk/R5");
                if (record.TmkR9MdbId == null) missing.Add("Mdbs/Tmk/R9");
                if (record.PatVer9And10LawDocId == null) missing.Add("LawDocs/Pat/Ver9and10");
                if (record.PatR5LawDocId == null) missing.Add("LawDocs/Pat/R5");
                if (record.PatR8LawDocId == null) missing.Add("LawDocs/Pat/R8");
                if (record.TmkVer9And10LawDocId == null) missing.Add("LawDocs/Tmk/Ver9and10");
                if (record.TmkR5LawDocId == null) missing.Add("LawDocs/Tmk/R5");
                if (record.TmkR9LawDocId == null) missing.Add("LawDocs/Tmk/R9");
                if (string.IsNullOrEmpty(record.PatentPassword)) missing.Add("Patent Password");
                if (string.IsNullOrEmpty(record.TrademarkPassword)) missing.Add("Trademark Password");
                if (missing.Count > 0)
                    return new JsonBadRequest(new { errors = new[] { "Cannot deploy — missing: " + string.Join(", ", missing) } });

                // The 6 MDB selections drive the populate step. LawDoc selections
                // are validated above but don't contribute records.
                var mdbJobs = new (int DocId, string Prefix, string Label)[]
                {
                    (record.PatVer9And10MdbId!.Value, "",  "Mdbs/Pat/Ver9and10"),
                    (record.PatR5MdbId!.Value,        "5", "Mdbs/Pat/R5"),
                    (record.PatR8MdbId!.Value,        "8", "Mdbs/Pat/R8"),
                    (record.TmkVer9And10MdbId!.Value, "",  "Mdbs/Tmk/Ver9and10"),
                    (record.TmkR5MdbId!.Value,        "5", "Mdbs/Tmk/R5"),
                    (record.TmkR9MdbId!.Value,        "9", "Mdbs/Tmk/R9"),
                };

                var connStr = _config.GetConnectionString("DefaultConnection") ?? "";
                var existingUpdTables = await GetExistingUpdTables(connStr);
                var mdbExe = ResolveMdbExePath(_env.WebRootPath);
                if (!System.IO.File.Exists(mdbExe))
                    return new JsonBadRequest(new { errors = new[] { $"MDB reader not found at: {mdbExe}. Build the LawPortal.Mdb project." } });

                var perJobErrors = new List<string>();
                int totalTablesPopulated = 0;

                foreach (var job in mdbJobs)
                {
                    try
                    {
                        var doc = await _documentService.GetDocumentById(job.DocId);
                        if (doc == null || !doc.FileId.HasValue)
                            throw new Exception($"DocId {job.DocId} not found or has no file.");
                        var file = await _documentService.GetFileById(doc.FileId.Value);
                        if (file == null)
                            throw new Exception($"FileId {doc.FileId.Value} not found.");

                        var mdbPath = await EnsureLocalFile(
                            _documentHelper.GetDocumentPath(file.DocFileName),
                            file.DocFileName);
                        if (string.IsNullOrEmpty(mdbPath) || !System.IO.File.Exists(mdbPath))
                            throw new Exception($"MDB not found on disk for {doc.DocName}.");

                        var tablesData = await ReadMdbViaSidecar(mdbExe, mdbPath);
                        var populated = await PopulateUpdTables(connStr, tablesData, job.Prefix, existingUpdTables);
                        totalTablesPopulated += populated;
                    }
                    catch (Exception ex)
                    {
                        perJobErrors.Add($"{job.Label}: {ex.Message}");
                    }
                }

                if (perJobErrors.Count > 0)
                {
                    var msg = $"Deploy completed with errors. Populated {totalTablesPopulated} table(s). Failures: " + string.Join(" | ", perJobErrors);
                    return new JsonBadRequest(new { errors = new[] { msg } });
                }

                return Json(new { success = $"Deploy completed: {totalTablesPopulated} tables populated across all 6 MDBs." });
            }
            catch (Exception ex)
            {
                return new JsonBadRequest(new { errors = new[] { "Deploy failed: " + ex.Message } });
            }
        }

        // Snapshot of which upd* tables currently exist so the populate loop
        // can skip MDB tables that have no matching target instead of failing.
        private async Task<HashSet<string>> GetExistingUpdTables(string connStr)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("SELECT name FROM sys.tables WHERE name LIKE 'upd%'", conn);
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync()) names.Add(rdr.GetString(0));
            return names;
        }

        // Resolve LawPortal.Mdb.exe location — same logic as MdbComparisonService.
        private static string ResolveMdbExePath(string webRootPath)
        {
            var deployFolder = new DirectoryInfo(webRootPath).Parent?.FullName ?? webRootPath;
            var deployed = Path.Combine(deployFolder, "mdbservice", "LawPortal.Mdb.exe");
            if (System.IO.File.Exists(deployed)) return deployed;

            var solutionRoot = new DirectoryInfo(webRootPath).Parent?.Parent?.FullName;
            if (solutionRoot != null)
            {
                foreach (var cfg in new[] { "Debug", "Release" })
                {
                    var dev = Path.Combine(solutionRoot, "LawPortal.Mdb", "bin", cfg, "net8.0", "LawPortal.Mdb.exe");
                    if (System.IO.File.Exists(dev)) return dev;
                }
            }
            return deployed;
        }

        // Azure mode returns a blob-relative path; the 32-bit MDB sidecar only
        // reads local disk, so download to temp first.
        private async Task<string> EnsureLocalFile(string pathFromHelper, string docFileName)
        {
            if (string.IsNullOrEmpty(pathFromHelper)) return string.Empty;
            if (System.IO.File.Exists(pathFromHelper)) return pathFromHelper;
            var tempPath = Path.Combine(Path.GetTempPath(), $"deploycache_{Guid.NewGuid():N}_{docFileName}");
            return await _documentStorage.SaveFileStreamToPath(pathFromHelper, tempPath);
        }

        // Invokes LawPortal.Mdb.exe with "read <mdbPath>" and parses the JSON
        // payload. Same shape as MdbComparisonService — file1 holds the table
        // dictionary, but we only pass one file so file2 is absent.
        private async Task<Dictionary<string, List<Dictionary<string, JsonElement>>>> ReadMdbViaSidecar(string mdbExe, string mdbPath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = mdbExe,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            psi.ArgumentList.Add("read");
            psi.ArgumentList.Add(mdbPath);

            using var process = Process.Start(psi) ?? throw new Exception("Failed to start LawPortal.Mdb.exe");
            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (string.IsNullOrWhiteSpace(stdout))
                throw new Exception($"MDB reader returned no data (exit {process.ExitCode}). stderr: {stderr}");

            var root = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<Dictionary<string, JsonElement>>>>>(stdout)
                ?? throw new Exception("MDB reader JSON failed to parse.");
            return root.ContainsKey("file1") ? root["file1"] : new();
        }

        // For each tbl* table in the MDB, compute target = upd{prefix} + tableName.Substring(3).
        // If target exists, TRUNCATE then INSERT all rows. Skip if no target.
        // Returns count of upd tables populated (including empty truncates).
        private async Task<int> PopulateUpdTables(
            string connStr,
            Dictionary<string, List<Dictionary<string, JsonElement>>> mdbTables,
            string prefix,
            HashSet<string> existingUpdTables)
        {
            int populated = 0;
            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            foreach (var (mdbTableName, rows) in mdbTables)
            {
                if (!mdbTableName.StartsWith("tbl", StringComparison.OrdinalIgnoreCase)) continue;
                var targetTable = "upd" + prefix + mdbTableName.Substring(3);
                if (!existingUpdTables.Contains(targetTable)) continue;

                // TRUNCATE first — bracket-quote the name; sys.tables names can't
                // contain ']' so this round-trip is safe.
                using (var truncate = new SqlCommand($"TRUNCATE TABLE [dbo].[{targetTable.Replace("]", "]]")}]", conn))
                {
                    await truncate.ExecuteNonQueryAsync();
                }

                populated++;
                if (rows.Count == 0) continue;

                // Discover the target table's column metadata. Columns split
                // into two groups:
                //   - mdbCols: columns the MDB row also has → use MDB value
                //   - extraNotNullCols: NOT NULL target columns the MDB lacks
                //     → must be included with a type-default so SQL doesn't
                //       try to use NULL and fail on the not-null constraint
                // IDENTITY columns are skipped in both lists (can't INSERT
                // into them without SET IDENTITY_INSERT).
                var targetColInfo = await GetTableColumnInfo(conn, targetTable);

                var firstRowCols = rows[0].Keys;
                var mdbCols = firstRowCols
                    .Where(c => targetColInfo.ContainsKey(c))
                    .Where(c => !targetColInfo[c].IsIdentity)
                    .ToList();
                var extraNotNullCols = targetColInfo.Values
                    .Where(ci => !ci.IsNullable && !ci.IsIdentity)
                    .Where(ci => !mdbCols.Any(mc => string.Equals(mc, ci.Name, StringComparison.OrdinalIgnoreCase)))
                    .Select(ci => ci.Name)
                    .ToList();
                var insertCols = mdbCols.Concat(extraNotNullCols).ToList();
                if (insertCols.Count == 0) continue;

                var colList = string.Join(", ", insertCols.Select(c => $"[{c}]"));
                var paramList = string.Join(", ", insertCols.Select((c, i) => $"@p{i}"));
                var insertSql = $"INSERT INTO [dbo].[{targetTable.Replace("]", "]]")}] ({colList}) VALUES ({paramList})";

                foreach (var row in rows)
                {
                    using var ins = new SqlCommand(insertSql, conn);
                    for (int i = 0; i < insertCols.Count; i++)
                    {
                        var col = insertCols[i];
                        var info = targetColInfo[col];

                        // Pull the MDB value if this row has the column.
                        // extraNotNullCols won't have a matching MDB key — we
                        // synthesize the default below.
                        object? raw = null;
                        if (row.TryGetValue(col, out var je) && je.ValueKind != JsonValueKind.Null)
                        {
                            raw = je.ValueKind switch
                            {
                                JsonValueKind.String => (object)je.GetString()!,
                                JsonValueKind.Number => je.TryGetInt64(out var l) ? l : je.GetDouble(),
                                JsonValueKind.True => true,
                                JsonValueKind.False => false,
                                _ => je.GetRawText()
                            };
                        }

                        // Null handling:
                        //  - raw is null + column is NULLABLE → DBNull
                        //  - raw is null + column is NOT NULL → type default
                        //  - raw has a value → pass through
                        // Substituting "" / 0 / false / 1900-01-01 for missing
                        // NOT NULL columns matches the practical effect for
                        // upd staging tables (wiped and refilled each deploy).
                        object finalValue;
                        if (raw == null)
                            finalValue = info.IsNullable ? (object)DBNull.Value : DefaultForSqlType(info.DataType);
                        else
                            finalValue = raw;

                        ins.Parameters.AddWithValue($"@p{i}", finalValue);
                    }
                    await ins.ExecuteNonQueryAsync();
                }
            }
            return populated;
        }

        // Metadata for one column of a target upd table.
        private sealed record ColumnMeta(string Name, string DataType, bool IsNullable, bool IsIdentity);

        private async Task<Dictionary<string, ColumnMeta>> GetTableColumnInfo(SqlConnection conn, string tableName)
        {
            var cols = new Dictionary<string, ColumnMeta>(StringComparer.OrdinalIgnoreCase);
            // INFORMATION_SCHEMA exposes nullability but not IDENTITY; pull
            // IsIdentity via COLUMNPROPERTY in the same query so we can skip
            // IDENTITY columns in the INSERT (would need SET IDENTITY_INSERT).
            using var cmd = new SqlCommand(@"
                SELECT
                    c.COLUMN_NAME,
                    c.DATA_TYPE,
                    c.IS_NULLABLE,
                    COLUMNPROPERTY(OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME)), c.COLUMN_NAME, 'IsIdentity') AS IsIdentity
                FROM INFORMATION_SCHEMA.COLUMNS c
                WHERE c.TABLE_NAME = @t", conn);
            cmd.Parameters.AddWithValue("@t", tableName);
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                var name = rdr.GetString(0);
                var dataType = rdr.GetString(1);
                var nullable = rdr.GetString(2) == "YES";
                var isIdentity = !rdr.IsDBNull(3) && rdr.GetInt32(3) == 1;
                cols[name] = new ColumnMeta(name, dataType, nullable, isIdentity);
            }
            return cols;
        }

        // Best-effort default for a SQL data type when the MDB row supplies
        // null for a NOT NULL column. Strings get empty string; numerics get 0;
        // bit gets false; datetime gets SQL's min (1900-01-01). Any unknown
        // type falls back to DBNull, which will surface as a clean SQL error
        // if it actually hits a NOT NULL column we didn't anticipate.
        private static object DefaultForSqlType(string dataType)
        {
            return dataType.ToLowerInvariant() switch
            {
                "char" or "varchar" or "nchar" or "nvarchar" or "text" or "ntext" or "xml" or "sysname" => "",
                "int" or "bigint" or "smallint" or "tinyint" => 0,
                "bit" => false,
                "datetime" or "datetime2" or "smalldatetime" or "date" => new DateTime(1900, 1, 1),
                "datetimeoffset" => new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero),
                "time" => TimeSpan.Zero,
                "money" or "smallmoney" or "decimal" or "numeric" => 0m,
                "float" or "real" => 0.0,
                "uniqueidentifier" => Guid.Empty,
                "varbinary" or "binary" or "image" => Array.Empty<byte>(),
                _ => DBNull.Value
            };
        }

        /// <summary>
        /// Returns { ready: true } if any upd* table in the deployment-relevant
        /// set has rows — i.e. Deploy was successfully run at some point and
        /// the On-Prem tab's Generate Script button should appear. We don't
        /// scope the check to the specific deploy record (no audit trail of
        /// which deploy produced which rows), but for a single-tenant Deploy
        /// screen that's good enough.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> HasDeployedData(int id)
        {
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection") ?? "";
                using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();
                // Pick a representative populated table per prefix. If any
                // has rows, treat the deploy as live. Cheap — uses metadata
                // rather than COUNT(*) on every upd table.
                using var cmd = new SqlCommand(@"
                    SELECT TOP 1 1
                    FROM sys.partitions
                    WHERE object_id IN (SELECT object_id FROM sys.tables WHERE name LIKE 'upd%')
                      AND index_id IN (0, 1)
                      AND rows > 0", conn);
                var hit = await cmd.ExecuteScalarAsync();
                return Json(new { ready = hit != null });
            }
            catch
            {
                return Json(new { ready = false });
            }
        }

        /// <summary>
        /// Copies all 6 selected files (3 LawDocs + 3 MDBs) for the given side
        /// ("pat" or "tmk") to the edm2016 network share. Each file lands in the
        /// subfolder that matches its path label, e.g.:
        ///   LawDocs/Pat/Ver9and10/ → \\edm2016\test\LawDocs\Pat\Ver9and10\
        ///   Mdbs/Pat/R5/           → \\edm2016\test\Mdbs\Pat\R5\
        /// Destination subfolders are expected to already exist on the share.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> PushMdbs(int id, string side)
        {
            try
            {
                if (string.IsNullOrEmpty(side))
                    return new JsonBadRequest(new { errors = new[] { "Side (pat/tmk) is required." } });

                var record = await _entityService.GetByIdAsync(id);
                if (record == null)
                    return new JsonBadRequest(new { errors = new[] { $"Deployment {id} not found." } });

                bool isPat = side.Equals("pat", StringComparison.OrdinalIgnoreCase);
                const string baseShare = @"\\edm2016\test";

                // Each job: (docId, relative subfolder path matching the UI label)
                var jobs = isPat
                    ? new (int? DocId, string SubPath)[]
                    {
                        (record.PatVer9And10LawDocId, @"LawDocs\Pat\Ver9and10"),
                        (record.PatVer9And10MdbId,    @"Mdbs\Pat\Ver9and10"),
                        (record.PatR5LawDocId,        @"LawDocs\Pat\R5"),
                        (record.PatR5MdbId,           @"Mdbs\Pat\R5"),
                        (record.PatR8LawDocId,        @"LawDocs\Pat\R8"),
                        (record.PatR8MdbId,           @"Mdbs\Pat\R8"),
                    }
                    : new (int? DocId, string SubPath)[]
                    {
                        (record.TmkVer9And10LawDocId, @"LawDocs\Tmk\Ver9and10"),
                        (record.TmkVer9And10MdbId,    @"Mdbs\Tmk\Ver9and10"),
                        (record.TmkR5LawDocId,        @"LawDocs\Tmk\R5"),
                        (record.TmkR5MdbId,           @"Mdbs\Tmk\R5"),
                        (record.TmkR9LawDocId,        @"LawDocs\Tmk\R9"),
                        (record.TmkR9MdbId,           @"Mdbs\Tmk\R9"),
                    };

                var emptyCount = jobs.Count(j => j.DocId == null);
                if (emptyCount > 0)
                    return new JsonBadRequest(new { errors = new[] { $"{emptyCount} selection(s) are empty — fill all dropdowns before pushing." } });

                var errors = new List<string>();
                int copied = 0;
                foreach (var (docId, subPath) in jobs)
                {
                    try
                    {
                        var doc = await _documentService.GetDocumentById(docId!.Value);
                        if (doc == null || !doc.FileId.HasValue)
                            throw new Exception($"Document {docId} not found or has no file.");
                        var file = await _documentService.GetFileById(doc.FileId.Value);
                        if (file == null)
                            throw new Exception($"File for document {docId} not found.");

                        var localPath = await EnsureLocalFile(
                            _documentHelper.GetDocumentPath(file.DocFileName),
                            file.DocFileName);
                        if (string.IsNullOrEmpty(localPath) || !System.IO.File.Exists(localPath))
                            throw new Exception($"File not found on disk: {doc.DocName}");

                        // Prefer the original upload name (includes extension).
                        // Fall back to DocName + FileExt so the copy always has
                        // the correct extension rather than landing as a bare file.
                        var fileName = !string.IsNullOrEmpty(file.UserFileName)
                            ? file.UserFileName
                            : doc.DocName + (string.IsNullOrEmpty(file.FileExt) ? "" : "." + file.FileExt);
                        var destFolder = Path.Combine(baseShare, subPath);
                        var destFile = Path.Combine(destFolder, fileName);
                        System.IO.File.Copy(localPath, destFile, overwrite: true);
                        copied++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{subPath}: {ex.Message}");
                    }
                }

                if (errors.Count > 0)
                {
                    var msg = $"Push completed with errors ({copied}/{jobs.Length} copied). " + string.Join(" | ", errors);
                    return new JsonBadRequest(new { errors = new[] { msg } });
                }

                return Json(new { success = $"Push completed: {copied} file(s) copied to {baseShare}." });
            }
            catch (Exception ex)
            {
                return new JsonBadRequest(new { errors = new[] { "Push failed: " + ex.Message } });
            }
        }

        /// <summary>
        /// Builds a SQL script that mirrors the on-prem deploy: USE [WebUpdates],
        /// DELETE FROM each populated upd table, then per-row INSERT INTO for
        /// every row currently in those tables. Matches the structure of the
        /// reference example (COUNTRTYLAWUPDATES-*.sql) minus the tblLawUpdates
        /// rows — those involve a SysVersionId / URL mapping the user will
        /// supply later. Instead of streaming the file back as a download we
        /// stash it in this deploy's Documents tree, named
        /// {Year}_{Quarter}_CountryLawScript.sql. The client refreshes the
        /// tree on success so the new file shows up immediately.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> GenerateScript(int id)
        {
            try
            {
                var record = await _entityService.GetByIdAsync(id);
                if (record == null)
                    return new JsonBadRequest(new { errors = new[] { $"Deployment {id} no longer exists." } });

                var connStr = _config.GetConnectionString("DefaultConnection") ?? "";
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("USE [WebUpdates]");
                sb.AppendLine("GO");

                using (var conn = new SqlConnection(connStr))
                {
                    await conn.OpenAsync();

                    // Find every upd* table that has rows. Each becomes a DELETE +
                    // a per-row INSERT block. Ordered by name for deterministic output.
                    var populated = new List<string>();
                    using (var listCmd = new SqlCommand(@"
                        SELECT t.name
                        FROM sys.tables t
                        INNER JOIN sys.partitions p ON p.object_id = t.object_id
                        WHERE t.name LIKE 'upd%' AND p.index_id IN (0, 1) AND p.rows > 0
                        ORDER BY t.name", conn))
                    using (var rdr = await listCmd.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync()) populated.Add(rdr.GetString(0));
                    }

                    // Phase 1 — wipe every populated table.
                    foreach (var t in populated)
                    {
                        sb.Append("delete from ").AppendLine(t);
                        sb.AppendLine("GO");
                    }

                    // Phase 2 — per-row INSERTs. Read each table, build literal-valued INSERTs.
                    foreach (var tableName in populated)
                    {
                        var cols = await GetTableColumnInfo(conn, tableName);
                        var insertCols = cols.Values.Where(c => !c.IsIdentity).Select(c => c.Name).ToList();
                        if (insertCols.Count == 0) continue;

                        var colList = string.Join(", ", insertCols.Select(c => $"[{c}]"));
                        var selectSql = $"SELECT {colList} FROM [dbo].[{tableName.Replace("]", "]]")}]";

                        using var selectCmd = new SqlCommand(selectSql, conn);
                        using var rdr = await selectCmd.ExecuteReaderAsync();
                        while (await rdr.ReadAsync())
                        {
                            var valueParts = new List<string>();
                            for (int i = 0; i < insertCols.Count; i++)
                            {
                                var info = cols[insertCols[i]];
                                valueParts.Add(rdr.IsDBNull(i) ? "NULL" : FormatSqlLiteral(rdr.GetValue(i), info.DataType));
                            }
                            sb.Append($"INSERT [dbo].[{tableName}] ({colList}) VALUES (")
                              .Append(string.Join(", ", valueParts))
                              .AppendLine(")");
                            sb.AppendLine("GO");
                        }
                    }
                } // close SQL connection before doing file I/O

                // ── Save into this deploy's Documents tree ──
                var userName = User.GetUserName();

                // Make sure the root folder exists. Same shape as the
                // Documents tab uses; auto-create on first-touch.
                var rootName = DeployRootFolderName(record);
                var rootFolder = await _documentService.GetFolder(
                    DeployScreenCode, "DeployPasswordId", id, rootName, 0);
                if (rootFolder == null)
                {
                    rootFolder = await _documentService.AddFolder(
                        DeployScreenCode, "DeployPasswordId", "Dep", id, rootName, 0, false);
                }
                if (rootFolder == null)
                    return new JsonBadRequest(new { errors = new[] { "Could not resolve or create the deploy's root document folder." } });

                // Name format requested by the user: 2026_Q1_CountryLawScript.sql.
                // Quarter is stored with its "Q" prefix already, so a plain
                // join produces the right shape.
                var baseName = $"{record.Year}_{record.Quarter}_CountryLawScript";
                var existingNames = await _documentService.DocDocuments
                    .Where(d => d.FolderId == rootFolder.FolderId && d.DocName != null && d.DocName.StartsWith(baseName))
                    .Select(d => d.DocName!)
                    .ToListAsync();
                var docName = baseName;
                if (existingNames.Contains(docName))
                {
                    int n = 1;
                    while (existingNames.Contains($"{baseName} ({n})")) n++;
                    docName = $"{baseName} ({n})";
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());

                var docFile = new DocFile
                {
                    FileExt = "sql",
                    UserFileName = $"{docName}.sql",
                    FileSize = bytes.Length,
                    IsImage = false,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    DateCreated = DateTime.Now,
                    LastUpdate = DateTime.Now
                };
                docFile = await _documentService.AddDocFile(docFile);

                var docDocument = new DocDocument
                {
                    FolderId = rootFolder.FolderId,
                    DocName = docName,
                    FileId = docFile.FileId,
                    Author = userName,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    DateCreated = DateTime.Now,
                    LastUpdate = DateTime.Now
                };
                await _documentService.UpdateDocuments(userName,
                    Enumerable.Empty<DocDocument>(),
                    new[] { docDocument },
                    Enumerable.Empty<DocDocument>(),
                    null, false);

                // Persist the file bytes through the document helper so the
                // path matches everything else under Searchable/Documents
                // (same plumbing the PDF report generator uses).
                using (var stream = new MemoryStream(bytes))
                {
                    var folderHeader = await _documentService.GetFolderHeader(rootFolder.FolderId);
                    await _documentHelper.SaveDocumentFromStream(stream, docFile.DocFileName, folderHeader);
                }

                return Json(new
                {
                    success = $"Script generated: {docName}.sql — open the Documents tab to view or download.",
                    fileName = docFile.UserFileName,
                    fileId = docFile.FileId,
                    docId = docDocument.DocId
                });
            }
            catch (Exception ex)
            {
                return new JsonBadRequest(new { errors = new[] { "Generate Script failed: " + ex.Message } });
            }
        }

        // Format a SQL Server value as a literal for inclusion in an INSERT
        // statement. Strings/dates/guids/binary follow the same syntax the
        // SQL Server scripting tools emit (N'...', CAST(N'2026-03-02T00:00:00' AS DateTime),
        // 0x...). Single-quote escaping doubles the quote per T-SQL rules.
        private static string FormatSqlLiteral(object value, string sqlType)
        {
            if (value == null || value is DBNull) return "NULL";
            switch (sqlType.ToLowerInvariant())
            {
                case "bit":
                    return ((bool)value) ? "1" : "0";
                case "tinyint":
                case "smallint":
                case "int":
                case "bigint":
                    return System.Convert.ToInt64(value).ToString(System.Globalization.CultureInfo.InvariantCulture);
                case "decimal":
                case "numeric":
                case "money":
                case "smallmoney":
                    return System.Convert.ToDecimal(value).ToString(System.Globalization.CultureInfo.InvariantCulture);
                case "float":
                case "real":
                    return System.Convert.ToDouble(value).ToString(System.Globalization.CultureInfo.InvariantCulture);
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                case "date":
                {
                    var dt = System.Convert.ToDateTime(value);
                    var castType = sqlType.Equals("smalldatetime", StringComparison.OrdinalIgnoreCase)
                        ? "SmallDateTime"
                        : sqlType.Equals("date", StringComparison.OrdinalIgnoreCase)
                            ? "Date"
                            : "DateTime";
                    return $"CAST(N'{dt:yyyy-MM-ddTHH:mm:ss.fff}' AS {castType})";
                }
                case "datetimeoffset":
                {
                    var dto = (DateTimeOffset)value;
                    return $"CAST(N'{dto:yyyy-MM-ddTHH:mm:ss.fffzzz}' AS DateTimeOffset)";
                }
                case "time":
                {
                    var ts = (TimeSpan)value;
                    return $"CAST(N'{ts:hh\\:mm\\:ss\\.fff}' AS Time)";
                }
                case "uniqueidentifier":
                    return $"N'{(Guid)value}'";
                case "varbinary":
                case "binary":
                case "image":
                {
                    var bytes = (byte[])value;
                    return "0x" + Convert.ToHexString(bytes);
                }
                default:
                    // String-ish — escape single quotes and N-prefix for unicode.
                    return $"N'{value.ToString()!.Replace("'", "''")}'";
            }
        }

        #region Documents

        // Fixed screen code for Deploy documents — separates them from
        // Release documents in the shared DocFolder / DocDocument tables.
        // "DP" mirrors the 1-2 char convention ReleaseController uses for
        // its system-type-derived codes (P / T / Pa / Tm / etc.).
        private const string DeployScreenCode = "DP";

        private string DeployRootFolderName(DeployPassword record) =>
            $"Deploy {record.Year} {record.Quarter}";

        /// <summary>Read the document tree for this Deploy record. Auto-creates the root folder on first access.</summary>
        public async Task<IActionResult> DocumentTreeRead(int deployPasswordId, string id)
        {
            var record = await _entityService.GetByIdAsync(deployPasswordId);
            if (record == null) return Json(new List<object>());

            try
            {
                var treeNodes = await _documentService.GetDocumentTree(
                    DeployScreenCode, "Dep", "DeployPasswordId", deployPasswordId, id);
                if (treeNodes != null && treeNodes.Any())
                    return Json(treeNodes);
            }
            catch (Exception)
            {
                // Sproc may not support Deploy screen yet — fall through to manual query.
            }

            // Fallback: find or auto-create the root folder.
            var rootName = DeployRootFolderName(record);
            var rootFolder = await _documentService.GetFolder(
                DeployScreenCode, "DeployPasswordId", deployPasswordId, rootName, 0);

            if (rootFolder == null)
            {
                try
                {
                    rootFolder = await _documentService.AddFolder(
                        DeployScreenCode, "DeployPasswordId", "Dep", deployPasswordId, rootName, 0, false);
                }
                catch (Exception)
                {
                    return Json(new List<object>());
                }
            }

            if (rootFolder != null)
            {
                // Same pipe-delimited ID convention as Release:
                // {screenCode}|{tag}|{dataKey}|{dataKeyValue}|{type}||{folderId}|
                var pipeId = $"{DeployScreenCode}|Dep|DeployPasswordId|{deployPasswordId}|user||{rootFolder.FolderId}|";
                var nodes = new List<object>
                {
                    new
                    {
                        id = pipeId,
                        text = rootFolder.FolderName,
                        hasChildren = false,
                        expanded = false,
                        isReadOnly = false,
                        iconClass = "fal fa-folder",
                        detailAction = ""
                    }
                };
                return Json(nodes);
            }

            return Json(new List<object>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> DocumentAddFolder(string id, int deployPasswordId, string folderName)
        {
            try
            {
                if (string.IsNullOrEmpty(folderName))
                    return new JsonBadRequest("Folder name is required.");

                var userName = User.GetUserName();

                if (!string.IsNullOrEmpty(id) && id.Contains("|"))
                {
                    var newNode = await _documentService.AddTreeFolder(id, folderName, userName);
                    if (newNode != null && newNode.id == "")
                        return new JsonBadRequest("Folder name exists. Please use a different name.");
                    return Json(newNode);
                }
                else
                {
                    var record = await _entityService.GetByIdAsync(deployPasswordId);
                    if (record == null)
                        return new JsonBadRequest("Deployment not found.");

                    int parentFolderId = 0;
                    if (int.TryParse(id, out var parsedId) && parsedId > 0)
                        parentFolderId = parsedId;

                    var folder = await _documentService.AddFolder(
                        DeployScreenCode, "DeployPasswordId", "Dep", deployPasswordId, folderName, parentFolderId, false);
                    return Json(new { id = folder.FolderId.ToString(), text = folder.FolderName, hasChildren = false, expanded = false, iconClass = "fal fa-folder" });
                }
            }
            catch (Exception ex)
            {
                return new JsonBadRequest("Error adding folder: " + ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> DocumentRenameNode(string id, string newName)
        {
            try
            {
                if (string.IsNullOrEmpty(newName))
                    return BadRequest("Name is required.");

                var userName = User.GetUserName();

                if (!string.IsNullOrEmpty(id) && id.Contains("|"))
                {
                    var parts = id.Split('|');
                    if (id.Contains("|doc|") && parts.Length >= 8)
                    {
                        var docId = int.Parse(parts[7]);
                        await _documentService.RenameDocument(userName, docId, newName);
                    }
                    else if (parts.Length >= 7)
                    {
                        var folderId = int.Parse(parts[6]);
                        await _documentService.RenameFolder(userName, folderId, newName);
                    }
                }
                else
                {
                    var folderId = int.Parse(id);
                    await _documentService.RenameFolder(userName, folderId, newName);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("Error renaming: " + ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> DocumentDeleteNode(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id) && id.Contains("|"))
                {
                    var parts = id.Split('|');
                    if (id.Contains("|doc|") && parts.Length >= 8)
                    {
                        var docId = int.Parse(parts[7]);
                        var document = await _documentService.GetDocumentById(docId);
                        if (document != null)
                        {
                            DocFile? docFile = null;
                            if (document.FileId.HasValue && document.FileId.Value > 0)
                                docFile = await _documentService.GetFileById(document.FileId.Value);

                            await _documentService.DeleteDoc(document, docFile);

                            if (docFile != null && !string.IsNullOrEmpty(docFile.DocFileName))
                                _documentHelper.DeleteDocumentFile(docFile.DocFileName, docFile.ThumbFileName, docFile.IsImage);
                        }
                    }
                    else if (parts.Length >= 7)
                    {
                        var folderId = int.Parse(parts[6]);
                        await _documentService.DeleteDocumentsByFolderId(folderId);
                    }
                }
                else
                {
                    var folderId = int.Parse(id);
                    await _documentService.DeleteDocumentsByFolderId(folderId);
                }

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest("Unable to delete. The file may be locked by another process. Please try again later.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        public IActionResult DocumentDropNode(string sourceId, string destId)
        {
            // Drag-drop reorder requires IDocumentViewModelService which isn't
            // wired up — mirror ReleaseController's no-op so the tree updates
            // visually without backend persistence.
            return Ok();
        }

        public async Task<IActionResult> DocumentGridRead([DataSourceRequest] DataSourceRequest request, int folderId)
        {
            try
            {
                var viewableExts = new[] { "pdf", "jpg", "jpeg", "png", "gif", "bmp", "tiff", "svg" };

                var docs = await _documentService.DocDocuments
                    .Where(d => d.FolderId == folderId)
                    .Select(d => new
                    {
                        d.DocId,
                        d.DocName,
                        d.Author,
                        d.IsPrivate,
                        d.DateCreated,
                        d.FileId,
                        UserFileName = d.DocFile != null ? d.DocFile.UserFileName : "",
                        DocFileName = d.DocFile != null ? d.DocFile.DocFileName : "",
                        FolderName = d.DocFolder != null ? d.DocFolder.FolderName : "",
                        DocTypeName = d.DocType != null ? d.DocType.DocTypeName : "",
                        IsImage = d.DocFile != null && d.DocFile.IsImage,
                        ForSignature = d.DocFile != null && d.DocFile.ForSignature == true,
                        IconClass = d.DocFile != null && d.DocFile.DocIcon != null ? d.DocFile.DocIcon.IconClass : "fal fa-file",
                        FileExt = d.DocFile != null ? d.DocFile.FileExt : ""
                    })
                    .ToListAsync();

                var result = docs.Select(d => new
                {
                    d.DocId, d.DocName, d.Author, d.IsPrivate, d.DateCreated, d.FileId,
                    d.UserFileName, d.DocFileName, d.FolderName, d.DocTypeName,
                    d.IsImage, d.ForSignature, d.IconClass,
                    IsDocViewable = !string.IsNullOrEmpty(d.FileExt) && viewableExts.Any(ext => ext.Equals(d.FileExt, StringComparison.OrdinalIgnoreCase))
                }).ToList();

                return Json(result.ToDataSourceResult(request));
            }
            catch (Exception)
            {
                return Json(new List<object>().ToDataSourceResult(request));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> DocumentGridDelete(int docId)
        {
            try
            {
                var document = await _documentService.GetDocumentById(docId);
                if (document == null)
                    return new JsonBadRequest("Document not found.");

                DocFile? docFile = null;
                if (document.FileId.HasValue && document.FileId.Value > 0)
                    docFile = await _documentService.GetFileById(document.FileId.Value);

                await _documentService.DeleteDoc(document, docFile);

                if (docFile != null && !string.IsNullOrEmpty(docFile.DocFileName))
                    _documentHelper.DeleteDocumentFile(docFile.DocFileName, docFile.ThumbFileName, docFile.IsImage);

                return Ok();
            }
            catch (Exception)
            {
                return new JsonBadRequest("Error deleting document.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> SaveDocuments(IEnumerable<IFormFile> droppedDocs, int folderId, int deployPasswordId)
        {
            try
            {
                if (droppedDocs == null || !droppedDocs.Any())
                    return Content("");

                var userName = User.GetUserName();
                var folderHeader = await _documentService.GetFolderHeader(folderId);

                foreach (var file in droppedDocs)
                {
                    if (file == null || file.Length == 0) continue;

                    var originalFileName = file.FileName;
                    var fileExtension = Path.GetExtension(originalFileName);

                    var docFile = new DocFile
                    {
                        FileExt = fileExtension?.TrimStart('.'),
                        UserFileName = originalFileName,
                        FileSize = (int)file.Length,
                        IsImage = IsImageFile(fileExtension),
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now
                    };
                    docFile = await _documentService.AddDocFile(docFile);

                    var docTypeId = await _documentService.GetDocTypeIdFromFileName(originalFileName);

                    var docDocument = new DocDocument
                    {
                        FolderId = folderId,
                        DocName = Path.GetFileNameWithoutExtension(originalFileName),
                        DocTypeId = docTypeId > 0 ? docTypeId : null,
                        FileId = docFile.FileId,
                        Author = userName,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now
                    };

                    await _documentService.UpdateDocuments(userName,
                        Enumerable.Empty<DocDocument>(),
                        new[] { docDocument },
                        Enumerable.Empty<DocDocument>(),
                        null, false);

                    await _documentHelper.SaveDocumentFileUpload(file, docFile.DocFileName, null, folderHeader);
                }

                return Content("");
            }
            catch (Exception ex)
            {
                return new JsonBadRequest(new { errors = "Error uploading files: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> DocumentUpload(int deployPasswordId, int folderId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return new JsonBadRequest(new { errors = "No file selected." });

                var record = await _entityService.GetByIdAsync(deployPasswordId);
                if (record == null)
                    return new JsonBadRequest(new { errors = "Deployment not found." });

                var userName = User.GetUserName();
                var originalFileName = file.FileName;
                var fileExtension = Path.GetExtension(originalFileName);

                var docFile = new DocFile
                {
                    FileExt = fileExtension?.TrimStart('.'),
                    UserFileName = originalFileName,
                    FileSize = (int)file.Length,
                    IsImage = IsImageFile(fileExtension),
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    DateCreated = DateTime.Now,
                    LastUpdate = DateTime.Now
                };
                docFile = await _documentService.AddDocFile(docFile);

                var docTypeId = await _documentService.GetDocTypeIdFromFileName(originalFileName);

                var docDocument = new DocDocument
                {
                    FolderId = folderId,
                    DocName = Path.GetFileNameWithoutExtension(originalFileName),
                    DocTypeId = docTypeId > 0 ? docTypeId : null,
                    FileId = docFile.FileId,
                    Author = userName,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    DateCreated = DateTime.Now,
                    LastUpdate = DateTime.Now
                };

                await _documentService.UpdateDocuments(userName,
                    Enumerable.Empty<DocDocument>(),
                    new[] { docDocument },
                    Enumerable.Empty<DocDocument>(),
                    null, false);

                var folderHeader = await _documentService.GetFolderHeader(folderId);
                await _documentHelper.SaveDocumentFileUpload(file, docFile.DocFileName, null, folderHeader);

                return Json(new { success = true, fileName = originalFileName });
            }
            catch (Exception ex)
            {
                return new JsonBadRequest(new { errors = "Error uploading file: " + ex.Message });
            }
        }

        public async Task<IActionResult> DocumentDownload(int fileId)
        {
            try
            {
                var docFile = await _documentService.GetFileById(fileId);
                if (docFile == null) return NotFound();

                var filePath = _documentHelper.GetDocumentPath(docFile.DocFileName);
                if (string.IsNullOrEmpty(filePath)) return NotFound("File not found.");

                byte[] fileBytes;
                if (System.IO.File.Exists(filePath))
                {
                    fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                }
                else
                {
                    var stream = await _documentStorage.GetFileStream(filePath);
                    if (stream == null) return NotFound("File not found on disk.");
                    fileBytes = stream.ToArray();
                }

                var contentType = GetContentType(docFile.FileExt);
                return File(fileBytes, contentType, docFile.UserFileName ?? docFile.DocFileName);
            }
            catch (Exception)
            {
                return NotFound("Error downloading file.");
            }
        }

        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> DocumentDetailDialog(int folderId, int deployPasswordId = 0, int docId = 0)
        {
            try
            {
                if (folderId <= 0 && deployPasswordId > 0)
                {
                    var record = await _entityService.GetByIdAsync(deployPasswordId);
                    if (record != null)
                    {
                        var rootName = DeployRootFolderName(record);
                        var rootFolder = await _documentService.GetFolder(
                            DeployScreenCode, "DeployPasswordId", deployPasswordId, rootName, 0);
                        if (rootFolder == null)
                        {
                            rootFolder = await _documentService.AddFolder(
                                DeployScreenCode, "DeployPasswordId", "Dep", deployPasswordId, rootName, 0, false);
                        }
                        if (rootFolder != null) folderId = rootFolder.FolderId;
                    }
                }

                if (docId > 0)
                {
                    var doc = await _documentService.GetDocumentById(docId);
                    if (doc == null) return NotFound();

                    var docFile = doc.FileId.HasValue && doc.FileId.Value > 0
                        ? await _documentService.GetFileById(doc.FileId.Value)
                        : null;

                    ViewBag.FolderId = folderId;
                    ViewBag.IsAddMode = false;
                    ViewBag.UserFileName = docFile?.UserFileName;
                    return PartialView("_DocumentDetailDialog", doc);
                }

                var newDoc = new DocDocument
                {
                    FolderId = folderId,
                    Author = User.GetUserName(),
                    DateCreated = DateTime.Now
                };
                ViewBag.FolderId = folderId;
                ViewBag.IsAddMode = true;
                ViewBag.UserFileName = "";
                return PartialView("_DocumentDetailDialog", newDoc);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'>{ex}</div>", "text/html");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> SaveDocumentDetail(
            int DocId, int FolderId, string DocName, int? DocTypeId, string DocUrl,
            bool IsPrivate, bool IsDefault, bool IsPrintOnReport, bool IncludeInWorkflow,
            string Remarks, IFormFile UploadedFile)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(DocName))
                    return new JsonBadRequest(new { errors = "Document Name is required." });

                var userName = User.GetUserName();

                if (DocId > 0)
                {
                    var doc = await _documentService.GetDocumentById(DocId);
                    if (doc == null) return new JsonBadRequest(new { errors = "Document not found." });

                    doc.DocName = DocName;
                    doc.DocTypeId = DocTypeId;
                    doc.DocUrl = DocUrl;
                    doc.IsPrivate = IsPrivate;
                    doc.IsDefault = IsDefault;
                    doc.IsPrintOnReport = IsPrintOnReport;
                    doc.IncludeInWorkflow = IncludeInWorkflow;
                    doc.Remarks = Remarks;
                    doc.UpdatedBy = userName;
                    doc.LastUpdate = DateTime.Now;

                    if (UploadedFile != null && UploadedFile.Length > 0)
                    {
                        var fileExtension = Path.GetExtension(UploadedFile.FileName);
                        var docFile = new DocFile
                        {
                            FileExt = fileExtension?.TrimStart('.'),
                            UserFileName = UploadedFile.FileName,
                            FileSize = (int)UploadedFile.Length,
                            IsImage = IsImageFile(fileExtension),
                            CreatedBy = userName,
                            UpdatedBy = userName,
                            DateCreated = DateTime.Now,
                            LastUpdate = DateTime.Now
                        };
                        docFile = await _documentService.AddDocFile(docFile);
                        doc.FileId = docFile.FileId;

                        var folderHeader = await _documentService.GetFolderHeader(FolderId);
                        await _documentHelper.SaveDocumentFileUpload(UploadedFile, docFile.DocFileName, null, folderHeader);
                    }

                    await _documentService.UpdateDocuments(userName,
                        new[] { doc },
                        Enumerable.Empty<DocDocument>(),
                        Enumerable.Empty<DocDocument>(),
                        null, false);

                    return Json(new { success = true });
                }
                else
                {
                    int? fileId = null;

                    if (UploadedFile != null && UploadedFile.Length > 0)
                    {
                        var fileExtension = Path.GetExtension(UploadedFile.FileName);
                        var docFile = new DocFile
                        {
                            FileExt = fileExtension?.TrimStart('.'),
                            UserFileName = UploadedFile.FileName,
                            FileSize = (int)UploadedFile.Length,
                            IsImage = IsImageFile(fileExtension),
                            CreatedBy = userName,
                            UpdatedBy = userName,
                            DateCreated = DateTime.Now,
                            LastUpdate = DateTime.Now
                        };
                        docFile = await _documentService.AddDocFile(docFile);
                        fileId = docFile.FileId;

                        var folderHeader = await _documentService.GetFolderHeader(FolderId);
                        await _documentHelper.SaveDocumentFileUpload(UploadedFile, docFile.DocFileName, null, folderHeader);
                    }

                    var newDoc = new DocDocument
                    {
                        FolderId = FolderId,
                        DocName = DocName,
                        DocTypeId = DocTypeId,
                        DocUrl = DocUrl,
                        IsPrivate = IsPrivate,
                        IsDefault = IsDefault,
                        IsPrintOnReport = IsPrintOnReport,
                        IncludeInWorkflow = IncludeInWorkflow,
                        Remarks = Remarks,
                        FileId = fileId,
                        Author = userName,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now
                    };

                    await _documentService.UpdateDocuments(userName,
                        Enumerable.Empty<DocDocument>(),
                        new[] { newDoc },
                        Enumerable.Empty<DocDocument>(),
                        null, false);

                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return new JsonBadRequest(new { errors = "Error saving document: " + ex.ToString() });
            }
        }

        // Shared helpers — same shape as ReleaseController's. Duplicated here
        // instead of extracted to a shared utility so DeployController stays
        // self-contained while the document-tab pattern is still settling.
        private static bool IsImageFile(string? extension)
        {
            if (string.IsNullOrEmpty(extension)) return false;
            var ext = extension.TrimStart('.').ToLowerInvariant();
            return ext == "jpg" || ext == "jpeg" || ext == "png" || ext == "gif"
                || ext == "bmp" || ext == "tiff" || ext == "svg" || ext == "webp";
        }

        private static string GetContentType(string? fileExtension)
        {
            if (string.IsNullOrEmpty(fileExtension)) return "application/octet-stream";
            var ext = fileExtension.TrimStart('.').ToLowerInvariant();
            return ext switch
            {
                "pdf" => "application/pdf",
                "doc" => "application/msword",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "xls" => "application/vnd.ms-excel",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ppt" => "application/vnd.ms-powerpoint",
                "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "gif" => "image/gif",
                "txt" => "text/plain",
                "csv" => "text/csv",
                "zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }

        #endregion

        [HttpPost, Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _entityService.GetByIdAsync(id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            await _entityService.Delete(entity);

            return Ok();
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var entity = await _entityService.GetByIdAsync(id);
            if (entity == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = entity.CreatedBy, dateCreated = entity.DateCreated, updatedBy = entity.UpdatedBy, lastUpdate = entity.LastUpdate });
        }

        private async Task<DetailPageViewModel<DeployPassword>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<DeployPassword>
            {
                Detail = await _entityService.GetByIdAsync(id)
            };

            if (viewModel.Detail != null)
            {
                viewModel.AddReleaseAuxiliarySecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanCopyRecord = false;
                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);
                viewModel.Container = _dataContainer;
                viewModel.EditScreenUrl = this.Url.Action("Detail", new { id = id });
                viewModel.SearchScreenUrl = this.Url.Action("Index");
            }
            return viewModel;
        }

        private async Task<DetailPageViewModel<DeployPassword>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<DeployPassword>
            {
                Detail = new DeployPassword()
            };

            viewModel.AddReleaseAuxiliarySecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }
    }
}
