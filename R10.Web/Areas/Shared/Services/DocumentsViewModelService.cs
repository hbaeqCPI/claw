using ActiveQueryBuilder.View.DatabaseSchemaView;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Security;
using R10.Web.Services.DocumentStorage;
using R10.Web.ViewComponents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static R10.Web.Helpers.ImageHelper;

namespace R10.Web.Areas.Shared.Services
{
    public class DocumentsViewModelService : IDocumentsViewModelService
    {
        private readonly IDocumentService _docService;
        private ISystemSettings<DefaultSetting> _settings;
        private ISystemSettings<PatSetting> _patSettings;
        private ISystemSettings<TmkSetting> _tmkSettings;
        private readonly ICountryApplicationService _applicationService;
        private readonly IActionDueService<PatActionDue, PatDueDate> _patActionDueService;
        private readonly IPatActionDueViewModelService _patActionDueViewModelService;
        private readonly IActionDueService<TmkActionDue, TmkDueDate> _tmkActionDueService;
        private readonly ITmkActionDueViewModelService _tmkActionDueViewModelService;
        private readonly ITmkTrademarkService _trademarkService;
        private readonly IDocumentHelper _documentHelper;
        private readonly IMapper _mapper;
        private readonly IWorkflowViewModelService _workflowViewModelService;
        private readonly ClaimsPrincipal _user;
        private readonly ICPiUserGroupManager _groupManager;
        private readonly UserManager<CPiUser> _userManager;
        private readonly IDocumentsAIViewModelService _documentsAIViewModelService;
        private readonly IAuthorizationService _authService;
        private readonly IDocumentStorage _documentStorage;
        private readonly IUrlHelper _url;

        private const string defaultDocTypeName = "Image";

        //add for deduplicate doc name --Yin
        private const string docNameSeparator = "--";


        public DocumentsViewModelService(
                    IDocumentService docService,
                    ISystemSettings<DefaultSetting> settings,
                    ISystemSettings<PatSetting> patSettings,
                    ISystemSettings<TmkSetting> tmkSettings,
                    ICountryApplicationService applicationService,
                    IActionDueService<PatActionDue, PatDueDate> patActionDueService,
                    IPatActionDueViewModelService patActionDueViewModelService,
                    IActionDueService<TmkActionDue, TmkDueDate> tmkActionDueService,
                    ITmkActionDueViewModelService tmkActionDueViewModelService,
                    ITmkTrademarkService trademarkService,
                    IDocumentHelper documentHelper,
                    IMapper mapper,
                    IWorkflowViewModelService workflowViewModelService,
                    ClaimsPrincipal user,
                    ICPiUserGroupManager groupManager,
                    UserManager<CPiUser> userManager, IDocumentsAIViewModelService documentsAIViewModelService,
                    IAuthorizationService authService,
                    IDocumentStorage documentStorage,
                    IUrlHelper url)
        {
            _docService = docService;
            _settings = settings;
            _patSettings = patSettings;
            _tmkSettings = tmkSettings;
            _patActionDueService = patActionDueService;
            _patActionDueViewModelService = patActionDueViewModelService;
            _tmkActionDueService = tmkActionDueService;
            _tmkActionDueViewModelService = tmkActionDueViewModelService;
            _applicationService = applicationService;
            _trademarkService = trademarkService;

            _documentHelper = documentHelper;
            _mapper = mapper;
            _workflowViewModelService = workflowViewModelService;

            _user = user;
            _groupManager = groupManager;
            _userManager = userManager;
            _documentsAIViewModelService = documentsAIViewModelService;
            _authService = authService;

            _documentStorage = documentStorage;

            _url = url;
        }

        public async Task<DocDocumentViewModel> CreateDocumentEditorViewModel(string documentLink, int folderId, int docId)
        {
            var model = new DocDocumentViewModel();
            if (docId > 0)
            {
                model = await _docService.DocDocuments.ProjectTo<DocDocumentViewModel>().FirstOrDefaultAsync(d => d.DocId == docId);
                if (model !=null) {
                    if (!string.IsNullOrEmpty(model.DocFileName))
                    {
                        var settings = await _settings.GetSetting();
                        var viewableExts = settings.ViewableDocs.Split("|");
                        model.IsDocViewable = viewableExts.Any(x => model.DocFileName.ToLower().EndsWith(x));
                    }
                    if (model.ThumbFileName == null && !string.IsNullOrEmpty(model.DocUrl))
                        model.ThumbFileName = "logo_url.png";

                    model.IsDocLinkable = !string.IsNullOrEmpty(model.DocUrl);

                    //build documentLink from the retrieved data
                    if (model.DocFolder != null)
                    {
                        documentLink = $"{model.DocFolder.SystemType}|{model.DocFolder.ScreenCode}|{model.DocFolder.DataKey}|{model.DocFolder.DataKeyValue}";
                    }

                    model.DefaultRespDocketings = await _docService.GetDocRespDocketingList(model.DocId);
                    model.DefaultRespReportings = await _docService.GetDocRespReportingList(model.DocId);
                }
            }
            else
            {
                if (folderId==0)
                   folderId = (await GetOrAddDefaultFolder(documentLink)).FolderId;

                var defaultDocType = await _docService.DocTypes.Where(d => d.DocTypeName == defaultDocTypeName).FirstOrDefaultAsync();
                if (defaultDocType != null)
                {
                    model.DocTypeId = defaultDocType.DocTypeId;
                    model.DocTypeName = defaultDocType.DocTypeName;
                }
                model.FolderId = folderId;
            }
            model.HasDefault = HasDefault(documentLink);
            model.ParentId = GetDataKeyValue(documentLink);
            return model;
        }

        public async Task<DefaultImageViewModel> GetDefaultImage(string system, string screenCode, string systemType, string dataKey, int dataKeyValue)
        {
            //folderId is needed when uploading new default image
            var docFolder = await _docService.DocFolders.FirstOrDefaultAsync(f => f.SystemType == systemType && f.DataKey == dataKey && f.DataKeyValue == dataKeyValue);
            if (docFolder == null)
                return new DefaultImageViewModel();

            //isImage checking is done in the view using ImageHelper.IsImageFile()
            var defaultImage = await _docService.DocDocuments.Where(d => d.FolderId == docFolder.FolderId && d.IsDefault)
                   .Select(d => new DefaultImageViewModel()
                   {
                       ImageId = d.DocId,
                       ImageFile = d.DocFile == null ? null : d.DocFile.DocFileName,
                       ImageTitle = d.DocName ?? "",
                       ImageTypeName = d.DocType != null ? d.DocType.DocTypeName : "",
                       ThumbnailFile = d.DocFile == null ? null : d.DocFile.ThumbFileName,
                       IsPublic = !d.IsPrivate,
                       System = system,
                       ScreenCode = screenCode,
                       Key = dataKeyValue,
                       FolderId = d.FolderId,
                       DriveItemId = d.DocFile == null ? null : d.DocFile.DriveItemId,
                       RootContainerId = d.DocFolder == null ? null : d.DocFolder.StorageRootContainerId,
                       DefaultFolderId = d.DocFolder == null ? null : d.DocFolder.StorageDefaultFolderId
                   }).FirstOrDefaultAsync();

            //return folder info if default image is not found
            return defaultImage ?? new DefaultImageViewModel()
            {
                System = system,
                ScreenCode = screenCode,
                Key = dataKeyValue,
                FolderId = docFolder.FolderId,
                RootContainerId = docFolder.StorageRootContainerId,
                DefaultFolderId = docFolder.StorageDefaultFolderId,
                IsPublic = true
            };
        }

        public async Task DeleteDocumentsByDriveItemId(string? driveItemId)
        {
            await _docService.DeleteDocumentsByDriveItemId(driveItemId);
        }

        public async Task DeleteDocumentsByFolderId(int folderId)
        {
            await _docService.DeleteDocumentsByFolderId(folderId);
        }

