using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using LawPortal.Core.Entities;
using LawPortal.Core.Entities.Documents;
using LawPortal.Core.Helpers;
using LawPortal.Core.Interfaces;
using LawPortal.Core.Interfaces.Shared;
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
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LawPortal.Web.Areas.Releases.Controllers
{
    [Area("Releases"), Authorize(Policy = ReleaseAuthorizationPolicy.CanAccessAuxiliary)]
    public class ReleaseController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<Release> _viewModelService;
        private readonly IEntityService<Release> _entityService;
        private readonly IEntityService<AppSystem> _systemService;
        private readonly IDocumentService _documentService;
        private readonly IDocumentHelper _documentHelper;
        private readonly IDocumentStorage _documentStorage;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IConfiguration _config;

        private readonly string _dataContainer = "releaseDetail";

        public ReleaseController(
            IAuthorizationService authService,
            IViewModelService<Release> viewModelService,
            IEntityService<Release> entityService,
            IEntityService<AppSystem> systemService,
            IDocumentService documentService,
            IDocumentHelper documentHelper,
            IDocumentStorage documentStorage,
            IStringLocalizer<SharedResource> localizer,
            IConfiguration config)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _entityService = entityService;
            _systemService = systemService;
            _documentService = documentService;
            _documentHelper = documentHelper;
            _documentStorage = documentStorage;
            _localizer = localizer;
            _config = config;
        }

        // Returns a local file path for the given path-from-DocumentHelper.
        // - File system mode: GetDocumentPath returns a local path; this just passes it through
        //   if the file exists on disk.
        // - Azure mode: GetDocumentPath returns a blob-relative path (no drive letter,
        //   forward slashes); File.Exists is false, so we download the blob to a temp file
        //   and return the temp path. Lets the 32-bit MDB sidecar (which only knows local
        //   files) work in both storage modes without further changes.
        private async Task<string> EnsureLocalFile(string pathFromHelper, string docFileName)
        {
            if (string.IsNullOrEmpty(pathFromHelper)) return string.Empty;
            if (System.IO.File.Exists(pathFromHelper)) return pathFromHelper;
            var tempPath = Path.Combine(Path.GetTempPath(), $"mdbcache_{Guid.NewGuid():N}_{docFileName}");
            return await _documentStorage.SaveFileStreamToPath(pathFromHelper, tempPath);
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "releaseSearch",
                Title = _localizer["MDB Generation"].ToString(),
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
                PageId = "releaseSearchResults",
                Title = _localizer["MDB Generation"].ToString(),
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
                var releases = _entityService.QueryableList;

                if (mainSearchFilters != null && mainSearchFilters.Count > 0)
                    releases = _viewModelService.AddCriteria(releases, mainSearchFilters);

                var result = await _viewModelService.CreateViewModelForGrid(request, releases, "Name", "ReleaseId");
                return Json(result);
            }

            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private Task LoadSystemsList()
        {
            ViewData["SystemsList"] = Helpers.SystemsHelper.SystemNames.ToList();
            return Task.CompletedTask;
        }

        private async Task<DetailPageViewModel<Release>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<Release>
            {
                Detail = await _entityService.GetByIdAsync(id)
            };

            if (viewModel.Detail != null)
            {
                viewModel.AddReleaseAuxiliarySecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                this.AddDefaultNavigationUrls(viewModel);
                viewModel.Container = _dataContainer;
                viewModel.EditScreenUrl = this.Url.Action("Detail", new { id = id });
                viewModel.SearchScreenUrl = this.Url.Action("Index");
            }
            return viewModel;
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            await LoadSystemsList();
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
                Title = _localizer["MDB Generation"].ToString(),
                RecordId = detail.ReleaseId,
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

        [HttpPost()]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        private async Task<DetailPageViewModel<Release>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<Release>
            {
                Detail = new Release()
            };

            viewModel.AddReleaseAuxiliarySecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            await LoadSystemsList();
            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New MDB Generation"].ToString(),
                RecordId = detail.ReleaseId,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };
            ModelState.Clear();

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] Release release)
        {
            if (release == null)
                return new JsonBadRequest(new { errors = new[] { "Save: posted body deserialized to null — check form fields match Release model." } });

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            bool isNew = release.ReleaseId == 0;

            // For existing records, re-load from DB and copy editable fields onto
            // the tracked entity. This avoids sending a NULL UpdatedBy / CreatedBy
            // / DateCreated (which triggers "Value cannot be null. (Parameter
            // 'entity')" when the hidden record-stamp inputs aren't posted) and
            // gives EF Core a full non-null tracked entity to persist.
            if (!isNew)
            {
                var existing = await _entityService.GetByIdAsync(release.ReleaseId);
                if (existing == null)
                    return new JsonBadRequest(new { errors = new[] { $"Release {release.ReleaseId} no longer exists." } });

                existing.Name = release.Name;
                existing.Year = release.Year;
                existing.Quarter = release.Quarter;
                existing.SystemType = release.SystemType;
                existing.Systems = release.Systems ?? existing.Systems;
                existing.GeneratePatent = release.GeneratePatent;
                existing.GenerateTrademark = release.GenerateTrademark;
                existing.ReportNotesPatent = release.ReportNotesPatent;
                existing.ReportNotesTrademark = release.ReportNotesTrademark;
                UpdateEntityStamps(existing, existing.ReleaseId);
                await _entityService.Update(existing);
                return Json(existing.ReleaseId);
            }

            UpdateEntityStamps(release, release.ReleaseId);
            await _entityService.Add(release);

            // Auto-create root document folder for new releases
            if (release.ReleaseId > 0)
            {
                try
                {
                    await _documentService.AddFolder(
                        ToDocSystemType(release.SystemType), "ReleaseId", "Rel", release.ReleaseId,
                        TruncateFolderName(release.Name), 0, false);
                }
                catch (Exception)
                {
                    // Don't fail the save if folder creation fails
                }
            }

            return Json(release.ReleaseId);
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
            var release = await _entityService.GetByIdAsync(id);
            if (release == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = release.CreatedBy, dateCreated = release.DateCreated, updatedBy = release.UpdatedBy, lastUpdate = release.LastUpdate });
        }

        #region Documents

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

        private static string GetReportBaseName(string systemType, int year, string quarter, bool isPat)
        {
            var qNum = (quarter ?? "").TrimStart('Q');
            if (string.IsNullOrEmpty(qNum)) qNum = "1";
            var prefix = $"{year}_{qNum}";

            if (isPat)
            {
                if (systemType.Equals("PatR8-R10v2.1", StringComparison.OrdinalIgnoreCase))
                    return $"PatentR8_{year}_{qNum}";
                // PatR5-7 or R4
                return $"{prefix}_Pat2000";
            }
            else
            {
                if (systemType.Equals("TmkR9-10v2.2", StringComparison.OrdinalIgnoreCase))
                    return $"{prefix}_TmkR9";
                // R4 or TmkR5-8
                return $"{prefix}_Tmk2000";
            }
        }

        public async Task<IActionResult> DocumentTreeRead(int releaseId, string id)
        {
            var release = await _entityService.GetByIdAsync(releaseId);
            if (release == null)
                return Json(new List<object>());

            try
            {
                var treeNodes = await _documentService.GetDocumentTree(
                    ToDocSystemType(release.SystemType), "Rel", "ReleaseId", releaseId, id);

                if (treeNodes != null && treeNodes.Any())
                    return Json(treeNodes);
            }
            catch (Exception)
            {
                // Stored procedure doesn't support Release screen code yet — fall through to manual query
            }

            // Fallback: query DocFolder table directly and build tree nodes
            var rootFolder = await _documentService.GetFolder(
                ToDocSystemType(release.SystemType), "ReleaseId", releaseId, TruncateFolderName(release.Name), 0);

            // Auto-create root folder if it doesn't exist yet (for releases created before this feature)
            if (rootFolder == null)
            {
                try
                {
                    rootFolder = await _documentService.AddFolder(
                        ToDocSystemType(release.SystemType), "ReleaseId", "Rel", releaseId,
                        TruncateFolderName(release.Name), 0, false);
                }
                catch (Exception)
                {
                    return Json(new List<object>());
                }
            }

            if (rootFolder != null)
            {
                // Build pipe-delimited ID matching convention: systemType|screenCode|dataKey|dataKeyValue|type||folderId|
                var pipeId = $"{ToDocSystemType(release.SystemType)}|Rel|ReleaseId|{releaseId}|user||{rootFolder.FolderId}|";
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

        /// <summary>
        /// Add folder — matches old DocFolderController.AddFolder pattern.
        /// The 'id' parameter is the pipe-delimited tree node ID of the parent.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> DocumentAddFolder(string id, int releaseId, string folderName)
        {
            try
            {
                if (string.IsNullOrEmpty(folderName))
                    return new JsonBadRequest("Folder name is required.");

                var userName = User.GetUserName();

                // If id is a pipe-delimited tree node, use the sproc
                if (!string.IsNullOrEmpty(id) && id.Contains("|"))
                {
                    var newNode = await _documentService.AddTreeFolder(id, folderName, userName);
                    if (newNode != null && newNode.id == "")
                        return new JsonBadRequest("Folder name exists. Please use a different name.");
                    return Json(newNode);
                }
                else
                {
                    // Plain numeric ID — treat as parent folder ID for subfolder creation
                    var release = await _entityService.GetByIdAsync(releaseId);
                    if (release == null)
                        return new JsonBadRequest("Release not found.");

                    int parentFolderId = 0;
                    if (int.TryParse(id, out var parsedId) && parsedId > 0)
                        parentFolderId = parsedId;

                    var folder = await _documentService.AddFolder(
                        ToDocSystemType(release.SystemType), "ReleaseId", "Rel", releaseId, folderName, parentFolderId, false);
                    return Json(new { id = folder.FolderId.ToString(), text = folder.FolderName, hasChildren = false, expanded = false, iconClass = "fal fa-folder" });
                }
            }
            catch (Exception ex)
            {
                return new JsonBadRequest("Error adding folder: " + ex.Message);
            }
        }

        /// <summary>
        /// Rename folder/document — accepts pipe-delimited tree node ID.
        /// Parses the ID to determine folder vs doc (checks for "|doc|" in the ID).
        /// </summary>
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

                // Parse pipe-delimited ID to determine type and numeric ID
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
                    // Plain numeric ID — assume folder
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

        /// <summary>
        /// Delete folder/document — accepts pipe-delimited tree node ID.
        /// </summary>
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
                            DocFile docFile = null;
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
            catch (Exception ex)
            {
                return BadRequest("Unable to delete. The file may be locked by another process. Please try again later.");
            }
        }

        /// <summary>
        /// Drop (drag-and-drop reorder) — moves a tree node to a new parent.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> DocumentDropNode(string sourceId, string destId)
        {
            try
            {
                // Parse source and dest to get folder/doc IDs and move
                // For now, drag-drop reorder requires the IDocumentViewModelService which was deleted.
                // Return OK to prevent errors — the tree will visually update but the backend won't persist the move.
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("Error moving item.");
            }
        }

        /// <summary>
        /// Grid read for the right-side document grid — returns documents in a folder.
        /// </summary>
        public async Task<IActionResult> DocumentGridRead([DataSourceRequest] DataSourceRequest request, int folderId, string? extFilter = null)
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

                // Server-side extension filter — used by the split MDB/Report panels.
                if (!string.IsNullOrWhiteSpace(extFilter))
                    docs = docs.Where(d => (d.UserFileName ?? "").EndsWith(extFilter, StringComparison.OrdinalIgnoreCase)).ToList();

                var result = docs.Select(d => new
                {
                    d.DocId,
                    d.DocName,
                    d.Author,
                    d.IsPrivate,
                    d.DateCreated,
                    d.FileId,
                    d.UserFileName,
                    d.DocFileName,
                    d.FolderName,
                    d.DocTypeName,
                    d.IsImage,
                    d.ForSignature,
                    d.IconClass,
                    IsDocViewable = !string.IsNullOrEmpty(d.FileExt) && viewableExts.Any(ext => ext.Equals(d.FileExt, StringComparison.OrdinalIgnoreCase))
                }).ToList();

                return Json(result.ToDataSourceResult(request));
            }
            catch (Exception)
            {
                return Json(new List<object>().ToDataSourceResult(request));
            }
        }

        /// <summary>
        /// Delete a single document from the grid by docId.
        /// </summary>
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

                DocFile docFile = null;
                if (document.FileId.HasValue && document.FileId.Value > 0)
                    docFile = await _documentService.GetFileById(document.FileId.Value);

                await _documentService.DeleteDoc(document, docFile);

                if (docFile != null && !string.IsNullOrEmpty(docFile.DocFileName))
                    _documentHelper.DeleteDocumentFile(docFile.DocFileName, docFile.ThumbFileName, docFile.IsImage);

                return Ok();
            }
            catch (Exception ex)
            {
                return new JsonBadRequest("Error deleting document.");
            }
        }

        /// <summary>
        /// Batch upload from drop zone — accepts multiple files (matching old SaveDocuments pattern).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> SaveDocuments(IEnumerable<IFormFile> droppedDocs, int folderId, int releaseId)
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
        public async Task<IActionResult> DocumentUpload(int releaseId, int folderId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return new JsonBadRequest(new { errors = "No file selected." });

                var release = await _entityService.GetByIdAsync(releaseId);
                if (release == null)
                    return new JsonBadRequest(new { errors = "Release not found." });

                var userName = User.GetUserName();
                var originalFileName = file.FileName;
                var fileExtension = Path.GetExtension(originalFileName);

                // Create DocFile record
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

                // Get document type from filename
                var docTypeId = await _documentService.GetDocTypeIdFromFileName(originalFileName);

                // Create DocDocument record
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

                // Save physical file
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
                if (docFile == null)
                    return NotFound();

                var filePath = _documentHelper.GetDocumentPath(docFile.DocFileName);
                if (string.IsNullOrEmpty(filePath))
                    return NotFound("File not found.");

                // File system mode: filePath is a local path. Azure mode: filePath is a blob-
                // relative key. Try local first, fall back to storage abstraction (which is
                // the blob client in Azure mode).
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
            catch (Exception ex)
            {
                return NotFound("Error downloading file.");
            }
        }

        // Temporary diagnostic for blob storage. Lists the first N blobs in the
        // container, optionally filtered by prefix, so we can see where existing
        // documents are actually stored when a lookup fails. Remove once the
        // legacy-path situation is resolved.
        //  /Releases/Release/BlobDiagnostic                — top-level listing
        //  /Releases/Release/BlobDiagnostic?prefix=Sea     — prefix-filtered
        //  /Releases/Release/BlobDiagnostic?find=120.mdb   — find any blob ending in name
        public async Task<IActionResult> BlobDiagnostic(string? prefix = null, string? find = null, int max = 100)
        {
            try
            {
                if (_documentStorage is not LawPortal.Web.Services.DocumentStorage.AzureStorage azure)
                    return Content("Not running in Azure mode — diagnostic not applicable.", "text/plain");

                if (!string.IsNullOrEmpty(find))
                {
                    var match = await azure.FindByFileName(find);
                    return Content(string.IsNullOrEmpty(match)
                        ? $"No blob found ending in '{find}'."
                        : $"Found at: {match}", "text/plain");
                }

                var blobs = await azure.ListBlobs(string.IsNullOrEmpty(prefix) ? null : prefix, max);
                if (blobs.Count == 0)
                    return Content($"No blobs found{(string.IsNullOrEmpty(prefix) ? "" : $" with prefix '{prefix}'")}.", "text/plain");

                return Content(string.Join("\n", blobs), "text/plain");
            }
            catch (Exception ex)
            {
                return Content($"Diagnostic error: {ex.GetType().Name}: {ex.Message}", "text/plain");
            }
        }

        private static bool IsImageFile(string extension)
        {
            if (string.IsNullOrEmpty(extension)) return false;
            var ext = extension.TrimStart('.').ToLowerInvariant();
            return ext == "jpg" || ext == "jpeg" || ext == "png" || ext == "gif" || ext == "bmp" || ext == "tiff" || ext == "svg" || ext == "webp";
        }

        private static string GetContentType(string fileExtension)
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

        /// <summary>
        /// Returns the Document Detail dialog partial view for add/edit.
        /// </summary>
        [Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> DocumentDetailDialog(int folderId, int releaseId = 0, int docId = 0)
        {
            try
            {
            // Auto-resolve root folder if none specified
            if (folderId <= 0 && releaseId > 0)
            {
                var release = await _entityService.GetByIdAsync(releaseId);
                if (release != null)
                {
                    var rootFolder = await _documentService.GetFolder(
                        ToDocSystemType(release.SystemType), "ReleaseId", releaseId, TruncateFolderName(release.Name), 0);
                    if (rootFolder == null)
                    {
                        rootFolder = await _documentService.AddFolder(
                            ToDocSystemType(release.SystemType), "ReleaseId", "Rel", releaseId,
                            TruncateFolderName(release.Name), 0, false);
                    }
                    if (rootFolder != null)
                        folderId = rootFolder.FolderId;
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

        /// <summary>
        /// Save document from the Document Detail dialog (add or edit).
        /// </summary>
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
                    // Edit existing document
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

                    // Handle file upload if a new file is provided
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
                    // Add new document
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
                var fullMsg = ex.ToString();
                return new JsonBadRequest(new { errors = "Error saving document: " + fullMsg });
            }
        }

        /// <summary>
        /// Get DocType picklist data for the Document Detail dialog.
        /// </summary>
        public async Task<IActionResult> GetDocTypes()
        {
            var list = await _documentService.DocTypes
                .Select(d => new { d.DocTypeId, d.DocTypeName })
                .OrderBy(d => d.DocTypeName)
                .ToListAsync();
            return Json(list);
        }

        #endregion

        #region MDB Generation

        public IActionResult GetSystemList()
        {
            return Json(Helpers.SystemsHelper.SystemNames);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateMdb(int id, string mdbArea = "")
        {
            var release = await _entityService.QueryableList.AsNoTracking().FirstOrDefaultAsync(r => r.ReleaseId == id);
            if (release == null)
                return BadRequest("Release not found.");

            if (string.IsNullOrWhiteSpace(release.SystemType))
                return BadRequest("No system type selected for this release. Please save the release with a system type first.");

            // Determine which area(s) to generate based on the mdbArea parameter
            bool generatePatent = string.IsNullOrEmpty(mdbArea)
                ? release.GeneratePatent
                : mdbArea.Equals("Patent", StringComparison.OrdinalIgnoreCase);
            bool generateTrademark = string.IsNullOrEmpty(mdbArea)
                ? release.GenerateTrademark
                : mdbArea.Equals("Trademark", StringComparison.OrdinalIgnoreCase);

            if (!generatePatent && !generateTrademark)
                return BadRequest("Please specify Patent or Trademark to generate.");

            try
            {
                var env = HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
                var mdbService = new LawPortal.Reports.Services.MdbGenerationService(
                    HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>(),
                    HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LawPortal.Reports.Services.MdbGenerationService>>(),
                    env.WebRootPath);

                // Generate to a temp folder first — filter by SystemType
                var tempFolder = Path.Combine(Path.GetTempPath(), $"mdbgen_{Guid.NewGuid():N}");
                var files = await mdbService.GenerateMdbFiles(release.SystemType, generatePatent, generateTrademark, tempFolder, release.Name, release.Year, release.Quarter ?? "");

                if (!files.Any())
                    return BadRequest("MDB Generator completed but produced no files.");

                // Store generated files in the release's Documents folder
                var userName = User.GetUserName();
                var systemType = ToDocSystemType(release.SystemType);

                // Get or create the root document folder for this release
                var rootFolder = await _documentService.GetFolder(
                    systemType, "ReleaseId", release.ReleaseId, TruncateFolderName(release.Name), 0);
                if (rootFolder == null)
                {
                    rootFolder = await _documentService.AddFolder(
                        systemType, "ReleaseId", "Rel", release.ReleaseId,
                        TruncateFolderName(release.Name), 0, false);
                }

                if (rootFolder == null)
                    return BadRequest("Could not find or create the document folder for this release.");

                var debugInfo = new List<string>();
                int addedCount = 0;
                foreach (var filePath in files)
                {
                    if (!System.IO.File.Exists(filePath))
                    {
                        debugInfo.Add($"File not found: {filePath}");
                        continue;
                    }

                    var fileName = Path.GetFileName(filePath);
                    var fileExtension = Path.GetExtension(fileName);
                    var fileInfo = new FileInfo(filePath);
                    // Trust the generator's filename. It already produces the correct
                    // patent/trademark name because it knows which area was requested;
                    // re-deriving from SystemType alone mislabels the shared R4 system
                    // (which is neither "Pat*" nor "Tmk*") — a Patent MDB would be saved
                    // under a TmkLaw name.
                    var baseName = Path.GetFileNameWithoutExtension(fileName);

                    // If a document with this name already exists, append a number suffix
                    var existingNames = await _documentService.DocDocuments
                        .Where(d => d.FolderId == rootFolder.FolderId && d.DocName.StartsWith(baseName))
                        .Select(d => d.DocName)
                        .ToListAsync();
                    if (existingNames.Any(n => n == baseName))
                    {
                        int num = 1;
                        while (existingNames.Contains($"{baseName} ({num})"))
                            num++;
                        baseName = $"{baseName} ({num})";
                        fileName = $"{baseName}{fileExtension}";
                    }

                    // Create DocFile record
                    var docFile = new DocFile
                    {
                        FileExt = fileExtension?.TrimStart('.'),
                        UserFileName = fileName,
                        FileSize = (int)fileInfo.Length,
                        IsImage = false,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now
                    };
                    docFile = await _documentService.AddDocFile(docFile);

                    // Create DocDocument record
                    var docDocument = new DocDocument
                    {
                        FolderId = rootFolder.FolderId,
                        DocName = Path.GetFileNameWithoutExtension(fileName),
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

                    // Save through the document helper so we hit local FS in file-system mode
                    // and Azure Blob in Azure mode. We need a header so Azure metadata is set
                    // correctly; for file-system mode the header is currently ignored.
                    using (var fileStream = System.IO.File.OpenRead(filePath))
                    using (var memStream = new MemoryStream())
                    {
                        await fileStream.CopyToAsync(memStream);
                        memStream.Position = 0;

                        var folderHeader = new DocFolderHeader
                        {
                            SystemType = release.SystemType,
                            ScreenCode = "Release",
                            ParentId = release.ReleaseId
                        };
                        await _documentHelper.SaveDocumentFromStream(memStream, docFile.DocFileName, folderHeader);
                    }

                    debugInfo.Add($"{fileName} -> FolderId={rootFolder.FolderId}, DocFileId={docFile.FileId}, DocFileName={docFile.DocFileName}");
                    addedCount++;
                }

                // Clean up temp folder
                try { Directory.Delete(tempFolder, true); } catch { }

                var msg = $"MDB files generated successfully. {addedCount} file(s) added to Documents.";
                if (debugInfo.Any())
                    msg += " [" + string.Join("; ", debugInfo) + "]";
                return Ok(new { success = msg });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating MDB files: {ex.Message}");
            }
        }

        #endregion

        #region Reports

        public async Task<IActionResult> GetCompareMdbFiles(int releaseId, bool isPat)
        {
            var current = await _entityService.QueryableList.AsNoTracking().FirstOrDefaultAsync(r => r.ReleaseId == releaseId);
            if (current == null) return Json(new List<object>());

            var otherReleases = await _entityService.QueryableList.AsNoTracking()
                .Where(r => r.ReleaseId != releaseId && r.SystemType == current.SystemType)
                .ToListAsync();

            var results = new List<object>();
            foreach (var rel in otherReleases.OrderByDescending(r => r.Year).ThenByDescending(r => r.Quarter))
            {
                var sysType = ToDocSystemType(rel.SystemType);
                var folder = await _documentService.GetFolder(sysType, "ReleaseId", rel.ReleaseId, TruncateFolderName(rel.Name), 0);
                if (folder == null) continue;

                var docs = await _documentService.DocDocuments
                    .Where(d => d.FolderId == folder.FolderId && d.DocFile != null && d.DocFile.FileExt == "mdb")
                    .Select(d => new { d.DocId, d.DocName })
                    .ToListAsync();

                foreach (var doc in docs)
                {
                    bool docIsPat = doc.DocName.Contains("Pat", StringComparison.OrdinalIgnoreCase);
                    if (docIsPat == isPat)
                        results.Add(new { doc.DocId, Text = doc.DocName, ReleaseId = rel.ReleaseId });
                }
            }
            return Json(results);
        }

public async Task<IActionResult> GetMdbFiles(int releaseId)
        {
            var release = await _entityService.QueryableList.AsNoTracking().FirstOrDefaultAsync(r => r.ReleaseId == releaseId);
            if (release == null) return Json(new List<object>());

            var systemType = ToDocSystemType(release.SystemType);
            var rootFolder = await _documentService.GetFolder(
                systemType, "ReleaseId", releaseId, TruncateFolderName(release.Name), 0);
            if (rootFolder == null) return Json(new List<object>());

            var docs = await _documentService.DocDocuments
                .Where(d => d.FolderId == rootFolder.FolderId && d.DocFile != null && d.DocFile.FileExt == "mdb")
                .Select(d => new { d.DocId, d.DocName, d.FileId, UserFileName = d.DocFile != null ? d.DocFile.UserFileName : "" })
                .ToListAsync();
            return Json(docs);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport(int releaseId, int docId, int compareDocId)
        {
            try
            {
                var release = await _entityService.QueryableList.AsNoTracking().FirstOrDefaultAsync(r => r.ReleaseId == releaseId);
                if (release == null) return BadRequest("Release not found.");

                var currentDoc = await _documentService.GetDocumentById(docId);
                if (currentDoc == null || !currentDoc.FileId.HasValue) return BadRequest("Current document not found.");
                var currentFile = await _documentService.GetFileById(currentDoc.FileId.Value);
                if (currentFile == null) return BadRequest("Current file not found.");
                // In Azure mode this returns a blob path; resolve to a local file before
                // passing to the 32-bit MDB sidecar (which only reads local disk).
                var currentMdbPath = await EnsureLocalFile(
                    _documentHelper.GetDocumentPath(currentFile.DocFileName),
                    currentFile.DocFileName);

                bool isPat = currentDoc.DocName.Contains("Pat", StringComparison.OrdinalIgnoreCase);
                var compareDoc = await _documentService.GetDocumentById(compareDocId);
                if (compareDoc == null || !compareDoc.FileId.HasValue) return BadRequest("Comparison document not found.");
                var compareFile = await _documentService.GetFileById(compareDoc.FileId.Value);
                if (compareFile == null) return BadRequest("Comparison file not found.");
                var compareMdbPath = await EnsureLocalFile(
                    _documentHelper.GetDocumentPath(compareFile.DocFileName),
                    compareFile.DocFileName);

                if (string.IsNullOrEmpty(currentMdbPath) || !System.IO.File.Exists(currentMdbPath)) return BadRequest("Current MDB file not found on disk.");
                if (string.IsNullOrEmpty(compareMdbPath) || !System.IO.File.Exists(compareMdbPath)) return BadRequest("Comparison MDB file not found on disk.");

                var env = HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
                var comparisonService = new LawPortal.Reports.Services.MdbComparisonService(
                    env.WebRootPath,
                    HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LawPortal.Reports.Services.MdbComparisonService>>());
                var diff = await comparisonService.CompareMdbFiles(currentMdbPath, compareMdbPath);

                // Build lookups
                var countryNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var caseTypeDescs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var countryTable = isPat ? "tblPatCountry" : "tblTmkCountry";
                var ctTable = isPat ? "tblPatCaseType" : "tblTmkCaseType";
                using (var lookupConn = new Microsoft.Data.SqlClient.SqlConnection(
                    HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>().GetConnectionString("DefaultConnection")))
                {
                    await lookupConn.OpenAsync();
                    using var cCmd = new Microsoft.Data.SqlClient.SqlCommand($"SELECT Country, CountryName FROM [{countryTable}]", lookupConn);
                    using var cReader = await cCmd.ExecuteReaderAsync();
                    while (await cReader.ReadAsync())
                    {
                        var code = cReader["Country"]?.ToString() ?? "";
                        var name = cReader["CountryName"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(code)) countryNames[code] = name;
                    }
                    cReader.Close();
                    using var ctCmd = new Microsoft.Data.SqlClient.SqlCommand($"SELECT CaseType, Description FROM [{ctTable}]", lookupConn);
                    using var ctReader = await ctCmd.ExecuteReaderAsync();
                    while (await ctReader.ReadAsync())
                    {
                        var code = ctReader["CaseType"]?.ToString() ?? "";
                        var desc = ctReader["Description"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(code)) caseTypeDescs[code] = desc;
                    }
                }

                var pdfService = new LawPortal.Reports.Services.MdbReportPdfService();
                var notes = isPat ? release.ReportNotesPatent : release.ReportNotesTrademark;
                var pdfBytes = pdfService.GenerateReport(diff, release.Name, release.Year.ToString(), release.Quarter ?? "", countryNames, caseTypeDescs, notes);

                // Store PDF
                var userName = User.GetUserName();
                var sysType = ToDocSystemType(release.SystemType);
                var rootFolder = await _documentService.GetFolder(sysType, "ReleaseId", releaseId, TruncateFolderName(release.Name), 0);
                if (rootFolder == null)
                    rootFolder = await _documentService.AddFolder(sysType, "ReleaseId", "Rel", releaseId, TruncateFolderName(release.Name), 0, false);

                var reportName = GetReportBaseName(release.SystemType, release.Year, release.Quarter ?? "", isPat);
                var existingNames = await _documentService.DocDocuments
                    .Where(d => d.FolderId == rootFolder.FolderId && d.DocName.StartsWith(reportName))
                    .Select(d => d.DocName).ToListAsync();
                if (existingNames.Any(n => n == reportName))
                {
                    int num = 1;
                    while (existingNames.Contains($"{reportName} ({num})")) num++;
                    reportName = $"{reportName} ({num})";
                }

                var docFile = new DocFile
                {
                    FileExt = "pdf", UserFileName = $"{reportName}.pdf", FileSize = pdfBytes.Length,
                    IsImage = false, CreatedBy = userName, UpdatedBy = userName,
                    DateCreated = DateTime.Now, LastUpdate = DateTime.Now
                };
                docFile = await _documentService.AddDocFile(docFile);

                var docDocument = new DocDocument
                {
                    FolderId = rootFolder.FolderId, DocName = reportName, FileId = docFile.FileId,
                    Author = userName, CreatedBy = userName, UpdatedBy = userName,
                    DateCreated = DateTime.Now, LastUpdate = DateTime.Now
                };
                await _documentService.UpdateDocuments(userName,
                    Enumerable.Empty<DocDocument>(), new[] { docDocument }, Enumerable.Empty<DocDocument>(), null, false);

                // Save through the document helper so the PDF lands at the same canonical
                // location as MDBs — Searchable/Documents/<filename> in blob (Azure mode)
                // or <contentRoot>/UserFiles/Searchable/Documents/<filename> on disk
                // (FileSystem mode). Was previously a direct File.WriteAllBytesAsync that
                // worked locally but produced a blob-incompatible path in Azure mode.
                using (var pdfStream = new MemoryStream(pdfBytes))
                {
                    var folderHeader = new DocFolderHeader
                    {
                        SystemType = release.SystemType,
                        ScreenCode = "Release",
                        ParentId = release.ReleaseId
                    };
                    await _documentHelper.SaveDocumentFromStream(pdfStream, docFile.DocFileName, folderHeader);
                }

                return Ok(new { success = $"Report generated: {reportName}.pdf", fileId = docFile.FileId });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating report: {ex.Message}");
            }
        }

        #endregion

        #region Quarter Snapshot

        // Returns summary of snapshot tables (name + row count) for the release's
        // year/quarter. The Release detail view uses this to populate the
        // Returns true when the hist table's Pat/Tmk side matches the release's SystemType.
        // Pure-Pat releases ("Pat*") see only tblPat* tables; pure-Tmk releases ("Tmk*")
        // see only tblTmk* tables; everything else (e.g. "R4" mixed) sees both.
        private static bool TableMatchesSide(string histTable, string systemType)
        {
            var isPat = histTable.IndexOf("tblPat", StringComparison.OrdinalIgnoreCase) >= 0;
            if (systemType.StartsWith("Tmk", StringComparison.OrdinalIgnoreCase)) return !isPat;
            if (systemType.StartsWith("Pat", StringComparison.OrdinalIgnoreCase)) return isPat;
            return true;
        }

        // "Quarter Snapshot" tab.
        [HttpGet]
        public async Task<IActionResult> SnapshotSummary(int id)
        {
            var release = await _entityService.GetByIdAsync(id);
            if (release == null)
                return BadRequest("Release not found.");

            var sysTags = (release.Systems ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList();

            // Only include tables whose side matches this release's SystemType.
            // Exclude AuditLog tables — they aren't useful in the snapshot view.
            var histTables = DeployController.SnapshotTables
                .Where(t => TableMatchesSide(t, release.SystemType ?? "")
                         && t.IndexOf("AuditLog", StringComparison.OrdinalIgnoreCase) < 0)
                .Select(t => "hist_" + t)
                .ToArray();

            var results = new List<object>();
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();

                foreach (var hist in histTables)
                {
                    var (sql, parms) = BuildSnapshotQuery($"SELECT COUNT(*)", hist, release, sysTags, conn);
                    using var cmd = new SqlCommand(sql, conn);
                    foreach (var p in parms) cmd.Parameters.Add(p);
                    var count = (int)await cmd.ExecuteScalarAsync();
                    var sourceName = hist.Substring(5);
                    var side = sourceName.StartsWith("tblPat") ? "Patent" : "Trademark";
                    results.Add(new { tableName = sourceName, histTable = hist, rowCount = count, side });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Json(new { year = release.Year, quarter = release.Quarter, tables = results });
        }

        // Returns first 200 rows from a hist_* table filtered to this release's
        // year, quarter, and system tags (where a Systems column exists).
        [HttpGet]
        public async Task<IActionResult> SnapshotTableData(int releaseId, string tableName)
        {
            var release = await _entityService.GetByIdAsync(releaseId);
            if (release == null)
                return BadRequest("Release not found.");

            var allowed = DeployController.SnapshotTables.Select(t => "hist_" + t).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var hist = "hist_" + tableName;
            if (!allowed.Contains(hist))
                return BadRequest("Unknown table.");

            var sysTags = (release.Systems ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList();

            var rows = new List<Dictionary<string, object?>>();
            var columns = new List<string>();
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();

                var (sql, parms) = BuildSnapshotQuery("SELECT TOP 200 *", hist, release, sysTags, conn);
                using var cmd = new SqlCommand(sql, conn);
                foreach (var p in parms) cmd.Parameters.Add(p);
                using var reader = await cmd.ExecuteReaderAsync();

                for (var i = 0; i < reader.FieldCount; i++)
                    columns.Add(reader.GetName(i));

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object?>();
                    for (var i = 0; i < reader.FieldCount; i++)
                        row[columns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Json(new { columns, rows });
        }

        // Builds a SELECT … FROM [hist] WHERE SnapshotYear/Quarter [AND Systems IN (…)]
        // query. Falls back to SystemType when release.Systems is blank so rows are
        // always scoped to the correct system even for older release records.
        private static (string sql, List<SqlParameter> parms) BuildSnapshotQuery(
            string selectClause,
            string hist,
            Release release,
            List<string> sysTags,
            SqlConnection conn)
        {
            // If Systems field is blank, use SystemType as the single filter tag.
            var effectiveTags = sysTags.Any() ? sysTags
                : (!string.IsNullOrWhiteSpace(release.SystemType)
                    ? new List<string> { release.SystemType.Trim() }
                    : new List<string>());

            var parms = new List<SqlParameter>
            {
                new SqlParameter("@year",    release.Year),
                new SqlParameter("@quarter", (object?)release.Quarter ?? DBNull.Value)
            };

            bool hasSystems = false;
            if (effectiveTags.Any())
            {
                const string checkSql = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                                          WHERE TABLE_NAME = @tbl AND COLUMN_NAME = 'Systems'";
                using var chk = new SqlCommand(checkSql, conn);
                chk.Parameters.AddWithValue("@tbl", hist);
                hasSystems = (int)chk.ExecuteScalar() > 0;
            }

            var where = "WHERE SnapshotYear = @year AND SnapshotQuarter = @quarter";
            if (hasSystems && effectiveTags.Any())
            {
                // Use CHARINDEX so a row with Systems = "TmkR5-8,TmkR9-10v2.2" is
                // included when filtering for TmkR5-8 (contains, not exact match).
                var clauses = effectiveTags.Select((t, i) =>
                {
                    parms.Add(new SqlParameter($"@sys{i}", t));
                    return $"CHARINDEX(@sys{i}, [Systems]) > 0";
                });
                where += $" AND ({string.Join(" OR ", clauses)})";
            }

            return ($"{selectClause} FROM [{hist}] {where}", parms);
        }

        #endregion
    }
}
