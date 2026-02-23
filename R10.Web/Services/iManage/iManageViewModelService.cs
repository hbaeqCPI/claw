using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using R10.Core.Entities.Documents;
using R10.Core.Helpers;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models;
using R10.Web.Models.IManageModels;
using System.Security.Claims;

namespace R10.Web.Services.iManage
{
    public interface IiManageViewModelService
    {
        Task<DocFolder> SaveFolderStorageSetting(string documentLink, string? rootContainerId, string? defaultFolderId);
        Task<DocFolder?> GetOrAddDefaultFolderByDocumentLink(string documentLink);
        string GetDefaultDocumentFolder(DocFolder? docFolder);
        Task<DocDocumentViewModel> GetNewDocDocumentViewModel(Document document, int folderId, bool isDefault = false);
        Task<DocFolder?> CreateImanageWorkspace(iManageClient client, string documentLink, string workspaceName, string? templateId, string? defaultFolderName);
        Task<WorkflowEmailAttachmentViewModel?> SaveImanageDocument(iManageClient client, IFormFile file, string iManageFolderId, int docFolderId, int parentId, bool isDefault);
        Task<WorkflowEmailAttachmentViewModel?> SaveImportedDocument(IFormFile file, string documentLink, string folderName = "", bool updateDocTables = true);
        Task DeleteDocumentByFileId(int fileId);
        Task<Stream?> GetDocumentAsStream(string driveItemId);
        Task<byte[]?> GetDocumentAsByteArrayByFileName(string fileName);
        Task<bool> RenameImanageWorkspace(string documentLink);
        Task<WorkflowEmailAttachmentViewModel?> SaveImanageDocument(iManageClient client, IFormFile file, string iManageFolderId, int docFolderId, int parentId, bool isDefault, bool updateDocTables);
        Task<(int FileId, string UserFileName)> SavePatentIDSDocumentToIManage(IFormFile formFile, int appId, Core.Entities.Shared.DefaultSetting settings, string idsFolderName = "References");
    }

    public class iManageViewModelService : IiManageViewModelService
    {
        private readonly IiManageClientFactory _iManageClientFactory;
        private readonly iManageSettings _iManageSettings;
        private readonly IDocumentsViewModelService _docViewModelService;
        private readonly ClaimsPrincipal _user;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ICountryApplicationService _applicationService;