        public async Task<DocFile> AddDocFile(DocDocumentViewModel viewModel, string fileName, int fileSize, bool isImage)
        {
            var docFile = new DocFile
            {
                FileExt = Path.GetExtension(fileName).Replace(".", ""),
                UserFileName = fileName, //original casing
                FileSize = fileSize,
                IsImage = isImage,
                CreatedBy = viewModel.UpdatedBy,
                DateCreated = viewModel.LastUpdate,
                UpdatedBy = viewModel.UpdatedBy,
                LastUpdate = viewModel.LastUpdate,
                ForSignature = viewModel.ForSignature,
                SignedDoc = viewModel.SignedDoc,
                DriveItemId = viewModel.DriveItemId
            };

            await _docService.AddDocFile(docFile);
            viewModel.FileId = docFile.FileId;
            return docFile;
        }

        public async Task UpdateDocFile(DocDocumentViewModel viewModel)
        {
            var docFile = await _docService.DocFiles.AsNoTracking().Where(f => f.FileId == viewModel.FileId).FirstOrDefaultAsync();
            if (docFile != null)
            {
                docFile.FileExt = Path.GetExtension(viewModel.UserFileName ?? "").Replace(".", "");
                docFile.UserFileName = viewModel.UserFileName ?? "";
                docFile.FileSize = viewModel.FileSize ?? 0;
                docFile.IsImage = viewModel.IsImage ?? false;

                await _docService.UpdateDocFile(docFile);
            }
        }

        public async Task RenameDocFile(int fileId, string name)
        {
            var docFile = await _docService.DocFiles.AsNoTracking().Where(f => f.FileId == fileId).FirstOrDefaultAsync();
            if (docFile != null)
            {
                docFile.UserFileName = $"{name}.{docFile.FileExt}";
                await _docService.UpdateDocFile(docFile);
            }
        }

        public async Task<DocFile?> GetDocFileByDocFileName(string docFileName)
        {
            return await _docService.DocFiles.AsNoTracking().Where(f => f.DocFileName == docFileName).FirstOrDefaultAsync();
        }

        public async Task<DocFile?> GetDocFileById(int fileId)
        {
            return await _docService.DocFiles.AsNoTracking().Where(f => f.FileId == fileId).FirstOrDefaultAsync();
        }

        public async Task<DocFile?> GetDocFileByIdAndFileName(int fileId, string fileName) {
            return await _docService.DocFiles.AsNoTracking().Where(f => f.FileId == fileId && f.UserFileName==fileName).FirstOrDefaultAsync();
        }

        public async Task<DocFile?> GetDocFileByDriveItemId(string driveItemId)
        {
            return await _docService.DocFiles.AsNoTracking().Where(f => f.DriveItemId == driveItemId).FirstOrDefaultAsync();
        }

        public async Task<DocDocument> SaveDocument(DocDocumentViewModel viewModel)
        {
            var patSettings = await _patSettings.GetSetting();
            var tmkSettings = await _tmkSettings.GetSetting();
            var settings = await _settings.GetSetting();

            // save/update document
            var document = _mapper.Map<DocDocumentViewModel, DocDocument>(viewModel);

            // move document to "Dockets for Verification" folder if CheckAct is checked and DocVerification module is on
            // only apply to files from CtryApplication/Trademark and when CheckAct value changes
            // if CheckDocket is checked move to "Dockets for Verification" folder
            // else move to default folder
            if (settings.DocumentStorage == DocumentStorageOptions.BlobOrFileSystem)
            {
                var documentLinkArray = viewModel.DocumentLink.Split("|"); //"P|Inv|InvId|[id]}"
                var systemType = documentLinkArray[0];
                var screenCode = documentLinkArray[1];
                var key = documentLinkArray[2];
                var value = int.Parse(documentLinkArray[3] ?? "0");
                if ((patSettings.IsDocumentVerificationOn && systemType.ToLower() == SystemTypeCode.Patent.ToLower() && screenCode.ToLower() == ScreenCode.Application.ToLower())
                    || (tmkSettings.IsDocumentVerificationOn && systemType.ToLower() == SystemTypeCode.Trademark.ToLower() && screenCode.ToLower() == ScreenCode.Trademark.ToLower()))
                {
                    var docVerificationFolderName = string.Empty;
                    switch (systemType)
                    {
                        case SystemTypeCode.Patent:
                            docVerificationFolderName = patSettings.DocVerificationDefaultFolderName;
                            break;
                        case SystemTypeCode.Trademark:
                            docVerificationFolderName = tmkSettings.DocVerificationDefaultFolderName;
                            break;
                    }

                    if (string.IsNullOrEmpty(docVerificationFolderName)) docVerificationFolderName = "Dockets for Verification";

                    var defaultFolder = await GetOrAddDefaultFolder(viewModel.DocumentLink);
                    var docVerificationFolder = await _docService.DocFolders.FirstOrDefaultAsync(f => f.SystemType == systemType && f.DataKey == key && f.DataKeyValue == value && !string.IsNullOrEmpty(f.FolderName) && f.FolderName.ToLower() == docVerificationFolderName.ToLower());
                
                    var currentCheckAct = await _docService.DocDocuments.AsNoTracking().Where(d => d.DocId == document.DocId).Select(d => d.CheckAct).FirstOrDefaultAsync();

                    if (document.CheckAct && ((document.DocId <= 0) || (document.DocId > 0 && currentCheckAct != document.CheckAct)))
                    {
                        if (docVerificationFolder == null)
                            docVerificationFolder = await _docService.AddFolder(systemType, key, screenCode, value, docVerificationFolderName, 0, false);
                                                
                        document.FolderId = docVerificationFolder.FolderId;
                    }                    
                    else if (!document.CheckAct && document.DocId > 0 && currentCheckAct != document.CheckAct && docVerificationFolder != null && document.FolderId == docVerificationFolder.FolderId)
                    {
                        document.FolderId = defaultFolder.FolderId;
                    }
                }
            }            

            if (document.DocId > 0)
            {
                if (viewModel.ReleaseFileLock)
                {
                    document.LockedBy = "";
                }
                await _docService.UpdateDocuments(viewModel.UpdatedBy, new List<DocDocument>() { document }, new List<DocDocument>(), new List<DocDocument>());

                if (patSettings.IsDocumentVerificationOn || tmkSettings.IsDocumentVerificationOn)
                {
                    var docVerifications = await _docService.DocVerifications.Where(d => d.DocId == document.DocId).ToListAsync();
                    if (docVerifications.Any() && document.IsActRequired == false)
                    {
                        //Delete existing DocVerification records linked to DocId if IsActRequired is unchecked
                        await _docService.UpdateDocVerifications(document.DocId, viewModel.UpdatedBy, new List<DocVerification>(), new List<DocVerification>(), docVerifications);
                    }
                }
            }
            else
            {
                await _docService.UpdateDocuments(viewModel.CreatedBy, new List<DocDocument>(), new List<DocDocument>() { document }, new List<DocDocument>());
                viewModel.DocId = document.DocId;

                //verification
                if (viewModel.IsActRequired && viewModel.VerificationActionList != null && viewModel.VerificationActionList.Length > 0)
                {
                    var verifications = new List<DocVerification>();
                    foreach (var item in viewModel.VerificationActionList)
                    {
                        var keyIdArr = item.Split("|");
                        var dataKey = keyIdArr[0];
                        var dataKeyValue = keyIdArr[1];
                        var keyValue = 0;
                        var temp = int.TryParse(dataKeyValue, out keyValue);

                        var verification = new DocVerification
                        {
                            DocId = viewModel.DocId,
                            ActionTypeID = dataKey.ToLower() == "actiontypeid" ? keyValue : 0,
                            ActId = dataKey.ToLower() == "actid" ? keyValue : 0,
                            CreatedBy = viewModel.UpdatedBy,
                            UpdatedBy = viewModel.UpdatedBy,
                            DateCreated = viewModel.LastUpdate,
                            LastUpdate = viewModel.LastUpdate
                        };

                        verifications.Add(verification);
                    }
                    await _docService.AddDocVerifications(verifications);
                }
            }
                        
            if (settings.IsDocumentUploadAIOn && (document.DocFolder.DataKey == DataKey.Application || document.DocFolder.DataKey == DataKey.Trademark))
            {
                await _documentsAIViewModelService.ProcessUploadedDocuments(new List<DocDocument> { document });
            }

            return document;
        }

