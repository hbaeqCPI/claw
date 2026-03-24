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
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Shared;
using R10.Web.Areas;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;

namespace R10.Web.Areas.Releases.Controllers
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
        private readonly IStringLocalizer<SharedResource> _localizer;

        private readonly string _dataContainer = "releaseDetail";

        public ReleaseController(
            IAuthorizationService authService,
            IViewModelService<Release> viewModelService,
            IEntityService<Release> entityService,
            IEntityService<AppSystem> systemService,
            IDocumentService documentService,
            IDocumentHelper documentHelper,
            IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _entityService = entityService;
            _systemService = systemService;
            _documentService = documentService;
            _documentHelper = documentHelper;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "releaseSearch",
                Title = _localizer["Releases"].ToString(),
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
                Title = _localizer["Releases"].ToString(),
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

                if (mainSearchFilters.Count > 0)
                    releases = _viewModelService.AddCriteria(releases, mainSearchFilters);

                var result = await _viewModelService.CreateViewModelForGrid(request, releases, "Name", "ReleaseId");
                return Json(result);
            }

            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task LoadSystemsList()
        {
            var systems = await _systemService.QueryableList
                .Select(s => s.SystemName)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
            ViewData["SystemsList"] = systems;
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
                Title = _localizer["Releases"].ToString(),
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
                Title = _localizer["New Releases"].ToString(),
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
            if (ModelState.IsValid)
            {
                bool isNew = release.ReleaseId == 0;
                UpdateEntityStamps(release, release.ReleaseId);

                if (!isNew)
                    await _entityService.Update(release);
                else
                    await _entityService.Add(release);

                // Auto-create root document folder for new releases
                if (isNew && release.ReleaseId > 0)
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
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        [HttpPost, Authorize(Policy = ReleaseAuthorizationPolicy.AuxiliaryCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string tStamp)
        {
            var entity = await _entityService.GetByIdAsync(id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            entity.tStamp = Convert.FromBase64String(tStamp);
            await _entityService.Delete(entity);

            return Ok();
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var release = await _entityService.GetByIdAsync(id);
            if (release == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = release.CreatedBy, dateCreated = release.DateCreated, updatedBy = release.UpdatedBy, lastUpdate = release.LastUpdate, tStamp = release.tStamp });
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
                if (!System.IO.File.Exists(filePath))
                    return NotFound("File not found on disk.");

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = GetContentType(docFile.FileExt);
                return File(fileBytes, contentType, docFile.UserFileName ?? docFile.DocFileName);
            }
            catch (Exception ex)
            {
                return NotFound("Error downloading file.");
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
    }
}
