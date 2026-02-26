using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenIddict.Validation.AspNetCore;
using R10.Core.Identity;
using R10.Web.Areas.Shared.Controllers;
using R10.Web.Filters;
using R10.Web.Services.MailDownload;
using R10.Web.Services;
using R10.Web.Services.NetDocuments;
using R10.Web.Interfaces;
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using Microsoft.Extensions.Localization;
using R10.Web.Models;
using System.Text;
using System.Text.Json;
using R10.Core.Helpers;
using R10.Web.Models.NetDocumentsModels;
using R10.Web.ViewComponents;
using Kendo.Mvc.UI;
using R10.Web.Areas.Shared.ViewModels;
using Kendo.Mvc.Extensions;
using GleamTech.DocumentUltimate.AspNet.UI;
using GleamTech.FileProviders;
using R10.Core.Entities.Documents;
using Microsoft.EntityFrameworkCore;
using R10.Web.Helpers;
using AutoMapper;
// using R10.Core.Interfaces.RMS; // Removed during deep clean
// using R10.Core.Interfaces.ForeignFiling; // Removed during deep clean
using R10.Web.Security;

namespace R10.Web.Controllers
{
    [Authorize(AuthenticationSchemes = $"Identity.Application,{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme}")]
    [NetDocumentsAuthorizationFilter()]
    [ServiceFilter(typeof(ExceptionFilter))]
    public class NetDocsController : DocumentUploadController
    {
        private readonly INetDocumentsClientFactory _netDocumentsClientFactory;
        private readonly INetDocumentsAuthProvider _authProvider;
        private readonly NetDocumentsSettings _netDocumentsSettings;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IDocumentService _docService;
        private readonly INetDocumentsViewModelService _netDocsViewModelService;
        private readonly IMapper _mapper;