        public async Task UpdateDocResponsible(List<string> responsibleList, string userName, int docId)
        {
            await _docService.UpdateDocRespDocketing(responsibleList, userName, docId);
        }

        public async Task<(bool IsNew, bool IsReassigned)> SaveRespDocketing(DocDocumentViewModel viewModel, string userName)
        {            
            var hasNewRespDocketing = false;
            var hasRespDocketingReassigned = false;
            var currentRespDocketings = viewModel.RespDocketings?.ToList();
            var oldRespDocketings = await _docService.GetDocRespDocketingList(viewModel.DocId);

            //If already has responsible
            //Check if there are new responsibles
            //Check if there are responsibles removed
            if (oldRespDocketings != null && oldRespDocketings.Count > 0)
            {
                if (currentRespDocketings != null && currentRespDocketings.Count > 0)
                {
                    //New responsibles not in existing responsibles (added)
                    if (currentRespDocketings.Any(c => !oldRespDocketings.Contains(c)))
                        hasNewRespDocketing = true;
                    //Existing responsibles not in new responsibles (deleted/reassigned)
                    if (oldRespDocketings.Any(c => !currentRespDocketings.Contains(c)))
                        hasRespDocketingReassigned = true;
                }
                //All responsibles got deleted
                else
                {
                    hasRespDocketingReassigned = true;
                }
            }
            //Brand new responsibles
            else
            {
                if (currentRespDocketings != null && currentRespDocketings.Count > 0)
                    hasNewRespDocketing = true;
            }

            //Save resp-docketings
            if (viewModel.RespDocketings != null && viewModel.RespDocketings.Length > 0)
            {
                await _docService.UpdateDocRespDocketing(viewModel.RespDocketings.ToList(), userName, viewModel.DocId);
            }
            else
            {
                await _docService.DeleteDocRespDocketing(viewModel.DocId);
            }

            return (hasNewRespDocketing, hasRespDocketingReassigned);
        }

        public async Task<(bool IsNew, bool IsReassigned)> SaveRespReporting(DocDocumentViewModel viewModel, string userName)
        {            
            var hasNewRespReporting = false;
            var hasRespReportingReassigned = false;
            var currentRespReportings = viewModel.RespReportings?.ToList();
            var oldRespReporting = await _docService.GetDocRespReportingList(viewModel.DocId);

            //If already has resp-reporting
            //Check if there are new resp-reportings
            //Check if there are resp-reportings removed
            if (oldRespReporting != null && oldRespReporting.Count > 0)
            {
                if (currentRespReportings != null && currentRespReportings.Count > 0)
                {
                    //New resp-repotings not in existing resp-reportings (added)
                    if (currentRespReportings.Any(c => !oldRespReporting.Contains(c)))
                        hasNewRespReporting = true;
                    //Existing resp-reportings not in new resp-reportings (deleted/reassigned)
                    if (oldRespReporting.Any(c => !currentRespReportings.Contains(c)))
                        hasRespReportingReassigned = true;
                }
                //All resp-reportings got deleted
                else
                {
                    hasRespReportingReassigned = true;
                }
            }
            //Brand new resp-reportings
            else
            {
                if (currentRespReportings != null && currentRespReportings.Count > 0)
                    hasNewRespReporting = true;
            }

            //Save resp-reportings
            if (viewModel.RespReportings != null && viewModel.RespReportings.Length > 0)
            {
                await _docService.UpdateDocRespReporting(viewModel.RespReportings.ToList(), userName, viewModel.DocId);
            }
            else
            {
                await _docService.DeleteDocRespReporting(viewModel.DocId);
            }

            return (hasNewRespReporting, hasRespReportingReassigned);
        }

        public async Task<bool> SaveDocumentPopup(DocDocumentViewModel viewModel, string rootPath)
        {
            var patSettings = await _patSettings.GetSetting();
            var tmkSettings = await _tmkSettings.GetSetting();

            // save any uploaded file
            if (viewModel.UploadedFiles != null)
            {
                var uploadedFile = viewModel.UploadedFiles.First();
                var fileName = uploadedFile.FileName.ToLower();
                //viewModel.DocTypeId = await _docService.GetDocTypeIdFromFileName(fileName);       // this is entered on screen
                var newFile = await AddDocFile(viewModel, uploadedFile.FileName, (int)uploadedFile.Length, uploadedFile.ContentType.Contains("image"));

                // get folder header for systemtype, screencode, etc.
                var folderHeader = await _docService.GetFolderHeader(viewModel.FolderId);

                if (await _documentHelper.SaveDocumentFileUpload(uploadedFile, newFile.DocFileName, newFile.ThumbFileName, folderHeader))
                {
                    //save document info to db
                    viewModel.FileId = newFile.FileId;
                    viewModel.UserFileName = fileName;

                    //reset source since user manually upload doc
                    viewModel.Source = DocumentSourceType.Manual;                    
                }
            }

            // save/update document
            await SaveDocument(viewModel);

            return true;
        }

        public async Task<bool> SaveUploadedDocuments(List<DocDocumentViewModel> viewModels)
        {
            var folderHeader = new DocFolderHeader();
            var folder = new DocFolder();

            if (viewModels.Any())
            {
                var viewModel = viewModels.First();
                folder = viewModel.DocFolder;

                folderHeader = new DocFolderHeader { SystemType = folder.SystemType, ScreenCode = folder.ScreenCode, ParentId = folder.DataKeyValue };
            }

            foreach (DocDocumentViewModel viewModel in viewModels)
            {
                var uploadedFile = viewModel.UploadedFile;
                var fileName = uploadedFile.FileName.ToLower(); 
                var newFile = await AddDocFile(viewModel, uploadedFile.FileName, (int)uploadedFile.Length, uploadedFile.ContentType.Contains("image"));

                if (await _documentHelper.SaveDocumentFileUpload(uploadedFile, newFile.DocFileName, newFile.ThumbFileName, folderHeader))
                {
                    //save document info to db
                    viewModel.FileId = newFile.FileId;
                }
            }

            List<DocDocument> documents = new List<DocDocument>();
            Dictionary<string, int> docNameDict = await GetDocNameListByFolderId(viewModels[0].FolderId);
            foreach (DocDocumentViewModel viewModel in viewModels)
            {
                string tempDocName = Path.GetFileNameWithoutExtension(viewModel.UserFileName);
                viewModel.DocName = UpdateDictAndGetName(ref docNameDict, tempDocName);
                viewModel.DocTypeId = await _docService.GetDocTypeIdFromFileName(viewModel.UserFileName);
                var document = _mapper.Map<DocDocumentViewModel, DocDocument>(viewModel);
                documents.Add(document);
            }
            await _docService.UpdateDocuments(viewModels.First().CreatedBy, new List<DocDocument>(), documents, new List<DocDocument>(), folder);
            foreach (var doc in documents) {
                var vm = viewModels.FirstOrDefault(vm => vm.DocName == doc.DocName && vm.DocTypeId == doc.DocTypeId);
                if (vm != null)
                    vm.DocId = doc.DocId;
            }

            var settings = await _settings.GetSetting();
            if (settings.IsDocumentUploadAIOn && folder.DataKey !=null && folder.DataKey==DataKey.Application || folder.DataKey == DataKey.Trademark)
            {
                await _documentsAIViewModelService.ProcessUploadedDocuments(documents);
            }
            return true;

        }

        public string GetDocumentBasePath() {
            return _documentHelper.GetDocumentBasePath();
        }

        public async Task<DocFolder> GetOrAddDefaultFolder(string documentLink)
        {
            var documentLinkArray = documentLink.Split("|");
            var systemType = documentLinkArray[0];
            var screenCode = documentLinkArray[1];
            var dataKey = documentLinkArray[2];
            var dataKeyValue = Convert.ToInt32(documentLinkArray[3]);

            var folder = await _docService.GetFolder(systemType, dataKey, dataKeyValue, "Documents", 0);

            if (folder == null) //try without parent id to avoid adding duplicate record
                folder = await _docService.DocFolders.FirstOrDefaultAsync(f => f.SystemType == systemType && f.DataKey == dataKey && f.DataKeyValue == dataKeyValue && f.FolderName == "Documents");

            if (folder == null)
                return await _docService.AddFolder(systemType, dataKey, screenCode, dataKeyValue, "Documents", 0, true);

            return folder;
        }

