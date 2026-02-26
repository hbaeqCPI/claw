using Microsoft.AspNetCore.Http;
using R10.Core.DTOs;
using R10.Core.Entities.Documents;
using R10.Web.Areas.Shared.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IDocumentViewModelService
    {

        #region Search
        Task<List<LookupDTO>> GetSystemList(List<string> userSystems);

        Task<List<LookupDTO>> GetSystemShortNameList(List<string> userSystems);

        Task<List<LookupDTO>> GetScreenList(string systemType);

        string GetSubSearchView(string systemType, string screenCode);              // don't make async to avoid concurrency issues

        string GetSearchResultView(string systemType, string screenCode);           // don't make async to avoid concurrency issues
        #endregion


        #region Tree Node
        //Task<DocFolder> GetUserFolderView(string treeNodeId);
        DocFolderViewModel GetUserFolderView(string treeNodeId);

        Task<bool> DeleteTreeNode(string treeNodeId);
        Task<bool> RenameTreeNode(string treeNodeId, string newName, string userName);

        Task<bool> DropTreeNode(string sourceId, string destId, string userName);
        int GetNodeId(string treeNodeId);

        #endregion

        #region Documents
        Task<List<DocDocumentListViewModel>> GetDocumentsByFolderId(int folderId);
        Task<DocDocumentViewModel> CreateDocumentEditorViewModel(int folderId, int docId);
        Task<bool> SaveUploadedDocument(DocDocument document, IFormFile uploadedFile, string rootPath);
        Task<bool> SaveDocumentPopup(DocDocumentViewModel viewModel, string rootPath);
        Task<bool> SaveUploadedDocuments(List<DocDocumentViewModel> viewModels);

        #endregion

        #region Fixed Folder/Documents
        Task<DocFixedFolderViewModel> GetFixedFolderView(string treeNodeId);
        Task<T> GetFixedDocDetail<T>(string id) where T : class;
        Task<T> GetIDSDetail<T>(string id) where T : class;
        #endregion

        #region Root Node Detail
        Task<DocInventionViewModel> GetInventionDetail(string treeNodeId);
        Task<DocCtryAppViewModel> GetCtryAppDetail(string treeNodeId);
        Task<DocTrademarkViewModel> GetTrademarkDetail(string treeNodeId);

        Task<DocPatActViewModel> GetPatActionDetail(string treeNodeId);
        Task<DocTmkActViewModel> GetTmkActionDetail(string treeNodeId);

        Task<DocPatCostViewModel> GetPatCostDetail(string treeNodeId);
        Task<DocPatCostInvViewModel> GetPatCostInvDetail(string treeNodeId);
        Task<DocTmkCostViewModel> GetTmkCostDetail(string treeNodeId);


        #endregion

        #region Miscellaneous
        Task<string> GetPhysicalFolder(string treeNodeId);

        #endregion

    }
}
