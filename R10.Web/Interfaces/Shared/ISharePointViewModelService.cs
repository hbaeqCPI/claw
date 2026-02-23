using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Web.Areas;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Shared.ViewModels.SharePoint;
using R10.Web.Extensions;

namespace R10.Web.Interfaces
{
    public interface ISharePointViewModelService
    {
        Task<bool> IsAuthenticatedToSharePoint(ClaimsPrincipal user);
        Task<SharePointFolderViewModel> GetFolders(string screenCode, int recordId, string systemType, string? subScreenCode = "");
        string GetDocLibraryFromSystemTypeCode(string systemType);
        string GetDocLibraryFromDocumentCode(string documentCode);
        Task<List<string>> GetFolders(string system, string folder, int recordId);
        string GetSharePointSystemFolder(string systemType);
        //List<string> GetDocumentFolders(string docLibraryFolder, string recKey);
        Task<string> GetActionDueRecKey(string systemType, int actId);
        string GetDocLibraryFolderFromScreenCode(string screenCode);
        Task<List<SharePointReportImage>> GetReportImages(string data);
        Task<SharePointReportImageViewModel> GetReportImageFile(string system, string itemId, string fileName);
        Task<List<SharePointReportImage>> GetReportDefaultImagesForPrintScreen(string system, string moduleCode, string data);
        Task<List<SharePointImageList>> GetReportImagesListForPrintScreen(string system, string moduleCode, string data);
        string GetDocLibrary(string systemTypeCode);
        string GetDocLibraryFolder(string ModuleCode);
        Task<string> GetRecKey(string docLibrary, string docLibraryFolder, int id);
        Task<List<LookupDTO>> GetChildrenRecKeys(string docLibrary, int parentId);
        Task SyncToDocumentTables(ClaimsPrincipal user);
        Task SyncToDocumentTables(List<string> docLibraries);
        Task ClearSyncFlagToDocumentTables(ClaimsPrincipal user);
        Task ClearSyncFlagToDocumentTables(List<string> docLibraries);
        Task SyncToDocumentTables(SharePointSyncToDocViewModel sync);
        Task SyncToDocumentTablesDelete(string docLibrary, string driveItemId);
        Task SyncToDocumentTablesUpdateDelete(ClaimsPrincipal user);
        Task SyncToDocumentTablesUpdateDelete(List<string> docLibraries);
        Task GetIsPrivateDocumentInfoFromDocTable(List<SharePointDocumentViewModel> documents);
        Task GetDocumentInfoFromDocTable(SharePointDocumentEntryViewModel viewModel);
        Task SyncToDocumentTablesInit(SharePointDocumentEntryViewModel viewModel);
        Task<DefaultImageViewModel> GetDefaultImageDocumentInfoFromDocTable(string docLibrary, string docLibraryFolder, int dataKeyValue);
        Task<DefaultImageViewModel> GetDefaultImageDocumentInfoByRecKey(string docLibrary, string docLibraryFolder, string recKey);
        Task SyncToDocumentTablesCopy(string author, List<SharePointSyncCopyDTO> sharePointSyncItems);
        void GetListItemValues(SharePointSyncDTO newSync, Microsoft.Graph.ListItem item);
        Task SyncToDocumentTablesCopy(string author,List<SharePointSyncDTO> driveItems);
        Task RenameRecordKey(string docLibrary, string docLibraryFolder, string oldRecordKey, string newRecordKey);
        string GetSystemCodeFromDocLibrary(string docLibrary);
        string GetDataKeyFromDocLibraryFolder(string docLibraryFolder);

        Task SaveImportedDocument(IFormFile formFile, string fileName, int parentId, string docLibrary, string docLibraryFolder, string recKey);

        Task<Stream?> GetDocumentAsStream(string docLibrary, string driveItemId);
    }
}

 