        public async Task<string> GenerateFolderName(string documentLink)
        {
            return await _docService.GenerateFolderName(documentLink);
        }

        public async Task<(string? ClientCode, string? ClientName, string? MatterNumber)> GetClientMatter(string documentLink)
        {
            return await _docService.GetClientMatter(documentLink);
        }

        public async Task<string> GetRootDocumentLink(string documentLink)
        {
            var parentDocLink = await _docService.GetParentDocumentLink(documentLink);
            if (string.IsNullOrEmpty(parentDocLink))
                return documentLink;

            return await GetRootDocumentLink(parentDocLink);
        }

        /// <summary>
        /// Get DocFolder or parent record's DocFolder
        /// </summary>
        /// <param name="documentLink"></param>
        /// <returns></returns>
        public async Task<DocFolder?> GetDefaultFolderByDocumentLink(string documentLink)
        {
            var folder = await GetFolderByDocumentLink(documentLink);
            if (folder == null || string.IsNullOrEmpty(folder.StorageRootContainerId))
            {
                var parentDocLink = await _docService.GetParentDocumentLink(documentLink);
                if (!string.IsNullOrEmpty(parentDocLink))
                    return  await GetDefaultFolderByDocumentLink(parentDocLink);
            }

            return folder;
        }

        public async Task<DocFolder?> GetFolderByDocumentLink(string documentLink)
        {
            var documentLinkArray = documentLink.Split("|"); //"P|Inv|InvId|[id]}"
            var systemType = documentLinkArray[0];
            var key = documentLinkArray[2];
            var value = int.Parse(documentLinkArray[3] ?? "0");
            return await _docService.DocFolders.FirstOrDefaultAsync(f => f.SystemType == systemType && f.DataKey == key && f.DataKeyValue == value);
        }

        public async Task<DocDocument?> GetDocumentByDriveItemId(string documentId)
        {
            return await _docService.DocDocuments.Include(d => d.DocFolder).FirstOrDefaultAsync(d => d.DocFile != null && d.DocFile.DriveItemId == documentId);
        }

        public async Task<int> GetDocTypeIdFromFileName(string fileName)
        {
            return await _docService.GetDocTypeIdFromFileName(fileName);
        }

        public async Task SaveFolderStorageSetting(DocFolder folder)
        {
            await _docService.SaveFolderStorageSetting(folder);
        }

        public async Task<List<DocDocumentListViewModel>> ApplyCriteria(List<DocDocumentListViewModel> documents, List<QueryFilterViewModel> criteria) {

            var folder = criteria.FirstOrDefault(c => c.Property == "FolderId");
            var document = criteria.FirstOrDefault(c => c.Property == "DocId");
            var docName = criteria.FirstOrDefault(c => c.Property == "DocName");
            var docFileName = criteria.FirstOrDefault(c => c.Property == "DocFileName");
            var tag = criteria.FirstOrDefault(c => c.Property == "Tag");
            var userFileName = criteria.FirstOrDefault(c => c.Property == "UserFileName");

            if (docName != null)
            {
                documents = documents.Where(i => i.DocName.ToLower().Contains(docName.Value.ToLower().Replace("%", ""))).ToList();
                criteria.Remove(docName);
            }

            if (docFileName != null)
            {
                documents = documents.Where(i => i.DocFileName.ToLower().Contains(docFileName.Value.ToLower().Replace("%", ""))).ToList();
                criteria.Remove(docFileName);
            }

            if (userFileName != null)
            {
                documents = documents.Where(i => i.UserFileName !=null && i.UserFileName.ToLower().Contains(userFileName.Value.ToLower().Replace("%", ""))).ToList();
                criteria.Remove(userFileName);
            }

            if (tag != null)
            {
                documents = documents.Where(i => i.Tags.Any(t => t.ToLower().Contains(tag.Value.ToLower().Replace("%", "")))).ToList();
                criteria.Remove(tag);
            }

            if (document != null)
            {
                if (document.Value != "0")
                {
                    var docId = Convert.ToInt32(document.Value);
                    documents = documents.Where(i => i.DocId == docId).ToList();
                    if (folder != null)
                    {
                        criteria.Remove(folder);
                        folder = null;
                    }
                }
                criteria.Remove(document);
            }

            if (folder != null)
            {
                if (folder.Value != "0")
                {
                    var parentFolderId = Convert.ToInt32(folder.Value);
                    var folderIds = await _docService.GetFolderIds(parentFolderId);
                    if (folderIds.Any())
                        documents = documents.Where(i => folderIds.Any(f => f == i.FolderId)).ToList();
                }
                criteria.Remove(folder);
            }

            criteria.ForEach(c => {
                if (!(c.Property.StartsWith("DateCreated") || c.Property == "FolderId" || c.Property == "DocId" || c.Property == "ParentId"))
                {
                    c.Operator = "contains";
                    c.Value = c.Value.Replace("%", "");
                }
            });
            return documents;
        }


        public async Task<bool> SaveDocumentFromStream(DocDocumentViewModel viewModel, MemoryStream stream, bool updateParentStamp = true)
        {
            var fileName = viewModel.DocFileName ?? "";
            var newFile = await AddDocFile(viewModel, fileName, 0, ImageHelper.IsImageFile(fileName));

            var documentLink = $"{viewModel.SystemType}|{viewModel.ScreenCode}|{viewModel.DataKey}|{viewModel.ParentId}";
            var folder = await GetOrAddDefaultFolder(documentLink);
            var folderHeader = new DocFolderHeader { SystemType = viewModel.SystemType, ScreenCode = viewModel.ScreenCode, ParentId = viewModel.ParentId };

            if (await _documentHelper.SaveDocumentFromStream(stream, newFile.DocFileName, folderHeader))
            {
                viewModel.FileId = newFile.FileId;
                viewModel.FolderId = folder.FolderId;

                string tempDocName = Path.GetFileNameWithoutExtension(viewModel.DocFileName);
                viewModel.DocTypeId = await _docService.GetDocTypeIdFromFileName(viewModel.DocFileName);
                viewModel.Author = viewModel.UpdatedBy;
                viewModel.DocName = tempDocName;

                var document = _mapper.Map<DocDocumentViewModel, DocDocument>(viewModel);
                await _docService.UpdateDocuments(viewModel.CreatedBy, new List<DocDocument>(), new List<DocDocument>() { document }, new List<DocDocument>(), folder, updateParentStamp);
            }
            return true;
        }


