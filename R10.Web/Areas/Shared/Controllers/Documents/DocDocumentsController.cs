using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Security;
using R10.Web.Helpers;
using R10.Web.Services.DocumentStorage;
using R10.Web.Services;
using R10.Web.Services.MailDownload;
using Newtonsoft.Json;
using Microsoft.Graph;
using System.Globalization;
using R10.Core.Interfaces.RMS;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using ActiveQueryBuilder.View.DatabaseSchemaView;
using Microsoft.Extensions.Options;
using R10.Web.Filters;
using ActiveQueryBuilder.Web.Server.Models;
using R10.Web.Areas.Shared.Services;
using R10.Core.Interfaces.Patent;
using R10.Core.Identity;
using Microsoft.AspNetCore.Identity;
using GleamTech.IO;
using iText.Kernel.Pdf;
using R10.Core.Interfaces.ForeignFiling;
using OpenIddict.Validation.AspNetCore;

namespace R10.Web.Areas.Shared.Controllers
{
    [Area("Shared"), Authorize(AuthenticationSchemes = $"Identity.Application,{OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme}")]
    public class DocDocumentsController : DocumentUploadController
    {
        private readonly IDocumentService _docService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IDocumentStorage _documentStorageService;
        private readonly IRMSDueDocService _rmsDueDocService;
        private readonly IFFDueDocService _ffDueDocService;
        private readonly IChildEntityService<DocDocument, DocDocumentTag> _documentTagService;
        private readonly IDocumentHelper _documentHelper;

        private readonly IParentEntityService<PatActionType, PatActionParameter> _patActionTypeEntityService;
        private readonly IActionDueService<PatActionDue, PatDueDate> _patActionDueEntityService;
        private readonly ICountryApplicationService _countryApplicationService;
        private readonly IRTSService _rtsService;

        private readonly IParentEntityService<TmkActionType, TmkActionParameter> _tmkActionTypeEntityService;
        private readonly IActionDueService<TmkActionDue, TmkDueDate> _tmkActionDueEntityService;
        private readonly ITmkTrademarkService _tmkTrademarkService;

        private readonly IParentEntityService<GMActionType, GMActionParameter> _gmActionTypeEntityService;
        private readonly IActionDueService<GMActionDue, GMDueDate> _gmActionDueEntityService;
        private readonly IGMMatterService _gmMatterService;
        private readonly IGMMatterCountryService _gmMatterCountryService;

        private readonly EPOMailboxSettings _epoMailboxSettings;
        private readonly IParentEntityService<EPOCommunication, EPOCommunicationDoc> _epoCommunicationDocService;

        private readonly IDocumentsAIViewModelService _documentsAIViewModelService;

        private const string ImageEditor = "Views/Document/_DocumentEditor.cshtml";
        private const string ImageEditorZoom = "Views/Document/_DocumentEditorZoom.cshtml";
        private const string ImageMerger = "Views/Document/_DocumentMerge.cshtml";

        public DocDocumentsController(
                    IDocumentService docService,
                    IDocumentsViewModelService docViewModelService,
                    IStringLocalizer<SharedResource> localizer,
                    ISystemSettings<DefaultSetting> settings,
                    IMapper mapper,
                    IWebHostEnvironment hostingEnvironment,
                    IDocumentStorage documentStorageService,
                    IMailDownloadService mailDownloadService,
                    IOptions<GraphSettings> graphSettings,
                    IAuthorizationService authService,
                    ISystemSettings<PatSetting> patSettings,
                    ISystemSettings<TmkSetting> tmkSettings,
                    ISystemSettings<GMSetting> gmSettings,
                    IRMSDueDocService rmsDueDocService,
                    IFFDueDocService ffDueDocService,
                    ILogger<DocDocumentsController> logger,
                    IChildEntityService<DocDocument, DocDocumentTag> documentTagService,
                    IDocumentHelper documentHelper,
                    IParentEntityService<PatActionType, PatActionParameter> patActionTypeEntityService,
                    IActionDueService<PatActionDue, PatDueDate> patActionDueEntityService,
                    ICountryApplicationService countryApplicationService,
                    IRTSService rtsService,
                    IParentEntityService<TmkActionType, TmkActionParameter> tmkActionTypeEntityService,
                    IActionDueService<TmkActionDue, TmkDueDate> tmkActionDueEntityService,
                    ITmkTrademarkService tmkTrademarkService,
                    IParentEntityService<GMActionType, GMActionParameter> gmActionTypeEntityService,
                    IActionDueService<GMActionDue, GMDueDate> gmActionDueEntityService,
                    IGMMatterService gmMatterService,
                    IGMMatterCountryService gmMatterCountryService,
                    IOptions<ServiceAccount> serviceAccount,
                    IOptions<EPOMailboxSettings> epoMailboxSettings,
                    IParentEntityService<EPOCommunication, EPOCommunicationDoc> epoCommunicationDocService,
                    IEPOService epoService,
                    IEntityService<EPOCommunication> epoCommunicationService,
                    IDocumentsAIViewModelService documentsAIViewModelService
            ) : base(mailDownloadService, docViewModelService, graphSettings, serviceAccount, logger, authService, settings, patSettings, tmkSettings, gmSettings, epoService, epoCommunicationService)
        {
            _docService = docService;
            _localizer = localizer;
            _mapper = mapper;
            _documentStorageService = documentStorageService;
            _hostingEnvironment = hostingEnvironment;
            _rmsDueDocService = rmsDueDocService;
            _ffDueDocService = ffDueDocService;
            _documentTagService = documentTagService;
            _documentHelper = documentHelper;

            _patActionTypeEntityService = patActionTypeEntityService;
            _patActionDueEntityService = patActionDueEntityService;
            _countryApplicationService = countryApplicationService;
            _rtsService = rtsService;

            _tmkActionTypeEntityService = tmkActionTypeEntityService;
            _tmkActionDueEntityService = tmkActionDueEntityService;
            _tmkTrademarkService = tmkTrademarkService;

            _gmActionTypeEntityService = gmActionTypeEntityService;
            _gmActionDueEntityService = gmActionDueEntityService;
            _gmMatterService = gmMatterService;
            _gmMatterCountryService = gmMatterCountryService;

            _epoMailboxSettings = epoMailboxSettings.Value;
            _epoCommunicationDocService = epoCommunicationDocService;

            _documentsAIViewModelService = documentsAIViewModelService;
        }

