using Microsoft.AspNetCore.Http;
using R10.Core.DTOs;
using R10.Core.Entities.Documents;
using R10.Web.Areas.Shared.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static R10.Web.Helpers.ImageHelper;

namespace R10.Web.Interfaces
{
    public interface IDocumentsViewModelService
    {

        Task<DocDocumentViewModel> CreateDocumentEditorViewModel(string documentLink, int folderId, int docId);
        Task<DefaultImageViewModel> GetDefaultImage(string system, string screenCode, string systemType, string dataKey, int dataKeyValue);
        Task<DocFile> AddDocFile(DocDocumentViewModel viewModel, string fileName, int fileSize, bool isImage);
        Task UpdateDocFile(DocDocumentViewModel viewModel);
        Task RenameDocFile(int fileId, string fileName);
        Task<DocFile?> GetDocFileByDocFileName(string docFileName);
        Task<DocFile?> GetDocFileById(int fileId);
        Task<DocFile?> GetDocFileByIdAndFileName(int fileId,string fileName);
        Task<DocFile?> GetDocFileByDriveItemId(string driveItemId);
        Task DeleteDocumentsByDriveItemId(string? driveItemId);
        Task DeleteDocumentsByFolderId(int folderId);
        Task<DocDocument> SaveDocument(DocDocumentViewModel viewModel);
        Task<(bool IsNew, bool IsReassigned)> SaveRespDocketing(DocDocumentViewModel viewModel, string userName);
        Task<(bool IsNew, bool IsReassigned)> SaveRespReporting(DocDocumentViewModel viewModel, string userName);
        Task UpdateDocResponsible(List<string> responsibleList, string userName, int docId);
        Task<bool> SaveDocumentPopup(DocDocumentViewModel viewModel, string rootPath);
        Task<bool> SaveUploadedDocuments(List<DocDocumentViewModel> viewModels);
        Task<bool> SaveDocumentFromStream(DocDocumentViewModel viewModel, MemoryStream stream, bool updateParentStamp = true);
        Task<DocFolder> GetOrAddDefaultFolder(string documentLink);

        Task<string> GenerateFolderName(string documentLink);
        Task<(string? ClientCode, string? ClientName, string? MatterNumber)> GetClientMatter(string documentLink);
        Task<string> GetRootDocumentLink(string documentLink);

        /// <summary>
        /// Get DocFolder or parent record's DocFolder
        /// </summary>
        /// <param name="documentLink"></param>
        /// <returns></returns>
        Task<DocFolder?> GetDefaultFolderByDocumentLink(string documentLink);
        Task<DocFolder?> GetFolderByDocumentLink(string documentLink);
        Task<DocDocument?> GetDocumentByDriveItemId(string documentId);
        Task<int> GetDocTypeIdFromFileName(string fileName);
        Task SaveFolderStorageSetting(DocFolder folder);
        string GetDocumentBasePath();
        Task<List<DocDocumentListViewModel>> ApplyCriteria(List<DocDocumentListViewModel> documents, List<QueryFilterViewModel> criteria);
        Task<List<WorkflowViewModel>> GeneratePatentActionWorkflow(List<WorkflowEmailAttachmentViewModel> attachments, int parentId);
        Task<List<WorkflowViewModel>> GenerateTrademarkActionWorkflow(List<WorkflowEmailAttachmentViewModel> attachments, int parentId);
        Task<List<WorkflowViewModel>> GenerateGMActionWorkflow(List<WorkflowEmailAttachmentViewModel> attachments, int parentId);
        Task<List<WorkflowViewModel>> GenerateCountryAppWorkflow(List<WorkflowEmailAttachmentViewModel> attachments, int parentId, bool isNewFileUpload = false, bool isNewRespDocketing = false, bool isRespDocketingReassigned = false, string newRespDocketingUrl = "", string reassignedRespDocketingUrl = "", bool isNewEPOFileDownload = false, bool hasNewRespReporting = false, bool hasRespReportingReassigned = false, string newRespReportingUrl = "", string reassignedRespReportingUrl = "");
        Task<List<WorkflowViewModel>> GenerateTrademarkWorkflow(List<WorkflowEmailAttachmentViewModel> attachments, int parentId, bool isNewFileUpload = false, bool isNewRespDocketing = false, bool isRespDocketingReassigned = false, string newRespDocketingUrl = "", string reassignedRespDocketingUrl = "", bool hasNewRespReporting = false, bool hasRespReportingReassigned = false, string newRespReportingUrl = "", string reassignedRespReportingUrl = "");
        Task<List<WorkflowViewModel>> GenerateGMWorkflow(List<WorkflowEmailAttachmentViewModel> attachments, int parentId, bool isNewFileUpload = false, bool isNewRespDocketing = false, bool isRespDocketingReassigned = false, string newRespDocketingUrl = "", string reassignedRespDocketingUrl = "", bool hasNewRespReporting = false, bool hasRespReportingReassigned = false, string newRespReportingUrl = "", string reassignedRespReportingUrl = "");

        Task MarkForSignature(int fileId, string documentLink, int qeSetupId, string roleLink, bool isDMSInventorSignature = false);

        Task<bool> CanModifyDocument(string documentLink);
        Task SaveImportedDocument(IFormFile formFile, string fileName, string documentLink);

        Task<List<WorkflowEmailViewModel>> ProcessDocVerificationNewActWorkflow(int docId, string patEmailUrl, string tmkEmailUrl, string gmEmailurl);

        Task<Stream?> GetDocumentAsStream(string system, string fileName, CPiSavedFileType savedFileType);
        Task<string> GetResponsibleEmail(string groupIds = "", string userIds = "");
        Task<string> SaveReportFile(MemoryStream stream, string fileExtension, string userName,string prefix);


    }
}
