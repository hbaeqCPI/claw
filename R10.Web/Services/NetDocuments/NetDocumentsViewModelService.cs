using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using R10.Web.Models;
using R10.Web.Models.NetDocumentsModels;
using System.Security.Claims;

namespace R10.Web.Services.NetDocuments
{
    public interface INetDocumentsViewModelService : IDocumentStorageViewModelService<NetDocumentsClient>
    {

    }

    public class NetDocumentsViewModelService : DocumentStorageViewModelService<NetDocumentsClient>, INetDocumentsViewModelService
    {
        public override string DocFolderName => "NetDocs";

        private readonly INetDocumentsClientFactory _netDocumentsClientFactory;
        private readonly NetDocumentsSettings _netDocumentsSettings;

        public NetDocumentsViewModelService(
            Interfaces.IDocumentsViewModelService docViewModelService, 
            ClaimsPrincipal user, IStringLocalizer<SharedResource> localizer, 
            ICountryApplicationService applicationService,
            INetDocumentsClientFactory netDocumentsClientFactory,
            IOptions<NetDocumentsSettings> netDocumentsSettings) : base(docViewModelService, user, localizer, applicationService)
        {
            _netDocumentsClientFactory = netDocumentsClientFactory;
            _netDocumentsSettings = netDocumentsSettings.Value;
        }

        public override async Task<DocFolder?> CreateWorkspace(NetDocumentsClient httpClient, string documentLink)
        {
            var clientMatter = await _docViewModelService.GetClientMatter(documentLink);
            var workspace = await httpClient.GetWorkspaceByMatter(clientMatter.MatterNumber, _netDocumentsSettings.IsClientMatter ? clientMatter.ClientCode : "");

            //create workspace
            if (workspace == null || string.IsNullOrEmpty(workspace.Id))
            {
                if (_netDocumentsSettings.IsClientMatter)
                {
                    //check client attribute
                    var clientAttr = await httpClient.GetProfileAttribute(_netDocumentsSettings.WorkspaceClientAttributeId, clientMatter.ClientCode);

                    //add client entry
                    if (string.IsNullOrEmpty(clientAttr?.Key))
                        await httpClient.SetProfileAttribute(_netDocumentsSettings.WorkspaceClientAttributeId, new ProfileAttribute()
                        {
                            Key = clientMatter.ClientCode,
                            Description = clientMatter.ClientName
                        });
                }

                //add matter entry which creates new workspace
                await httpClient.SetProfileAttribute(_netDocumentsSettings.WorkspaceMatterAttributeId, new ProfileAttribute()
                {
                    Key = clientMatter.MatterNumber,
                    Parent = _netDocumentsSettings.IsClientMatter ? clientMatter.ClientCode : null,
                    Description = clientMatter.MatterNumber
                });

                workspace = await httpClient.GetWorkspaceByMatter(clientMatter.MatterNumber, _netDocumentsSettings.IsClientMatter ? clientMatter.ClientCode : "");
            }

            if (workspace != null && !string.IsNullOrEmpty(workspace.Id))
            {
                //get default folder
                var defaultFolderId = "";
                var folders = await httpClient.GetFolderList(workspace.Id);
                if (folders?.Any() ?? false)
                {
                    //find folder name from appsettings
                    if (!string.IsNullOrEmpty(_netDocumentsSettings.DefaultFolderName))
                        defaultFolderId = folders.Where(f => String.Equals(f.Name, _netDocumentsSettings.DefaultFolderName, StringComparison.OrdinalIgnoreCase)).OrderBy(f => f.Level).FirstOrDefault()?.Id;

                    //use first folder if default folder is not found
                    if (string.IsNullOrEmpty(defaultFolderId))
                        defaultFolderId = folders.OrderBy(f => f.Level).First().Id;
                }

                //create doc folder
                return await SaveFolderStorageSetting(documentLink, workspace.Id, defaultFolderId);
            }

            return new DocFolder();
        }

        public override Task<bool> RenameWorkspace(string documentLink)
        {
            throw new NotImplementedException();
        }

        public override async Task DeleteDocumentByFileId(int fileId)
        {
            var docFile = await _docViewModelService.GetDocFileById(fileId);
            if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
            {
                using (var client = await _netDocumentsClientFactory.GetClient())
                {
                    await client.DeleteDocument(docFile.DriveItemId);
                    await _docViewModelService.DeleteDocumentsByDriveItemId(docFile.DriveItemId);
                }
            }
        }