        public iManageViewModelService(
            IiManageClientFactory iManageClientFactory,
            IOptions<iManageSettings> iManageSettings,
            IDocumentsViewModelService docViewModelService, 
            ClaimsPrincipal user, 
            IStringLocalizer<SharedResource> localizer,
            ICountryApplicationService applicationService)
        {
            _iManageClientFactory = iManageClientFactory;
            _iManageSettings = iManageSettings.Value;
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
                    FolderName = "iManage",
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

        public async Task<DocDocumentViewModel> GetNewDocDocumentViewModel(Document document, int folderId, bool isDefault = false)
        {
            var fileName = document.GetFileName();
            return new DocDocumentViewModel()
            {
                //docDocument
                FolderId = folderId,
                Author = _user.GetEmail(),
                DocName = document.Name,
                DocTypeId = await _docViewModelService.GetDocTypeIdFromFileName(fileName),
                Source = DocumentSourceType.Manual,
                IsDefault = isDefault,

                //docFile
                FileId = 0,
                UserFileName = fileName,
                FileSize = document.Size,
                IsImage = document.IsImage(),
                DriveItemId = document.Id,

                CreatedBy = _user.GetUserName(),
                DateCreated = DateTime.Now,
                UpdatedBy = _user.GetUserName(),
                LastUpdate = DateTime.Now
            };
        }

        public async Task<DocFolder?> CreateImanageWorkspace(iManageClient client, string documentLink, string workspaceName, string? templateId, string? defaultFolderName)
        {
            Workspace? workspace;

            //validate workspace name 
            if (String.IsNullOrEmpty(workspaceName))
                return new DocFolder();

            //check if workspace name exists
            var workspaces = await client.GetWorkspacesByName(workspaceName);

            if ((workspaces?.Any() ?? false) && string.Equals(workspaces.First().Name, workspaceName, StringComparison.OrdinalIgnoreCase))
                //use existing workspace
                workspace = workspaces.First();
            else
            {
                //create workspace
                workspace = await client.CreateWorkspace(workspaceName);

                //apply template
                if (workspace != null && !string.IsNullOrEmpty(workspace.Id) && !string.IsNullOrEmpty(templateId))
                    await client.ApplyWorkspaceTemplate(workspace.Id, templateId);
            }

            if (workspace != null && !string.IsNullOrEmpty(workspace.Id))
            {
                //get default folder
                var defaultFolderId = "";
                var folders = await client.GetFolders(workspace.Id);
                if (folders?.Data?.Any() ?? false)
                {
                    //find folder name from appsettings
                    if (!string.IsNullOrEmpty(defaultFolderName))
                        defaultFolderId = folders.Data.Where(f => String.Equals(f.Name, defaultFolderName, StringComparison.OrdinalIgnoreCase)).OrderBy(f => f.ParentId).FirstOrDefault()?.Id;

                    //use first folder if default folder is not found
                    if (string.IsNullOrEmpty(defaultFolderId))
                        defaultFolderId = folders.Data.Where(f => f.ParentId == workspace.Id).OrderBy(f => f.Name).First().Id;
                }

                //create doc folder
                return await SaveFolderStorageSetting(documentLink, workspace.Id, defaultFolderId);
            }

            return new DocFolder();
        }

        public async Task<WorkflowEmailAttachmentViewModel?> SaveImanageDocument(iManageClient client, IFormFile file, string iManageFolderId, int docFolderId, int parentId, bool isDefault)
        {
            return await SaveImanageDocument(client, file, iManageFolderId, docFolderId, parentId, isDefault, true);
        }

        public async Task<WorkflowEmailAttachmentViewModel?> SaveImanageDocument(iManageClient client, IFormFile file, string iManageFolderId, int docFolderId, int parentId, bool isDefault, bool updateDocTables)
        {
            byte[] data;
            using (var br = new BinaryReader(file.OpenReadStream()))
                data = br.ReadBytes((int)file.OpenReadStream().Length);

            var bytes = new ByteArrayContent(data);
            var document = await client.UploadDocument(iManageFolderId, bytes, file.FileName);

            if (document != null && updateDocTables)
            {
                //create docViewModel
                var docViewModel = await GetNewDocDocumentViewModel(document, docFolderId, isDefault);

                //create docFile and update docViewModel.FileId
                var docFile = await _docViewModelService.AddDocFile(docViewModel, docViewModel.UserFileName ?? "", docViewModel.FileSize ?? 0, docViewModel.IsImage ?? false);

                //create docDocument
                var docDocument = await _docViewModelService.SaveDocument(docViewModel);

                return new WorkflowEmailAttachmentViewModel
                {
                    Id = document.Id,
                    DocId = docDocument.DocId,
                    FileId = docDocument.FileId,
                    OrigFileName = docViewModel.UserFileName,
                    FileName = document.Id, //$"{docDocument.FileId}{Path.GetExtension(docViewModel.UserFileName)}",
                    DocParent = parentId
                };
            }

            return null;
        }


        public async Task<WorkflowEmailAttachmentViewModel?> SaveImportedDocument(IFormFile file, string documentLink, string folderName = "", bool updateDocTables = true)
        {
            using (var client = await _iManageClientFactory.GetClient())
            {
                var docFolder = await GetOrAddDefaultFolderByDocumentLink(documentLink);
                var folderId = GetDefaultDocumentFolder(docFolder);
                var parentId = int.Parse(documentLink.Split("|")[3] ?? "0");

                // create workspace
                if (string.IsNullOrEmpty(folderId) && _iManageSettings.WorkspaceCreation == WorkspaceCreation.Auto)
                {
                    var rootDocumentLink = await _docViewModelService.GetRootDocumentLink(documentLink);
                    var name = await _docViewModelService.GenerateFolderName(rootDocumentLink);

                    docFolder = await CreateImanageWorkspace(client, rootDocumentLink, name, _iManageSettings.WorkspaceTemplateId, _iManageSettings.DefaultFolderName);
                    folderId = GetDefaultDocumentFolder(docFolder);
                }

                // get optional folder
                if (docFolder != null && !string.IsNullOrEmpty(folderName))
                {
                    var folders = await client.GetFolders(docFolder.StorageRootContainerId);
                    if (folders?.Data?.Any() ?? false)
                    {
                        var folder = folders.Data.Where(f => String.Equals(f.Name, folderName, StringComparison.OrdinalIgnoreCase)).OrderBy(f => f.ParentId).FirstOrDefault();
                        if (folder != null)
                            folderId = folder.Id;
                    }
                }

                // check if folder is a workspace
                if (docFolder != null && folderId == docFolder.StorageRootContainerId)
                {
                    var container = await client.GetContainer(folderId);
                    if (container.ContainerType == ContainerType.Workspace)
                        throw new Exception($"Cannot upload document to workspace \"{container.Name}\".");
                }

                if (docFolder == null || string.IsNullOrEmpty(folderId))
                    throw new Exception($"Folder not found. Unable to upload document {file.FileName} to {documentLink}.");

                return await SaveImanageDocument(client, file, folderId, docFolder.FolderId, parentId, false, updateDocTables);
            }
        }

        public async Task DeleteDocumentByFileId(int fileId)
        {
            var docFile = await _docViewModelService.GetDocFileById(fileId);
            if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
            {
                using (var client = await _iManageClientFactory.GetClient())
                {
                    if (await client.DeleteDocument(docFile.DriveItemId))
                        await _docViewModelService.DeleteDocumentsByDriveItemId(docFile.DriveItemId);
                }
            }
        }

        public async Task<Stream?> GetDocumentAsStream(string driveItemId)
        {
            using (var client = await _iManageClientFactory.GetClient())
            {
                return await client.GetDocumentAsStream(driveItemId);
            }
        }

        public async Task<byte[]?> GetDocumentAsByteArrayByFileName(string fileName)
        {
            var docFile = await _docViewModelService.GetDocFileByDocFileName(fileName);
            if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
            {
                using (var client = await _iManageClientFactory.GetClient())
                {
                    var response = await client.GetDocument(docFile.DriveItemId);
                    var bytes = await response.Content.ReadAsByteArrayAsync();

                    return bytes;
                }
            }

            return null;
        }

        public async Task<bool> RenameImanageWorkspace(string documentLink)
        {
            var folder = await _docViewModelService.GetFolderByDocumentLink(documentLink);
            var workspaceName = await _docViewModelService.GenerateFolderName(documentLink);

            if (!string.IsNullOrEmpty(folder?.StorageRootContainerId) && !string.IsNullOrEmpty(workspaceName))
            {
                using (var client = await _iManageClientFactory.GetClient())
                {
                    return await client.RenameWorkspace(folder.StorageRootContainerId, workspaceName);
                }
            }

            return false;
        }

        public async Task<(int FileId, string UserFileName)> SavePatentIDSDocumentToIManage(IFormFile formFile, int appId, Core.Entities.Shared.DefaultSetting settings, string idsFolderName = "References")
        {
            var ctryApp = await _applicationService.GetById(appId);
            if (ctryApp == null) return (0, string.Empty);

            var documentLink = $"{SystemTypeCode.Patent}|{ScreenCode.Application}|{DataKey.Application}|{ctryApp.AppId}";
            
            // Get CtryApp DocFolder - this could return parent (Invention) DocFolder or empty
            var appDocFolder = await GetOrAddDefaultFolderByDocumentLink(documentLink);

            // Get root document link (Invention)
            var rootDocumentLink = await _docViewModelService.GetRootDocumentLink(documentLink);

            // Get root folder name (Invention-CaseNumber)
            var rootFolderName = await _docViewModelService.GenerateFolderName(rootDocumentLink);

            using (var client = await _iManageClientFactory.GetClient())
            {
                // Get the root container folder - create if not found             
                var rootDocFolder = await CreateImanageWorkspace(client, rootDocumentLink, rootFolderName, _iManageSettings.WorkspaceTemplateId, _iManageSettings.DefaultFolderName);
                var rootContainerId = rootDocFolder?.StorageRootContainerId ?? "";
                
                // Check and create CtryApp folder if not found
                if (string.IsNullOrEmpty(rootContainerId))
                {
                    throw new Exception($"Cannot get storage root container id for \"{documentLink}\".");                    
                }

                var rootContainerSubFolders = await client.GetFolders(rootContainerId);

                var appFolderId = rootContainerId;
                if (!settings.IsCountryApplicationDocumentRoot)
                {
                    var appFolderName = await _docViewModelService.GenerateFolderName(documentLink);
                    // Check if there is a sub folder under rootContainerFolder correspond to the CtryApp record (Country~SubCase)
                    var appFolder = rootContainerSubFolders?.Data?.FirstOrDefault(d => String.Equals(d.Name, appFolderName, StringComparison.OrdinalIgnoreCase) && d.ContainerType == Models.IManageModels.ContainerType.Folder && d.ParentId == rootContainerId);
                    // Create sub folder for CtryApp under Root Container folder (Invention) if not found
                    if (appFolder == null)
                        appFolder = await client.CreateRootFolder(rootContainerId, appFolderName);

                    if (appFolder == null || string.IsNullOrEmpty(appFolder.Id))
                    {
                        throw new Exception($"Cannot get sub folder for \"{documentLink}\".");                        
                    }
                    else
                    {
                        // Create CtryApp DocFolder if missing
                        if (appDocFolder == null)
                        {
                            appDocFolder = await SaveFolderStorageSetting(documentLink, rootContainerId, null);
                        }
                    }
                    appFolderId = appFolder.Id;
                }

                // Create ids sub folder under CtryApp folder if not found
                var idsSubFolder = rootContainerSubFolders?.Data?.FirstOrDefault(d => String.Equals(d.Name, idsFolderName, StringComparison.OrdinalIgnoreCase) && d.ContainerType == Models.IManageModels.ContainerType.Folder && d.ParentId == appFolderId);
                if (idsSubFolder == null)
                {
                    if (settings.IsCountryApplicationDocumentRoot)
                    {
                        idsSubFolder = await client.CreateRootFolder(appFolderId, idsFolderName);
                    }
                    else
                    {
                        idsSubFolder = await client.CreateSubFolder(appFolderId, idsFolderName);
                    }
                    
                    if (idsSubFolder == null || string.IsNullOrEmpty(idsSubFolder.Id))
                    {                        
                        throw new Exception($"Cannot create {idsFolderName} sub folder for \"{documentLink}\".");
                    }                        
                }                

                if (idsSubFolder == null || string.IsNullOrEmpty(idsSubFolder.Id))
                {                    
                    throw new Exception($"{idsFolderName} sub folder not found. Unable to upload document {formFile.FileName} to {documentLink}.");
                }                    

                if (appDocFolder == null)
                {
                    throw new Exception($"DocFolder not found for \"{documentLink}\".");
                }                    

                var emailWorkflow = await SaveImanageDocument(client, formFile, idsSubFolder.Id, appDocFolder.FolderId, ctryApp.AppId, false, true);
                
                return (emailWorkflow?.FileId ?? 0, emailWorkflow?.OrigFileName ?? string.Empty);
            }
        }
    }
}