        public async Task<IActionResult> ImageAdd([DataSourceRequest] DataSourceRequest request, string documentLink, int folderId,string? roleLink)
        {
            var model = await _docViewModelService.CreateDocumentEditorViewModel(documentLink, folderId, 0);
            model.Author = User.GetEmail();
            model.RoleLink = roleLink;
            model.Source = DocumentSourceType.Manual;

            ViewData["DocumentLink"] = documentLink;
            var docLinkArr = documentLink.Split("|");
            var dataKey = docLinkArr[2];

            model.RandomGuid = Guid.NewGuid().ToString();

            return PartialView(ImageEditor, model);
        }

        public async Task<IActionResult> GridUpdate([DataSourceRequest] DataSourceRequest request, int id, string documentLink, string? roleLink, bool showDocumentViewer = false)
        {
            if (!await _docViewModelService.CanModifyDocument(documentLink))
                return BadRequest();

            var model = await _docViewModelService.CreateDocumentEditorViewModel(documentLink, 0, id);
            if (model.DocFolder == null || !VerifySystemPermission(documentLink, model.DocFolder))
                return BadRequest();

            var lockedBy = await _docService.IsLocked(id, User.GetUserName());
            if (!string.IsNullOrEmpty(lockedBy))
            {
                return BadRequest(_localizer["Document is currently checked out by: "] + lockedBy);
            }

            //model.DocViewer = docViewer; 
            model.RoleLink = roleLink;

            ViewData["DocumentLink"] = documentLink;

            if (showDocumentViewer)
            {
                model.ViewFilePath = _documentStorageService.GetFilePath(model.DocFolder.SystemType ?? "", model.DocFileName ?? "", R10.Web.Helpers.ImageHelper.CPiSavedFileType.DocMgt);
                model.ViewFileType = R10.Web.Helpers.ImageHelper.CPiSavedFileType.DocMgt;
                return PartialView(ImageEditorZoom, model);
            }
            
            return PartialView(ImageEditor, model);
        }



