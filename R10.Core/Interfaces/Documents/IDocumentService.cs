using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Patent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IDocumentService
    {
        IQueryable<DocSystem> DocSystems { get; }
        IQueryable<DocMatterTree> DocMatterTrees { get; }

        IQueryable<DocFolder> DocFolders { get; }
        IQueryable<DocDocument> DocDocuments { get; }
        IQueryable<DocDocumentTag> DocDocumentTags { get; }
        IQueryable<DocType> DocTypes { get; }
        IQueryable<DocFile> DocFiles { get; }
        IQueryable<DocIcon> DocIcons { get; }
        IQueryable<DocFixedFolder> DocFixedFolders { get; }
        IQueryable<SharePointFileSignature> SharePointFileSignatures { get; }
        IQueryable<DocFileSignature> DocFileSignatures { get; }
        IQueryable<EFSLog> EFSLogs { get; }
        IQueryable<LetterLog> LetterLogs { get; }
        IQueryable<DocFileSignatureRecipient> DocFileSignatureRecipients { get; }
        IQueryable<DocVerification> DocVerifications { get; }
        IQueryable<DocResponsibleDocketing> DocRespDocketings { get; }
        IQueryable<DocResponsibleReporting> DocRespReportings { get; }
        IQueryable<DocQuickEmailLog> DocQuickEmailLogs { get; }
        IQueryable<PatEPODocumentCombined> PatEPODocumentCombineds { get; }

        Task<bool> IsUserRestrictedFromPrivateDocuments();
        Task<int> AddFileToFileHandler(string fileName, string userName,string? driveItemId="");

        #region Document Tree
        Task<IEnumerable<DocTreeDTO>> GetDocumentTree(string systemType, string screenCode, string dataKey, int dataKeyValue, string id);
        Task<IEnumerable<DocTreeDTO>> GetApplicableDocTree(string systemType, string screenCode, string dataKey, int dataKeyValue, string id);
        Task<IEnumerable<DocTreeEmailApiDTO>> GetDocumentTreeEmailApi(string systemType, string screenCode, string dataKey, int dataKeyValue, string id);
        Task<DocTreeDTO> AddTreeFolder(string id, string folderName, string userName);

        Task<bool> DeleteDoc(DocDocument document, DocFile docFile);
        #endregion

        #region Folder
        Task<DocFolder> GetFolderByIdAsync(int folderId);
        DocFolder GetFolderById(int folderId);
        Task<DocFolderHeader> GetFolderHeader(int folderId);
        Task<DocFolder> AddFolder(string systemType, string dataKey, string screenCode, int dataKeyValue, string folderName, int parentId, bool isFixed);
        Task<bool> RenameFolder(string userName, int folderId, string newName);
        Task<bool> UpdateFolders(string userName, IEnumerable<DocFolder> updatedFolders, IEnumerable<DocFolder> newFolders, IEnumerable<DocFolder> deletedFolders);
        Task<DocFolder> GetFolder(string systemType, string dataKey, int dataKeyValue, string folderName, int parentId);
        Task<List<int>> GetFolderIds(int parentFolderId);
        #endregion

        #region Documents
        Task<DocDocument> GetDocumentById(int docId);

        Task<bool> RenameDocument(string userName, int docId, string newName);

        Task<bool> UpdateFolder(string userName, DocFolder folder);

        Task<bool> UpdateDocuments(string userName, IEnumerable<DocDocument> updatedDocuments, IEnumerable<DocDocument> newDocuments, IEnumerable<DocDocument> deletedDocuments, DocFolder? docFolder = null, bool updateParentStamp = true);

        Task<string> IsLocked(int docId, string userName);
        Task<string> CheckoutImage(int docId, string userName);
        Task UpdateTags(int docId, string[] tags);

        #endregion

        #region Verification
        Task<bool> HasDocVerification(string systemType, int actId, int id);
        Task AddDocVerifications(List<DocVerification> docVerifications);
        Task<bool> UpdateDocVerifications(int docId, string userName, IEnumerable<DocVerification> updatedDocVerifications, IEnumerable<DocVerification> newDocVerifications, IEnumerable<DocVerification> deletedDocVerifications, string documentLink = "");

        /// <summary>
        /// Update date verified and verified by in tblDocVerification
        /// </summary>
        /// <param name="keyIds"></param>
        /// <param name="verifiedDate"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task UpdateVerificationActionVerify(List<string> keyIds, DateTime? verifiedDate, string? userName);

        /// <summary>
        /// Update ActId after creating action(s) and set ActionTypeId to 0
        /// </summary>
        /// <param name="systemType"></param>
        /// <param name="parentId"></param>
        /// <param name="actId"></param>
        /// <param name="actionTypeId"></param>
        /// <returns></returns>
        Task UpdateVerificationActId(string systemType, int parentId, int actId, int actionTypeId);

        /// <summary>
        /// Update ActionTypeId before removing action(s) and set ActId to 0
        /// </summary>
        /// <param name="systemType"></param>
        /// <param name="parentId"></param>
        /// <param name="actId"></param>
        /// <param name="actionTypeId"></param>
        /// <returns></returns>
        Task UpdateVerificationActionTypeId(string systemType, int parentId, int actId, int actionTypeId);

        Task MarkVerificationNewActionAsProcessed(int actId, int docId);
        #endregion

        #region File 
        Task DeleteDocFile(DocFile docFile);
        Task<DocFile> AddDocFile(DocFile docFile);
        Task UpdateDocFile(DocFile docFile);
        Task<DocFile> GetFileById(int fileId);
        Task<DocFile> GetFileByFileName(string fileName);
        Task<string> GetFileNameById(int fileId);
        Task<DocViewDTO> GetDocViewInfo(string id);
        Task<int> GetFileOtherRefCount(int docId, int fileId);
        Task MarkForSignature(int fileId, string documentLink, int qeSetupId, string roleLink, bool isDMSInventorSignature = false);
        Task MarkSharePointFileForSignature(string driveItemId, string docLibrary, string docLibraryFolder, string recKey, string screenCode, int qeSetupId,string fileName,DateTime? docDate, int parentId, string systemTypeCode, string? roleLink, bool isDMSInventorSignature = false);
        Task SetEnvelopeId(int fileId, string envelopeId);
        Task SetEnvelopeIdForSharePointFile(string itemId, string envelopeId);
        Task SetEnvelopeIdForLetterFile(int letLogId, string envelopeId);
        Task SetEnvelopeIdForEFSFile(int efsLogId, string envelopeId);
        Task MarkSignedDoc(int sourcefileId, int signedFileId);
        Task MarkSignedDocForSharePoint(string sourceDriveItemId, string signedDriveItemId,string fileName);
        Task DeleteSharePointForSignature(string driveItemId);
        Task MarkSignedLetter(int sourceLetLogId, string newLetterFile, string itemId, string userName);
        Task MarkSignedEFSLog(int sourceEfsLogId, string newEfsFile, string itemId, string userName);
        Task AddSignatureRecipients(List<DocFileSignatureRecipient> recipients, string envelopeId);
        Task UpdateSharePointSignatureStatus(string envelopeId, string status);
        Task UpdateFileSignatureStatus(string envelopeId, string status);
        Task UpdateLetSignatureStatus(string envelopeId, string status);
        Task UpdateEFSSignatureStatus(string envelopeId, string status);
        Task UpdateSignatureRecipientStatus(string envelopeId, List<DocuSignRecipientUpdateDTO> recipientDTOs);
        #endregion

        #region Document Type
        Task<int> GetDocTypeIdFromFileName(string fileName);
        //Task<bool> IsImageType(int docTypeId);
        #endregion

        #region Fixed Folder/Documents
        Task<T> GetFixedDocDetail<T>(string id) where T : class;
        Task<T> GetIDSDetail<T>(string id) where T : class;

        #endregion

        #region Outlook Email
        Task<int> LogOutlookEmail(string userEmail, string systemType, string screenCode, DocFile docFile, DocOutlook docOutlook, KeyTextDTO[] selectedCases, KeyTextDTO[] selectedCasesPaths);
        Task<CaseLogDTO[]> GetOulookCaseLogByEmailId(int? cpiEmailId);
        #endregion

        #region Gmail Email
        Task<OutlookLinkedCases> SaveGmailEmail(string contentRootPath, string userEmail, string systemType, string screenCode, string selectedCases, string gmailMsgId, string msgSubject, string encodedMsg);

        Task<CaseLogDTO[]> GetGmailCaseLogByEmailId(string gmailMsgId);

        Task LogGmailEmail(List<DocGmailCaseLink> links);
        #endregion

        #region Responsible Docketing
        Task<List<string>> GetDocRespDocketingList(int docId);
        Task<List<string>> GetDocRespDocketingNameList(int docId);
        Task UpdateDocRespDocketing(List<string> responsibleList, string userName, int docId);
        Task DeleteDocRespDocketing(int docId);
        Task<DocResponsibleLog> GetDeletedDocRespDocketing(int docId);
        Task<DocResponsibleLog> GetAddedDocRespDocketing(int docId);
        Task LinkDocWithVerifications(int docId, string randomGuid);
        #endregion

        #region Responsible Reporting
        Task<List<string>> GetDocRespReportingList(int docId);
        Task<List<string>> GetDocRespReportingNameList(int docId);
        Task UpdateDocRespReporting(List<string> responsibleList, string userName, int docId);
        Task DeleteDocRespReporting(int docId);
        Task<DocResponsibleLog> GetDeletedDocRespReporting(int docId);
        Task<DocResponsibleLog> GetAddedDocRespReporting(int docId);
        //Task LinkDocWithVerifications(int docId, string randomGuid);
        #endregion

        #region Helper for Sharepoint integration
        Task<List<DocInfoDTO>> GetDocumentInfoFromDriveItemIds(List<string?> driveItemIds);
        Task<DocDocument> GetDefaultImage(string systemType, string dataKey, int dataKeyValue);
        #endregion

        Task<string?> GetParentDocumentLink(string documentLink);
        Task<string> GenerateFolderName(string documentLink);
        Task<(string? ClientCode, string? ClientName, string? MatterNumber)> GetClientMatter(string documentLink);
        Task SaveFolderStorageSetting(DocFolder folder);
        Task DeleteDocumentsByDriveItemId(string? driveItemId);
        Task DeleteDocumentsByFolderId(int folderId);       

        void DetachAllEntities();
        Task AddDocQuickEmailLogs(List<DocQuickEmailLog> docQuickEmailLogs);

        #region Trade Secret
        Task LogDocTradeSecretActivityByDocId(int docid);
        Task LogDocTradeSecretActivityByFileId(int fileId);
        Task LogDocTradeSecretActivityByFileIds(List<int> fileIds);
        Task LogDocTradeSecretActivityByFileName(string fileName);
        Task LogDocTradeSecretActivityByFileNames(List<string?> fileNames);
        Task LogDocTradeSecretActivityByDriveItemId(string driveItemId);
        Task LogDocTradeSecretActivityByDriveItemIds(List<string?> driveItemIds);
        #endregion
    }
}