        public async Task<List<WorkflowViewModel>> GenerateCountryAppWorkflow(List<WorkflowEmailAttachmentViewModel> attachments, int parentId, bool isNewFileUpload = false, bool hasNewRespDocketing = false, bool hasRespDocketingReassigned = false, string newRespDocketingUrl = "", string reassignedRespDocketingUrl = "", bool isNewEPOFileDownloaded = false, bool hasNewRespReporting = false, bool hasRespReportingReassigned = false, string newRespReportingUrl = "", string reassignedRespReportingUrl = "")
        {
            var settings = await _settings.GetSetting();

            var workFlows = new List<WorkflowViewModel>();
            var application = await _applicationService.CountryApplications.Where(c => c.AppId == parentId).Include(c => c.Invention).FirstOrDefaultAsync();

            //var workflowActions = await _workflowViewModelService.GetCountryApplicationWorkflowActions(application, PatWorkflowTriggerType.NewFileUploaded, false);
            var workflowActions = new List<PatWorkflowAction>();

            if (isNewFileUpload)
                workflowActions.AddRange(await _workflowViewModelService.GetCountryApplicationWorkflowActions(application, PatWorkflowTriggerType.NewFileUploaded, false));

            if (hasNewRespDocketing)
                workflowActions.AddRange(await _workflowViewModelService.GetCountryApplicationWorkflowActions(application, PatWorkflowTriggerType.DocumentRespDocketingAssigned, false));

            if (hasRespDocketingReassigned)
                workflowActions.AddRange(await _workflowViewModelService.GetCountryApplicationWorkflowActions(application, PatWorkflowTriggerType.DocumentRespDocketingReAssigned, false));

            if (isNewEPOFileDownloaded)
                workflowActions.AddRange(await _workflowViewModelService.GetCountryApplicationWorkflowActions(application, PatWorkflowTriggerType.NewEPOFileDownloaded, false));

            if (hasNewRespReporting)
                workflowActions.AddRange(await _workflowViewModelService.GetCountryApplicationWorkflowActions(application, PatWorkflowTriggerType.DocumentRespReportingAssigned, false));

            if (hasRespReportingReassigned)
                workflowActions.AddRange(await _workflowViewModelService.GetCountryApplicationWorkflowActions(application, PatWorkflowTriggerType.DocumentRespReportingReAssigned, false));

            workflowActions = workflowActions.Where(a => a.Workflow.SystemScreen == null || a.Workflow.SystemScreen.ScreenCode.ToLower() == "ca-workflow").ToList();
            workflowActions = _workflowViewModelService.ClearPatBaseWorkflowActions(workflowActions);

            if (workflowActions.Any())
            {
                //attachments.ForEach(a => a.FileName = a.FileName.ToLower());
                foreach (var item in workflowActions)
                {
                    var filteredAttachments = attachments;
                    if (!string.IsNullOrEmpty(item.Workflow.TriggerValueName))
                    {
                        filteredAttachments = attachments.Where(f => item.Workflow.TriggerValueName.ToLower().Replace("*", "").Split(',').Any(tv => f.FileName.ToLower().Contains(tv))).ToList();
                    }

                    if (filteredAttachments.Any())
                    {

                        var workFlow = new WorkflowViewModel
                        {
                            ActionTypeId = item.ActionTypeId,
                            ActionValueId = item.ActionValueId,
                            Preview = item.Preview,
                            AutoAttachImages = item.IncludeAttachments,
                            Attachments = filteredAttachments,
                            AttachmentFilter = item.AttachmentFilter
                        };

                        if (item.Workflow.TriggerTypeId == (int)PatWorkflowTriggerType.DocumentRespDocketingAssigned)
                        {
                            workFlow.EmailUrl = newRespDocketingUrl;
                            var addedResponsibleLog = new DocResponsibleLog();

                            var docDocument = new DocDocument();
                            if (settings.IsSharePointIntegrationOn)
                            {
                                docDocument = await _docService.DocDocuments.AsNoTracking().SingleOrDefaultAsync(d => d.DocFile.DriveItemId == filteredAttachments.First().Id);
                            }
                            else
                            {
                                docDocument = await _docService.GetDocumentById(filteredAttachments.First().DocId);
                            }

                            if (docDocument != null)
                                addedResponsibleLog = await _docService.GetAddedDocRespDocketing(docDocument.DocId);

                            if (addedResponsibleLog != null) 
                                workFlow.EmailTo = await GetResponsibleEmail(addedResponsibleLog.GroupIds ?? "", addedResponsibleLog.UserIds ?? "");

                        }
                        else if (item.Workflow.TriggerTypeId == (int)PatWorkflowTriggerType.DocumentRespDocketingReAssigned)
                        {                            
                            workFlow.EmailUrl = reassignedRespDocketingUrl;
                            var deletedResponsibleLog = new DocResponsibleLog();

                            var docDocument = new DocDocument();
                            if (settings.IsSharePointIntegrationOn)
                            {
                                docDocument = await _docService.DocDocuments.AsNoTracking().SingleOrDefaultAsync(d => d.DocFile.DriveItemId == filteredAttachments.First().Id);
                            }
                            else
                            {
                                docDocument = await _docService.GetDocumentById(filteredAttachments.First().DocId);
                            }

                            if (docDocument != null)
                                deletedResponsibleLog = await _docService.GetDeletedDocRespDocketing(docDocument.DocId);                                                     

                            if (deletedResponsibleLog != null)
                                workFlow.EmailTo = await GetResponsibleEmail(deletedResponsibleLog.GroupIds ?? "", deletedResponsibleLog.UserIds ?? "");
                        }
                        else if (item.Workflow.TriggerTypeId == (int)PatWorkflowTriggerType.DocumentRespReportingAssigned)
                        {
                            workFlow.EmailUrl = newRespReportingUrl;
                            var addedResponsibleLog = new DocResponsibleLog();

                            var docDocument = new DocDocument();
                            if (settings.IsSharePointIntegrationOn)
                            {
                                docDocument = await _docService.DocDocuments.AsNoTracking().SingleOrDefaultAsync(d => d.DocFile.DriveItemId == filteredAttachments.First().Id);
                            }
                            else
                            {
                                docDocument = await _docService.GetDocumentById(filteredAttachments.First().DocId);
                            }

                            if (docDocument != null)
                                addedResponsibleLog = await _docService.GetAddedDocRespReporting(docDocument.DocId);

                            if (addedResponsibleLog != null) 
                                workFlow.EmailTo = await GetResponsibleEmail(addedResponsibleLog.GroupIds ?? "", addedResponsibleLog.UserIds ?? "");

                        }
                        else if (item.Workflow.TriggerTypeId == (int)PatWorkflowTriggerType.DocumentRespReportingReAssigned)
                        {                            
                            workFlow.EmailUrl = reassignedRespReportingUrl;
                            var deletedResponsibleLog = new DocResponsibleLog();

                            var docDocument = new DocDocument();
                            if (settings.IsSharePointIntegrationOn)
                            {
                                docDocument = await _docService.DocDocuments.AsNoTracking().SingleOrDefaultAsync(d => d.DocFile.DriveItemId == filteredAttachments.First().Id);
                            }
                            else
                            {
                                docDocument = await _docService.GetDocumentById(filteredAttachments.First().DocId);
                            }

                            if (docDocument != null)
                                deletedResponsibleLog = await _docService.GetDeletedDocRespReporting(docDocument.DocId);                                                     

                            if (deletedResponsibleLog != null)
                                workFlow.EmailTo = await GetResponsibleEmail(deletedResponsibleLog.GroupIds ?? "", deletedResponsibleLog.UserIds ?? "");
                        }

                        workFlows.Add(workFlow);
                    }
                }

                _applicationService.DetachAllEntities();
                var createActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CreateAction).Distinct().ToList();
                foreach (var item in createActionWorkflows)
                {
                    await _applicationService.GenerateWorkflowAction(application.AppId, item.ActionValueId, DateTime.Now);
                }

                var closeActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CloseAction).Distinct().ToList();
                foreach (var item in closeActionWorkflows)
                {
                    var actionDuesToClose = await _applicationService.CloseWorkflowAction(application.AppId, item.ActionValueId);
                    if (actionDuesToClose.Any())
                    {
                        foreach (var actionDueToClose in actionDuesToClose)
                        {
                            await _patActionDueService.Update(actionDueToClose);
                        }
                    }
                }

