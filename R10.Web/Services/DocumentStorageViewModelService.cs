using R10.Core.Entities.Documents;
using R10.Core.Entities.Patent;
using R10.Core.Helpers;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Interfaces;
using R10.Web.Models;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace R10.Web.Services
{
    public interface IDocumentStorageViewModelService<T> where T : HttpClient
    {
        string DocFolderName { get; }
        Task<DocFolder> SaveFolderStorageSetting(string documentLink, string? rootContainerId, string? defaultFolderId);
        Task<DocFolder?> GetOrAddDefaultFolderByDocumentLink(string documentLink);
        string GetDefaultDocumentFolder(DocFolder? docFolder);
        Task<DocDocumentViewModel> GetNewDocDocumentViewModel(string documentLink, string fileName, string docName, int fileSize, bool isImage, string driveItemId, int folderId, bool isDefault = false, bool isActRequired = false, bool checkAct = false, bool sendToClient = false);
        Task<DocFile?> GetDocFileByDriveItemId(string driveItemId);
        Task<DocFolder?> CreateWorkspace(T httpClient, string documentLink);
        Task<bool> RenameWorkspace(string documentLink);
        Task<WorkflowEmailAttachmentViewModel?> SaveDocument(T httpClient, IFormFile file, string documentLink, string folderId, int docFolderId, int parentId, string docSource = DocumentSourceType.Manual, bool updateDocTables = true, bool isDefault = false, bool isActRequired = false, bool checkAct = false, bool sendToClient = false);
        Task<WorkflowEmailAttachmentViewModel?> SaveImportedDocument(IFormFile file, string documentLink, string folderName = "", string docSource = DocumentSourceType.Manual, bool updateDocTables = true, bool isDefault = false, bool isActRequired = false, bool checkAct = false, bool sendToClient = false);
        Task DeleteDocumentByFileId(int fileId);
        Task<Stream?> GetDocumentAsStream(string driveItemId);
        Task<byte[]?> GetDocumentAsByteArrayByFileName(string fileName);
        Task<(int FileId, string UserFileName)> SavePatentIDSDocument(IFormFile formFile, int appId, string idsFolderName = "References");
        Task SaveEPODocument(IFormFile formFile, int appId, bool isDocVerificationOn, string docSource, bool isDefault = false, bool isActRequired = false, bool checkAct = false, bool sendToClient = false);

    }

    public abstract class DocumentStorageViewModelService<T> : IDocumentStorageViewModelService<T> where T : HttpClient
    {
        protected readonly IDocumentsViewModelService _docViewModelService;
        protected readonly ClaimsPrincipal _user;
        protected readonly IStringLocalizer<SharedResource> _localizer;
        protected readonly ICountryApplicationService _applicationService;

        /// <summary>
        /// tblDocFolder.FolderName
        /// </summary>
        public abstract string DocFolderName { get; }

        protected DocumentStorageViewModelService(IDocumentsViewModelService docViewModelService, ClaimsPrincipal user, IStringLocalizer<SharedResource> localizer, ICountryApplicationService applicationService)
        {
            _docViewModelService = docViewModelService;
            _user = user;
            _localizer = localizer;
            _applicationService = applicationService;
        }

        /// <summary>
        /// Update tblDocFolder
        /// </summary>
        /// <param name="documentLink"></param>
        /// <param name="rootContainerId"></param>
        /// <param name="defaultFolderId"></param>
        /// <returns></returns>
        public async Task<DocFolder> SaveFolderStorageSetting(string documentLink, string? rootContainerId, string? defaultFolderId)
        {
            var docFolder = await _docViewModelService.GetFolderByDocumentLink(documentLink);
            if (docFolder == null)
            {
                var documentLinkArray = documentLink.Split("|");

                docFolder = new DocFolder()
                {
                    SystemType = documentLinkArray[0],
                    ScreenCode = documentLinkArray[1],
                    DataKey = documentLinkArray[2],
                    DataKeyValue = int.Parse(documentLinkArray[3] ?? "0"),
                    FolderName = !string.IsNullOrEmpty(DocFolderName) ? DocFolderName  : "Documents",
                    Author = _user.GetEmail(),
                    CreatedBy = _user.GetUserName(),
                    DateCreated = DateTime.Now
                };
            }
            else
            {
                //delete DocDocuments if StorageRootContainerId is updated
                if (docFolder.StorageRootContainerId != rootContainerId)
                    await _docViewModelService.DeleteDocumentsByFolderId(docFolder.FolderId);
            }

            docFolder.StorageRootContainerId = rootContainerId;
            docFolder.StorageDefaultFolderId = defaultFolderId;
            docFolder.UpdatedBy = _user.GetUserName();
            docFolder.LastUpdate = DateTime.Now;

            await _docViewModelService.SaveFolderStorageSetting(docFolder);

            return docFolder;
        }

        public async Task<DocFolder?> GetOrAddDefaultFolderByDocumentLink(string documentLink)
        {
            var documentLinkArray = documentLink.Split("|");
            var systemType = documentLinkArray[0]?.ToUpper();
            var screenCode = documentLinkArray[1];
            var dataKey = documentLinkArray[2]?.ToLower();
            var dataKeyValue = int.Parse(documentLinkArray[3] ?? "0");

            //get docFolder, use parent docFolder if not found
            var docFolder = await _docViewModelService.GetDefaultFolderByDocumentLink(documentLink);

            //create docFolder if not found but parent docFolder exists
            if (docFolder != null && (docFolder.SystemType?.ToLower() != systemType?.ToLower() || docFolder.DataKey?.ToLower() != dataKey?.ToLower() || docFolder.DataKeyValue != dataKeyValue))
                return await SaveFolderStorageSetting(documentLink, docFolder.StorageRootContainerId, docFolder.StorageDefaultFolderId);

            return docFolder;
        }

        /// <summary>
        /// Get default storage container id
        /// </summary>
        /// <param name="docFolder"></param>
        /// <returns>StorageDefaultFolderId or StorageRootContainerId if StorageDefaultFolderId is null</returns>
        public string GetDefaultDocumentFolder(DocFolder? docFolder)
        {
            var folder = "";

            if (docFolder != null)
                folder = string.IsNullOrEmpty(docFolder.StorageDefaultFolderId) ? docFolder.StorageRootContainerId : docFolder.StorageDefaultFolderId;

            return (folder ?? "");
        }

        public async Task<DocDocumentViewModel> GetNewDocDocumentViewModel(string documentLink, string fileName, string docName, int fileSize, bool isImage, string driveItemId, int folderId, bool isDefault = false, bool isActRequired = false, bool checkAct = false, bool sendToClient = false)
        {
            var documentLinkArray = documentLink.Split("|");
            var systemType = documentLinkArray[0]?.ToUpper();
            var screenCode = documentLinkArray[1];
            var dataKey = documentLinkArray[2]?.ToLower();
            var dataKeyValue = int.Parse(documentLinkArray[3] ?? "0");

            return new DocDocumentViewModel()
            {
                DocFileName = fileName,                 

                SystemType = systemType,
                ScreenCode = screenCode,
                ParentId = dataKeyValue,
                DataKey = dataKey,

                //docDocument
                FolderId = folderId,
                Author = _user.GetEmail(),
                DocName = docName,
                DocTypeId = await _docViewModelService.GetDocTypeIdFromFileName(fileName),
                Source = DocumentSourceType.Manual,
                IsDefault = isDefault,
                IsActRequired = isActRequired,
                CheckAct = checkAct,
                SendToClient = sendToClient,

                //docFile
                FileId = 0,
                UserFileName = fileName,
                FileSize = fileSize,
                IsImage = isImage,
                DriveItemId = driveItemId,

                CreatedBy = _user.GetUserName(),
                DateCreated = DateTime.Now,
                UpdatedBy = _user.GetUserName(),
                LastUpdate = DateTime.Now
            };
        }

        public async Task<DocFile?> GetDocFileByDriveItemId(string driveItemId)
        {
            return await _docViewModelService.GetDocFileByDriveItemId(driveItemId);
        }

        public abstract Task<WorkflowEmailAttachmentViewModel?> SaveImportedDocument(IFormFile file, string documentLink, string folderName = "", string docSource = DocumentSourceType.Manual, bool updateDocTables = true, bool isDefault = false, bool isActRequired = false, bool checkAct = false, bool sendToClient = false);
        public abstract Task DeleteDocumentByFileId(int fileId);
        public abstract Task<Stream?> GetDocumentAsStream(string driveItemId);
        public abstract Task<byte[]?> GetDocumentAsByteArrayByFileName(string fileName);
        public abstract Task<WorkflowEmailAttachmentViewModel?> SaveDocument(T httpClient, IFormFile file, string documentLink, string folderId, int docFolderId, int parentId, string docSource = DocumentSourceType.Manual, bool updateDocTables = true, bool isDefault = false, bool isActRequired = false, bool checkAct = false, bool sendToClient = false);
        public abstract Task<(int FileId, string UserFileName)> SavePatentIDSDocument(IFormFile formFile, int appId, string idsFolderName = "References");
        public abstract Task<DocFolder?> CreateWorkspace(T httpClient, string documentLink);
        public abstract Task<bool> RenameWorkspace(string documentLink);
        public abstract Task SaveEPODocument(IFormFile formFile, int appId, bool isDocVerificationOn, string docSource, bool isDefault = false, bool isActRequired = false, bool checkAct = false, bool sendToClient = false);
    }
}