        public override async Task<byte[]?> GetDocumentAsByteArrayByFileName(string fileName)
        {
            var docFile = await _docViewModelService.GetDocFileByDocFileName(fileName);
            if (docFile != null && !string.IsNullOrEmpty(docFile.DriveItemId))
            {
                using (var client = await _netDocumentsClientFactory.GetClient())
                {
                    var response = await client.GetDocument(docFile.DriveItemId);
                    var bytes = await response.Content.ReadAsByteArrayAsync();

                    return bytes;
                }
            }

            return null;
        }

        public override async Task<Stream?> GetDocumentAsStream(string driveItemId)
        {
            using (var client = await _netDocumentsClientFactory.GetClient())
            {
                return await client.GetDocumentAsStream(driveItemId);
            }
        }

        public override async Task<WorkflowEmailAttachmentViewModel?> SaveDocument(NetDocumentsClient httpClient, IFormFile file, string documentLink, string folderId, int docFolderId, int parentId, string docSource = DocumentSourceType.Manual, bool updateDocTables = true, bool isDefault = false, bool isActRequired = false, bool checkAct = false, bool sendToClient = false)
        {
            byte[] data;
            using (var br = new BinaryReader(file.OpenReadStream()))
                data = br.ReadBytes((int)file.OpenReadStream().Length);

            var bytes = new ByteArrayContent(data);
            var document = await httpClient.UploadDocument(folderId, bytes, file.FileName);

            if (document != null && updateDocTables)
            {
                //create docViewModel
                var docViewModel = await GetNewDocDocumentViewModel(documentLink, document.GetFileName(), document.Attributes?.Name ?? "", document.Attributes?.Size ?? 0, document.IsImage(), document.Id ?? "", docFolderId, isDefault);

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

        public override async Task<WorkflowEmailAttachmentViewModel?> SaveImportedDocument(IFormFile file, string documentLink, string folderName = "", string docSource = DocumentSourceType.Manual, bool updateDocTables = true, bool isDefault = false, bool isActRequired = false, bool checkAct = false, bool sendToClient = false)
        {
            using (var client = await _netDocumentsClientFactory.GetClient())
            {
                var docFolder = await GetOrAddDefaultFolderByDocumentLink(documentLink);
                var folderId = GetDefaultDocumentFolder(docFolder);
                var parentId = int.Parse(documentLink.Split("|")[3] ?? "0");

                // create workspace
                if (string.IsNullOrEmpty(folderId) && _netDocumentsSettings.WorkspaceCreation == WorkspaceCreation.Auto)
                {
                    var rootDocumentLink = await _docViewModelService.GetRootDocumentLink(documentLink);
                    var name = await _docViewModelService.GenerateFolderName(rootDocumentLink);

                    docFolder = await CreateWorkspace(client, rootDocumentLink);
                    folderId = GetDefaultDocumentFolder(docFolder);
                }

                // get optional folder
                if (docFolder != null && !string.IsNullOrEmpty(folderName))
                {
                    var folders = await client.GetFolders(docFolder.StorageRootContainerId);
                    if (folders?.Results?.Any() ?? false)
                    {
                        var folder = folders.Results.Where(f => String.Equals(f.Name, folderName, StringComparison.OrdinalIgnoreCase)).OrderBy(f => f.ParentId).FirstOrDefault();
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

                return await SaveDocument(client, file, documentLink, folderId, docFolder.FolderId, parentId, docSource, updateDocTables, isDefault, isActRequired, checkAct, sendToClient);
            }
        }

        public override async Task<(int FileId, string UserFileName)> SavePatentIDSDocument(IFormFile formFile, int appId, string idsFolderName = "References")
        {
            var documentLink = $"{SystemTypeCode.Patent}|{ScreenCode.IDS}|{DataKey.Application}|{appId}";
            var emailWorkflow = await SaveImportedDocument(formFile, documentLink, idsFolderName);

            return (emailWorkflow?.FileId ?? 0, emailWorkflow?.OrigFileName ?? string.Empty);
        }

        public override async Task SaveEPODocument(IFormFile formFile, int appId, bool isDocVerificationOn = false, string docSource = DocumentSourceType.Manual, bool isDefault = false, bool isActRequired = false, bool checkAct = false, bool sendToClient = false)
        {
            if (appId > 0 || (appId <= 0 && isDocVerificationOn))
            {
                var docFolder = new DocFolder();
                var documentLink = string.Empty;
                var iManageFolderId = string.Empty;

                if (appId > 0)
                    documentLink = $"{SystemTypeCode.Patent}|{ScreenCode.Application}|{DataKey.Application}|{appId}";
                else if (appId <= 0 && isDocVerificationOn)
                    documentLink = "|||0"; //todo: create dummy workspace

                await SaveImportedDocument(formFile, documentLink, "", docSource, true, isDefault, isActRequired, checkAct, sendToClient);
            }
        }
    }
}