        public async Task<IActionResult> GridDelete([Bind(Prefix = "deleted")] DocDocumentListViewModel deleted, string documentLink)
        {
            if (deleted.DocId > 0)
            {
                if (!await _docViewModelService.CanModifyDocument(documentLink))
                    return BadRequest();

                var model = await _docViewModelService.CreateDocumentEditorViewModel(documentLink, 0, deleted.DocId);
                if (model.DocFolder == null || !VerifySystemPermission(documentLink, model.DocFolder))
                    return BadRequest();


                if (!string.IsNullOrEmpty(deleted.DocFileName)) {
                    if (await _docService.GetFileOtherRefCount(deleted.DocId, deleted.FileId) == 0)
                    {
                        _documentHelper.DeleteDocumentFile((deleted.DocFileName ?? ""), (deleted.ThumbFileName ?? ""), ImageHelper.IsImageFile(deleted.DocFileName ?? ""));
                    }
                }
                await _docService.UpdateDocuments(User.GetUserName(), new List<DocDocument>(), new List<DocDocument>(), new List<DocDocument>() { _mapper.Map<DocDocument>(deleted) });

                return Ok(new { success = _localizer["Document has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDocument(DocDocumentViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (!await _docViewModelService.CanModifyDocument(viewModel.DocumentLink))
                return BadRequest();            

            if (string.IsNullOrEmpty(viewModel.DocUrl) && _docService.DocTypes.Any(t => t.DocTypeId == viewModel.DocTypeId && t.DocTypeName == "Link"))
            {
                return BadRequest(_localizer["Please enter a valid link"].Value);
            }

            if (viewModel.DocId > 0)
            {
                var model = await _docViewModelService.CreateDocumentEditorViewModel(viewModel.DocumentLink, 0, viewModel.DocId);
                viewModel.tStamp = model.tStamp; //because of tags entry
                if (model.DocFolder == null || !VerifySystemPermission(viewModel.DocumentLink, model.DocFolder))
                    return BadRequest();
            }

            var lockedBy = await _docService.IsLocked(viewModel.DocId, User.GetUserName());
            if (!string.IsNullOrEmpty(lockedBy))
            {
                return BadRequest(_localizer["Document is currently checked out by: "] + lockedBy);
            }

            var linkVerification = false;
            if (viewModel.DocId <= 0 && viewModel.IsActRequired) linkVerification = true;            

            UpdateEntityStamps(viewModel, viewModel.DocId);
            await _docViewModelService.SaveDocumentPopup(viewModel, _hostingEnvironment.ContentRootPath);

            if (linkVerification) await _docService.LinkDocWithVerifications(viewModel.DocId, viewModel.RandomGuid ?? "");

            //Responsible Docketing
            var respDocketing = await _docViewModelService.SaveRespDocketing(viewModel, User.GetUserName());
            var hasNewRespDocketing = respDocketing.IsNew;
            var hasRespDocketingReassigned = respDocketing.IsReassigned;
            //Responsible Reporting
            var respReporting = await _docViewModelService.SaveRespReporting(viewModel, User.GetUserName());
            var hasNewRespReporting = respReporting.IsNew;
            var hasRespReportingReassigned = respReporting.IsReassigned;

            var hasDocVerificationEmailWorkflows = await ProcessDocVerificationNewActWorkflow(viewModel.DocId);

            if (!string.IsNullOrEmpty(viewModel.UserFileName) || hasNewRespDocketing || hasRespDocketingReassigned || hasNewRespReporting || hasRespReportingReassigned || (hasDocVerificationEmailWorkflows != null && hasDocVerificationEmailWorkflows.Count > 0))
            {
                var isNewFileUpload = false;
                var attachments = new List<WorkflowEmailAttachmentViewModel>();
                //Get UserFileName if hasNewResponsible/hasResponsibleReassigned is true
                if (string.IsNullOrEmpty(viewModel.UserFileName))
                {
                    var userFileName = await _docService.DocFiles.AsNoTracking().Where(d => d.FileId == viewModel.FileId).Select(d => d.UserFileName).FirstOrDefaultAsync();
                    if (!string.IsNullOrEmpty(userFileName))
                        //Use actual filename for locating the file (Use FileId, DocFileName could be Null)
                        //attachments.Add(new WorkflowEmailAttachmentViewModel { DocId = viewModel.DocId, FileId = viewModel.FileId, OrigFileName = userFileName, FileName = userFileName, DocParent = viewModel.ParentId });
                        attachments.Add(new WorkflowEmailAttachmentViewModel { DocId = viewModel.DocId, FileId = viewModel.FileId, OrigFileName = userFileName, FileName = $"{viewModel.FileId}{Path.GetExtension(userFileName)}", DocParent = viewModel.ParentId });
                }
                else
                {
                    isNewFileUpload = true;
                    //Use actual filename for locating the file (Use FileId, DocFileName could be Null)
                    //attachments.Add(new WorkflowEmailAttachmentViewModel { DocId = viewModel.DocId, FileId = viewModel.FileId, FileName = viewModel.UserFileName, DocParent = viewModel.ParentId });
                    attachments.Add(new WorkflowEmailAttachmentViewModel { DocId = viewModel.DocId, FileId = viewModel.FileId, FileName = $"{viewModel.FileId}{Path.GetExtension(viewModel.UserFileName)}", DocParent = viewModel.ParentId });
                }                

                var workflowHeader = await GenerateWorkflow(viewModel.DocumentLink, attachments, isNewFileUpload, hasNewRespDocketing, hasRespDocketingReassigned, hasNewRespReporting, hasRespReportingReassigned);
                var eSignatureWorkflows = isNewFileUpload ? await GenerateSignatureWorkflow(workflowHeader, viewModel.DocumentLink, attachments, viewModel.ParentId, viewModel.RoleLink) : new List<WorkflowSignatureViewModel>();
                var emailWorkflows = GenerateEmailWorkflow(workflowHeader, attachments, viewModel.ParentId);

                if (hasDocVerificationEmailWorkflows != null && hasDocVerificationEmailWorkflows.Count > 0) emailWorkflows.AddRange(hasDocVerificationEmailWorkflows);

                if (emailWorkflows.Any() || eSignatureWorkflows !=null)
                {
                    var emailUrl = "";
                    if (emailWorkflows != null && emailWorkflows.Any())
                        emailUrl = emailWorkflows.First().emailUrl;
                    
                    return Json(new { id = viewModel.ParentId, sendEmail = true, folderId = viewModel.FolderId, emailUrl, emailWorkflows, eSignatureWorkflows });
                }
            }
            
            return Json(new
            {
                folderId = viewModel.FolderId
            });

        }


        public async Task<IActionResult> ImageIsLocked([DataSourceRequest] DataSourceRequest request, int id, string documentLink)
        {
            var lockedBy = await _docService.IsLocked(id, User.GetUserName());
            if (!string.IsNullOrEmpty(lockedBy))
                return BadRequest(_localizer["Document is currently checked out by: "] + lockedBy);

            return Ok();
        }

        public async Task<IActionResult> ImageCheckout([DataSourceRequest] DataSourceRequest request, int id, string documentLink)
        {
            if (!await _docViewModelService.CanModifyDocument(documentLink))
                return BadRequest();

            var model = await _docViewModelService.CreateDocumentEditorViewModel(documentLink, 0, id);
            if (model.DocFolder == null || !VerifySystemPermission(documentLink, model.DocFolder))
                return BadRequest();

            var fileType = ImageHelper.CPiSavedFileType.DocMgt;
            var lockedBy = await _docService.IsLocked(id, User.GetUserName());
            if (!string.IsNullOrEmpty(lockedBy))
                return BadRequest(_localizer["Document is currently checked out by: "] + lockedBy);

            // log trade secret download
            var settings = await _patSettings.GetSetting();
            if (settings.IsTradeSecretOn)
                await _docService.LogDocTradeSecretActivityByDocId(id);

            var fileName = await _docService.CheckoutImage(id, User.GetUserName());

            var file = await _documentStorageService.GetFileStream("", fileName, fileType);
            if (file != null)
                return new FileStreamResult(file.Stream, file.ContentType) { FileDownloadName = file.OrigFileName };
            else
                return BadRequest("File not found.");
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDropped(IEnumerable<IFormFile> droppedFiles, string documentLink, int folderId,string? roleLink)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return await SaveDroppedFiles(droppedFiles, documentLink, folderId, roleLink, false, null);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDroppedDefaultImage(IFormFile droppedFile, string documentLink, int folderId, string? roleLink)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (ImageHelper.IsImageFile(droppedFile.FileName))
            {
                return await SaveDroppedFiles(new List<IFormFile> { droppedFile }, documentLink, folderId, roleLink, true, null);
            }
            else {
                return BadRequest(_localizer["Please upload an image file."].Value);
            }
        }

        private async Task<IActionResult> SaveDroppedFiles(IEnumerable<IFormFile> droppedFiles, string documentLink, int folderId, string? roleLink, bool isDefault, List<string>? responsibles, string? source = DocumentSourceType.Manual)
        {
            if (droppedFiles.Count() <= 0)
            {
                return BadRequest(_localizer["No Document to upload"].ToString());
            }

            if (!await _docViewModelService.CanModifyDocument(documentLink))
                return BadRequest();

            var folder = new DocFolder();
            if (folderId > 0)
                folder = _docService.GetFolderById(folderId);
            else
            {
                folder = await _docViewModelService.GetOrAddDefaultFolder(documentLink);
                folderId = folder.FolderId;
            }
            if (!VerifySystemPermission(documentLink, folder))
                return BadRequest();
                        
            var parentId = folder.DataKeyValue;
            var viewModels = new List<DocDocumentViewModel>();
            foreach (var file in droppedFiles)
            {
                var viewModel = new DocDocumentViewModel();
                viewModel.ParentId = parentId;
                viewModel.UploadedFile = file;
                viewModel.Author = User.GetEmail();
                viewModel.CreatedBy = User.GetUserName(); //need to pass these to add in tblDocFile
                viewModel.UpdatedBy = User.GetUserName();
                viewModel.LastUpdate = DateTime.Now;
                viewModel.DateCreated = DateTime.Now;
                viewModel.UserFileName = file.FileName;
                viewModel.FolderId = folderId;
                viewModel.DocumentLink = documentLink;
                viewModel.DocFolder = folder;
                viewModel.IsDefault = isDefault;
                viewModel.Source = source;
                //viewModel.GroupId = groupId;
                //viewModel.UserId = userId;

                viewModels.Add(viewModel);
            }

            await _docViewModelService.SaveUploadedDocuments(viewModels);
            //Use actual filename for locating the file (Use FileId, DocFileName could be Null)
            //var attachments = viewModels.Select(vm => new WorkflowEmailAttachmentViewModel { DocId = vm.DocId, FileId = vm.FileId, OrigFileName = vm.UserFileName, FileName = vm.UserFileName, DocParent = parentId }).ToList();
            var attachments = viewModels.Select(vm => new WorkflowEmailAttachmentViewModel { DocId = vm.DocId, FileId = vm.FileId, OrigFileName = vm.UserFileName, FileName = $"{vm.FileId}{Path.GetExtension(vm.UserFileName)}", DocParent = parentId }).ToList();

            var hasNewRespDocketing = false;
            //Add/populate tblDocResponsibles
            if (responsibles != null && responsibles.Any())
            {
                foreach (var vm in viewModels)
                {
                    await _docService.UpdateDocRespDocketing(responsibles, User.GetUserName(), vm.DocId);
                };
                hasNewRespDocketing = true;
            }

            var workflowHeader = await GenerateWorkflow(documentLink, attachments, isNewFileUpload: true, hasNewRespDocketing: hasNewRespDocketing);
            var eSignatureWorkflows = await GenerateSignatureWorkflow(workflowHeader, documentLink, attachments, parentId, roleLink);
            var emailWorkflows = GenerateEmailWorkflow(workflowHeader, attachments, parentId);

            if (emailWorkflows.Any() || eSignatureWorkflows.Any())
            {
                var emailUrl = "";
                if (emailWorkflows != null && emailWorkflows.Any())
                  emailUrl = emailWorkflows.First().emailUrl;

                return Json(new { id = parentId, sendEmail = true, folderId = folderId, emailUrl, emailWorkflows, eSignatureWorkflows });
            }

            return Json(new
            {
                folderId = folderId
            });
        }

        public async Task<IActionResult> ImageAddDedocket(string documentLink, string system, string respOffice)
        {
            if (User.IsDeDocketer(system, respOffice))
            {
                var model = await _docViewModelService.CreateDocumentEditorViewModel(documentLink, 0, 0);
                model.Author = User.GetEmail();
                model.SaveAction = "SaveDocumentDedocket";

                return PartialView(ImageEditor, model);
            }
            return BadRequest();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDocumentDeDocket(DocDocumentViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            UpdateEntityStamps(viewModel, viewModel.DocId);

            viewModel.DocId = 0; //add only
            await _docViewModelService.SaveDocumentPopup(viewModel, _hostingEnvironment.ContentRootPath);
            return Json(new { newName = viewModel.DocName });
        }

        [Authorize(Policy = RMSAuthorizationPolicy.DecisionMaker)]
        public async Task<IActionResult> ImageAddRMS(string documentLink, int dueId, int docId)
        {
            if (string.IsNullOrEmpty(documentLink) || dueId == 0)
                return BadRequest();

            var model = await _docViewModelService.CreateDocumentEditorViewModel(documentLink, 0, 0);
            model.Author = User.GetEmail();
            model.SaveAction = "SaveDocumentRMS";

            ViewData["DueId"] = dueId;
            ViewData["RequiredDocId"] = docId;
            ViewData["RequiredDocs"] = Array.Empty<object>();

            var defaultDocType = await _docService.DocTypes.Where(d => (d.DocTypeName ?? "").ToLower() == "file").FirstOrDefaultAsync();
            if (defaultDocType != null)
            {
                model.DocTypeId = defaultDocType.DocTypeId;
                model.DocTypeName = defaultDocType.DocTypeName;
            }

            return PartialView("Views/Document/_RequiredDocumentEditor.cshtml", model);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDocumentRMS(DocDocumentViewModel viewModel, int dueId, int requiredDocId)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (dueId == 0 || requiredDocId == 0)
                return BadRequest();

            UpdateEntityStamps(viewModel, viewModel.DocId);

            viewModel.DocId = 0; //add only
            await _docViewModelService.SaveDocumentPopup(viewModel, _hostingEnvironment.ContentRootPath);

            //save RMSDueDoc, add RMSDueDocUploadLog
            await _rmsDueDocService.SaveUploaded(dueId, requiredDocId, User.GetUserName(), true, viewModel.FileId ?? 0, viewModel.UserFileName ?? "", System.Convert.FromBase64String(""));
            return Json(new { newName = viewModel.DocName });
        }

        [Authorize(Policy = ForeignFilingAuthorizationPolicy.DecisionMaker)]
        public async Task<IActionResult> ImageAddFF(string documentLink, int dueId, int docId)
        {
            if (string.IsNullOrEmpty(documentLink) || dueId == 0)
                return BadRequest();

            var model = await _docViewModelService.CreateDocumentEditorViewModel(documentLink, 0, 0);
            model.Author = User.GetEmail();
            model.SaveAction = "SaveDocumentFF";

            ViewData["DueId"] = dueId;
            ViewData["RequiredDocId"] = docId;
            ViewData["RequiredDocs"] = Array.Empty<object>();

            var defaultDocType = await _docService.DocTypes.Where(d => (d.DocTypeName ?? "").ToLower() == "file").FirstOrDefaultAsync();
            if (defaultDocType != null)
            {
                model.DocTypeId = defaultDocType.DocTypeId;
                model.DocTypeName = defaultDocType.DocTypeName;
            }

            return PartialView("Views/Document/_RequiredDocumentEditor.cshtml", model);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDocumentFF(DocDocumentViewModel viewModel, int dueId, int requiredDocId)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (dueId == 0 || requiredDocId == 0)
                return BadRequest();

            UpdateEntityStamps(viewModel, viewModel.DocId);

            viewModel.DocId = 0; //add only
            await _docViewModelService.SaveDocumentPopup(viewModel, _hostingEnvironment.ContentRootPath);

            //save FFDueDoc, add FFDueDocUploadLog
            await _ffDueDocService.SaveUploaded(dueId, requiredDocId, User.GetUserName(), true, viewModel.FileId ?? 0, viewModel.UserFileName ?? "", System.Convert.FromBase64String(""));
            return Json(new { newName = viewModel.DocName });
        }

        [HttpGet()]
        public async Task<IActionResult> ImageMerge()
        {    
            return PartialView(ImageMerger);
        }

        [HttpPost]
        public async Task<IActionResult> MergeImages(string documentLink, string mergedDocName, List<DocDocumentViewModel> docList)
        {
            if (string.IsNullOrEmpty(documentLink))
                return BadRequest(_localizer["Missing document link."]);

            if (string.IsNullOrEmpty(mergedDocName))
                return BadRequest(_localizer["Merged Document Name is required."]);

            if (docList == null || docList.Count < 1)
                return BadRequest(_localizer["No documents selected to merge."]);

            if (docList.Any(d => string.IsNullOrEmpty(d.DocFileName) || !d.DocFileName.EndsWith(".pdf")))
                return BadRequest(_localizer["Please selecte PDF files only."]);

            var docBytes = new List<byte[]>();

            foreach (var doc in docList)
            {
                var docStream = await _documentStorageService.GetFileStream("", doc.DocFileName ?? "", ImageHelper.CPiSavedFileType.DocMgt);
                if (docStream != null)
                {
                    docBytes.Add(docStream.Stream.ToBytes());
                }
            }

            if (docBytes.Count > 0)
            {
                byte[]? mergedPdf = null;

                // Create a MemoryStream to hold the combined PDF
                using (MemoryStream ms = new MemoryStream())
                {
                    // Initialize PDF writer
                    PdfWriter writer = new PdfWriter(ms);
                    // Initialize PDF document
                    PdfDocument pdf = new PdfDocument(writer);
                    foreach (byte[] pdfBytes in docBytes)
                    {
                        // Create a PdfReader
                        PdfReader reader = new PdfReader(new MemoryStream(pdfBytes));
                        // Initialize source PDF document
                        PdfDocument sourcePdf = new PdfDocument(reader);
                        // Copy pages from source PDF to the destination PDF
                        sourcePdf.CopyPagesTo(1, sourcePdf.GetNumberOfPages(), pdf);
                        // Close the source PDF
                        sourcePdf.Close();
                    }
                    // Close the destination PDF
                    pdf.Close();

                    mergedPdf = ms.ToArray();
                }

                if (mergedPdf != null && mergedPdf.Length > 0)
                {
                    if (!mergedDocName.EndsWith(".pdf")) mergedDocName += ".pdf";

                    var userName = User.GetUserName();

                    var documentLinkArr = documentLink.Split("|");
                    var systemType = documentLinkArr[0];
                    var screenCode = documentLinkArr[1];
                    var dataKey = documentLinkArr[2];
                    var dataKeyValueStr = documentLinkArr[3];
                    int dataKeyValue = 0;

                    if (int.TryParse(dataKeyValueStr, out dataKeyValue))
                    {
                        using (var ms = new MemoryStream(mergedPdf))
                        {
                            var docViewModel = new DocDocumentViewModel
                            {
                                DocFileName = mergedDocName,
                                Author = User.GetEmail(),
                                CreatedBy = userName,
                                UpdatedBy = userName,
                                DateCreated = DateTime.Now,
                                LastUpdate = DateTime.Now,
                                SystemType = systemType,
                                ScreenCode = screenCode,
                                ParentId = dataKeyValue,
                                DataKey = dataKey,
                                Source = DocumentSourceType.Manual,
                            };
                            await _docViewModelService.SaveDocumentFromStream(docViewModel, ms, false);
                        }
                        return Ok(_localizer["Documents merged successfully."]);
                    }
                }
            }
            return BadRequest();
        }

        #region Document Tree
        public async Task<ActionResult> GetApplicableDocTree(string documentLink, string id)
        {
            
            var documentLinkArray = documentLink.Split("|");
            var systemType = documentLinkArray[0];
            var screenCode = documentLinkArray[1];
            var dataKey = documentLinkArray[2];
            var dataKeyValue = Convert.ToInt32(documentLinkArray[3]);

            var subTree = await _docService.GetApplicableDocTree(systemType, screenCode, dataKey, dataKeyValue, id);
            return Json(subTree);
        }
        #endregion

        #region Document Tags
        public async Task<IActionResult> GetDocumentTags()
        {
            var tags = await _docService.DocDocumentTags.Select(t => t.Tag).Distinct().ToArrayAsync();
            return Json(tags);
            
        }

        public async Task<IActionResult> DocumentTagsRead([DataSourceRequest] DataSourceRequest request, int docId)
        {
            var tags = await _docService.DocDocumentTags.Where(t=> t.DocId==docId).OrderBy(t=> t.Tag).ToListAsync();
            return Json(tags.ToDataSourceResult(request));
        }

        public async Task<IActionResult> DocumentTagsUpdate(int parentId,
            [Bind(Prefix = "updated")] IEnumerable<DocDocumentTag> updated,
            [Bind(Prefix = "new")] IEnumerable<DocDocumentTag> added,
            [Bind(Prefix = "deleted")] IEnumerable<DocDocumentTag> deleted)
        {
            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                

                await _documentTagService.Update(parentId, User.GetUserName(),
                    _mapper.Map<List<DocDocumentTag>>(updated),
                    _mapper.Map<List<DocDocumentTag>>(added),
                    _mapper.Map<List<DocDocumentTag>>(deleted)
                    );
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                    _localizer["Document Tag has been saved successfully."].ToString() :
                    _localizer["Document Tags have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }
        
        public async Task<IActionResult> DocumentTagDelete([Bind(Prefix = "deleted")] DocDocumentTag deleted)
        {
            if (deleted.DocTagId >= 0)
            {
                await _documentTagService.Update(deleted.DocId, User.GetUserName(), new List<DocDocumentTag>(), new List<DocDocumentTag>(), new List<DocDocumentTag>() { deleted });
                return Ok(new { success = _localizer["Document Tag has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        
        #endregion


        protected bool VerifySystemPermission(string documentLink, DocFolder folder)
        {
            var documentLinkArray = documentLink.Split("|");
            var systemTypeCode = documentLinkArray[0];
            return systemTypeCode == folder.SystemType;
        }

        protected override async Task<IActionResult> SaveDroppedEmails(IEnumerable<IFormFile> droppedFiles, string documentLink, string folderId, string? roleLink, List<string>? responsibles)
        {
            folderId = string.IsNullOrEmpty(folderId) ? "0" : folderId;
            return await SaveDroppedFiles(droppedFiles, documentLink, int.Parse(folderId), roleLink, false, responsibles, DocumentSourceType.CPIMail);
        }               

        #region Document Verification  
        [HttpPost]
        public async Task<IActionResult> SaveSearchLink(string ids, List<DocVerificationSearchLinkViewModel> selectedRecords)
        {
            if (string.IsNullOrEmpty(ids) || !selectedRecords.Any()) return Ok();

            var emailWorkflows = new List<WorkflowEmailViewModel>();
            var hasNewRespDocketing = false;
            var hasNewRespReporting = false;

            var userName = User.GetUserName();
            var userEmail = User.GetEmail();
            var docIdList = ids.Split("|").Where(d => !string.IsNullOrEmpty(d)).Select(int.Parse)?.Where(d => d > 0).ToList();

            var newDocList = new List<DocDocument>();

            foreach (var recArr in selectedRecords)
            {
                hasNewRespDocketing = false;
                hasNewRespReporting = false;

                if (recArr.RespDocketings != null && recArr.RespDocketings.Length > 0)
                    hasNewRespDocketing = true;

                if (recArr.RespReportings != null && recArr.RespReportings.Length > 0)
                    hasNewRespReporting = true;

                var documentLink = GetDocumentLink(recArr.Link ?? "");
                if (!await _docViewModelService.CanModifyDocument(documentLink))
                    continue;

                var docFolder = await _docViewModelService.GetOrAddDefaultFolder(documentLink);

                var docList = await _docService.DocDocuments.AsNoTracking()
                                            .Where(d => docIdList != null && docIdList.Contains(d.DocId))
                                            .Select(d => new DocDocument()
                                            {
                                                FolderId = docFolder.FolderId,
                                                Author = userEmail,
                                                DocName = d.DocName,
                                                DocTypeId = d.DocTypeId,
                                                FileId = d.FileId,
                                                IsPrivate = false,
                                                IsActRequired = recArr.IsActRequired,                                               
                                                CreatedBy = userName,
                                                UpdatedBy = userName,
                                                DateCreated = DateTime.Now,
                                                LastUpdate = DateTime.Now,
                                                Source = d.Source ?? DocumentSourceType.Manual
                                            }).ToListAsync();

                if (docList.Count > 0)
                {
                    await _docService.UpdateDocuments(userName, new List<DocDocument>(), docList, new List<DocDocument>(), docFolder);
                    newDocList = docList.Select(d => new DocDocument { DocId = d.DocId, FileId = d.FileId }).ToList();
                }                    
                
                foreach (var doc in docList)
                {
                    //Save Responsible Docketing
                    if (hasNewRespDocketing)
                    {
                        await _docService.UpdateDocRespDocketing(recArr.RespDocketings == null ? new List<string>() : recArr.RespDocketings.ToList(), userName, doc.DocId);
                    }

                    //Save Responsible Reporting
                    if (hasNewRespReporting)
                    {
                        await _docService.UpdateDocRespReporting(recArr.RespReportings == null ? new List<string>() : recArr.RespReportings.ToList(), userName, doc.DocId);
                    }

                    //Prepare workflows
                    var userFileName = await _docService.DocFiles.AsNoTracking().Where(d => d.FileId == doc.FileId).Select(d => d.UserFileName).FirstOrDefaultAsync();
                    if (!string.IsNullOrEmpty(userFileName))
                    {
                        var attachments = new List<WorkflowEmailAttachmentViewModel>() {
                            //Use actual filename for locating the file (Use FileId, DocFileName could be Null)
                            //new WorkflowEmailAttachmentViewModel { DocId = doc.DocId, FileId = doc.FileId, FileName = userFileName, DocParent = docFolder.DataKeyValue }
                            new WorkflowEmailAttachmentViewModel { DocId = doc.DocId, FileId = doc.FileId, FileName = $"{doc.FileId}{Path.GetExtension(userFileName)}", DocParent = docFolder.DataKeyValue }
                        };

                        var workflowHeader = await GenerateWorkflow(documentLink, attachments, true, hasNewRespDocketing, false, hasNewRespReporting, false);
                        var emailWorkflowList = GenerateEmailWorkflow(workflowHeader, attachments, docFolder.DataKeyValue);
                        if (emailWorkflowList.Any())
                            emailWorkflows.AddRange(emailWorkflowList);
                    }
                }
            }

            //Delete old docs after linked to record(s)
            if (docIdList != null && docIdList.Count > 0)
            {
                //Update docId for MyEPO if module is on
                if (_epoMailboxSettings.IsAPIOn && (newDocList != null && newDocList.Count > 0))
                {
                    var epoDocList = await _epoCommunicationDocService.ChildService.QueryableList.Where(d => docIdList.Contains(d.DocId)).ToListAsync();
                    if (epoDocList != null && epoDocList.Count > 0)
                    {
                        var oldDocList = await _docService.DocDocuments.AsNoTracking().Where(d => docIdList.Contains(d.DocId)).Select(d => new { d.DocId, d.FileId }).ToListAsync();
                        foreach (var epoDoc in epoDocList)
                        {
                            var oldDoc = oldDocList.Where(d => d.DocId == epoDoc.DocId).FirstOrDefault();
                            if (oldDoc != null)
                            {
                                var newDoc = newDocList.Where(d => d.FileId == oldDoc.FileId).FirstOrDefault();
                                if (newDoc != null)
                                    epoDoc.DocId = newDoc.DocId;
                            }
                        }
                        await _epoCommunicationDocService.ChildService.Update(epoDocList);
                    }
                }

                foreach (var val in docIdList)
                {                    
                    var model = await _docViewModelService.CreateDocumentEditorViewModel("|||0", 0, val);
                    model.DocFolder = null;
                    model.DocFile = null;
                    model.DocVerifications = null;
                    model.DocResponsibleDocketings = null;
                    await _docService.UpdateDocuments(userName, new List<DocDocument>(), new List<DocDocument>(), new List<DocDocument>() { _mapper.Map<DocDocument>(model) });
                }
            }

            //Process AI
            var settings = await _settings.GetSetting();
            if (settings.IsDocumentUploadAIOn && newDocList != null && newDocList.Count > 0)
            {
                var newDocIdList = newDocList.Select(d => d.DocId).ToHashSet();
                var patTmkDocuments = await _docService.DocDocuments.AsNoTracking()
                    .Where(d => newDocIdList.Contains(d.DocId) && d.DocFolder != null && (d.DocFolder.DataKey == DataKey.Application || d.DocFolder.DataKey == DataKey.Trademark))
                    .Include(d => d.DocFolder).Include(d => d.DocFile).ToListAsync();

                await _documentsAIViewModelService.ProcessUploadedDocuments(patTmkDocuments);
            }

            if (emailWorkflows != null && emailWorkflows.Any())
            {
                var emailUrl = emailWorkflows.First().emailUrl;
                return Json(new { id = 0, sendEmail = true, folderId = 0, emailUrl, emailWorkflows });
            }

            return Json(new { docIds = 0 });
        }
        
        private string GetDocumentLink(string recordLink)
        {
            var documentLink = "";
                        
            if (!string.IsNullOrEmpty(recordLink))
            {
                var recordArr = recordLink.Split("/");
                var systemType = recordArr[0];
                var screenType = recordArr[1];
                var recordId = recordArr[3];

                switch (systemType)
                {
                    case SystemType.Patent:
                        {
                            documentLink = SystemTypeCode.Patent;
                            if (screenType.ToLower() == "countryapplication")
                                documentLink += "|" + ScreenCode.Application + "|AppId";
                            else if (screenType.ToLower() == "invention")
                                documentLink += "|" + ScreenCode.Invention + "|InvId";
                            break;
                        }
                    case SystemType.Trademark:
                        {
                            documentLink = SystemTypeCode.Trademark;
                            if (screenType.ToLower() == "tmktrademark")
                                documentLink += "|" + ScreenCode.Trademark + "|TmkId";
                            break;
                        }
                    case SystemType.GeneralMatter:
                        {
                            documentLink = SystemTypeCode.GeneralMatter;
                            if (screenType.ToLower() == "matter")
                                documentLink += "|" + ScreenCode.GeneralMatter + "|MatId";
                            break;
                        }
                    default:
                        break;
                }

                documentLink += "|" + recordId;
            }
            return documentLink;
        }
        
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> SaveDroppedDocVerification(IEnumerable<IFormFile> droppedFiles)
        {
            if (droppedFiles.Count() <= 0)
            {
                return BadRequest(_localizer["No Document to upload"].ToString());
            }
                        
            var emptyFolder = await _docService.DocFolders.Where(d => string.IsNullOrEmpty(d.SystemType) && string.IsNullOrEmpty(d.ScreenCode) && string.IsNullOrEmpty(d.DataKey) && d.DataKeyValue == 0).FirstOrDefaultAsync();

            if (emptyFolder == null)
            {
                emptyFolder = await _docService.AddFolder("", "", "", 0, "Documents", 0, false);
            }
            var viewModels = new List<DocDocumentViewModel>();
            foreach (var file in droppedFiles)
            {
                var viewModel = new DocDocumentViewModel();
                viewModel.ParentId = 0;
                viewModel.UploadedFile = file;
                viewModel.Author = User.GetEmail();
                viewModel.CreatedBy = User.GetUserName(); //need to pass these to add in tblDocFile
                viewModel.UpdatedBy = User.GetUserName();
                viewModel.LastUpdate = DateTime.Now;
                viewModel.DateCreated = DateTime.Now;
                viewModel.UserFileName = file.FileName;
                viewModel.FolderId = emptyFolder.FolderId;
                viewModel.DocumentLink = "|||0";
                viewModel.DocFolder = emptyFolder;
                viewModel.Source = DocumentSourceType.Manual;

                viewModels.Add(viewModel);
            }

            await _docViewModelService.SaveUploadedDocuments(viewModels);

            return Json(new
            {
                folderId = 0
            });
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DVDeleteDocuments(List<int> ids)
        {
            var deleteList = await _docService.DocDocuments.Where(d => ids.Contains(d.DocId)).ToListAsync();

            if (deleteList.Any())
                await _docService.UpdateDocuments(User.GetUserName(), new List<DocDocument>(), new List<DocDocument>(), deleteList);

            return Ok(new { success = _localizer["Document(s) has been deleted successfully."].ToString() });
        }
                
        public async Task<IActionResult> NewDocUpdate([DataSourceRequest] DataSourceRequest request, int id)
        {
            var folder = new DocFolder();

            if (id > 0)
            {
                folder = await _docService.DocFolders.Where(d => d.DocDocuments != null && d.DocDocuments.Any(dc => dc.DocId == id)).FirstOrDefaultAsync();
            }
            else
            {
                folder = await _docService.DocFolders.Where(d => string.IsNullOrEmpty(d.SystemType) && string.IsNullOrEmpty(d.ScreenCode) && string.IsNullOrEmpty(d.DataKey) && d.DataKeyValue == 0).FirstOrDefaultAsync();
            }

            if (folder == null)
            {
                folder = await _docService.AddFolder("", "", "", 0, "Documents", 0, false);
            }

            var documentLink = folder.SystemType + "|" + folder.ScreenCode + "|" + folder.DataKey + "|" + folder.DataKeyValue.ToString();
            var model = await _docViewModelService.CreateDocumentEditorViewModel(documentLink, folder.FolderId, id);

            var lockedBy = await _docService.IsLocked(id, User.GetUserName());
            if (!string.IsNullOrEmpty(lockedBy))
            {
                return BadRequest(_localizer["Document is currently checked out by: "] + lockedBy);
            }

            model.SaveAction = "SaveVerificationNewDoc";
            model.DocumentLink = documentLink;

            ViewData["IsNewDocVerification"] = documentLink == "|||0";
            ViewData["DocumentLink"] = documentLink;
            ViewData["ActivePage"] = "";

            if (model.DocId == 0) model.Author = User.GetEmail();

            //model.DocViewer = docViewer; 

            model.ViewFilePath = _documentStorageService.GetFilePath(folder.SystemType ?? "", model.DocFileName ?? "", R10.Web.Helpers.ImageHelper.CPiSavedFileType.DocMgt);
            model.ViewFileType = R10.Web.Helpers.ImageHelper.CPiSavedFileType.DocMgt;

            return PartialView(id > 0 ? ImageEditorZoom : ImageEditor, model);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveVerificationNewDoc(DocDocumentViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });
                        
            UpdateEntityStamps(viewModel, viewModel.DocId);
            await _docViewModelService.SaveDocumentPopup(viewModel, _hostingEnvironment.ContentRootPath);

            //Responsible Docketing
            var respDocketing = await _docViewModelService.SaveRespDocketing(viewModel, User.GetUserName());
            var hasNewRespDocketing = respDocketing.IsNew;
            var hasRespDocketingReassigned = respDocketing.IsReassigned;

            //Responsible Reporting
            var respReporting = await _docViewModelService.SaveRespReporting(viewModel, User.GetUserName());
            var hasNewRespReporting = respReporting.IsNew;
            var hasRespReportingReassigned = respReporting.IsReassigned;

            var hasDocVerificationEmailWorkflows = await ProcessDocVerificationNewActWorkflow(viewModel.DocId);

            if (!string.IsNullOrEmpty(viewModel.UserFileName) || hasNewRespDocketing || hasRespDocketingReassigned || hasNewRespReporting || hasRespReportingReassigned || (hasDocVerificationEmailWorkflows != null && hasDocVerificationEmailWorkflows.Count > 0))
            {
                var isNewFileUpload = false;
                var attachments = new List<WorkflowEmailAttachmentViewModel>();
                //Get UserFileName if hasNewResponsible/hasResponsibleReassigned is true
                if (string.IsNullOrEmpty(viewModel.UserFileName))
                {
                    var userFileName = await _docService.DocFiles.AsNoTracking().Where(d => d.FileId == viewModel.FileId).Select(d => d.UserFileName).FirstOrDefaultAsync();
                    if (!string.IsNullOrEmpty(userFileName))
                        //Use actual filename for locating the file (Use FileId, DocFileName could be Null)
                        //attachments.Add(new WorkflowEmailAttachmentViewModel { DocId = viewModel.DocId, FileId = viewModel.FileId, OrigFileName = userFileName, FileName = userFileName, DocParent = viewModel.ParentId });
                        attachments.Add(new WorkflowEmailAttachmentViewModel { DocId = viewModel.DocId, FileId = viewModel.FileId, OrigFileName = userFileName, FileName = $"{viewModel.FileId}{Path.GetExtension(userFileName)}", DocParent = viewModel.ParentId });
                }
                else
                {
                    isNewFileUpload = true;
                    //Use actual filename for locating the file (Use FileId, DocFileName could be Null)
                    //attachments.Add(new WorkflowEmailAttachmentViewModel { DocId = viewModel.DocId, FileId = viewModel.FileId, FileName = viewModel.UserFileName, DocParent = viewModel.ParentId });
                    attachments.Add(new WorkflowEmailAttachmentViewModel { DocId = viewModel.DocId, FileId = viewModel.FileId, FileName = $"{viewModel.FileId}{Path.GetExtension(viewModel.UserFileName)}", DocParent = viewModel.ParentId });
                }

                var workflowHeader = await GenerateWorkflow(viewModel.DocumentLink, attachments, isNewFileUpload, hasNewRespDocketing, hasRespDocketingReassigned, hasNewRespReporting, hasRespReportingReassigned);
                var eSignatureWorkflows = isNewFileUpload ? await GenerateSignatureWorkflow(workflowHeader, viewModel.DocumentLink, attachments, viewModel.ParentId, viewModel.RoleLink) : new List<WorkflowSignatureViewModel>();
                var emailWorkflows = GenerateEmailWorkflow(workflowHeader, attachments, viewModel.ParentId);

                if (hasDocVerificationEmailWorkflows != null && hasDocVerificationEmailWorkflows.Count > 0) emailWorkflows.AddRange(hasDocVerificationEmailWorkflows);

                if (emailWorkflows.Any() || eSignatureWorkflows != null)
                {
                    var emailUrl = "";
                    if (emailWorkflows != null && emailWorkflows.Any())
                        emailUrl = emailWorkflows.First().emailUrl;

                    return Json(new { id = viewModel.ParentId, sendEmail = true, folderId = viewModel.FolderId, emailUrl, emailWorkflows, eSignatureWorkflows });
                }
            }

            return Json(new { folderId = 0 });
        }         
        #endregion       
    }
}