                return workFlows;
            }
            return null;
        }

        public async Task<List<WorkflowViewModel>> GenerateTrademarkWorkflow(List<WorkflowEmailAttachmentViewModel> attachments, int parentId, bool isNewFileUpload = false, bool hasNewRespDocketing = false, bool hasRespDocketingReassigned = false, string newRespDocketingUrl = "", string reassignedRespDocketingUrl = "", bool hasNewRespReporting = false, bool hasRespReportingReassigned = false, string newRespReportingUrl = "", string reassignedRespReportingUrl = "")
        {
            var settings = await _settings.GetSetting();

            var workFlows = new List<WorkflowViewModel>();
            var trademark = await _trademarkService.GetByIdAsync(parentId);

            //var workflowActions = await _workflowViewModelService.GetTrademarkWorkflowActions(trademark, TmkWorkflowTriggerType.NewFileUploaded, false);
            var workflowActions = new List<TmkWorkflowAction>();

            if (isNewFileUpload)
                workflowActions.AddRange(await _workflowViewModelService.GetTrademarkWorkflowActions(trademark, TmkWorkflowTriggerType.NewFileUploaded, false));

            if (hasNewRespDocketing)
                workflowActions.AddRange(await _workflowViewModelService.GetTrademarkWorkflowActions(trademark, TmkWorkflowTriggerType.DocumentRespDocketingAssigned, false));

            if (hasRespDocketingReassigned)
                workflowActions.AddRange(await _workflowViewModelService.GetTrademarkWorkflowActions(trademark, TmkWorkflowTriggerType.DocumentRespDocketingReAssigned, false));

            if (hasNewRespReporting)
                workflowActions.AddRange(await _workflowViewModelService.GetTrademarkWorkflowActions(trademark, TmkWorkflowTriggerType.DocumentRespReportingAssigned, false));

            if (hasRespReportingReassigned)
                workflowActions.AddRange(await _workflowViewModelService.GetTrademarkWorkflowActions(trademark, TmkWorkflowTriggerType.DocumentRespReportingReAssigned, false));

            workflowActions = workflowActions.Where(a => a.Workflow.SystemScreen == null || a.Workflow.SystemScreen.ScreenCode.ToLower() == "tmk-workflow").ToList();
            workflowActions = _workflowViewModelService.ClearTmkBaseWorkflowActions(workflowActions);
            if (workflowActions.Any())
            {
                //attachments.ForEach(a => a.FileName = a.FileName.ToLower());
                foreach (var item in workflowActions)
                {
                    var filteredAttachments = attachments;
                    if (!string.IsNullOrEmpty(item.Workflow.TriggerValueName))
                    {
                        filteredAttachments = attachments.Where(f => item.Workflow.TriggerValueName.ToLower().Replace("*", "").Split(',').Any(tv => f.FileName.ToLower().Contains(tv))).ToList();
                    }

                    if (filteredAttachments.Any())
                    {

                        var workFlow = new WorkflowViewModel
                        {
                            ActionTypeId = item.ActionTypeId,
                            ActionValueId = item.ActionValueId,
                            Preview = item.Preview,
                            AutoAttachImages = item.IncludeAttachments,
                            Attachments = filteredAttachments,
                            AttachmentFilter = item.AttachmentFilter
                        };

                        if (item.Workflow.TriggerTypeId == (int)TmkWorkflowTriggerType.DocumentRespDocketingAssigned)
                        {
                            workFlow.EmailUrl = newRespDocketingUrl;
                            var addedResponsibleLog = new DocResponsibleLog();

                            var docDocument = new DocDocument();
                            if (settings.IsSharePointIntegrationOn)
                            {
                                docDocument = await _docService.DocDocuments.AsNoTracking().SingleOrDefaultAsync(d => d.DocFile.DriveItemId == filteredAttachments.First().Id);
                            }
                            else
                            {
                                docDocument = await _docService.GetDocumentById(filteredAttachments.First().DocId);
                            }

                            if (docDocument != null)
                                addedResponsibleLog = await _docService.GetAddedDocRespDocketing(docDocument.DocId);

                            if (addedResponsibleLog != null)
                                workFlow.EmailTo = await GetResponsibleEmail(addedResponsibleLog.GroupIds ?? "", addedResponsibleLog.UserIds ?? "");

                        }
                        else if (item.Workflow.TriggerTypeId == (int)TmkWorkflowTriggerType.DocumentRespDocketingReAssigned)
                        {
                            workFlow.EmailUrl = reassignedRespDocketingUrl;
                            var deletedResponsibleLog = new DocResponsibleLog();

                            var docDocument = new DocDocument();
                            if (settings.IsSharePointIntegrationOn)
                            {
                                docDocument = await _docService.DocDocuments.AsNoTracking().SingleOrDefaultAsync(d => d.DocFile.DriveItemId == filteredAttachments.First().Id);
                            }
                            else
                            {
                                docDocument = await _docService.GetDocumentById(filteredAttachments.First().DocId);
                            }

                            if (docDocument != null)
                                deletedResponsibleLog = await _docService.GetDeletedDocRespDocketing(docDocument.DocId);

                            if (deletedResponsibleLog != null)
                                workFlow.EmailTo = await GetResponsibleEmail(deletedResponsibleLog.GroupIds ?? "", deletedResponsibleLog.UserIds ?? "");
                        }
                        else if (item.Workflow.TriggerTypeId == (int)TmkWorkflowTriggerType.DocumentRespReportingAssigned)
                        {
                            workFlow.EmailUrl = newRespReportingUrl;
                            var addedResponsibleLog = new DocResponsibleLog();

                            var docDocument = new DocDocument();
                            if (settings.IsSharePointIntegrationOn)
                            {
                                docDocument = await _docService.DocDocuments.AsNoTracking().SingleOrDefaultAsync(d => d.DocFile.DriveItemId == filteredAttachments.First().Id);
                            }
                            else
                            {
                                docDocument = await _docService.GetDocumentById(filteredAttachments.First().DocId);
                            }

                            if (docDocument != null)
                                addedResponsibleLog = await _docService.GetAddedDocRespReporting(docDocument.DocId);

                            if (addedResponsibleLog != null)
                                workFlow.EmailTo = await GetResponsibleEmail(addedResponsibleLog.GroupIds ?? "", addedResponsibleLog.UserIds ?? "");

                        }
                        else if (item.Workflow.TriggerTypeId == (int)TmkWorkflowTriggerType.DocumentRespReportingReAssigned)
                        {
                            workFlow.EmailUrl = reassignedRespReportingUrl;
                            var deletedResponsibleLog = new DocResponsibleLog();

                            var docDocument = new DocDocument();
                            if (settings.IsSharePointIntegrationOn)
                            {
                                docDocument = await _docService.DocDocuments.AsNoTracking().SingleOrDefaultAsync(d => d.DocFile.DriveItemId == filteredAttachments.First().Id);
                            }
                            else
                            {
                                docDocument = await _docService.GetDocumentById(filteredAttachments.First().DocId);
                            }

                            if (docDocument != null)
                                deletedResponsibleLog = await _docService.GetDeletedDocRespReporting(docDocument.DocId);

                            if (deletedResponsibleLog != null)
                                workFlow.EmailTo = await GetResponsibleEmail(deletedResponsibleLog.GroupIds ?? "", deletedResponsibleLog.UserIds ?? "");
                        }

                        workFlows.Add(workFlow);
                    }
                }

                _trademarkService.DetachAllEntities();
                var createActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.CreateAction).Distinct().ToList();
                foreach (var item in createActionWorkflows)
                {
                    await _trademarkService.GenerateWorkflowAction(trademark.TmkId, item.ActionValueId);
                }

                var closeActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.CloseAction).Distinct().ToList();
                foreach (var item in closeActionWorkflows)
                {
                    var actionDuesToClose = await _trademarkService.CloseWorkflowAction(trademark.TmkId, item.ActionValueId);
                    if (actionDuesToClose.Any())
                    {
                        foreach (var actionDueToClose in actionDuesToClose)
                        {
                            await _tmkActionDueService.Update(actionDueToClose);
                        }
                    }
                }

                return workFlows;
            }
            return null;
        }

        // GenerateGMWorkflow method removed - GeneralMatter module deleted

        public async Task<List<WorkflowViewModel>> GeneratePatentActionWorkflow(List<WorkflowEmailAttachmentViewModel> attachments, int parentId)
        {
            var workFlows = new List<WorkflowViewModel>();
            var actionDue = await _patActionDueService.QueryableList.Where(a => a.ActId == parentId).Include(a => a.CountryApplication).ThenInclude(c => c.Invention).FirstOrDefaultAsync();
            var workflowActions = await _workflowViewModelService.GetPatActionDueWorkflowActions(actionDue, PatWorkflowTriggerType.NewFileUploaded, false);
            workflowActions = workflowActions.Where(a => a.Workflow.SystemScreen == null || a.Workflow.SystemScreen.ScreenCode.ToLower() == "act-workflow").ToList();
            workflowActions = _workflowViewModelService.ClearPatBaseWorkflowActions(workflowActions);

            if (workflowActions.Any())
            {
                attachments.ForEach(a => a.FileName = a.FileName.ToLower());
                foreach (var item in workflowActions)
                {
                    var filteredAttachments = attachments;
                    if (!string.IsNullOrEmpty(item.Workflow.TriggerValueName)) {
                        filteredAttachments = attachments.Where(f => item.Workflow.TriggerValueName.ToLower().Replace("*", "").Split(',').Any(tv => f.FileName.ToLower().Contains(tv))).ToList();
                    }

                    if (filteredAttachments.Any()) {

                        var workFlow = new WorkflowViewModel
                        {
                            ActionTypeId = item.ActionTypeId,
                            ActionValueId = item.ActionValueId,
                            Preview = item.Preview,
                            AutoAttachImages = item.IncludeAttachments,
                            Attachments = filteredAttachments,
                            AttachmentFilter = item.AttachmentFilter
                        };
                        workFlows.Add(workFlow);
                    }
                }

                _applicationService.DetachAllEntities();
                var createActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CreateAction).Distinct().ToList();
                foreach (var item in createActionWorkflows)
                {
                    await _applicationService.GenerateWorkflowAction(parentId, item.ActionValueId, DateTime.Now);
                }

                var closeActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CloseAction).Distinct().ToList();
                foreach (var item in closeActionWorkflows)
                {
                    var actionDuesToClose = await _applicationService.CloseWorkflowAction(parentId, item.ActionValueId);
                    if (actionDuesToClose.Any())
                    {
                        foreach (var actionDueToClose in actionDuesToClose)
                        {
                            await _patActionDueService.Update(actionDueToClose);
                        }
                    }
                }

                return workFlows;
            }
            return null;
        }

        public async Task<List<WorkflowViewModel>> GenerateTrademarkActionWorkflow(List<WorkflowEmailAttachmentViewModel> attachments, int parentId)
        {
            var workFlows = new List<WorkflowViewModel>();
            var actionDue = await _tmkActionDueService.QueryableList.Where(a => a.ActId == parentId).Include(t => t.TmkTrademark).FirstOrDefaultAsync();
            
            var workflowActions = await _workflowViewModelService.GetTmkActionDueWorkflowActions(actionDue, TmkWorkflowTriggerType.NewFileUploaded, false);
            workflowActions = workflowActions.Where(a => a.Workflow.SystemScreen == null || a.Workflow.SystemScreen.ScreenCode.ToLower() == "act-workflow").ToList();
            workflowActions = _workflowViewModelService.ClearTmkBaseWorkflowActions(workflowActions);
            if (workflowActions.Any())
            {
                attachments.ForEach(a => a.FileName = a.FileName.ToLower());
                foreach (var item in workflowActions)
                {
                    var filteredAttachments = attachments;
                    if (!string.IsNullOrEmpty(item.Workflow.TriggerValueName))
                    {
                        filteredAttachments = attachments.Where(f => item.Workflow.TriggerValueName.ToLower().Replace("*", "").Split(',').Any(tv => f.FileName.ToLower().Contains(tv))).ToList();
                    }

                    if (filteredAttachments.Any())
                    {
                        var workFlow = new WorkflowViewModel
                        {
                            ActionTypeId = item.ActionTypeId,
                            ActionValueId = item.ActionValueId,
                            Preview = item.Preview,
                            AutoAttachImages = item.IncludeAttachments,
                            Attachments = filteredAttachments,
                            AttachmentFilter = item.AttachmentFilter
                        };
                        workFlows.Add(workFlow);
                    }
                }

                _trademarkService.DetachAllEntities();
                var createActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.CreateAction).Distinct().ToList();
                foreach (var item in createActionWorkflows)
                {
                    await _trademarkService.GenerateWorkflowAction(parentId, item.ActionValueId);
                }

                var closeActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.CloseAction).Distinct().ToList();
                foreach (var item in closeActionWorkflows)
                {
                    var actionDuesToClose = await _trademarkService.CloseWorkflowAction(parentId, item.ActionValueId);
                    if (actionDuesToClose.Any())
                    {
                        foreach (var actionDueToClose in actionDuesToClose)
                        {
                            await _tmkActionDueService.Update(actionDueToClose);
                        }
                    }
                    
                }

                return workFlows;
            }
            return null;
        }

        // GenerateGMActionWorkflow method removed - GeneralMatter module deleted

        protected bool HasDefault(string documentLink)
        {
            var documentLinkArray = documentLink.Split("|");
            var screenCode = documentLinkArray[1];
            bool hasDefault = screenCode == ScreenCode.Invention || screenCode == ScreenCode.Application || screenCode == ScreenCode.Trademark;
            return hasDefault;
        }

        protected int GetDataKeyValue(string documentLink)
        {
            var documentLinkArray = documentLink.Split("|");
            return Convert.ToInt32(documentLinkArray[3]);
        }

        //add for doc name deduplication --Yin
        protected async Task<Dictionary<string, int>> GetDocNameListByFolderId(int folderId)
        {
            var documents = await GetDocumentsByFolderId(folderId);
            Dictionary<string, int> docNameList = new Dictionary<string, int>();
            Regex regex = new Regex(docNameSeparator + @"(\d+)$");
            foreach (var doc in documents)
            {
                string docName = doc.DocName;
                int docNo = 0;
                if (regex.Match(docName).Success)
                {
                    int lastOccurrOfSeparator = docName.LastIndexOf(docNameSeparator);
                    string newdocName = docName.Substring(0, lastOccurrOfSeparator);
                    docNo = Int32.Parse(docName.Substring(lastOccurrOfSeparator + docNameSeparator.Length));
                    if (docNameList.ContainsKey(newdocName))
                    {
                        if (docNameList[newdocName] < docNo) docNameList[newdocName] = docNo;
                    }
                    else
                    {
                        docNameList[newdocName] = 0;
                    }
                }
                else
                {
                    docNameList[docName] = (docNameList.ContainsKey(docName)) ? docNameList[docName] + 1 : 0;
                }
            }
            return docNameList;
        }

        protected string UpdateDictAndGetName(ref Dictionary<string, int> dict, string docName)
        {
            if (dict.ContainsKey(docName))
            {
                int docNo = dict[docName] + 1;
                dict[docName] = docNo;
                docName = docName + docNameSeparator + docNo;
            }
            else
            {
                dict[docName] = 0;
            }
            return docName;
        }

        protected async Task<List<DocDocumentListViewModel>> GetDocumentsByFolderId(int folderId)
        {
            var model = await _docService.DocDocuments.Where(d => d.FolderId == folderId).ProjectTo<DocDocumentListViewModel>().ToListAsync();
            var defaultFileIcon = await _docService.DocIcons.Where(i => i.FileExt == "filedefault").Select(i => i.IconClass).FirstOrDefaultAsync();
            model.Where(r => string.IsNullOrEmpty(r.IconClass)).Each(r => r.IconClass = defaultFileIcon);

            // mark viewable/linkable files
            var settings = await _settings.GetSetting();
            var viewableExts = settings.ViewableDocs.Split("|");
            model.Where(r => !string.IsNullOrEmpty(r.DocFileName)).Each(r => r.IsDocViewable = viewableExts.Any(x => r.DocFileName.ToLower().EndsWith(x)));
            model.Where(r => !string.IsNullOrEmpty(r.DocUrl)).Each(r => r.IsDocLinkable = true);

            return model;
        }

        public async Task<string> GetResponsibleEmail(string groupIds = "", string userIds = "")
        {
            var emailStr = "";
            var emailList = new List<string>();
            if (!string.IsNullOrEmpty(groupIds))
            {
                var groupIdList = groupIds.Split("|").Where(d => !string.IsNullOrEmpty(d))
                                            .Select(d =>
                                            {
                                                int intVal;
                                                bool isInt = int.TryParse(d, out intVal);
                                                return new { intVal, isInt };
                                            })
                                            .Where(d => d.isInt).Select(d => d.intVal).ToList();
                foreach (var id in groupIdList)
                {
                    var users = _groupManager.GetUsers(id);
                    if (users.Any())
                    {
                        emailList.AddRange(users.Where(d => !string.IsNullOrEmpty(d.Email)).Select(x => x.Email).ToList());
                    }
                }
                
            }
            
            if (!string.IsNullOrEmpty(userIds))
            {
                var userIdList = userIds.Split("|").Where(d => !string.IsNullOrEmpty(d)).ToList();
                foreach (var id in userIdList)
                {
                    var user = await _userManager.FindByIdAsync(id);
                    if (user != null)
                    {
                        emailList.Add(user.Email);
                    }
                }                
            }

            if (emailList.Count > 0) emailStr = string.Join(";", emailList.Distinct().ToList());
            
            return emailStr;
        }

        public async Task MarkForSignature(int fileId, string documentLink, int qeSetupId, string roleLink, bool isDMSInventorSignature = false)
        {
            await _docService.MarkForSignature(fileId, documentLink, qeSetupId, roleLink, isDMSInventorSignature);
        }


        public async Task<bool> CanModifyDocument(string documentLink)
        {
            var authorized = false;
            var respOffice = "";

            var documentLinkArray = documentLink.Split("|");
            var systemTypeCode = documentLinkArray[0];
            var screenCode = documentLinkArray[1];
            var dataKey = (documentLinkArray[2] ?? "").ToLower();
            if (documentLinkArray.Length > 4)
                respOffice = documentLinkArray[4];

            switch (systemTypeCode)
            {
                case SystemTypeCode.Patent:
                    authorized = (await _authService.AuthorizeAsync(_user, PatentAuthorizationPolicy.CanUploadDocuments)).Succeeded;
                    if (!authorized)
                    {
                        var settings = await _patSettings.GetSetting();
                        if (settings.IsDeDocketOn && screenCode == ScreenCode.Action)
                        {
                            authorized = _user.IsDeDocketer(SystemType.Patent, respOffice);
                        }

                        if (!authorized && dataKey.StartsWith("costtrack"))
                            authorized = (await _authService.AuthorizeAsync(_user, PatentAuthorizationPolicy.CostTrackingUpload)).Succeeded;
                    }
                    break;
                case SystemTypeCode.PatInvention:
                    authorized = (await _authService.AuthorizeAsync(_user, PatentAuthorizationPolicy.CanUploadDocuments)).Succeeded;
                    if (!authorized)
                    {
                        var settings = await _patSettings.GetSetting();
                        if (settings.IsDeDocketOn && screenCode == ScreenCode.Action)
                        {
                            authorized = _user.IsDeDocketer(SystemType.Patent, respOffice);
                        }

                        if (!authorized && dataKey.StartsWith("costtrack"))
                            authorized = (await _authService.AuthorizeAsync(_user, PatentAuthorizationPolicy.CostTrackingUpload)).Succeeded;
                    }
                    break;
                case SystemTypeCode.Trademark:
                    authorized = (await _authService.AuthorizeAsync(_user, TrademarkAuthorizationPolicy.CanUploadDocuments)).Succeeded;
                    if (!authorized)
                    {
                        var settings = await _tmkSettings.GetSetting();
                        if (settings.IsDeDocketOn && screenCode == ScreenCode.Action)
                        {
                            authorized = _user.IsDeDocketer(SystemType.Trademark, respOffice);
                        }

                        if (!authorized && dataKey.StartsWith("costtrack"))
                            authorized = (await _authService.AuthorizeAsync(_user, TrademarkAuthorizationPolicy.CostTrackingUpload)).Succeeded;
                    }
                    break;
                case SystemTypeCode.DMS:
                    authorized = (await _authService.AuthorizeAsync(_user, DMSAuthorizationPolicy.CanUploadDocuments)).Succeeded;
                    break;

                case SystemTypeCode.PatClearance:
                    authorized = (await _authService.AuthorizeAsync(_user, PatentClearanceAuthorizationPolicy.CanAccessSystem)).Succeeded;
                    break;

                case SystemTypeCode.Clearance:
                    authorized = (await _authService.AuthorizeAsync(_user, SearchRequestAuthorizationPolicy.CanAccessSystem)).Succeeded;
                    break;

                case SystemTypeCode.Shared:
                    authorized = (await _authService.AuthorizeAsync(_user, SharedAuthorizationPolicy.CanUploadDocuments)).Succeeded;
                    break;
            }
            return authorized;
        }

        public async Task SaveImportedDocument(IFormFile formFile, string fileName, string documentLink)
        {
            var folder = await GetOrAddDefaultFolder(documentLink);

            var parentId = folder.DataKeyValue;
            var viewModels = new List<DocDocumentViewModel>
            {
                new DocDocumentViewModel()
                {
                    ParentId = parentId,
                    UploadedFile = formFile,
                    Author = _user.GetEmail(),
                    CreatedBy = _user.GetUserName(),
                    UpdatedBy = _user.GetUserName(),
                    LastUpdate = DateTime.Now,
                    DateCreated = DateTime.Now,
                    UserFileName = fileName,
                    FolderId = folder.FolderId,
                    DocumentLink = documentLink,
                    DocFolder = folder,
                    Source = DocumentSourceType.Manual,
                    IsActRequired = false,
                    IsVerified = false
                }
            };
            await SaveUploadedDocuments(viewModels);
        }

        public async Task<List<WorkflowEmailViewModel>> ProcessDocVerificationNewActWorkflow(int docId, string patEmailurl, string tmkEmailUrl, string gmEmailUrl)
        {
            var emailWorkflows = new List<WorkflowEmailViewModel>();

            var systemType = string.Empty;
            var docFolder = await _docService.DocFolders.AsNoTracking().Where(f => f.DocDocuments != null && f.DocDocuments.Any(d => d.DocId == docId)).Select(f => new { f.DataKeyValue, f.SystemType }).FirstOrDefaultAsync();

            if (docFolder != null) systemType = docFolder.SystemType;

            var newActIds = await _docService.DocVerifications.AsNoTracking()
                                    .Where(d => d.DocId == docId && d.ActId > 0 && d.WorkflowStatus == DocVerificationWorkflowStatus.ToBeProcess)
                                    .Select(d => d.ActId)
                                    .ToListAsync();

            if (newActIds != null && newActIds.Count() > 0 && !string.IsNullOrEmpty(systemType))
            {
                if (systemType.ToLower() == SystemTypeCode.Patent.ToLower())
                {
                    // Patent action workflow removed during debloat
                }
                else if (systemType.ToLower() == SystemTypeCode.Trademark.ToLower())
                {
                    // Trademark action workflow removed during debloat
                }
                // GeneralMatter case removed - module deleted
                
                foreach (var actId in newActIds)
                {
                    await _docService.MarkVerificationNewActionAsProcessed(actId ?? 0, docId);
                }
            }
            return emailWorkflows;
        }

        public async Task<Stream?> GetDocumentAsStream(string system, string fileName, CPiSavedFileType savedFileType)
        {
            var sourceFile = _documentStorage.GetFilePath("", fileName, ImageHelper.CPiSavedFileType.DocMgt);
            return await _documentStorage.GetFileStream(sourceFile);
        }

        public async Task<string> SaveReportFile(MemoryStream stream,string fileExtension,string userName,string prefix)
        {
            Guid uuid = Guid.NewGuid();
            var uuidFileName = prefix + uuid.ToString();
            var fileName = uuidFileName + $".{fileExtension}";

            var docFile = new DocFile
            {
                FileExt = fileExtension,
                UserFileName = fileName, 
                CreatedBy = userName,
                DateCreated = DateTime.Now,
                UpdatedBy = userName,
                LastUpdate = DateTime.Now,
            };

            await _docService.AddDocFile(docFile);
            var fullPath = _documentStorage.GetFilePath("", uuidFileName, ImageHelper.CPiSavedFileType.ReportFile);
            await _documentStorage.SaveFile(stream.ToArray(), fullPath, new DocumentStorageHeader { });
            return $"{uuidFileName}~{docFile.FileId.ToString()}";

        }

    }
}
