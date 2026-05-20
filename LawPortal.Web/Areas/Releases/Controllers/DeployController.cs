using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IStringLocalizer<SharedResource> _localizer;

        private readonly string _dataContainer = "deployDetail";

        public DeployController(
            IAuthorizationService authService,
            IViewModelService<DeployPassword> viewModelService,
            IEntityService<DeployPassword> entityService,
            IEntityService<Release> releaseService,
            IDocumentService documentService,
            IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _entityService = entityService;
            _releaseService = releaseService;
            _documentService = documentService;
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

        [HttpPost, Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearUpdTables()
        {
            try
            {
                var config = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
                var connStr = config.GetConnectionString("DefaultConnection");

                var tableNames = new List<string>();
                using (var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr))
                {
                    await conn.OpenAsync();
                    using (var listCmd = new Microsoft.Data.SqlClient.SqlCommand(
                        "SELECT name FROM sys.tables WHERE is_ms_shipped = 0 AND name LIKE 'upd%' ORDER BY name", conn))
                    using (var rdr = await listCmd.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                            tableNames.Add(rdr.GetString(0));
                    }

                    foreach (var t in tableNames)
                    {
                        // Bracket-quote the name; sys.tables names can't contain ']' so this
                        // round-trip is safe. TRUNCATE TABLE doesn't accept a parameter.
                        using var truncateCmd = new Microsoft.Data.SqlClient.SqlCommand(
                            $"TRUNCATE TABLE [dbo].[{t.Replace("]", "]]")}]", conn);
                        await truncateCmd.ExecuteNonQueryAsync();
                    }
                }

                return Json(new { success = $"Successfully cleared all {tableNames.Count} update tables." });
            }
            catch (Exception ex)
            {
                return new JsonBadRequest(new { errors = "Error clearing tables: " + ex.Message });
            }
        }

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