        public NetDocsController(
            INetDocumentsClientFactory netDocumentsClientFactory,
            INetDocumentsAuthProvider authProvider,
            IOptions<NetDocumentsSettings> netDocumentsSettings,
            IStringLocalizer<SharedResource> localizer,
            IMailDownloadService mailDownloadService,
            IDocumentsViewModelService docViewModelService,
            IOptions<GraphSettings> graphSettings,
            IOptions<ServiceAccount> serviceAccount,
            ILogger<DocDocumentsController> logger,
            IAuthorizationService authService,
            ISystemSettings<DefaultSetting> settings,
            ISystemSettings<PatSetting> patSettings,
            ISystemSettings<TmkSetting> tmkSettings,
            IEPOService epoService,
            IEntityService<EPOCommunication> epoCommunicationService,
            IDocumentService docService,
            INetDocumentsViewModelService netDocsViewModelService,
            IMapper mapper
        ) : base(mailDownloadService, docViewModelService, graphSettings, serviceAccount, logger, authService, settings, patSettings, tmkSettings, epoService, epoCommunicationService)
        {
            _netDocumentsClientFactory = netDocumentsClientFactory;
            _authProvider = authProvider;
            _netDocumentsSettings = netDocumentsSettings.Value;
            _localizer = localizer;
            _docService = docService;
            _netDocsViewModelService = netDocsViewModelService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Setup(string documentLink, string? token)
        {
            var permission = GetViewerPermission(token);
            if (!permission.CanSetupFolder)
                return Forbid();

            if (string.IsNullOrEmpty(documentLink))
                return BadRequest();            
            
            var model = new SetupViewModel() { Permission = permission };
            var folder = await _docViewModelService.GetFolderByDocumentLink(documentLink);
            if (folder != null)
            {
                model.DocDefaultFolderId = folder.StorageDefaultFolderId;

                if (!string.IsNullOrEmpty(folder.StorageRootContainerId))
                {
                    using (var client = await _netDocumentsClientFactory.GetClient())
                    {
                        var container = await client.GetContainer(folder.StorageRootContainerId);
                        if (container == null)
                        {
                            //container is deleted or user has no permission
                            //todo: get user's permission to the container ??? (no api endoint for this)
                            //allow admin to edit
                            if (User.IsAdmin())
                                model.DocRootContainerId = folder.StorageRootContainerId;
                            else
                                return Forbid();
                        }
                        else
                        {
                            model.DocRootContainerId = container.Id;
                            model.DocRootContainerName = container.Name;
                        }

                    }
                }
            }
            ViewData["CanCreateWorkspace"] = _netDocumentsSettings.WorkspaceCreation == WorkspaceCreation.Manual;
            return PartialView("_Setup", model);
            
        }

        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Setup")]
        public async Task<IActionResult> SaveSetUp(string documentLink, string? rootContainerId, string? defaultFolderId, string? token)
        {
            if (!(GetViewerPermission(token).CanSetupFolder))
                return Forbid();

            if (string.IsNullOrEmpty(documentLink))
                return BadRequest();

            await _netDocsViewModelService.SaveFolderStorageSetting(documentLink, rootContainerId, defaultFolderId);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> NewWorkspace(string? documentLink, string? token)
        {
            var permission = GetViewerPermission(token);
            if (!permission.CanSetupFolder)
                return Forbid();

            if (string.IsNullOrEmpty(documentLink))
                return BadRequest();

            var setting = await _settings.GetSetting();
            var clientMatter = await _docViewModelService.GetClientMatter(documentLink);
            if (_netDocumentsSettings.IsClientMatter && string.IsNullOrEmpty(clientMatter.ClientCode))
                return BadRequest(_localizer[$"{setting.LabelClient} code is required."].Value);

            //netdocs workspace name is auto generated based on client/matter profile attributes
            //var rootDocumentLink = await _docViewModelService.GetRootDocumentLink(documentLink);
            //var name = await _docViewModelService.GenerateFolderName(rootDocumentLink);
            var name = _netDocumentsSettings.IsClientMatter ? $"{clientMatter.ClientCode}-{clientMatter.MatterNumber}" : clientMatter.MatterNumber;

            return PartialView("_NewWorkspace", name);
        }

        [HttpPost]
        public async Task<IActionResult> NewWorkspace(string? documentLink, string? token, string? workspaceName)
        {
            var permission = GetViewerPermission(token);
            if (!permission.CanSetupFolder)
                return Forbid();

            if (string.IsNullOrEmpty(documentLink) || string.IsNullOrEmpty(workspaceName))
                return BadRequest();

            var model = new SetupViewModel() { DocRootContainerName = workspaceName, Permission = permission };
            using (var client = await _netDocumentsClientFactory.GetClient())
            {
                var docFolder = await _netDocsViewModelService.CreateWorkspace(client, documentLink);
                if (docFolder != null && docFolder.FolderId > 0)
                {
                    model.DocRootContainerId = docFolder.StorageRootContainerId;
                    model.DocDefaultFolderId = docFolder.StorageDefaultFolderId;

                    return Json(model);
                }
            }

            return BadRequest(_localizer["Unable to create workspace."].Value);
        }

        public async Task<IActionResult> GetContainer(string? containerId)
        {
            using (var client = await _netDocumentsClientFactory.GetClient())
            {
                var container = await client.GetContainer(containerId);
                //return Ok(container);
                //match js casing
                return Ok(new { id = container.Id, name = container.Name });
            }
        }

        public async Task<IActionResult> GetFolderList(string containerId)
        {
            using (var client = await _netDocumentsClientFactory.GetClient())
            {
                return Ok(await client.GetFolderList(containerId));
            }
        }

        public async Task<IActionResult> GetFolderTree(string? containerId, string pageId, string documentLink, string? token)
        {
            if (string.IsNullOrEmpty(documentLink))
                return BadRequest();

            using (var client = await _netDocumentsClientFactory.GetClient())
            {
                var docFolder = await _docViewModelService.GetDefaultFolderByDocumentLink(documentLink);
                var rootContainerId = "";

                if ((docFolder == null || string.IsNullOrEmpty(docFolder.StorageRootContainerId)) && _netDocumentsSettings.WorkspaceCreation == WorkspaceCreation.Auto)
                {
                    docFolder = await _netDocsViewModelService.CreateWorkspace(client, documentLink);
                }

                if (docFolder != null)
                {
                    rootContainerId = docFolder.StorageRootContainerId;
                    if (string.IsNullOrEmpty(containerId))
                        containerId = docFolder.StorageDefaultFolderId;
                }

                var container = await client.GetContainer(rootContainerId);
                var folders = await client.GetFolderTree(container);
                var permission = GetViewerPermission(token);

                return PartialView("_FolderTree", new FolderTreeViewModel()
                {
                    PageId = pageId,
                    ContainerId = containerId,
                    RootContainer = container,
                    Folders = container.ContainerType == ContainerType.Workspace || folders.Count == 0 ? folders : folders[0]?.SubFolders,
                    Permission = permission
                });
            }
        }

        public IActionResult GetViewer(DocumentDisplayOption displayOption, string pageId, string? token)
        {
            var model = new DocumentViewerViewModel()
            {
                PageId = pageId,
                Permission = GetViewerPermission(token)
            };

            if (displayOption == DocumentDisplayOption.GalleryView)
                return PartialView("_DocumentGallery", model);

            return PartialView("_DocumentGrid", model);
        }

        public async Task<IActionResult> ViewerRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> criteria)
        {
            var model = new List<DocumentViewModel>();
            var containerId = criteria?.GetContainerId();

            if (!string.IsNullOrEmpty(containerId))
            {
                using (var client = await _netDocumentsClientFactory.GetClient())
                {
                    var documents = await client.GetDocuments(containerId, criteria);

                    //get workspace id from document
                    var workspaceId = documents.FirstOrDefault(d => d.Ancestors?.Any(a => a.Type == ContainerType.Workspace.ToString()) ?? false)?.Ancestors?.FirstOrDefault(a => a.Type == ContainerType.Workspace.ToString())?.Id;
                    
                    //get workspace id from container
                    if (string.IsNullOrEmpty(workspaceId))
                    {
                        var container = await client.GetContainer(containerId);
                        workspaceId = container.ContainerType == ContainerType.Workspace ? container.Id : container?.Ancestors?.FirstOrDefault(a => a.Type == ContainerType.Workspace.ToString())?.Id;
                    }

                    //get folders for displaying folder names
                    var folders = await client.GetFolderList(workspaceId);

                    model = documents.Select(d => new DocumentViewModel()
                    {
                        Id = d.Id,
                        Title = d.GetFileName(), // GleamTech doc viewer needs filename with extension
                        Name = d.Attributes?.Name,
                        Author = d.Attributes?.CreatedBy,
                        Version = d.DocNum,
                        Size = d.Attributes?.Size ?? 0,
                        ContainerId = d.Ancestors?.FirstOrDefault()?.Id,
                        ContainerName = folders.FirstOrDefault(f => f.Id == d.Ancestors?.FirstOrDefault()?.Id)?.Name,
                        IconClass = d.GetIcon(),
                        Extension = d.Attributes?.Ext,
                        LastUser = d.Attributes?.ModifiedBy,
                        EditDate = d.Attributes?.Modified,
                        CreateDate = d.Attributes?.Created,
                        IsImage = d.IsImage(),
                        ImageUrl = Url.Action("Download", "NetDocs", new { area = "", id = d.Id }),
                        WorkUrl = Url.Action("Open", "NetDocs", new { area = "", id = d.Id })
                    }).ToList();
                }
            }

            return Json(model.ToDataSourceResult(request));
        }

        public async Task<IActionResult> GetDocumentNames(string containerId)
        {
            using (var client = await _netDocumentsClientFactory.GetClient())
            {
                var documents = await client.GetDocuments(containerId, true);
                return Ok(documents.Select(d => new { Name = d.Attributes?.Name }).Distinct().OrderBy(d => d.Name).ToList());
            }
        }

        public IActionResult GetTypes([DataSourceRequest] DataSourceRequest request)
        {
            return Ok(NetDocumentsService.DocumentIcons.Select(i => new { Value = i.Key.ToLower(), Text = i.Key }).OrderBy(i => i.Text).ToList());
        }

        /// <summary>
        /// GleamTech document viewer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fileName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet()]
        public async Task<IActionResult> DocumentViewer(string id, string fileName, string token)
        {
            if (!(GetViewerPermission(token).CanDownload))
                return Forbid();

            var client = await _netDocumentsClientFactory.GetClient();
            var stream = await client.GetDocumentAsStream(id);
            if (stream == null || stream.Length == 0)
                return BadRequest(_localizer["Unable to retrieve document."].Value);

            var settings = await _patSettings.GetSetting();
            var documentViewer = new DocumentViewer
            {
                Width = settings.DocViewerWidth,
                Height = settings.DocViewerHeight,
                Resizable = true,
                Document = new StreamFileProvider(fileName, stream),
                DisplayLanguage = "en"
            };

            // log trade secret download
            if (settings.IsTradeSecretOn)
                await _docService.LogDocTradeSecretActivityByDriveItemId(id);

            return PartialView("/Areas/Shared/Views/Documents/_DocumentViewer.cshtml", documentViewer);
        }

        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> SaveDroppedDocVerification(IEnumerable<IFormFile> droppedFiles)
        {
            if (droppedFiles.Count() <= 0)
            {
                return BadRequest(_localizer["No Document to upload"].ToString());
            }

            var docFolder = await _docService.DocFolders.Where(d => string.IsNullOrEmpty(d.SystemType) && string.IsNullOrEmpty(d.ScreenCode) && string.IsNullOrEmpty(d.DataKey) && d.DataKeyValue == 0).FirstOrDefaultAsync();

            if (docFolder == null)
                docFolder = await _docService.AddFolder("", "", "", 0, "Documents", 0, false);

            await SaveDroppedFiles(droppedFiles, "|||0", docFolder.StorageDefaultFolderId ?? "", null, false, null, DocumentSourceType.Manual);

            return Ok();
        }

        protected override async Task<IActionResult> SaveDroppedEmails(IEnumerable<IFormFile> droppedFiles, string documentLink, string folderId, string? roleLink, List<string>? responsibles)
        {
            return await SaveDroppedFiles(droppedFiles, documentLink, folderId, roleLink, false, responsibles, DocumentSourceType.CPIMail);
        }

        private DocumentViewerPermission GetViewerPermission(string? token)
        {
            try
            {
                return JsonSerializer.Deserialize<DocumentViewerPermission>((token ?? "").Decrypt(User.GetEncryptionKey())) ?? new DocumentViewerPermission();
            }
            catch
            {
                return new DocumentViewerPermission();
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDropped(IEnumerable<IFormFile> droppedFiles, string documentLink, string folderId, string? roleLink, string? token)
        {
            var permission = GetViewerPermission(token);
            if (!permission.CanUpload)
                return Forbid();

            return await SaveDroppedFiles(droppedFiles, documentLink, folderId, roleLink, false, null);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDroppedDefaultImage(IFormFile droppedFile, string documentLink, string? roleLink)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (ImageHelper.IsImageFile(droppedFile.FileName))
            {
                return await SaveDroppedFiles(new List<IFormFile> { droppedFile }, documentLink, "", roleLink, true, null);
            }
            else
            {
                return BadRequest(_localizer["Please upload an image file."].Value);
            }
        }

        private async Task<IActionResult> SaveDroppedFiles(IEnumerable<IFormFile> droppedFiles, string documentLink, string folderId, string? roleLink, bool isDefault, List<string>? responsibles, string? source = DocumentSourceType.Manual)
        {
            if (droppedFiles.Count() <= 0)
                return BadRequest(_localizer["No document to upload."].Value);

            if (!await _docViewModelService.CanModifyDocument(documentLink) && documentLink != "|||0")
                return Forbid();

            //get or create docFolder
            var docFolder = await _netDocsViewModelService.GetOrAddDefaultFolderByDocumentLink(documentLink);

            //for workflow generation
            var hasNewResponsible = (responsibles != null && responsibles.Any());
            var parentId = int.Parse(documentLink.Split("|")[3] ?? "0");
            var attachments = new List<WorkflowEmailAttachmentViewModel>();

            using (var client = await _netDocumentsClientFactory.GetClient())
            {
                if (docFolder == null && _netDocumentsSettings.WorkspaceCreation == WorkspaceCreation.Auto)
                {
                    docFolder = await _netDocsViewModelService.CreateWorkspace(client, documentLink);
                }

                if (string.IsNullOrEmpty(folderId))
                    folderId = _netDocsViewModelService.GetDefaultDocumentFolder(docFolder);

                if (docFolder == null || string.IsNullOrEmpty(folderId))
                    return BadRequest(_localizer["Folder not found."].Value);

                foreach (var file in droppedFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        var workflowViewModel = await _netDocsViewModelService.SaveDocument(client, file, documentLink, folderId, docFolder.FolderId, parentId, isDefault: isDefault);

                        if (workflowViewModel != null)
                        {
                            //create responsible
                            if (hasNewResponsible && responsibles != null)
                                await _docViewModelService.UpdateDocResponsible(responsibles, User.GetUserName(), workflowViewModel.DocId);

                            //create workflow email attachments
                            attachments.Add(workflowViewModel);
                        }
                    }
                }
            }

            //generate workflow
            var workflowHeader = await GenerateWorkflow(documentLink, attachments, isNewFileUpload: true, hasNewRespDocketing: hasNewResponsible);

            //generate signature workflow
            var eSignatureWorkflows = await GenerateSignatureWorkflow(workflowHeader, documentLink, attachments, parentId, roleLink);

            //generate email workflow
            var emailWorkflows = GenerateEmailWorkflow(workflowHeader, attachments, parentId);

            //return workflows
            if (emailWorkflows.Any() || eSignatureWorkflows.Any())
            {
                var emailUrl = "";
                if (emailWorkflows != null && emailWorkflows.Any())
                    emailUrl = emailWorkflows.First().emailUrl;

                return Json(new { id = parentId, sendEmail = true, folderId = docFolder.FolderId, emailUrl, emailWorkflows, eSignatureWorkflows });
            }

            return Ok();
        }
        
        /// <summary>
        /// Get default image for DefaultImage view component
        /// </summary>
        /// <param name="id">NetDocumemts document env id</param>
        /// <returns></returns>
        public async Task<IActionResult> DefaultImage(string id)
        {
            // use local info to reduce api calls
            var docFile = await _netDocsViewModelService.GetDocFileByDriveItemId(id);

            return PartialView("_DefaultImage", docFile);
        }

        /// <summary>
        /// Get default image for search results grid
        /// </summary>
        /// <param name="fileName">DocFile.DocFileName</param>
        /// <returns></returns>
        public async Task<IActionResult> DefaultGridImage(string fileName)
        {
            var document = new Document();
            var docFile = await _docViewModelService.GetDocFileByDocFileName(fileName);
            if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
                using (var client = await _netDocumentsClientFactory.GetClient())
                {
                    document = await client.GetDocumentProfile(docFile.DriveItemId);
                }

            return PartialView("_DefaultGridImage", document);
        }

        /// <summary>
        /// POST /Dowload/{DocId}
        /// Download XHR is using Fetch API with POST method. 
        /// Document DocId is in request body
        /// </summary>
        /// <param name="id">DocId</param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Download([FromBody] string id)
        {
            return await DownloadDocument(id);
        }

        /// <summary>
        /// GET /Dowload/{DocId}
        /// Image downloader for <img> tag in gallery view
        /// </summary>
        /// <param name="id">DocId</param>
        /// <returns></returns>
        [HttpGet, ActionName("Download")]
        public async Task<IActionResult> GetImage(string id)
        {
            return await DownloadDocument(id);
        }

        /// <summary>
        /// GET /GetFile/{FileId}
        /// Download document using tblDocFile.FileId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetFile(int id)
        {
            var docFile = await _docViewModelService.GetDocFileById(id);
            if (docFile == null || string.IsNullOrEmpty(docFile.DriveItemId))
                return NotFound(_localizer["Document not found"]);

            return await DownloadDocument(docFile.DriveItemId);
        }

        /// <summary>
        /// Open NetDocs document editor
        /// </summary>
        /// <param name="id">DocId</param>
        /// <returns></returns>
        public IActionResult Open(string id)
        {
            if (string.IsNullOrEmpty(id))
                return UnprocessableEntity("Document Id is required.");

            //redirect to NetDocs only if using auth code or pkce flow
            //var authFlow = _authProvider.GetAuthenticationFlow();
            //if (authFlow.IsAuthCodeFlow())
            return Redirect($"{_netDocumentsSettings.DocumentUrl}&q==999({id})");
        }

        /// <summary>
        /// Open NetDocs document editor using tblDocFile.FileId
        /// </summary>
        /// <param name="id">FileId</param>
        /// <returns></returns>
        public async Task<IActionResult> OpenFile(int id)
        {
            var docFile = await _docViewModelService.GetDocFileById(id);
            if (docFile == null || string.IsNullOrEmpty(docFile.DriveItemId))
                return NotFound(_localizer["Document not found"]);

            return Open(docFile.DriveItemId);
        }

        /// <summary>
        /// Get document as FileContentResult
        /// </summary>
        /// <param name="id">DocId</param>
        /// <returns></returns>
        private async Task<IActionResult> DownloadDocument(string id)
        {
            if (string.IsNullOrEmpty(id))
                return UnprocessableEntity("Document Id is required.");

            using (var client = await _netDocumentsClientFactory.GetClient())
            {
                // log trade secret download
                var settings = await _patSettings.GetSetting();
                if (settings.IsTradeSecretOn)
                    await _docService.LogDocTradeSecretActivityByDriveItemId(id);

                return await client.DownloadDocument(id);
            }
        }

        [HttpGet]
        public async Task<IActionResult> UploadDocument(string folderId, string documentLink, string roleLink, string token)
        {
            if (!(GetViewerPermission(token).CanUpload))
                return Forbid();

            if (string.IsNullOrEmpty(folderId) || string.IsNullOrEmpty(documentLink))
                return BadRequest();

            //get docFolder, use parent docFolder if not found
            var docFolder = await _netDocsViewModelService.GetOrAddDefaultFolderByDocumentLink(documentLink);
            if (docFolder == null)
                return BadRequest(_localizer["Folder not found."].Value);

            ViewData["FormAction"] = Url.Action("SaveDocument", new { folderId = folderId, documentLink = documentLink, roleLink = roleLink, token = token });
            ViewData["DocumentLink"] = documentLink;

            var viewModel = new DocumentViewModel()
            {
                FolderId = docFolder.FolderId,
                Source = DocumentSourceType.Manual,
                RandomGuid = Guid.NewGuid().ToString()
            };

            return PartialView("_DocumentEditor", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditDocument(string docId, string folderId, string documentLink, string roleLink, string token)
        {
            if (!(GetViewerPermission(token).CanEdit))
                return Forbid();

            if (string.IsNullOrEmpty(docId) || string.IsNullOrEmpty(folderId) || string.IsNullOrEmpty(documentLink))
                return BadRequest();

            var model = new DocumentViewModel();
            var client = await _netDocumentsClientFactory.GetClient();
            var document = await client.GetDocumentProfile(docId);
            if (string.IsNullOrEmpty(document?.Id))
                return BadRequest();

            //get docDocument using NetDocs DocId (1:1)
            var docDocument = await _docViewModelService.GetDocumentByDriveItemId(docId);
            if (docDocument == null)
            {
                //get docFolder, use parent docFolder if not found
                var docFolder = await _netDocsViewModelService.GetOrAddDefaultFolderByDocumentLink(documentLink);
                if (docFolder == null)
                    return BadRequest(_localizer["Folder not found."].Value);

                //get docDocumentModel for creating docFile and docDocument
                var docViewModel = await _netDocsViewModelService.GetNewDocDocumentViewModel(documentLink, document.GetFileName(), document.Attributes?.Name ?? "", document.Attributes?.Size ?? 0, document.IsImage(), document.Id ?? "", docFolder.FolderId);

                //create docFile and update docViewModel.FileId
                var docFile = await _docViewModelService.AddDocFile(docViewModel, docViewModel.UserFileName ?? "", docViewModel.FileSize ?? 0, docViewModel.IsImage ?? false);

                //create docDocument
                docDocument = await _docViewModelService.SaveDocument(docViewModel);
            }
            else
            {
                if (docDocument.DocFolder != null)
                {
                    var documentLinkArray = documentLink.Split("|");
                    var systemType = documentLinkArray[0]?.ToUpper();
                    var screenCode = documentLinkArray[1];
                    var dataKey = documentLinkArray[2]?.ToLower();
                    var dataKeyValue = int.Parse(documentLinkArray[3] ?? "0");

                    ViewData["FromParent"] = (docDocument.DocFolder.SystemType?.ToUpper() != systemType || docDocument.DocFolder.DataKey?.ToLower() != dataKey || docDocument.DocFolder.DataKeyValue != dataKeyValue);
                }
            }

            //set CPI properties
            model = _mapper.Map<DocumentViewModel>(docDocument);

            //set responsible
            if (model.DocId > 0)
            {
                model.DefaultRespDocketings = await _docService.GetDocRespDocketingList(model.DocId);
                model.DefaultRespReportings = await _docService.GetDocRespReportingList(model.DocId);
            }                

            //set NetDocs properties
            model.Id = document.Id;
            model.Name = document.Attributes?.Name;
            //model.Type = document.Attributes?.Ext;
            model.Extension = document.Attributes?.Ext;
            model.DocumentName = document.Attributes?.Name;
            //model.WorkType = document.Attributes?.Ext;
            model.IsImage = document.IsImage();

            ViewData["DocumentLink"] = documentLink;
            ViewData["FormAction"] = Url.Action("SaveDocument", new { folderId =  folderId, documentLink = documentLink, roleLink = roleLink, token = token });
            return PartialView("_DocumentEditor", model);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDocument(DocumentViewModel model, string folderId, string documentLink, string roleLink) //, string token)
        {
            if (!await _docViewModelService.CanModifyDocument(documentLink))
                return BadRequest(_localizer["No upload permission."].Value);

            var client = await _netDocumentsClientFactory.GetClient();
            var document = _mapper.Map<Document>(model);
            var docViewModel = _mapper.Map<DocDocumentViewModel>(model);

            var linkVerification = false;
            if (docViewModel.DocId <= 0 && docViewModel.IsActRequired) linkVerification = true;      

            //validate folder setup
            if (docViewModel.FolderId == 0)
                return BadRequest(_localizer["Folder not found."].Value);

            //update time stamps
            if (docViewModel.DocId == 0)
            {
                docViewModel.CreatedBy = User.GetUserName();
                docViewModel.DateCreated = DateTime.Now;
            }
            docViewModel.UpdatedBy = User.GetUserName();
            docViewModel.LastUpdate = DateTime.Now;

            var isNewFileUpload = false;

            //upload
            if (model.FormFile != null && model.FormFile.Length > 0 && !string.IsNullOrEmpty(folderId))
            {
                byte[] data;
                using (var br = new BinaryReader(model.FormFile.OpenReadStream()))
                    data = br.ReadBytes((int)model.FormFile.OpenReadStream().Length);
                var bytes = new ByteArrayContent(data);

                if (string.IsNullOrEmpty(model.Id))
                    document = await client.UploadDocument(folderId, bytes, model.FormFile.FileName);
                else
                    document = await client.CreateVersion(model.Id, bytes, model.FormFile.FileName);

                if (document != null)
                {
                    //docDocument
                    docViewModel.Author = User.GetEmail();
                    docViewModel.Source = DocumentSourceType.Manual;
                    docViewModel.DocTypeId = await _docViewModelService.GetDocTypeIdFromFileName(string.IsNullOrEmpty(docViewModel.UserFileName) ? document.GetFileName() : docViewModel.UserFileName);
                    docViewModel.DocName = document.Attributes?.Name ?? docViewModel.DocName;

                    //docFile
                    docViewModel.UserFileName = model.FormFile.FileName;
                    docViewModel.FileSize = document.Attributes?.Size;
                    docViewModel.IsImage = document.IsImage();
                    docViewModel.DriveItemId = document.Id;

                    //update/create docFile
                    if ((docViewModel.FileId ?? 0) > 0)
                        await _docViewModelService.UpdateDocFile(docViewModel);
                    else
                    {
                        var docFile = await _docViewModelService.AddDocFile(docViewModel, docViewModel.UserFileName ?? "", docViewModel.FileSize ?? 0, docViewModel.IsImage ?? false);
                        docViewModel.FileId = docFile.FileId;
                    }

                    isNewFileUpload = true;
                }
            }
            else 
            {
                //update NetDocs name
                if (!string.IsNullOrEmpty(model.Id) && !string.IsNullOrEmpty(model.Name) && (model.Name != model.DocumentName))
                {
                    await client.UpdateDocumentProfile(model.Id, new UpdatableDocumentProfile()
                    {
                        StandardAttributes = new UpdatableStandardAttributes() { Name = model.Name }
                    });

                    //update DocFile FileName
                    await _docViewModelService.RenameDocFile(model.FileId ?? 0, model.Name);

                    //update DocDocument.DocName
                    docViewModel.DocName = model.Name;
                }
            }

            if (document == null || string.IsNullOrEmpty(document.Id))
                return BadRequest();

            var successMessage = _localizer["Document has been uploaded successfully."].Value;

            // Move file to "Dockets for Verification" folder if CheckAct is checked and DocVerification is on
            // only apply to files from CtryApplication/Trademark/GeneralMatter and when CheckAct value changes
            // if CheckDocket is checked move to "Dockets for Verification" folder
            // else move to default folder
            var patSettings = await _patSettings.GetSetting();
            var tmkSettings = await _tmkSettings.GetSetting();
            var documentLinkArr = new List<string>();
            var systemType = string.Empty;
            var screenCode = string.Empty;
            var dataKey = string.Empty;
            var dataKeyValue = 0;
            if (!string.IsNullOrEmpty(documentLink))
                documentLinkArr = documentLink.Split("|").ToList();

            if (documentLinkArr.Count > 0)
            {
                systemType = documentLinkArr[0];
                screenCode = documentLinkArr[1];
                dataKey = documentLinkArr[2];
                dataKeyValue = int.Parse(documentLinkArr[3] ?? "0");
            }
            
            if (!string.IsNullOrEmpty(folderId)
                && ((patSettings.IsDocumentVerificationOn && systemType.ToLower() == SystemTypeCode.Patent.ToLower() && screenCode.ToLower() == ScreenCode.Application.ToLower())
                    || (tmkSettings.IsDocumentVerificationOn && systemType.ToLower() == SystemTypeCode.Trademark.ToLower() && screenCode.ToLower() == ScreenCode.Trademark.ToLower())))
            {
                var docVerificationFolderName = "Dockets for Verification";
                //var docVerificationFolderName = string.Empty;
                switch (systemType)
                {
                    case SystemTypeCode.Patent:
                        docVerificationFolderName = patSettings.DocVerificationDefaultFolderName;
                        break;
                    case SystemTypeCode.Trademark:
                        docVerificationFolderName = tmkSettings.DocVerificationDefaultFolderName;
                        break;
                }

                //if (string.IsNullOrEmpty(docVerificationFolderName)) docVerificationFolderName = "Dockets for Verification";

                //Get current folder
                Folder? currentFolder = await client.GetFolder(folderId);
                //Folder? currentFolder = null;
                Folder? docVerificationFolder = null;
                //var library = client.GetLibrary(folderId);
                //var request = new HttpRequestMessage(HttpMethod.Get, $"libraries/{library}/folders?container_id={folderId}&limit={1}"); //no leading slash to use BaseAddress
                //var response = await client.SendAsync(request);
                //var result = await response.GetContentAsStringAsync();
                //if (!string.IsNullOrEmpty(result)) 
                //{
                //    var resultFolders = JsonSerializer.Deserialize<FoldersResponse>(result);
                //    if (resultFolders != null && resultFolders.Data != null && resultFolders.Data.Count > 0)
                //        currentFolder = resultFolders.Data.FirstOrDefault();
                //}

                var currentCheckAct = await _docService.DocDocuments.AsNoTracking().Where(d => d.DocId == docViewModel.DocId).Select(d => d.CheckAct).FirstOrDefaultAsync();

                if (currentFolder != null && docViewModel.CheckAct != currentCheckAct && !string.IsNullOrEmpty(currentFolder.Name)
                    && ((docViewModel.CheckAct && currentFolder.Name.ToLower() != docVerificationFolderName.ToLower())
                    || (!docViewModel.CheckAct && currentFolder.Name.ToLower() == docVerificationFolderName.ToLower())))
                {
                    //Get "Dockets for Verification" folder
                    if (currentFolder.Name.ToLower() != docVerificationFolderName.ToLower())
                    {
                        //request = new HttpRequestMessage(HttpMethod.Get, $"libraries/{library}/folders?container_id={currentFolder.ParentId}"); //no leading slash to use BaseAddress
                        //response = await client.SendAsync(request);
                        //result = await response.GetContentAsStringAsync();
                        //if (!string.IsNullOrEmpty(result)) 
                        //{
                        //    var resultFolders = JsonSerializer.Deserialize<FoldersResponse>(result);
                        //    if (resultFolders != null && resultFolders.Data != null && resultFolders.Data.Count > 0)
                        //        docVerificationFolder = resultFolders.Data.Where(d => !string.IsNullOrEmpty(d.Name) && d.Name.ToLower() == docVerificationFolderName.ToLower()).FirstOrDefault();
                        //}
                        var resultFolders = await client.GetFolders(currentFolder.ParentId);
                        if (resultFolders != null && resultFolders.Results != null && resultFolders.Results.Count > 0)
                            docVerificationFolder = resultFolders.Results.Where(d => !string.IsNullOrEmpty(d.Name) && d.Name.ToLower() == docVerificationFolderName.ToLower()).FirstOrDefault();
                    }
                    
                    var moveSuccess = false;
                    //Move doc from current folder to "Dockets for Verification" folder
                    if (docViewModel.CheckAct && ((docViewModel.DocId <= 0) || (docViewModel.DocId > 0 && currentCheckAct != docViewModel.CheckAct)) && currentFolder.Name.ToLower() != docVerificationFolderName.ToLower())
                    {
                        if (docVerificationFolder == null && !string.IsNullOrEmpty(currentFolder.ParentId))
                            docVerificationFolder = await client.CreateFolder(currentFolder.ParentId, docVerificationFolderName);
                            
                        if (docVerificationFolder != null)
                            moveSuccess = await client.MoveDocument(docViewModel.DriveItemId ?? (document.Id ?? ""), currentFolder.Id ?? "", docVerificationFolder.Id ?? "");
                    }
                    //Move doc from "Dockets for Verification" folder to default folder
                    else if (!docViewModel.CheckAct && docViewModel.DocId > 0 && currentCheckAct != docViewModel.CheckAct && currentFolder.Name.ToLower() == docVerificationFolderName.ToLower())
                    {
                        //Get default folder id
                        var defaultFolderId = await _docService.DocFolders.AsNoTracking()
                            .Where(d => !string.IsNullOrEmpty(d.SystemType) && d.SystemType.ToLower() == systemType.ToLower() 
                                && !string.IsNullOrEmpty(d.ScreenCode) && d.ScreenCode.ToLower() == screenCode.ToLower() 
                                && !string.IsNullOrEmpty(d.DataKey) && d.DataKey.ToLower() == dataKey.ToLower() 
                                && d.DataKeyValue == dataKeyValue)
                            .Select(d => d.StorageDefaultFolderId).FirstOrDefaultAsync();

                        if (!string.IsNullOrEmpty(defaultFolderId) && currentFolder.Id != defaultFolderId)
                            moveSuccess = await client.MoveDocument(docViewModel.DriveItemId ?? (document.Id ?? ""), currentFolder.Id ?? "", defaultFolderId);
                    }
                }                
            }

            //save docDocument
            var docDocument = await _docViewModelService.SaveDocument(docViewModel);

            //save fileId
            model.FileId = docDocument.FileId;

            //save responsible
            var respDocketing = await _docViewModelService.SaveRespDocketing(docViewModel, User.GetUserName());
            var respReporting = await _docViewModelService.SaveRespReporting(docViewModel, User.GetUserName());

            //link verification
            if (linkVerification)
                await _docService.LinkDocWithVerifications(docViewModel.DocId, docViewModel.RandomGuid ?? "");

            //generate workflow
            //docViewModel.UserFileName is set when file was uploaded
            if (!string.IsNullOrEmpty(docViewModel.UserFileName) || respDocketing.IsNew || respDocketing.IsReassigned || respReporting.IsNew || respReporting.IsReassigned)
            {
                var parentId = int.Parse(documentLink.Split("|")[3] ?? "0");
                var attachments = new List<WorkflowEmailAttachmentViewModel>() {
                    new WorkflowEmailAttachmentViewModel
                    {
                        Id = document.Id,
                        DocId = docDocument.DocId,
                        FileId = docDocument.FileId,
                        OrigFileName = docViewModel.UserFileName,
                        FileName = document.Id, // $"{docDocument.FileId}{Path.GetExtension(docViewModel.UserFileName)}",
                        DocParent = parentId
                    }
                };

                //generate workflow
                var workflowHeader = await GenerateWorkflow(documentLink, attachments, isNewFileUpload: isNewFileUpload, hasNewRespDocketing: respDocketing.IsNew, hasRespDocketingReassigned: respDocketing.IsReassigned, hasNewRespReporting: respReporting.IsNew, hasRespReportingReassigned: respReporting.IsReassigned);

                //generate signature workflow
                var eSignatureWorkflows = await GenerateSignatureWorkflow(workflowHeader, documentLink, attachments, parentId, roleLink);

                //generate email workflow
                var emailWorkflows = GenerateEmailWorkflow(workflowHeader, attachments, parentId);

                //return workflows
                if (emailWorkflows.Any() || eSignatureWorkflows.Any())
                {
                    var emailUrl = "";
                    if (emailWorkflows != null && emailWorkflows.Any())
                        emailUrl = emailWorkflows.First().emailUrl;

                    return Json(new { success = successMessage, id = parentId, sendEmail = true, folderId = docViewModel.FolderId, emailUrl, emailWorkflows, eSignatureWorkflows });
                }
            }

            return Ok(new { success = successMessage });
        }
        

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDocument(string id, string token)
        {
            if (!(GetViewerPermission(token).CanDelete))
                return Forbid();

            using (var client = await _netDocumentsClientFactory.GetClient())
            {
                await _docViewModelService.DeleteDocumentsByDriveItemId(id);
                await client.DeleteDocument(id);
            }

            return Ok();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDocuments(List<string> ids, string token)
        {
            if (ids.Count == 0) return BadRequest();

            if (!(GetViewerPermission(token).CanDelete))
                return Forbid();

            using (var client = await _netDocumentsClientFactory.GetClient())
            {
                foreach (var id in ids)
                {
                    await _docViewModelService.DeleteDocumentsByDriveItemId(id);
                    await client.DeleteDocument(id);
                }

                return Ok();
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefaultFolder(string? documentLink, string? id, string? token)
        {
            if (!(GetViewerPermission(token).CanEdit))
                return Forbid();

            if (string.IsNullOrEmpty(documentLink) || string.IsNullOrEmpty(id))
                return BadRequest();

            var docFolder = await _netDocsViewModelService.GetOrAddDefaultFolderByDocumentLink(documentLink);
            if (docFolder == null || string.IsNullOrEmpty(docFolder.StorageRootContainerId))
                return BadRequest();

            docFolder.StorageDefaultFolderId = id;
            docFolder.UpdatedBy = User.GetUserName();
            docFolder.LastUpdate = DateTime.Now;

            await _docViewModelService.SaveFolderStorageSetting(docFolder);

            return Ok();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFolder(string id, string? token)
        {
            if (!(GetViewerPermission(token).CanDelete))
                return Forbid();

            using (var client = await _netDocumentsClientFactory.GetClient())
            {
                await client.DeleteFolder(id);
            }

            return Ok();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenameFolder(string id, string name, string? token)
        {
            if (!(GetViewerPermission(token).CanEdit))
                return Forbid();

            using (var client = await _netDocumentsClientFactory.GetClient())
            {
                await client.RenameFolder(id, name);
            }

            return Ok();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFolder(string id, string name, ContainerType parentContainerType, string? token)
        {
            if (!(GetViewerPermission(token).CanEdit))
                return Forbid();

            using (var client = await _netDocumentsClientFactory.GetClient())
            {
                await client.CreateFolder(id, name);
            }

            return Ok();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveFolder(string folderId, string destinationFolderId, string? token)
        {
            if (!(GetViewerPermission(token).CanEdit))
                return Forbid();

            using (var client = await _netDocumentsClientFactory.GetClient())
            {
                await client.MoveFolder(folderId, destinationFolderId);
            }

            return Ok();
        }

        // RMS ImageAddRMS/SaveDocumentRMS and FF ImageAddFF/SaveDocumentFF methods removed - modules deleted
    }
}
