using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
// using R10.Core.Entities.DMS; // Removed during deep clean
// using R10.Core.Entities.PatClearance; // Removed during deep clean
// using R10.Core.Entities.Clearance; // Removed during deep clean
using R10.Core.Entities.Shared;
using R10.Core.Interfaces;
using R10.Core.Helpers;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Security.Claims;
using R10.Core.Identity;
using R10.Core.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using R10.Core.Interfaces.Patent;
using R10.Core.Services.Shared;
using System.Reflection.Metadata.Ecma335;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Data.SqlClient.Server;
using R10.Core.Entities.GlobalSearch;
using R10.Core.Interfaces.Shared;

namespace R10.Core.Services.Documents
{
    public class DocumentService : IDocumentService
    {
        private readonly IApplicationDbContext _repository;
        private readonly ISystemSettings<DefaultSetting> _settings;        
        private readonly ICPiUserSettingManager _userSettingManager;
        private readonly ClaimsPrincipal _user;
        private readonly ICountryApplicationService _applicationService;
        private readonly ITmkTrademarkService _trademarkService;
//         private readonly IGMMatterService _matterService; // Removed during deep clean
        private readonly ILoggerService<Log> _errorLogger;
        private readonly ITradeSecretService _tradeSecretService;

        private const string _defaultFolderName = "Incoming Email";

        public DocumentService(
            IApplicationDbContext repository,
            ISystemSettings<DefaultSetting> settings,            
            ICPiUserSettingManager userSettingManager,
            ClaimsPrincipal user,
            ICountryApplicationService applicationService,
            ITmkTrademarkService trademarkService,
//             IGMMatterService matterService, // Removed during deep clean
            ILoggerService<Log> errorLogger,
            ITradeSecretService tradeSecretService
            )
        {
            _repository = repository;
            _settings = settings;            
            _userSettingManager = userSettingManager;
            _user = user;
            _applicationService = applicationService;
            _trademarkService = trademarkService;
            // _matterService = matterService; // Removed during deep clean
            _errorLogger = errorLogger;
            _tradeSecretService = tradeSecretService;
        }

        public IQueryable<DocSystem> DocSystems => _repository.DocSystems.AsNoTracking();
        public IQueryable<DocMatterTree> DocMatterTrees => _repository.DocMatterTrees.AsNoTracking();

        public IQueryable<DocFolder> DocFolders => _repository.DocFolders.AsNoTracking();
        public IQueryable<DocDocument> DocDocuments => _repository.DocDocuments.AsNoTracking();
        public IQueryable<DocDocumentTag> DocDocumentTags => _repository.DocDocumentTags.AsNoTracking();
        public IQueryable<DocType> DocTypes => _repository.DocTypes.AsNoTracking();
        public IQueryable<DocFile> DocFiles => _repository.DocFiles.AsNoTracking();
        public IQueryable<DocIcon> DocIcons => _repository.DocIcons.AsNoTracking();

        public IQueryable<DocFixedFolder> DocFixedFolders => _repository.DocFixedFolders.AsNoTracking();
        public IQueryable<SharePointFileSignature> SharePointFileSignatures => _repository.SharePointFileSignatures.AsNoTracking();
        public IQueryable<DocFileSignature> DocFileSignatures => _repository.DocFileSignatures.AsNoTracking();
        public IQueryable<EFSLog> EFSLogs => _repository.EFSLogs.AsNoTracking();
        public IQueryable<LetterLog> LetterLogs => _repository.LetterLogs.AsNoTracking();
        public IQueryable<DocFileSignatureRecipient> DocFileSignatureRecipients => _repository.DocFileSignatureRecipients.AsNoTracking();

        public IQueryable<DocVerification> DocVerifications => _repository.DocVerifications.AsNoTracking();
        public IQueryable<DocResponsibleDocketing> DocRespDocketings => _repository.DocRespDocketings.AsNoTracking();
        public IQueryable<DocResponsibleReporting> DocRespReportings => _repository.DocRespReportings.AsNoTracking();
        public IQueryable<DocQuickEmailLog> DocQuickEmailLogs => _repository.DocQuickEmailLogs.AsNoTracking();
        public IQueryable<PatEPODocumentCombined> PatEPODocumentCombineds => _repository.PatEPODocumentCombineds.AsNoTracking();


        public async Task<bool> IsUserRestrictedFromPrivateDocuments()
        {
            var userAccountSettings = await _userSettingManager.GetUserSetting<UserAccountSettings>(_user.GetUserIdentifier());
            if (userAccountSettings?.RestrictPrivateDocuments == null)
                return false;
            return userAccountSettings.RestrictPrivateDocuments;
        }

        public async Task<int> AddFileToFileHandler(string fileName, string userName,string? driveItemId="")
        {
            var docFile = new DocFile
            {
                FileExt = Path.GetExtension(fileName).Replace(".", ""),
                UserFileName = fileName,
                DriveItemId = driveItemId,
                CreatedBy = userName,
                DateCreated = DateTime.Now,
                UpdatedBy = userName,
                LastUpdate = DateTime.Now
            };
            await AddDocFile(docFile);
            return docFile.FileId;
        }

        #region Document Tree
        public async Task<IEnumerable<DocTreeDTO>> GetDocumentTree(string systemType, string screenCode, string dataKey, int dataKeyValue, string id)
        {
            if (id == null)
                id = $"{systemType}|{screenCode}|{dataKey}|{dataKeyValue.ToString()}|||0|0";       // set initial id

            var subTree = await _repository.DocTreeDTO.FromSqlInterpolated($"Exec procDoc_TV @Id={id}").AsNoTracking().ToListAsync();
            return subTree;
        }
        public async Task<IEnumerable<DocTreeDTO>> GetApplicableDocTree(string systemType, string screenCode, string dataKey, int dataKeyValue, string id)
        {
            if (id == null)
                id = $"{systemType}|{screenCode}|{dataKey}|{dataKeyValue.ToString()}|||0|0";       // set initial id

            var subTree = await _repository.DocTreeDTO.FromSqlInterpolated($"Exec procDoc_TV_V2 @Id={id}").AsNoTracking().ToListAsync();
            return subTree;
        }

        public async Task<IEnumerable<DocTreeEmailApiDTO>> GetDocumentTreeEmailApi(string systemType, string screenCode, string dataKey, int dataKeyValue, string id)
        {
            if (id == null)
                id = $"{systemType}|{screenCode}|{dataKey}|{dataKeyValue.ToString()}|||0|0";       // set initial id
            else
                id = $"{systemType}|{screenCode}|{dataKey}|{dataKeyValue.ToString()}|||0|{id}";       // set initial id

            var subTree = await _repository.DocTreeEmailApiDTO.FromSqlInterpolated($"Exec procDoc_TVEmailApi @Id={id}").AsNoTracking().ToListAsync();
            return subTree;
        }

        public async Task<DocTreeDTO> AddTreeFolder(string id, string folderName, string userName)
        {
            var folderNode = _repository.DocTreeDTO.FromSqlInterpolated($"Exec procDoc_AddFolder @Id={id}, @FolderName={folderName}, @CreatedBy={userName}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return folderNode;
        }

        public async Task<DocFolder> AddFolder(string systemType, string dataKey, string screenCode, int dataKeyValue, string folderName, int parentId, bool isFixed)
        {
            var author = _user.GetEmail();
            var createdBy = _user.GetUserName();

            var folder = new DocFolder
            {
                SystemType = systemType,
                ScreenCode = screenCode,
                DataKey = dataKey,
                DataKeyValue = Convert.ToInt32(dataKeyValue),
                Author = author,
                FolderName = folderName,
                ParentFolderId = parentId,
                IsFixed = isFixed,
                CreatedBy = createdBy,
                UpdatedBy = createdBy,
                DateCreated = DateTime.Now,
                LastUpdate = DateTime.Now,
            };
            _repository.DocFolders.Add(folder);
            await _repository.SaveChangesAsync();
            _repository.Entry(folder).State = EntityState.Detached;
            return folder;
        }

        public async Task<bool> DeleteFolder(int id)
        {
            var folder = await GetFolderByIdAsync(id);
            if (folder == null) return false;

            _repository.DocFolders.Remove(folder);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDoc(DocDocument document, DocFile docFile)
        {
            _repository.DocDocuments.Remove(document);

            if (docFile != null)
                _repository.DocFiles.Remove(docFile);

            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<List<int>> GetFolderIds(int parentFolderId)
        {
            var folderIds = await _repository.LookupIntDTO.FromSqlInterpolated($"WITH cte_folder AS (SELECT FolderId FROM tblDocFolder WHERE FolderId={parentFolderId} UNION ALL SELECT f.FolderId FROM tblDocFolder f INNER JOIN cte_folder f2 ON f.ParentFolderId=f2.FolderId ) SELECT FolderId as Value FROM cte_folder").AsNoTracking().ToListAsync();
            return folderIds.Select(i => i.Value).ToList();
        }

        #endregion

        #region Folder
        public async Task<DocFolder> GetFolderByIdAsync(int folderId)
        {
            return await DocFolders.AsNoTracking().SingleOrDefaultAsync(f => f.FolderId == folderId);
        }

        public DocFolder GetFolderById(int folderId)       // used to return view data; had errors when run async
        {
            return DocFolders.AsNoTracking().SingleOrDefault(f => f.FolderId == folderId);
        }

        public async Task<DocFolderHeader> GetFolderHeader(int folderId)
        {
            return await DocFolders.Where(f => f.FolderId == folderId).AsNoTracking()
                        .Select(f => new DocFolderHeader { SystemType = f.SystemType, ScreenCode = f.ScreenCode, ParentId = f.DataKeyValue }).FirstOrDefaultAsync();
        }

        public async Task<bool> RenameFolder(string userName, int folderId, string newName)
        {
            // update specific fields
            var folder = await GetFolderByIdAsync(folderId);

            folder.FolderName = newName;
            folder.UpdatedBy = userName;
            folder.LastUpdate = DateTime.Now;

            var docFolder = _repository.DocFolders.Attach(folder);
            docFolder.Property(f => f.FolderName).IsModified = true;
            docFolder.Property(f => f.UpdatedBy).IsModified = true;
            docFolder.Property(f => f.LastUpdate).IsModified = true;
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateFolder(string userName, DocFolder folder)
        {
            // update specific fields
            folder.UpdatedBy = userName;
            folder.LastUpdate = DateTime.Now;

            var docFolder = _repository.DocFolders.Attach(folder);
            docFolder.Property(f => f.FolderName).IsModified = true;
            docFolder.Property(f => f.Remarks).IsModified = true;
            docFolder.Property(f => f.IsPrivate).IsModified = true;
            docFolder.Property(f => f.UpdatedBy).IsModified = true;
            docFolder.Property(f => f.LastUpdate).IsModified = true;
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task SaveFolderStorageSetting(DocFolder folder)
        {
            if (folder.FolderId == 0)
                _repository.DocFolders.Add(folder);
            else
            {
                var docFolder = _repository.DocFolders.Attach(folder);
                docFolder.Property(f => f.StorageRootContainerId).IsModified = true;
                docFolder.Property(f => f.StorageDefaultFolderId).IsModified = true;
            }

            await _repository.SaveChangesAsync();

            //detach entity to prevent ef tracking error
            _repository.Entry(folder).State = EntityState.Detached;
        }

        public async Task DeleteDocumentsByDriveItemId(string? driveItemId)
        {
            var docFiles = await _repository.DocFiles.Where(f => f.DriveItemId == driveItemId).ToListAsync();
            if (docFiles.Any())
            {
                var docDocuments = await _repository.DocDocuments.Where(d => docFiles.Select(f => f.FileId).ToList().Contains(d.FileId ?? 0)).ToListAsync();
                if (docDocuments.Any())
                    _repository.DocDocuments.RemoveRange(docDocuments);

                _repository.DocFiles.RemoveRange(docFiles);

                await _repository.SaveChangesAsync();
            }
        }

        public async Task DeleteDocumentsByFolderId(int folderId)
        {
            var docDocuments = await _repository.DocDocuments.Where(d => d.FolderId == folderId).ToListAsync();
            if (docDocuments.Any())
            {
                _repository.DocDocuments.RemoveRange(docDocuments);
                await _repository.SaveChangesAsync();
            }
        }

        public async Task<bool> UpdateFolders(string userName, IEnumerable<DocFolder> updatedFolders, IEnumerable<DocFolder> newFolders, IEnumerable<DocFolder> deletedFolders)
        {
            var lastUpdate = DateTime.Now;

            foreach (var item in updatedFolders)
            {
                item.UpdatedBy = userName;
                item.LastUpdate = lastUpdate;
            }

            foreach (var item in newFolders)
            {
                item.CreatedBy = userName;
                item.DateCreated = lastUpdate;
                item.UpdatedBy = userName;
                item.LastUpdate = lastUpdate;
            }

            var dbSet = _repository.DocFolders;

            if (updatedFolders.Any())
                dbSet.UpdateRange(updatedFolders);

            if (newFolders.Any())
                dbSet.AddRange(newFolders);

            if (deletedFolders.Any())
                dbSet.RemoveRange(deletedFolders);

            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<DocFolder> GetFolder(string systemType, string dataKey, int dataKeyValue, string folderName, int parentId)
        {
            var folder = await _repository.DocFolders.Where(f => f.SystemType == systemType && f.DataKey == dataKey && f.DataKeyValue == dataKeyValue && f.FolderName == folderName && f.ParentFolderId == parentId).AsNoTracking().FirstOrDefaultAsync();
            return folder;
        }

        private async Task<DocFolder> AddFolder(DocFolder docFolder)
        {
            _repository.DocFolders.Add(docFolder);
            await _repository.SaveChangesAsync();
            return docFolder;
        }

        #endregion

        #region Documents
        public async Task<DocDocument> GetDocumentById(int docId)
        {
            return await DocDocuments.SingleOrDefaultAsync(d => d.DocId == docId);
        }

        public async Task<bool> RenameDocument(string userName, int docId, string newName)
        {
            // update specific fields
            var document = await GetDocumentById(docId);

            document.DocName = newName;
            document.UpdatedBy = userName;
            document.LastUpdate = DateTime.Now;

            var docFolder = _repository.DocDocuments.Attach(document);
            docFolder.Property(d => d.DocName).IsModified = true;
            docFolder.Property(d => d.UpdatedBy).IsModified = true;
            docFolder.Property(d => d.LastUpdate).IsModified = true;
            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateDocuments(string userName, IEnumerable<DocDocument> updatedDocuments, IEnumerable<DocDocument> newDocuments, IEnumerable<DocDocument> deletedDocuments, DocFolder? docFolder = null, bool updateParentStamp = true)
        {
            int folderId = 0;

            if (docFolder == null)
            {
                if (newDocuments.Any())
                    folderId = newDocuments.First().FolderId;
                else if (updatedDocuments.Any())
                    folderId = updatedDocuments.First().FolderId;
                else if (deletedDocuments.Any())
                    folderId = deletedDocuments.First().FolderId;

                docFolder = await GetFolderByIdAsync(folderId);
            }

            await UpdateChild(userName, docFolder, updatedDocuments, newDocuments, deletedDocuments, updateParentStamp);
            return true;

        }

        private async Task<DocDocument> AddDocument(DocDocument docDocument)
        {
            _repository.DocDocuments.Add(docDocument);
            await _repository.SaveChangesAsync();
            return docDocument;
        }

        public async Task<string> IsLocked(int docId, string userName)
        {
            var image = await DocDocuments.FirstOrDefaultAsync(d => d.DocId == docId);
            if (image != null && !string.IsNullOrEmpty(image.LockedBy) && image.LockedBy != userName)
                return image.LockedBy;
            return string.Empty;
        }

        public async Task<string> CheckoutImage(int docId, string userName)
        {
            var image = await _repository.DocDocuments.FirstOrDefaultAsync(d => d.DocId == docId);
            if (image != null)
            {
                image.LockedBy = userName;
                await _repository.SaveChangesAsync();

                var docFile = await _repository.DocFiles.FirstOrDefaultAsync(f => f.FileId == image.FileId);
                return docFile.DocFileName;
            }
            return string.Empty;
        }

        public async Task UpdateTags(int docId, string[] tags) {
            //var documentTags = 
            //await _repository.DocDocumentTags.Where(t=> t.DocId==docId).ExecuteDeleteAsync();
            //await _repository.DocDocumentTags.AddRange(tags.Select(t=> new DocDocumentTag))
            //await _repository.SaveChangesAsync();
            
        }
        #endregion

        #region Verification
        public async Task<bool> HasDocVerification(string systemType, int actId, int id)
        {
            var screenCode = "";
            var dataKey = "";
            GetScreenCodeDataKey(systemType, ref screenCode, ref dataKey);

            return await _repository.DocVerifications
                                            .AnyAsync(d => d.ActId == actId
                                                && (d.DocDocument != null && d.DocDocument.DocFolder != null && d.DocDocument.DocFolder.DataKeyValue == id
                                                    && (d.DocDocument.DocFolder.DataKey ?? "").ToLower() == dataKey.ToLower()
                                                    && (d.DocDocument.DocFolder.ScreenCode ?? "").ToLower() == screenCode.ToLower()
                                                    && (d.DocDocument.DocFolder.SystemType ?? "").ToLower() == systemType.ToLower())
                                            );
        }
        public async Task AddDocVerifications(List<DocVerification> docVerifications)
        {
            if (docVerifications.Any())
            {
                var settings = await _settings.GetSetting();

                var patActIds = new List<int>();
                var tmkActIds = new List<int>();
                var gmActIds = new List<int>();

                foreach (var docVerification in docVerifications)
                {
                    var systemType = await _repository.DocDocuments.AsNoTracking().Where(d => d.DocId == docVerification.DocId).Select(d => d.DocFolder.SystemType).FirstOrDefaultAsync();
                    
                    if (string.IsNullOrEmpty(systemType)) continue;

                    if (systemType.ToLower() == "p" && docVerification.ActId > 0)
                        patActIds.Add(docVerification.ActId ?? 0);
                    else if (systemType.ToLower() == "t" && docVerification.ActId > 0)
                        tmkActIds.Add(docVerification.ActId ?? 0);
                    else if (systemType.ToLower() == "g" && docVerification.ActId > 0)
                        gmActIds.Add(docVerification.ActId ?? 0);
                }

                if (patActIds.Count > 0)
                {
                    var patActions = await _repository.PatActionDues.Where(a => patActIds.Contains(a.ActId)).ToListAsync();
                    if (patActions.Count > 0)
                    {
                        patActions.ForEach(a => { a.DateVerified = null; a.VerifiedBy = null; a.VerifierId = null; });
                    }
                }
                if (tmkActIds.Count > 0)
                {
                    var tmkActions = await _repository.TmkActionDues.Where(a => tmkActIds.Contains(a.ActId)).ToListAsync();
                    if (tmkActions.Count > 0)
                    {
                        tmkActions.ForEach(a => { a.DateVerified = null; a.VerifiedBy = null; a.VerifierId = null; });
                    }
                }
                // Removed during deep clean - GeneralMatter module removed
                // if (gmActIds.Count > 0)
                // {
                //     var gmActions = await _repository.GMActionsDue.Where(a => gmActIds.Contains(a.ActId)).ToListAsync();
                //     if (gmActions.Count > 0)
                //     {
                //         gmActions.ForEach(a => { a.DateVerified = null; a.VerifiedBy = null; a.VerifierId = null; });
                //     }
                // }

                _repository.DocVerifications.AddRange(docVerifications);
                await _repository.SaveChangesAsync();
            }
        }

        public async Task<bool> UpdateDocVerifications(int docId, string userName, IEnumerable<DocVerification> updated, IEnumerable<DocVerification> added, IEnumerable<DocVerification> deleted, string documentLink = "")
        {
            var parentId = 0;
            var systemType = "";
            var lastUpdate = DateTime.Now;
            var resetActIds = new List<int?>();
            var addedActIds = new List<int>();

            var dbSet = _repository.DocVerifications;

            if (docId > 0)
            {
                var docFolder = await _repository.DocFolders.AsNoTracking().Where(f => f.DocDocuments != null && f.DocDocuments.Any(d => d.DocId == docId)).Select(f => new { f.DataKeyValue, f.SystemType }).FirstOrDefaultAsync();
                parentId = docFolder != null ? docFolder.DataKeyValue : 0;
                systemType = docFolder != null ? docFolder.SystemType : "";
            }            
            else if (docId <= 0 && !string.IsNullOrEmpty(documentLink))
            {
                var docLinkArr = documentLink.Split("|");
                systemType = docLinkArr[0];
                if (int.TryParse(docLinkArr[3], out int tempId)) { parentId = tempId; }
            }
            
            foreach (var item in updated)
            {
                item.UpdatedBy = userName;
                item.LastUpdate = lastUpdate;
                item.DocDocument = null;                
                //Create new action due if BaseDate is provided with ActionTypeID
                if (item.ActionTypeID > 0 && item.BaseDate != null)
                {
                    var newActID = await GenerateAction(systemType ?? "", parentId, item.ActionTypeID ?? 0, (DateTime)item.BaseDate);
                    if (newActID > 0)
                    {
                        item.ActionTypeID = 0;
                        item.ActId = newActID;
                        addedActIds.Add(newActID);
                        item.WorkflowStatus = DocVerificationWorkflowStatus.ToBeProcess;
                    }
                }  
            }

            foreach (var item in added)
            {
                item.CreatedBy = userName;
                item.DateCreated = lastUpdate;
                item.UpdatedBy = userName;
                item.LastUpdate = lastUpdate;
                item.DocDocument = null;                
                //Create new action due if BaseDate is provided with ActionTypeID
                if (item.ActionTypeID > 0 && item.BaseDate != null)
                {
                    var newActID = await GenerateAction(systemType ?? "", parentId, item.ActionTypeID ?? 0, (DateTime)item.BaseDate);
                    if (newActID > 0)
                    {
                        item.ActionTypeID = 0;
                        item.ActId = newActID;
                        addedActIds.Add(newActID);
                        item.WorkflowStatus = DocVerificationWorkflowStatus.ToBeProcess;
                    }
                }
            }            
                        
            if (updated.Any())
            {
                var updatedVerifyIds = updated.Select(d => d.VerifyId).ToList();
                var existingVerifications = await _repository.DocVerifications.AsNoTracking().Where(d => updatedVerifyIds.Contains(d.VerifyId)).ToListAsync();
                if (existingVerifications.Any())
                {
                    resetActIds.AddRange(updated.Where(d => d.ActId > 0 && existingVerifications.Any(e => e.VerifyId == d.VerifyId && e.ActId != d.ActId)).Select(d => d.ActId).ToList());
                }
                dbSet.UpdateRange(updated);
            }

            if (added.Any())
            {
                resetActIds.AddRange(added.Where(d => d.ActId > 0 && !resetActIds.Contains(d.ActId)).Select(d => d.ActId).ToList());
                dbSet.AddRange(added);
            }

            if (deleted.Any())
            {
                resetActIds.AddRange(deleted.Where(d => d.ActId > 0 && !resetActIds.Contains(d.ActId)).Select(d => d.ActId).ToList());
                dbSet.RemoveRange(deleted);
            }
                

            //Clear Date Verified and VerifiedBy after updating/inserting new tblDocVerification with ActIds or unchecking "Docket Required" (IsActRequired)
            //Reset if "Check Docket" (CheckDocket) is not checked on tbl_ActionDue
            if (addedActIds.Count > 0) resetActIds.RemoveAll(d => addedActIds.Contains(d ?? 0));

            if (resetActIds.Count > 0 && !string.IsNullOrEmpty(systemType))
            {
                if (systemType.ToLower() == "p")
                {
                    var patActions = await _repository.PatActionDues.Where(a => resetActIds.Contains(a.ActId) && !a.CheckDocket).ToListAsync();
                    if (patActions.Count > 0)
                    {
                        patActions.ForEach(a => { a.DateVerified = null; a.VerifiedBy = null; a.VerifierId = null; });                        
                    }
                }
                else if (systemType.ToLower() == "t")
                {
                    var tmkActions = await _repository.TmkActionDues.Where(a => resetActIds.Contains(a.ActId) && !a.CheckDocket).ToListAsync();
                    if (tmkActions.Count > 0)
                    {
                        tmkActions.ForEach(a => { a.DateVerified = null; a.VerifiedBy = null; a.VerifierId = null; });                        
                    }
                }
                // Removed during deep clean - GeneralMatter module removed
                // else if (systemType.ToLower() == "g")
                // {
                //     var gmActions = await _repository.GMActionsDue.Where(a => resetActIds.Contains(a.ActId) && !a.CheckDocket).ToListAsync();
                //     if (gmActions.Count > 0)
                //     {
                //         gmActions.ForEach(a => { a.DateVerified = null; a.VerifiedBy = null; a.VerifierId = null; });
                //     }
                // }
            }

            await _repository.SaveChangesAsync();         

            return true;
        }

        /// <summary>
        /// Update date verified and verified by in tblDocVerification
        /// </summary>
        /// <param name="keyIds"></param>
        /// <param name="verifiedDate"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task UpdateVerificationActionVerify(List<string> keyIds, DateTime? verifiedDate, string? userName)
        {
            var patActions = new List<PatActionDue>();
            var tmkActions = new List<TmkActionDue>();
//             var gmActions = new List<GMActionDue>(); // Removed during deep clean
            foreach (var item in keyIds)
            {
                var keyArr = item.Split("|");
                var systemType = keyArr[0];
                var actId = keyArr[1];
                var keyId = 0;
                var temp = int.TryParse(actId, out keyId);

                //Update DateVerified, VerifiedBy, and VerifierId in tbl_ActionDue
                if (systemType.ToLower() == "p")
                {
                    patActions.AddRange(await _repository.PatActionDues.Where(d => d.ActId == keyId).ToListAsync());
                }
                if (systemType.ToLower() == "t")
                {
                    tmkActions.AddRange(await _repository.TmkActionDues.Where(d => d.ActId == keyId).ToListAsync());
                }
                // Removed during deep clean - GeneralMatter module removed
                // if (systemType.ToLower() == "g")
                // {
                //     gmActions.AddRange(await _repository.GMActionsDue.Where(d => d.ActId == keyId).ToListAsync());
                // }
            }
            var userId = _user.GetUserIdentifier();

            if (patActions.Count > 0)
            {
                patActions.ForEach(a => { a.DateVerified = verifiedDate; a.VerifiedBy = userName; a.VerifierId = userId; });
                await _repository.SaveChangesAsync();                
            }
            if (tmkActions.Count > 0)
            {
                tmkActions.ForEach(a => { a.DateVerified = verifiedDate; a.VerifiedBy = userName; a.VerifierId = userId; });
                await _repository.SaveChangesAsync();
            }
            // Removed during deep clean - GeneralMatter module removed
            // if (gmActions.Count > 0)
            // {
            //     gmActions.ForEach(a => { a.DateVerified = verifiedDate; a.VerifiedBy = userName; a.VerifierId = userId; });
            //     await _repository.SaveChangesAsync();
            // }
        }

        /// <summary>
        /// Update ActId after creating action(s) and set ActionTypeId to 0
        /// </summary>
        /// <param name="systemType"></param>
        /// <param name="parentId"></param>
        /// <param name="actId"></param>
        /// <param name="actionTypeId"></param>
        /// <returns></returns>
        public async Task UpdateVerificationActId(string systemType, int parentId, int actId, int actionTypeId)
        {            
            var verifications = new List<DocVerification>();
            var screenCode = "";
            var dataKey = "";
            GetScreenCodeDataKey(systemType, ref screenCode, ref dataKey);
            verifications = await _repository.DocVerifications
                                            .Where(dv => dv.ActionTypeID == actionTypeId
                                                && dv.DocDocument != null && dv.DocDocument.DocFolder != null && dv.DocDocument.DocFolder.DataKeyValue == parentId
                                                && (dv.DocDocument.DocFolder.DataKey ?? "").ToLower() == dataKey.ToLower()
                                                && (dv.DocDocument.DocFolder.ScreenCode ?? "").ToLower() == screenCode.ToLower()
                                                && (dv.DocDocument.DocFolder.SystemType ?? "").ToLower() == systemType.ToLower()                                                
                                            ).ToListAsync();

            if (verifications.Any())
            {
                verifications.ForEach(v => { v.ActionTypeID = 0; v.ActId = actId; v.LastUpdate = DateTime.Now; });
                await _repository.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Update ActionTypeId before removing action(s) and set ActId to 0
        /// </summary>
        /// <param name="systemType"></param>
        /// <param name="parentId"></param>
        /// <param name="actId"></param>
        /// <param name="actionTypeId"></param>
        /// <returns></returns>
        public async Task UpdateVerificationActionTypeId(string systemType, int parentId, int actId, int actionTypeId)
        {                    
            var verifications = new List<DocVerification>();
            var screenCode = "";
            var dataKey = "";
            GetScreenCodeDataKey(systemType, ref screenCode, ref dataKey);
            verifications = await _repository.DocVerifications
                                            .Where(dv => dv.ActId == actId
                                                && dv.DocDocument != null && dv.DocDocument.DocFolder != null && dv.DocDocument.DocFolder.DataKeyValue == parentId
                                                && (dv.DocDocument.DocFolder.DataKey ?? "").ToLower() == dataKey.ToLower()
                                                && (dv.DocDocument.DocFolder.ScreenCode ?? "").ToLower() == screenCode.ToLower()
                                                && (dv.DocDocument.DocFolder.SystemType ?? "").ToLower() == systemType.ToLower()
                                            ).ToListAsync();

            if (verifications.Any())
            {
                verifications.ForEach(v => { v.ActionTypeID = actionTypeId; v.ActId = 0; });
                await _repository.SaveChangesAsync();
            }
        }

        private void GetScreenCodeDataKey(string systemType, ref string screenCode, ref string dataKey)
        {
            switch (systemType.ToLower())
            {
                case "p":
                    {
                        screenCode = "CA";
                        dataKey = "AppId";
                        break;
                    }
                case "t":
                    {
                        screenCode = "Tmk";
                        dataKey = "TmkId";
                        break;
                    }
                case "g":
                    {
                        screenCode = "GM";
                        dataKey = "MatId";
                        break;
                    }
                default: { break; }
            }
        }

        private async Task<int> GenerateAction(string systemType, int parentId, int actionTypeId, DateTime baseDate)
        {
            int newActId = 0;

            if (systemType.ToLower() == "p")
            {
                await _applicationService.GenerateWorkflowAction(parentId, actionTypeId, baseDate);                        
                var patActionType = await _repository.PatActionTypes.AsNoTracking().Where(d => d.ActionTypeID == actionTypeId).FirstOrDefaultAsync();                        
                if (patActionType == null) return newActId;

                newActId = await _repository.PatActionDues.AsNoTracking().Where(d => d.ActionType == patActionType.ActionType && d.BaseDate.Date == baseDate.Date).Select(d => d.ActId).FirstOrDefaultAsync();                 
            }
            else if (systemType.ToLower() == "t")
            {
                await _trademarkService.GenerateWorkflowAction(parentId, actionTypeId, baseDate);
                var tmkActionType = await _repository.TmkActionTypes.AsNoTracking().Where(d => d.ActionTypeID == actionTypeId).FirstOrDefaultAsync();                        
                if (tmkActionType == null) return newActId;

                newActId = await _repository.TmkActionDues.AsNoTracking().Where(d => d.ActionType == tmkActionType.ActionType && d.BaseDate.Date == baseDate.Date).Select(d => d.ActId).FirstOrDefaultAsync(); 
            }
            // Removed during deep clean - GeneralMatter module removed
            // else if (systemType.ToLower() == "g")
            // {
            //     await _matterService.GenerateWorkflowAction(parentId, actionTypeId, baseDate);
            //     var gmActionType = await _repository.GMActionTypes.AsNoTracking().Where(d => d.ActionTypeID == actionTypeId).FirstOrDefaultAsync();
            //     if (gmActionType == null) return newActId;
            //
            //     newActId = await _repository.GMActionsDue.AsNoTracking().Where(d => d.ActionType == gmActionType.ActionType && d.BaseDate.Date == baseDate.Date).Select(d => d.ActId).FirstOrDefaultAsync();
            // }

            return newActId;
        }

        public async Task MarkVerificationNewActionAsProcessed(int actId, int docId)
        {
            await _repository.DocVerifications.Where(f => f.DocId == docId && f.ActId == actId)
                    .ExecuteUpdateAsync(f => f.SetProperty(p => p.WorkflowStatus, p => DocVerificationWorkflowStatus.Processed));
        }        
        #endregion

        #region File
        public async Task DeleteDocFile(DocFile docFile)
        {            
            _repository.DocFiles.Remove(docFile);
            await _repository.SaveChangesAsync();
        }

        public async Task<DocFile> AddDocFile(DocFile docFile)
        {
            _repository.DocFiles.Add(docFile);
            await _repository.SaveChangesAsync();
            return docFile;
        }

        public async Task UpdateDocFile(DocFile docFile)
        {
            _repository.DocFiles.Update(docFile);
            await _repository.SaveChangesAsync();
        }

        public async Task<DocFile> GetFileById(int fileId)
        {
            var docFile = await DocFiles.SingleOrDefaultAsync(f => f.FileId == fileId);
            return docFile;
        }

        public async Task<DocFile> GetFileByFileName(string fileName)
        {
            var docFile = await DocFiles.SingleOrDefaultAsync(f => f.DocFileName==fileName);
            return docFile;
        }

        public async Task<string> GetFileNameById(int fileId)
        {
            return await DocFiles.Where(f => f.FileId == fileId).Select(f => f.DocFileName).FirstOrDefaultAsync();
        }

        public async Task<DocViewDTO> GetDocViewInfo(string id)
        {
            var docInfo = await _repository.DocViewDTO.FromSqlInterpolated($"Exec procDoc_DocView @Id={id}").AsNoTracking().FirstOrDefaultAsync();
            return docInfo;
        }

        public async Task<int> GetFileOtherRefCount(int docId, int fileId)
        {
            var fileCount = await DocDocuments.Where(d => d.FileId == fileId && d.DocId != docId).CountAsync();
            return fileCount;
        }
        public async Task MarkForSignature(int fileId, string documentLink, int qeSetupId, string roleLink, bool isDMSInventorSignature = false)
        {
            if (!_repository.DocFileSignatures.Any(s => s.FileId == fileId))
            {
                var documentLinkArray = documentLink.Split("|");
                var systemType = documentLinkArray[0];
                var screenCode = documentLinkArray[1];
                var dataKey = documentLinkArray[2];
                var dataKeyValue = Convert.ToInt32(documentLinkArray[3]);

                var settings = await _settings.GetSetting();
                var isReviewed = false;

                if (!settings.IsESignatureReviewOn)
                    isReviewed = true;

                await _repository.DocFiles.Where(f => f.FileId == fileId).ExecuteUpdateAsync(f => f.SetProperty(p => p.ForSignature, p => true));

                var newDocFileSignature = new DocFileSignature
                {
                    FileId = fileId,
                    SystemType = systemType,
                    ScreenCode = screenCode,
                    DataKey = dataKey,
                    DataKeyValue = dataKeyValue,
                    QESetupId = qeSetupId,
                    RoleLink = roleLink,
                    CreatedBy = _user.GetUserName(),
                    UpdatedBy = _user.GetUserName(),
                    DateCreated = DateTime.Now,
                    LastUpdate = DateTime.Now,
                    SignatureReviewed= isReviewed
                };
                _repository.DocFileSignatures.Add(newDocFileSignature);
                await _repository.SaveChangesAsync();
                
                // Removed during deep clean - DMS/Disclosure module removed
                // if (isDMSInventorSignature)
                // {
                //     await _repository.Disclosures.Where(d => d.DMSId == dataKeyValue).ExecuteUpdateAsync(d => d.SetProperty(p => p.SignatureFileId, p => newDocFileSignature.SignatureFileId));
                // }
            }
        }

        public async Task MarkSharePointFileForSignature(string driveItemId, string docLibrary, string docLibraryFolder, string recKey, string screenCode, int qeSetupId,string fileName, 
                                                         DateTime? docDate, int parentId,string systemTypeCode, string? roleLink, bool isDMSInventorSignature = false) {

            var settings = await _settings.GetSetting();
            var isReviewed = false;

            if (!settings.IsESignatureReviewOn)
                isReviewed = true;

            var newSharePointFileSignature = new SharePointFileSignature
            {
                DriveItemId= driveItemId,
                DocLibrary=docLibrary,
                DocLibraryFolder=docLibraryFolder,
               // RecKey=recKey,
                ScreenCode = screenCode,
                QESetupId = qeSetupId,
                FileName=fileName,
                FileDate=docDate,
                ParentId=parentId,
                SystemTypeCode=systemTypeCode,
                RoleLink=roleLink,
                CreatedBy = _user.GetUserName(),
                UpdatedBy = _user.GetUserName(),
                DateCreated = DateTime.Now,
                LastUpdate = DateTime.Now,
                SignatureReviewed = isReviewed
            };
            _repository.SharePointFileSignatures.Add(newSharePointFileSignature);
            await _repository.SaveChangesAsync();

            // Removed during deep clean - DMS/Disclosure module removed
            // if (isDMSInventorSignature)
            // {
            //     await _repository.Disclosures.Where(d => d.DMSId == parentId).ExecuteUpdateAsync(d => d.SetProperty(p => p.SignatureFileId, p => newSharePointFileSignature.SignatureFileId));
            // }
        }


        public async Task SetEnvelopeId(int fileId, string envelopeId)
        {
            await _repository.DocFileSignatures.Where(f => f.FileId == fileId).ExecuteUpdateAsync(f => f.SetProperty(p => p.EnvelopeId, p => envelopeId));
        }
        public async Task SetEnvelopeIdForSharePointFile(string itemId, string envelopeId)
        {
            await _repository.SharePointFileSignatures.Where(f => f.DriveItemId==itemId).ExecuteUpdateAsync(f => f.SetProperty(p => p.EnvelopeId, p => envelopeId));
        }
        public async Task SetEnvelopeIdForLetterFile(int letLogId, string envelopeId)
        {
            await _repository.LetterLogs.Where(f => f.LetLogId == letLogId).ExecuteUpdateAsync(f =>
              f.SetProperty(p => p.EnvelopeId, p => envelopeId)
               .SetProperty(p => p.SentToDocuSign, true));
        }
        public async Task SetEnvelopeIdForEFSFile(int efsLogId, string envelopeId)
        {
            await _repository.EFSLogs.Where(f => f.EfsLogId==efsLogId).ExecuteUpdateAsync(f =>
              f.SetProperty(p => p.EnvelopeId, p => envelopeId)
               .SetProperty(p => p.SentToDocuSign, true));
        }
        public async Task MarkSignedDoc(int sourcefileId, int signedFileId)
        {
            await _repository.DocFileSignatures.Where(f => f.FileId == sourcefileId).ExecuteUpdateAsync(f =>
            f.SetProperty(p => p.SignatureCompleted, true).SetProperty(p => p.SignedDocFileId, signedFileId)
             .SetProperty(p => p.UpdatedBy, _user.GetUserName()).SetProperty(p => p.LastUpdate, DateTime.Now));
        }
        public async Task MarkSignedDocForSharePoint(string sourceDriveItemId, string signedDriveItemId, string fileName)
        {
            await _repository.SharePointFileSignatures.Where(f => f.DriveItemId == sourceDriveItemId).ExecuteUpdateAsync(f =>
            f.SetProperty(p => p.SignatureCompleted, true).SetProperty(p => p.SignedDocDriveItemId, signedDriveItemId)
             .SetProperty(p => p.SignedFileName, fileName)
             .SetProperty(p => p.UpdatedBy, _user.GetUserName()).SetProperty(p => p.LastUpdate, DateTime.Now));
        }
        public async Task DeleteSharePointForSignature(string driveItemId) {
            await _repository.SharePointFileSignatures.Where(f => f.DriveItemId == driveItemId).ExecuteDeleteAsync();
        }

        public async Task MarkSignedLetter(int sourceLetLogId,string newLetterFile,string itemId, string userName)
        {
            var origLog = await _repository.LetterLogs.FirstOrDefaultAsync(l => l.LetLogId == sourceLetLogId);
            if (origLog != null)
            {
                origLog.SignatureCompleted = true;
                origLog.SignedFileName = newLetterFile;
                origLog.SignedDocDriveItemId = itemId;

                var clone = await _repository.LetterLogs.Include(l => l.LetterLogDetails).AsNoTracking().FirstOrDefaultAsync(l => l.LetLogId == sourceLetLogId);
                clone.LetLogId = 0;
                clone.LetFile = newLetterFile;
                clone.GenDate = DateTime.Now;
                clone.GenBy = userName;
                clone.ItemId = itemId;
                clone.SentToDocuSign = false;
                clone.SignatureCompleted = false;

                foreach (var item in clone.LetterLogDetails) { 
                    item.LetLogId = 0;
                    item.LogDtlId = 0;
                }
                await _repository.LetterLogs.AddAsync(clone);
                await _repository.SaveChangesAsync();
                await _repository.LetterLogs.Where(l => l.LetLogId == origLog.LetLogId).ExecuteUpdateAsync(p => p.SetProperty(x => x.SignedLetLogId, x => clone.LetLogId));

            }
        }

        public async Task MarkSignedEFSLog(int sourceEfsLogId, string newEfsFile, string itemId, string userName)
        {
            var origLog = await _repository.EFSLogs.FirstOrDefaultAsync(l => l.EfsLogId == sourceEfsLogId);
            if (origLog != null)
            {
                origLog.SignatureCompleted = true;
                origLog.SignedFileName = newEfsFile;
                origLog.SignedDocDriveItemId = itemId;

                var clone = await _repository.EFSLogs.AsNoTracking().FirstOrDefaultAsync(l => l.EfsLogId == sourceEfsLogId);
                clone.EfsLogId = 0;
                clone.EfsFile = newEfsFile;
                clone.GenDate = DateTime.Now;
                clone.GenBy = userName;
                clone.ItemId = itemId;
                clone.SentToDocuSign = false;
                clone.SignatureCompleted = false;
                await _repository.EFSLogs.AddAsync(clone);
                await _repository.SaveChangesAsync();
                await _repository.EFSLogs.Where(l => l.EfsLogId == origLog.EfsLogId).ExecuteUpdateAsync(p => p.SetProperty(x => x.SignedEfsLogId, x => clone.EfsLogId));
            }
        }

        public async Task AddSignatureRecipients(List<DocFileSignatureRecipient> recipients, string envelopeId)
        {
            if (!_repository.DocFileSignatureRecipients.Any(s => s.EnvelopeId == envelopeId))
            {       
                recipients.ForEach(d => { d.CreatedBy = d.UpdatedBy = _user.GetUserName(); d.DateCreated = d.LastUpdate = DateTime.Now; });
                _repository.DocFileSignatureRecipients.AddRange(recipients);
                await _repository.SaveChangesAsync();                
            }
        }

        public async Task UpdateSharePointSignatureStatus(string envelopeId, string status)
        {
            await _repository.SharePointFileSignatures.Where(d => d.EnvelopeId == envelopeId)
                .ExecuteUpdateAsync(f => f.SetProperty(p => p.EnvelopeStatus, status));
        }

        public async Task UpdateFileSignatureStatus(string envelopeId, string status)
        {            
            await _repository.DocFileSignatures.Where(d => d.EnvelopeId == envelopeId)
                .ExecuteUpdateAsync(f => f.SetProperty(p => p.EnvelopeStatus, status));
        }

        public async Task UpdateLetSignatureStatus(string envelopeId, string status)
        {
            await _repository.LetterLogs.Where(d => d.EnvelopeId == envelopeId)
                .ExecuteUpdateAsync(f => f.SetProperty(p => p.EnvelopeStatus, status));
        }

        public async Task UpdateEFSSignatureStatus(string envelopeId, string status)
        {
            await _repository.EFSLogs.Where(d => d.EnvelopeId == envelopeId)
                .ExecuteUpdateAsync(f => f.SetProperty(p => p.EnvelopeStatus, status));
        }

        public async Task UpdateSignatureRecipientStatus(string envelopeId, List<DocuSignRecipientUpdateDTO> recipientDTOs)
        {
            var recipientIds = recipientDTOs.Select(d => d.RecipientId).Distinct().ToList();
            var recipients = await _repository.DocFileSignatureRecipients.Where(d => d.EnvelopeId == envelopeId && recipientIds.Contains(d.RecipientId ?? 0)).ToListAsync();

            if (recipients != null && recipients.Count > 0)
            {
                foreach (var recipient in recipients)
                {
                    var recipientDTO = recipientDTOs.Where(d => d.RecipientId == recipient.RecipientId).FirstOrDefault();
                    if (recipientDTO != null)
                    {
                        recipient.Status = recipientDTO.Status;
                        recipient.SentDate = recipientDTO.sentDateTime;
                        recipient.SignedDate = recipientDTO.signedDateTime;
                    }                        
                }

                _repository.DocFileSignatureRecipients.UpdateRange(recipients);
                await _repository.SaveChangesAsync();                
            }            
        }
        #endregion

        #region Document Type
        public async Task<int> GetDocTypeIdFromFileName(string fileName)
        {
            int docTypeId = 0;

            // assign default doctype for file
            docTypeId = DocTypes.Where(dt => dt.EvalOrder == 0).Select(dt => dt.DocTypeId).FirstOrDefault();

            // check if there is a more fitting doctype
            var docType = await DocTypes.Where(dt => dt.EvalOrder != 0 && dt.RegExFilter.Length > 0).OrderBy(dt => dt.EvalOrder).ToListAsync();
            foreach (var dt in docType)
            {
                Regex docRegex = new Regex(dt.RegExFilter);
                if (docRegex.IsMatch(fileName))
                {
                    docTypeId = dt.DocTypeId;
                    break;
                }
            }

            return docTypeId;
        }

        #endregion

        #region Fixed Folder/Documents
        //public async Task<DocImageDetailDTO> GetDocImageDetail(string id)

        public async Task<T> GetFixedDocDetail<T>(string id) where T : class
        {
            var dbQuery = _repository.Set<T>();
            var docDetail = dbQuery.FromSqlInterpolated($"Exec procDoc_FixedDocDetail @Id={id}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return docDetail;
        }

        public async Task<T> GetIDSDetail<T>(string id) where T : class
        {
            var dbQuery = _repository.Set<T>();
            var docDetail = dbQuery.FromSqlInterpolated($"Exec procDoc_IDSDetail @Id={id}").AsNoTracking().AsEnumerable().FirstOrDefault();
            return docDetail;
        }


        #endregion

        #region Update Child & Parent Stamps
        private async Task UpdateChild(string userName, DocFolder mainRecord, IEnumerable<DocDocument> updated, IEnumerable<DocDocument> added, IEnumerable<DocDocument> deleted, bool updateParentStamp = true)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                //mainRecord.UpdatedBy = userName;
                //mainRecord.LastUpdate = DateTime.Now;
                var lastUpdate = DateTime.Now;

                foreach (var item in updated)
                {
                    item.UpdatedBy = userName;
                    //item.LastUpdate = mainRecord.LastUpdate;
                    item.LastUpdate = lastUpdate;
                }

                foreach (var item in added)
                {
                    //item.CreatedBy = mainRecord.UpdatedBy;
                    //item.DateCreated = mainRecord.LastUpdate;
                    //item.UpdatedBy = mainRecord.UpdatedBy;
                    //item.LastUpdate = mainRecord.LastUpdate;
                    item.CreatedBy = userName;
                    item.DateCreated = lastUpdate;
                    item.UpdatedBy = userName;
                    item.LastUpdate = lastUpdate;
                }

                // update parent folder stamp fields
                mainRecord.UpdatedBy = userName;
                mainRecord.LastUpdate = lastUpdate;

                var parentSet = _repository.Set<DocFolder>();
                var parent = parentSet.Attach(mainRecord);

                parent.Property(c => c.UpdatedBy).IsModified = true;
                parent.Property(c => c.LastUpdate).IsModified = true;

                var dbSet = _repository.Set<DocDocument>();
                if (updated.Any())
                    dbSet.UpdateRange(updated);

                if (added.Any())
                    dbSet.AddRange(added);

                if (deleted.Any())
                    dbSet.RemoveRange(deleted);

                if (updated.Any(u => u.IsDefault) || added.Any(a => a.IsDefault))
                {
                    DocDocument defaultDoc;
                    defaultDoc = updated.Where(u => u.IsDefault).FirstOrDefault();
                    if (defaultDoc == null)
                        defaultDoc = added.Where(a => a.IsDefault).FirstOrDefault();
                                        
                    if (mainRecord.DataKeyValue > 0)
                        await ClearDefaultImage(mainRecord.SystemType, mainRecord.DataKey, mainRecord.DataKeyValue, defaultDoc.DocId);
                }
                if (mainRecord.DataKeyValue > 0 && updateParentStamp)
                    await UpdateParentStamp(mainRecord);

                await _repository.SaveChangesAsync();
                scope.Complete();

                //detach to prevent error during multiple updates from same thread
                _repository.Entry(mainRecord).State = EntityState.Detached;
            }
            if (mainRecord.DataKeyValue > 0)
                await SyncChildToDesignatedRecords(mainRecord);
        }
        private async Task ClearDefaultImage(string systemType, string dataKey, int dataKeyValue, int docId)
        {
            var result = await _repository.Database.ExecuteSqlInterpolatedAsync($"Update doc Set doc.IsDefault=0 from tblDocDocument doc Inner Join tblDocFolder f on f.FolderId=doc.FolderId Where f.SystemType={systemType} and f.DataKey={dataKey} and f.DataKeyValue={dataKeyValue} and doc.DocId <> {docId}");
        }

        private async Task UpdateParentStamp(DocFolder folder)
        {

            switch (folder.SystemType.ToUpper())
            {
                case "P":
                    await UpdatePatentParentStamp(folder);
                    break;

                case "T":
                    await UpdateTrademarkParentStamp(folder);
                    break;

                case "G":
                    await UpdateGeneralMatterParentStamp(folder);
                    break;

                case "D":
                    await UpdateDMSParentStamp(folder);
                    break;

                case "E":
                    await UpdatePatClearanceParentStamp(folder);
                    break;

                case "C":
                    await UpdateTmkSearchParentStamp(folder);
                    break;

            }
        }

        private async Task UpdatePatentParentStamp(DocFolder folder)
        {
            BaseEntity mainRecord = null;
            switch (folder.ScreenCode.ToUpper())
            {
                case "INV":
                    mainRecord = await _repository.Inventions.FirstOrDefaultAsync(r => r.InvId == folder.DataKeyValue);
                    break;

                case "CA":
                    mainRecord = await _repository.CountryApplications.FirstOrDefaultAsync(r => r.AppId == folder.DataKeyValue);
                    break;

                case "ACT":
                    mainRecord = await _repository.PatActionDues.FirstOrDefaultAsync(r => r.ActId == folder.DataKeyValue);
                    break;

                case "ACTINV":
                    mainRecord = await _repository.PatActionDueInvs.FirstOrDefaultAsync(r => r.ActId == folder.DataKeyValue);
                    break;

                case "COST":
                    mainRecord = await _repository.PatCostTracks.FirstOrDefaultAsync(r => r.CostTrackId == folder.DataKeyValue);
                    break;

                case "COSTINV":
                    mainRecord = await _repository.PatCostTrackInvs.FirstOrDefaultAsync(r => r.CostTrackInvId == folder.DataKeyValue);
                    break;
            }
            if (mainRecord != null)
            {
                UpdateStamp(folder.UpdatedBy, mainRecord);
            }
        }

        private async Task UpdateTrademarkParentStamp(DocFolder folder)
        {
            BaseEntity mainRecord = null;
            switch (folder.ScreenCode.ToUpper())
            {
                case "TMK":
                    mainRecord = await _repository.TmkTrademarks.FirstOrDefaultAsync(r => r.TmkId == folder.DataKeyValue);
                    break;

                case "ACT":
                    mainRecord = await _repository.TmkActionDues.FirstOrDefaultAsync(r => r.ActId == folder.DataKeyValue);
                    break;

                case "COST":
                    mainRecord = await _repository.TmkCostTracks.FirstOrDefaultAsync(r => r.CostTrackId == folder.DataKeyValue);
                    break;
            }
            if (mainRecord != null)
            {
                UpdateStamp(folder.UpdatedBy, mainRecord);
            }
        }

        // Removed during deep clean - GeneralMatter module removed
        private async Task UpdateGeneralMatterParentStamp(DocFolder folder)
        {
            // BaseEntity mainRecord = null;
            // switch (folder.ScreenCode.ToUpper())
            // {
            //     case "GM":
            //         mainRecord = await _repository.GMMatters.FirstOrDefaultAsync(r => r.MatId == folder.DataKeyValue);
            //         break;
            //
            //     case "ACT":
            //         mainRecord = await _repository.GMActionsDue.FirstOrDefaultAsync(r => r.ActId == folder.DataKeyValue);
            //         break;
            //
            //     case "COST":
            //         mainRecord = await _repository.GMCostTracks.FirstOrDefaultAsync(r => r.CostTrackId == folder.DataKeyValue);
            //         break;
            // }
            // if (mainRecord != null)
            // {
            //     UpdateStamp(folder.UpdatedBy, mainRecord);
            // }
            await Task.CompletedTask;
        }

        // Removed during deep clean - DMS/Disclosure module removed
        private async Task UpdateDMSParentStamp(DocFolder folder)
        {
            // if (folder.ScreenCode.ToUpper() == "DMS")
            // {
            //     var mainRecord = await _repository.Disclosures.FirstOrDefaultAsync(r => r.DMSId == folder.DataKeyValue);
            //     if (mainRecord != null)
            //     {
            //         UpdateStamp(folder.UpdatedBy, mainRecord);
            //     }
            // }
            await Task.CompletedTask;
        }

        // Removed during deep clean - PatClearance module removed
        private async Task UpdatePatClearanceParentStamp(DocFolder folder)
        {
            // if (folder.ScreenCode.ToUpper() == "PAC")
            // {
            //     var mainRecord = await _repository.PacClearances.FirstOrDefaultAsync(r => r.PacId == folder.DataKeyValue);
            //     if (mainRecord != null)
            //     {
            //         UpdateStamp(folder.UpdatedBy, mainRecord);
            //     }
            // }
            await Task.CompletedTask;
        }

        // Removed during deep clean - TmkClearance module removed
        private async Task UpdateTmkSearchParentStamp(DocFolder folder)
        {
            // if (folder.ScreenCode.ToUpper() == "TMC")
            // {
            //     var mainRecord = await _repository.TmcClearances.FirstOrDefaultAsync(r => r.TmcId == folder.DataKeyValue);
            //     if (mainRecord != null)
            //     {
            //         UpdateStamp(folder.UpdatedBy, mainRecord);
            //     }
            // }
            await Task.CompletedTask;
        }

        private void UpdateStamp<T>(string userName, T entity) where T : BaseEntity
        {
            entity.UpdatedBy = userName;
            entity.LastUpdate = DateTime.Now;
        }

        private async Task SyncChildToDesignatedRecords(DocFolder folder)
        {
            var screenCode = folder.ScreenCode?.ToUpper();
            if (screenCode == "CA")
            {
                var app = await _repository.CountryApplications.AsNoTracking().FirstOrDefaultAsync(r => r.AppId == folder.DataKeyValue);
                if (app != null)
                    await _applicationService.SyncChildToDesignatedApplications(app.AppId, app.Country, app.CaseType ?? "", folder.UpdatedBy ?? "", typeof(DocFolder));
            }
            else if (screenCode == "TMK")
            {
                var tmk = await _repository.TmkTrademarks.AsNoTracking().FirstOrDefaultAsync(r => r.TmkId == folder.DataKeyValue);
                if (tmk != null)
                    await _trademarkService.SyncChildToDesignatedTrademarks(tmk, new TmkTrademarkModifiedFields(), typeof(DocFolder));
            }
        }

        #endregion

        #region Outlook Email
        public async Task<int> LogOutlookEmail(string userEmail, string systemType, string screenCode, DocFile docFile, DocOutlook docOutlook, KeyTextDTO[] selectedCases, KeyTextDTO[] selectedCasesPaths)
        {
            // save to docmgt 
            var docMgtCases = await SaveOutlookToDocMgt(userEmail, systemType, screenCode, docFile, selectedCases, selectedCasesPaths);

            // add to Outlook log tables
            var cpiEmailId = await LogOutLookLinks(docFile.CreatedBy, systemType, docOutlook, docMgtCases);

            return cpiEmailId;
        }

        private async Task<OutlookLinkedCases> SaveOutlookToDocMgt(string userEmail, string systemType, string screenCode, DocFile docFile, KeyTextDTO[] selectedCases, KeyTextDTO[] selectedCasesPaths)
        {
            // note: email file created/saved in controller; docFile record created in controller
            var userName = docFile.CreatedBy;
            var docName = docFile.UserFileName.Substring(0, docFile.UserFileName.Length - 4);
            var docTypeId = await GetDocTypeIdFromFileName("x.msg");


            // then, save to docmgt tree of each cases in selectedCases; create folders if necessary
            // get docmgt folder info; add, if not yet existing
            var settings = await _settings.GetSetting();
            //var folderName = settings.EmailDocumentFolder;
            //if (folderName == null) folderName = _defaultFolderName;

            // create document for message for each cases
            var processedCases = new OutlookLinkedCases
            {
                FileId = docFile.FileId,
                DataKeyValue = new List<OutlookProcessedCases>()
            };

            string strDocIds = "";      // (2021-dec-02) retro-fix on null FileId for multiple cases saving; can't change the EFCore structure (n:n on DocDocument:DocFile) now since so many other logic affected
                                        // OutlookToCPi changed rel from 1:1 to n:n

            /*select dummy folder(top level folder in Add-in), save to default folder ("Incoming Email")
              //if no folder selected, save to default folder ("Incoming Email")
              //if default folder not created, create one then save
            */
            foreach (var selCase in selectedCases)
            {
                var key = selCase.key.Split("|");
                var dataKey = key[0];
                var dataKeyValue = int.Parse(key[1]);

                var folderName = string.IsNullOrEmpty(settings.EmailDocumentFolder) ? _defaultFolderName : settings.EmailDocumentFolder;
                int? folderId = 0;

                if (selectedCasesPaths.Length > 0)
                {
                    var idTextDTO = selectedCasesPaths.Where(p => p.key == selCase.key).FirstOrDefault();
                    string folderIdText = idTextDTO == null ? "0" : idTextDTO.text;
                    //convert folderId to integer
                    try
                    {
                        folderId = Int32.Parse(folderIdText);
                    }
                    catch (FormatException e)
                    {
                        await _errorLogger.Add(new Log { Message = e.Message });
                    }

                    //if (string.IsNullOrEmpty(folderName) || folderName == selCase.text)
                    //{
                    //    folderName = settings.EmailDocumentFolder;
                    //    if (folderName == null) folderName = _defaultFolderName;
                    //}
                }
                //check if default folder ("Incoming email") is created
                if (folderId == 0)
                {
                    folderName = settings.EmailDocumentFolder;
                    if (folderName == null) folderName = _defaultFolderName;
                    folderId = await DocFolders.Where(f => f.FolderName == folderName && f.DataKey == dataKey && f.DataKeyValue == dataKeyValue)
                                    .Select(f => f.FolderId).FirstOrDefaultAsync();
                }

                //int? folderId = await DocFolders.Where(f => f.FolderName == folderName && f.DataKey == dataKey && f.DataKeyValue == dataKeyValue)
                //                        .Select(f => f.FolderId).FirstOrDefaultAsync();

                // create folder, if case does not have it yet
                if (folderId == 0)
                {
                    folderId = await CreateFolder(folderName, userEmail, systemType, dataKey, dataKeyValue, screenCode, userName);
                }

                var docDocument = new DocDocument
                {
                    FolderId = folderId ?? 0,
                    Author = userName,
                    DocName = docName,
                    DocTypeId = docTypeId,
                    FileId = docFile.FileId,
                    IsPrivate = false,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    DateCreated = DateTime.Now,
                    LastUpdate = DateTime.Now
                };
                var newDocument = await AddDocument(docDocument);
                processedCases.DataKeyValue.Add(new OutlookProcessedCases { DataKey = dataKey, DataKeyValue = dataKeyValue, DocId = newDocument.DocId });
                strDocIds += "," + newDocument.DocId;
            }

            // (2021-dec-02) retro-fix on null FileId for multiple cases saving; can't change the EFCore structure (n:n on DocDocument:DocFile) now since so many other logic affected
            // OutlookToCPi changed rel from 1:1 to n:n
            if (strDocIds.Length > 0)
            {
                strDocIds = strDocIds.Substring(1);
                await FixDocId(strDocIds, docFile.FileId);
            }

            // save the database changes; docFile.filesize was updated in OutlookController but was not saved
            var modifiedFile = _repository.DocFiles.Attach(docFile);
            modifiedFile.Property(f => f.FileSize).IsModified = true;       // note: value updated in controller above
            await _repository.SaveChangesAsync();

            return processedCases;
        }

        private async Task<int?> CreateFolder(string folderName, string userEmail, string systemType, string dataKey, int dataKeyValue, string screenCode, string userName)
        {
            var docFolder = new DocFolder
            {
                Author = userEmail,
                SystemType = systemType,
                DataKey = dataKey,
                DataKeyValue = dataKeyValue,
                FolderName = folderName,
                ScreenCode = screenCode,
                ParentFolderId = 0,
                IsPrivate = false,
                CreatedBy = userName,
                UpdatedBy = userName,
                DateCreated = DateTime.Now,
                LastUpdate = DateTime.Now
            };
            var newFolder = await AddFolder(docFolder);
            int? folderId = newFolder.FolderId;
            return folderId;
        }

        private async Task FixDocId(string docIds, int fileId)
        {
            var sql = $"Update tblDocDocument Set FileId = {fileId.ToString()} Where DocId In ({docIds}) And FileId Is Null;";
            await _repository.Database.ExecuteSqlRawAsync(sql);
        }

        private async Task<int> LogOutLookLinks(string userName, string systemType, DocOutlook docOutlook, OutlookLinkedCases docMgtCases)
        {
            // docOutlook was filled in OutlookController; supply missing data here

            var dateNow = DateTime.Now;
            //var cpiEmailId = docOutlook.CPiEmailId;

            // Outlook ItemId is an unreliable key because it changes every time the message is moved to a different folder
            // Assign unique CPi email id, if Outlook CPiEmailId is null
            if (docOutlook.CPiEmailId == 0)
            {
                var docOutlookId = new DocOutlookId();
                docOutlookId.CreatedBy = userName;
                docOutlookId.DateCreated = dateNow;
                _repository.DocOutlookIds.Add(docOutlookId);
                await _repository.SaveChangesAsync();

                docOutlook.CPiEmailId = docOutlookId.CPiEmailId;
            }


            docOutlook.FileId = docMgtCases.FileId;
            docOutlook.CreatedBy = userName;
            _repository.DocOutlook.Add(docOutlook);
            await _repository.SaveChangesAsync();           // save docOutlook, the EmailId is needed for Outlook case links below

            foreach (var docCase in docMgtCases.DataKeyValue)
            {
                var caseLink = new DocOutlookCaseLink
                {
                    EmailId = docOutlook.EmailId,
                    SystemType = systemType,
                    DataKey = docCase.DataKey,
                    DataKeyValue = docCase.DataKeyValue,
                    DocId = docCase.DocId,
                    CreatedBy = userName,
                    DateCreated = dateNow
                };

                _repository.DocOutlookCaseLinks.Add(caseLink);
            }
            await _repository.SaveChangesAsync();

            return docOutlook.CPiEmailId;
        }

        public async Task<CaseLogDTO[]> GetOulookCaseLogByEmailId(int? cpiEmailId)
        {
            var idParam = new SqlParameter("Id", cpiEmailId ?? 0) { Direction = ParameterDirection.Input };
            var result = await _repository.CaseLogDTO.FromSqlRaw("Exec dbo.procDoc_OutlookLogCases @Id", idParam).ToArrayAsync();

            return result;
        }

        #endregion

        #region Gmail Email
        public async Task<OutlookLinkedCases> SaveGmailEmail(string contentRootPath, string userEmail, string systemType, string screenCode, string selectedCases, string gmailMsgId, string msgSubject, string encodedMsg)
        {
            // initialize            
            var userName = userEmail.Split("@")[0];

            var docName = msgSubject;
            var docTypeId = await GetDocTypeIdFromFileName("x.eml");

            // first, save file info to db; the file name is needed by MsgKit
            var docFile = new DocFile
            {
                FileExt = "eml",
                UserFileName = docName,
                FileSize = 0,
                IsImage = false,
                CreatedBy = userName,
                DateCreated = DateTime.Now,
                UpdatedBy = userName,
                LastUpdate = DateTime.Now
            };
            var newFile = await AddDocFile(docFile);

            // save email to physical file
            var settings = await _settings.GetSetting();
            //var docFilePath = settings.EmailSavePath;
            var docFilePath = Path.Combine(contentRootPath, @"UserFiles\Searchable\Documents");

            if (!docFilePath.EndsWith(@"\")) docFilePath += @"\";
            docFilePath += newFile.DocFileName;

            // second, create .eml file from base64 encoded and save to docFilePath            
            try
            {
                var decodedBytes = Convert.FromBase64String(encodedMsg);
                using (FileStream stream = new FileStream(docFilePath, FileMode.Create))
                {
                    stream.Write(decodedBytes, 0, decodedBytes.Length);
                }
                newFile.FileSize = decodedBytes.Length;
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw new Exception();
            }

            // then, save to docmgt tree of each cases in selectedCases; create folders if necessary
            // get docmgt folder info; add, if not yet existing
            var folderName = settings.EmailDocumentFolder;
            if (folderName == null) folderName = _defaultFolderName;

            // create document for message for each cases
            var docFileId = newFile.FileId;

            var processedCases = new OutlookLinkedCases
            {
                FileId = docFileId,
                DataKeyValue = new List<OutlookProcessedCases>()
            };

            foreach (var selCase in selectedCases.Split(","))
            {
                var key = selCase.Split("|");
                var dataKey = key[0];
                var dataKeyValue = Int32.Parse(key[1]);

                int? folderId = await DocFolders.Where(f => f.FolderName == folderName && f.DataKey == dataKey && f.DataKeyValue == dataKeyValue)
                                        .Select(f => f.FolderId).FirstOrDefaultAsync();
                if (folderId == 0)
                {
                    var docFolder = new DocFolder
                    {
                        Author = userEmail,
                        SystemType = systemType,
                        DataKey = dataKey,
                        DataKeyValue = dataKeyValue,
                        FolderName = folderName,
                        ParentFolderId = 0,
                        IsPrivate = false,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now,
                        ScreenCode = screenCode
                    };
                    var newFolder = await AddFolder(docFolder);
                    folderId = newFolder.FolderId;
                }


                var docDocument = new DocDocument
                {
                    FolderId = folderId ?? 0,
                    Author = userName,
                    DocName = docName,
                    DocTypeId = docTypeId,
                    FileId = docFileId,
                    IsPrivate = false,
                    CreatedBy = userName,
                    UpdatedBy = userName,
                    DateCreated = DateTime.Now,
                    LastUpdate = DateTime.Now,                    
                    Source = DocumentSourceType.Manual
                };
                var newDocument = await AddDocument(docDocument);

                processedCases.DataKeyValue.Add(new OutlookProcessedCases { DataKey = dataKey, DataKeyValue = dataKeyValue, DocId = newDocument.DocId });
            }

            // save the database changes
            // update filesize in tblDocFile
            var modifiedFile = _repository.DocFiles.Attach(docFile);
            modifiedFile.Property(f => f.FileSize).IsModified = true;       // note: value update above

            await _repository.SaveChangesAsync();

            //Log linked cases            
            try
            {                
                var logs = processedCases.DataKeyValue.Select(d => new DocGmailCaseLink()
                {
                    EmailId = gmailMsgId,
                    SystemType = systemType,
                    DataKey = d.DataKey,
                    DataKeyValue = d.DataKeyValue,
                    DocId = d.DocId,
                    CreatedBy = userName,
                    DateCreated = DateTime.Now
                }).ToList();
                await LogGmailEmail(logs);

                return processedCases;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public async Task<CaseLogDTO[]> GetGmailCaseLogByEmailId(string gmailMsgId)
        {
            //var idParam = new SqlParameter("Id", gmailMsgId) { Direction = ParameterDirection.Input };
            //var result = await _repository.CaseLogDTO.FromSqlRaw("Exec dbo.procDoc_GmailLogCases @Id", idParam).ToArrayAsync();
            var result = await _repository.CaseLogDTO.FromSqlInterpolated($"procDoc_GmailLogCases @Id={gmailMsgId}").AsNoTracking().ToArrayAsync();
            return result;
        }

        public async Task LogGmailEmail(List<DocGmailCaseLink> links)
        {
            if (links.Count > 0)
            {
                _repository.DocGmailCaseLinks.AddRange(links);
                await _repository.SaveChangesAsync();
            }
        }

        #endregion

        #region Responsible Docketing
        public async Task<List<string>> GetDocRespDocketingList(int docId)
        {
            var responsibles = DocRespDocketings.Where(d => d.DocId == docId && (!string.IsNullOrEmpty(d.UserId) || d.GroupId > 0))
                                        .Select(d => new { d.UserId, d.GroupId })
                                        .AsEnumerable()
                                        .SelectMany(d => new[] { d.UserId, d.GroupId.ToString() })
                                        .Select(d => d ?? "")
                                        .Where(d => !string.IsNullOrEmpty(d))
                                        .ToList();
            return responsibles;
        }

        public async Task<List<string>> GetDocRespDocketingNameList(int docId)
        {
            var responsibles = new List<string>();

            var responsibleList = await DocRespDocketings.Where(d => (docId > 0 && d.DocId == docId) && (!string.IsNullOrEmpty(d.UserId) || d.GroupId > 0))
                                        .Select(d => new { d.UserId, d.GroupId })                                        
                                        .ToListAsync();
            var userIds = responsibleList.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).ToList();
            var groupIds = responsibleList.Where(d => d.GroupId > 0).Select(d => d.GroupId).ToList();
            if (userIds.Any())
                responsibles.AddRange(await _repository.CPiUser.AsNoTracking().Where(d => userIds.Contains(d.Id)).Select(d => d.FirstName + " " + d.LastName).ToListAsync());

            if (groupIds.Any())
                responsibles.AddRange(await _repository.CPiGroups.AsNoTracking().Where(d => groupIds.Contains(d.Id)).Select(d => d.Name).ToListAsync());

            return responsibles;
        }

        public async Task UpdateDocRespDocketing(List<string> responsibleList, string userName, int docId)
        {
            var idList = responsibleList.Select(d => 
                                        { 
                                            int intVal; string strVal = d;
                                            bool isInt = int.TryParse(d, out intVal);                                             
                                            return new { intVal, strVal, isInt }; 
                                        })                                  
                                        .ToList();
            
            var existingResps = await DocRespDocketings.Where(d => d.DocId == docId).ToListAsync();

            DateTime today = DateTime.Now;  
            if (idList.Any())
            {
                var selectedUsers = idList.Where(d => !d.isInt)
                    .Select(d => new DocResponsibleDocketing
                    {
                        DocId = docId,                        
                        UserId = d.strVal,
                        GroupId = null,
                    }).ToList();

                var selectedGroups = idList.Where(d => d.isInt)
                    .Select(d => new DocResponsibleDocketing
                    {
                        DocId = docId,                        
                        UserId = null,
                        GroupId = d.intVal,
                    }).ToList();

                //Get deleted users/groups - existing users/groups not in selected users/groups
                var deleted = existingResps.Where(d => (!string.IsNullOrEmpty(d.UserId) && !selectedUsers.Any(s => s.UserId == d.UserId)) 
                                                    || (d.GroupId > 0 && !selectedGroups.Any(s => s.GroupId == d.GroupId))
                                            ).ToList();                
                
                //Get added users/groups - selected users/groups not in existing users/groups
                var added = selectedUsers.Where(d => !string.IsNullOrEmpty(d.UserId) && !existingResps.Any(s => s.UserId == d.UserId)).ToList();
                added.AddRange(selectedGroups.Where(d => d.GroupId > 0 && !existingResps.Any(s => s.GroupId == d.GroupId)).ToList());

                if (added.Any())
                {
                    added.ForEach(d => { d.CreatedBy = userName; d.UpdatedBy = userName; d.DateCreated = today; d.LastUpdate = today; });
                    _repository.DocRespDocketings.AddRange(added);

                    //Log new added                                   
                    _repository.DocResponsibleLogs.Add(new DocResponsibleLog()
                    {
                        DocId = docId,                        
                        UserIds = string.Join("|", added.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).ToList()),
                        GroupIds = string.Join("|", added.Where(d => d.GroupId > 0).Select(d => d.GroupId.ToString()).ToList()),
                        RespType = DocRespLogType.Docketing,
                        TransxType = DocRespLogTransxType.Update,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = today,
                        LastUpdate = today
                    });
                }

                if (deleted.Any())
                {
                    _repository.DocRespDocketings.RemoveRange(deleted);

                    //Log deleted 
                    _repository.DocResponsibleLogs.Add(new DocResponsibleLog()
                    {
                        DocId = docId,                        
                        UserIds = string.Join("|", deleted.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).ToList()),
                        GroupIds = string.Join("|", deleted.Where(d => d.GroupId > 0).Select(d => d.GroupId.ToString()).ToList()),
                        RespType = DocRespLogType.Docketing,
                        TransxType = DocRespLogTransxType.Delete,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = today,
                        LastUpdate = today
                    });
                }                    

                await _repository.SaveChangesAsync();                                
            }
        }

        public async Task DeleteDocRespDocketing(int docId)
        {            
            var deleted = await DocRespDocketings.Where(d => d.DocId == docId).ToListAsync();

            if (deleted.Any())
            {
                _repository.DocRespDocketings.RemoveRange(deleted);

                //Log deleted
                _repository.DocResponsibleLogs.Add(new DocResponsibleLog()
                    {
                        DocId = docId,
                        UserIds = string.Join("|", deleted.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).ToList()),
                        GroupIds = string.Join("|", deleted.Where(d => d.GroupId > 0).Select(d => d.GroupId.ToString()).ToList()),
                        RespType = DocRespLogType.Docketing,
                        TransxType = DocRespLogTransxType.Delete,
                        CreatedBy = _user.GetUserName(),
                        UpdatedBy = _user.GetUserName(),
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now
                    });

                await _repository.SaveChangesAsync();
            }                           
        }

        public async Task<DocResponsibleLog> GetDeletedDocRespDocketing(int docId)
        {
            return await _repository.DocResponsibleLogs.AsNoTracking().Where(d => d.DocId == docId && d.RespType == DocRespLogType.Docketing && d.TransxType == DocRespLogTransxType.Delete).OrderByDescending(o => o.DateCreated).FirstOrDefaultAsync() ?? new DocResponsibleLog();                     
        }

        public async Task<DocResponsibleLog> GetAddedDocRespDocketing(int docId)
        {
            return await _repository.DocResponsibleLogs.AsNoTracking().Where(d => d.DocId == docId && d.RespType == DocRespLogType.Docketing && d.TransxType == DocRespLogTransxType.Update).OrderByDescending(o => o.DateCreated).FirstOrDefaultAsync() ?? new DocResponsibleLog();
        }

        public async Task LinkDocWithVerifications(int docId, string randomGuid)
        {
            var result = await _repository.Database.ExecuteSqlInterpolatedAsync($"Update vef Set vef.DocId={docId}, vef.RandomGuid = null From tblDocVerification vef Where vef.RandomGuid={randomGuid} And vef.DocId Is Null");
        }
        #endregion

        #region Responsible Reporting
        public async Task<List<string>> GetDocRespReportingList(int docId)
        {
            var responsibleReportings = DocRespReportings
                                        .Where(d => d.DocId == docId && (!string.IsNullOrEmpty(d.UserId) || d.GroupId > 0))
                                        .Select(d => new { d.UserId, d.GroupId })
                                        .AsEnumerable()
                                        .SelectMany(d => new[] { d.UserId, d.GroupId.ToString() })
                                        .Select(d => d ?? "")
                                        .Where(d => !string.IsNullOrEmpty(d))                                        
                                        .ToList();
            return responsibleReportings;
        }

        public async Task<List<string>> GetDocRespReportingNameList(int docId)
        {
            var responsibles = new List<string>();

            var responsibleList = await DocRespReportings
                                        .Where(d => (docId > 0 && d.DocId == docId)
                                                && (!string.IsNullOrEmpty(d.UserId) || d.GroupId > 0))
                                        .Select(d => new { d.UserId, d.GroupId })
                                        .ToListAsync();
            var userIds = responsibleList.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).ToList();
            var groupIds = responsibleList.Where(d => d.GroupId > 0).Select(d => d.GroupId).ToList();
            if (userIds.Any())
                responsibles.AddRange(await _repository.CPiUser.AsNoTracking().Where(d => userIds.Contains(d.Id)).Select(d => d.FirstName + " " + d.LastName).ToListAsync());

            if (groupIds.Any())
                responsibles.AddRange(await _repository.CPiGroups.AsNoTracking().Where(d => groupIds.Contains(d.Id)).Select(d => d.Name).ToListAsync());

            return responsibles;
        }

        public async Task UpdateDocRespReporting(List<string> respReportingList, string userName, int docId)
        {
            var idList = respReportingList.Select(d =>
                                        {
                                            int intVal; string strVal = d;
                                            bool isInt = int.TryParse(d, out intVal);
                                            return new { intVal, strVal, isInt };
                                        })
                                        .ToList();

            var existingResps = await DocRespReportings.Where(d => d.DocId == docId).ToListAsync();

            DateTime today = DateTime.Now;
            if (idList.Any())
            {
                var selectedUsers = idList.Where(d => !d.isInt)
                    .Select(d => new DocResponsibleReporting
                    {
                        DocId = docId,                        
                        UserId = d.strVal,
                        GroupId = null,
                    }).ToList();

                var selectedGroups = idList.Where(d => d.isInt)
                    .Select(d => new DocResponsibleReporting
                    {
                        DocId = docId,                        
                        UserId = null,
                        GroupId = d.intVal,
                    }).ToList();

                //Get deleted users/groups - existing users/groups not in selected users/groups
                var deleted = existingResps.Where(d => (!string.IsNullOrEmpty(d.UserId) && !selectedUsers.Any(s => s.UserId == d.UserId))
                                                    || (d.GroupId > 0 && !selectedGroups.Any(s => s.GroupId == d.GroupId))
                                            ).ToList();

                //Get added users/groups - selected users/groups not in existing users/groups
                var added = selectedUsers.Where(d => !string.IsNullOrEmpty(d.UserId) && !existingResps.Any(s => s.UserId == d.UserId)).ToList();
                added.AddRange(selectedGroups.Where(d => d.GroupId > 0 && !existingResps.Any(s => s.GroupId == d.GroupId)).ToList());

                if (added.Any())
                {
                    added.ForEach(d => { d.CreatedBy = userName; d.UpdatedBy = userName; d.DateCreated = today; d.LastUpdate = today; });
                    _repository.DocRespReportings.AddRange(added);
                                        
                    //Log new added                                   
                    _repository.DocResponsibleLogs.Add(new DocResponsibleLog()
                    {
                        DocId = docId,                        
                        UserIds = string.Join("|", added.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).ToList()),
                        GroupIds = string.Join("|", added.Where(d => d.GroupId > 0).Select(d => d.GroupId.ToString()).ToList()),
                        RespType = DocRespLogType.Reporting,
                        TransxType = DocRespLogTransxType.Update,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = today,
                        LastUpdate = today
                    });
                }

                if (deleted.Any())
                {
                    _repository.DocRespReportings.RemoveRange(deleted);
                                        
                     //Log deleted 
                    _repository.DocResponsibleLogs.Add(new DocResponsibleLog()
                    {
                        DocId = docId,                        
                        UserIds = string.Join("|", deleted.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).ToList()),
                        GroupIds = string.Join("|", deleted.Where(d => d.GroupId > 0).Select(d => d.GroupId.ToString()).ToList()),
                        RespType = DocRespLogType.Reporting,
                        TransxType = DocRespLogTransxType.Delete,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = today,
                        LastUpdate = today
                    });
                }

                await _repository.SaveChangesAsync();
            }
        }

        public async Task DeleteDocRespReporting(int docId)
        {
            var deleted = await DocRespReportings.Where(d => d.DocId == docId).ToListAsync();

            if (deleted.Any())
            {
                _repository.DocRespReportings.RemoveRange(deleted);
                
                //Log deleted
                _repository.DocResponsibleLogs.Add(new DocResponsibleLog()
                    {
                        DocId = docId,
                        UserIds = string.Join("|", deleted.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).ToList()),
                        GroupIds = string.Join("|", deleted.Where(d => d.GroupId > 0).Select(d => d.GroupId.ToString()).ToList()),
                        RespType = DocRespLogType.Reporting,
                        TransxType = DocRespLogTransxType.Delete,
                        CreatedBy = _user.GetUserName(),
                        UpdatedBy = _user.GetUserName(),
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now
                    });

                await _repository.SaveChangesAsync();
            }
        }

        public async Task<DocResponsibleLog> GetDeletedDocRespReporting(int docId)
        {
            return await _repository.DocResponsibleLogs.AsNoTracking().Where(d => d.DocId == docId && d.RespType == DocRespLogType.Reporting && d.TransxType == DocRespLogTransxType.Delete).OrderByDescending(o => o.DateCreated).FirstOrDefaultAsync() ?? new DocResponsibleLog();  
        }

        public async Task<DocResponsibleLog> GetAddedDocRespReporting(int docId)
        {
            return await _repository.DocResponsibleLogs.AsNoTracking().Where(d => d.DocId == docId && d.RespType == DocRespLogType.Reporting && d.TransxType == DocRespLogTransxType.Update).OrderByDescending(o => o.DateCreated).FirstOrDefaultAsync() ?? new DocResponsibleLog();
        }        
        #endregion

        #region Helper for Sharepoint integration
        public async Task<List<DocInfoDTO>> GetDocumentInfoFromDriveItemIds(List<string?> driveItemIds)
        {
            var param = new SqlParameter("@List", SqlDbType.Structured);
            param.TypeName= "TVP_RecIdString";

            var records = new List<SqlDataRecord>();
            foreach (var item in driveItemIds)
            {
                var record = new SqlDataRecord(new SqlMetaData[] {
                    new SqlMetaData("Id", SqlDbType.VarChar,255),
                });
                record.SetString(0, item);
                records.Add(record);
            }
            param.Value = records;

            var list = await _repository.DocInfoDTO.FromSqlInterpolated($"Exec procDocSharePointGetDocInfo @List={param}").AsNoTracking().ToListAsync();
            return list;
        }

        public async Task<DocDocument> GetDefaultImage(string systemType, string dataKey, int dataKeyValue) {
            var defaultImage = await _repository.DocDocuments.Where(d => _repository.DocFolders.Where(f => f.SystemType == systemType && f.DataKey == dataKey && f.DataKeyValue == dataKeyValue)
               .Any(f => f.FolderId == d.FolderId) && d.IsDefault && d.DocFile.IsImage).Include(d=> d.DocFile).FirstOrDefaultAsync();
            return defaultImage;
        }
        
        public async Task<string?> GetParentDocumentLink(string documentLink)
        {
            var settings = await _settings.GetSetting();
            var documentLinkArray = documentLink.Split("|");
            var systemType = documentLinkArray[0]?.ToUpper();
            var screenCode = documentLinkArray[1];
            var dataKey = documentLinkArray[2]?.ToLower();
            var dataKeyValue = int.Parse(documentLinkArray[3] ?? "0");
            var parentKey = "";
            var parentKeyValue = 0;

            if (dataKeyValue <= 0)
                return string.Empty;

            if (systemType == "P" && dataKey == "actid")
            {
                //get country application
                screenCode = "CA";
                parentKey = "AppId";
                parentKeyValue = await _repository.PatActionDues.AsNoTracking().Where(d => d.ActId == dataKeyValue).Select(d => d.AppId).FirstOrDefaultAsync();
            }
            else if (systemType == "P" && dataKey == "costtrackid")
            {
                //get country application
                screenCode = "CA";
                parentKey = "AppId";
                parentKeyValue = await _repository.PatCostTracks.AsNoTracking().Where(d => d.CostTrackId == dataKeyValue).Select(d => d.AppId).FirstOrDefaultAsync();
            }
            else if (systemType == "P" && dataKey == "actinvid")
            {
                //get invention
                screenCode = "Inv";
                parentKey = "InvId";
                parentKeyValue = await _repository.PatActionDueInvs.AsNoTracking().Where(d => d.ActId == dataKeyValue).Select(d => d.InvId).FirstOrDefaultAsync();
            }
            else if (systemType == "P" && dataKey == "costtrackinvid")
            {
                //get invention
                screenCode = "Inv";
                parentKey = "InvId";
                parentKeyValue = await _repository.PatCostTrackInvs.AsNoTracking().Where(d => d.CostTrackInvId == dataKeyValue).Select(d => d.InvId).FirstOrDefaultAsync();
            }
            else if (systemType == "P" && dataKey == "appid" && !settings.IsCountryApplicationDocumentRoot)
            {
                //get invention
                screenCode = "Inv";
                parentKey = "InvId";
                parentKeyValue = await _repository.CountryApplications.AsNoTracking().Where(d => d.AppId == dataKeyValue).Select(d => d.InvId).FirstOrDefaultAsync();
            }
            else if (systemType == "T" && dataKey == "actid")
            {
                //get trademark
                screenCode = "Tmk";
                parentKey = "TmkId";
                parentKeyValue = await _repository.TmkActionDues.AsNoTracking().Where(d => d.ActId == dataKeyValue).Select(d => d.TmkId).FirstOrDefaultAsync();
            }
            else if (systemType == "T" && dataKey == "costtrackid")
            {
                //get trademark
                screenCode = "Tmk";
                parentKey = "TmkId";
                parentKeyValue = await _repository.TmkCostTracks.AsNoTracking().Where(d => d.CostTrackId == dataKeyValue).Select(d => d.TmkId).FirstOrDefaultAsync();
            }
            // Removed during deep clean - GeneralMatter module removed
            // else if (systemType == "G" && dataKey == "actid")
            // {
            //     //get general matters
            //     screenCode = "GM";
            //     parentKey = "MatId";
            //     parentKeyValue = await _repository.GMActionsDue.AsNoTracking().Where(d => d.ActId == dataKeyValue).Select(d => d.MatId).FirstOrDefaultAsync();
            // }
            // else if (systemType == "G" && dataKey == "costtrackid")
            // {
            //     //get general matters
            //     screenCode = "GM";
            //     parentKey = "MatId";
            //     parentKeyValue = await _repository.GMCostTracks.AsNoTracking().Where(d => d.CostTrackId == dataKeyValue).Select(d => d.MatId).FirstOrDefaultAsync();
            // }

            if (parentKeyValue > 0)
                return $"{systemType}|{screenCode}|{parentKey}|{parentKeyValue}";

            return string.Empty;
        }


        public async Task<string> GenerateFolderName(string documentLink)
        {
            var documentLinkArray = documentLink.Split("|");
            var systemType = documentLinkArray[0]?.ToUpper();
            var dataKey = documentLinkArray[2]?.ToLower();
            var dataKeyValue = int.Parse(documentLinkArray[3] ?? "0");
            var folderName = "";

            if (dataKeyValue <= 0)
                return string.Empty;

            if (systemType == "P" && dataKey == "invid")
                folderName = await _repository.Inventions.AsNoTracking().Where(d => d.InvId == dataKeyValue).Select(d => d.CaseNumber).FirstOrDefaultAsync();
            else if (systemType == "P" && dataKey == "appid")
                folderName = await _repository.CountryApplications.AsNoTracking().Where(d => d.AppId == dataKeyValue).Select(d => $"{d.CaseNumber} - {d.Country}" + (string.IsNullOrEmpty(d.SubCase) ? "" : $" - {d.SubCase}")).FirstOrDefaultAsync();
            else if (systemType == "P" && dataKey == "actid")
                folderName = await _repository.PatActionDues.AsNoTracking().Where(d => d.ActId == dataKeyValue).Select(d => d.ActionType).FirstOrDefaultAsync();
            else if (systemType == "P" && dataKey == "costtrackid")
                folderName = await _repository.PatCostTracks.AsNoTracking().Where(d => d.CostTrackId == dataKeyValue).Select(d => d.CostType).FirstOrDefaultAsync();
            else if (systemType == "P" && dataKey == "actinvid")
                folderName = await _repository.PatActionDueInvs.AsNoTracking().Where(d => d.ActId == dataKeyValue).Select(d => d.ActionType).FirstOrDefaultAsync();
            else if (systemType == "P" && dataKey == "costtrackinvid")
                folderName = await _repository.PatCostTrackInvs.AsNoTracking().Where(d => d.CostTrackInvId == dataKeyValue).Select(d => d.CostType).FirstOrDefaultAsync();

            else if (systemType == "T" && dataKey == "tmkid")
                folderName = await _repository.TmkTrademarks.AsNoTracking().Where(d => d.TmkId == dataKeyValue).Select(d => $"{d.CaseNumber} - {d.Country}" + (string.IsNullOrEmpty(d.SubCase) ? "" : $" - {d.SubCase}")).FirstOrDefaultAsync();
            else if (systemType == "T" && dataKey == "actid")
                folderName = await _repository.TmkActionDues.AsNoTracking().Where(d => d.ActId == dataKeyValue).Select(d => d.ActionType).FirstOrDefaultAsync();
            else if (systemType == "T" && dataKey == "costtrackid")
                folderName = await _repository.TmkCostTracks.AsNoTracking().Where(d => d.CostTrackId == dataKeyValue).Select(d => d.CostType).FirstOrDefaultAsync();

            // Removed during deep clean - GeneralMatter module removed
            // else if (systemType == "G" && dataKey == "matid")
            //     folderName = await _repository.GMMatters.AsNoTracking().Where(d => d.MatId == dataKeyValue).Select(d => d.CaseNumber).FirstOrDefaultAsync();
            // else if (systemType == "G" && dataKey == "actid")
            //     folderName = await _repository.GMActionsDue.AsNoTracking().Where(d => d.ActId == dataKeyValue).Select(d => d.ActionType).FirstOrDefaultAsync();
            // else if (systemType == "G" && dataKey == "costtrackid")
            //     folderName = await _repository.GMCostTracks.AsNoTracking().Where(d => d.CostTrackId == dataKeyValue).Select(d => d.CostType).FirstOrDefaultAsync();

            return folderName ?? $"{documentLinkArray[2]}.{documentLinkArray[3]}";
        }

        public async Task<(string? ClientCode, string? ClientName, string? MatterNumber)> GetClientMatter(string documentLink)
        {
            var documentLinkArray = documentLink.Split("|");
            var systemType = documentLinkArray[0]?.ToUpper();
            var dataKey = documentLinkArray[2]?.ToLower();
            var dataKeyValue = int.Parse(documentLinkArray[3] ?? "0");

            if (dataKeyValue > 0)
            {
                if (systemType == "P" && dataKey == "invid")
                {
                    var inv = await _repository.Inventions.AsNoTracking().Where(d => d.InvId == dataKeyValue).Select(d => new { d.CaseNumber, d.InvMatterNumber, d.Client.ClientCode, d.Client.ClientName }).FirstOrDefaultAsync();
                    if (inv != null)
                        return (inv.ClientCode, inv.ClientName, inv.InvMatterNumber ?? inv.CaseNumber);
                }
                else if (systemType == "P" && dataKey == "appid")
                {
                    var inv = await _repository.CountryApplications.AsNoTracking().Where(d => d.AppId == dataKeyValue).Select(d => new { d.CaseNumber, d.Invention.InvMatterNumber, d.Invention.Client.ClientCode, d.Invention.Client.ClientName }).FirstOrDefaultAsync();
                    if (inv != null)
                        return (inv.ClientCode, inv.ClientName, inv.InvMatterNumber ?? inv.CaseNumber);
                }
                else if (systemType == "T" && dataKey == "tmkid")
                {
                    var tmk = await _repository.TmkTrademarks.AsNoTracking().Where(d => d.TmkId == dataKeyValue).Select(d => new { d.CaseNumber, d.MatterNumber, d.Client.ClientCode, d.Client.ClientName }).FirstOrDefaultAsync();
                    if (tmk != null)
                        return (tmk.ClientCode, tmk.ClientName, tmk.MatterNumber ?? tmk.CaseNumber);
                }
                // Removed during deep clean - GeneralMatter module removed
                // else if (systemType == "G" && dataKey == "matid")
                // {
                //     var gm = await _repository.GMMatters.AsNoTracking().Where(d => d.MatId == dataKeyValue).Select(d => new { d.CaseNumber, d.MatterNumber, d.Client.ClientCode, d.Client.ClientName }).FirstOrDefaultAsync();
                //     if (gm != null)
                //         return (gm.ClientCode, gm.ClientName, gm.MatterNumber ?? gm.CaseNumber);
                // }
            }

            return ("", "", "");
        }

        #endregion

        public void DetachAllEntities() {
            _repository.DetachAllEntities();
        }
        public async Task AddDocQuickEmailLogs(List<DocQuickEmailLog> docQuickEmailLogs)
        {
            _repository.DocQuickEmailLogs.AddRange(docQuickEmailLogs);
            await _repository.SaveChangesAsync();
        }

        #region Trade Secret
        public async Task LogDocTradeSecretActivityByDocId(int docid)
        {
            var docDocument = await _repository.DocDocuments.AsNoTracking().Where(d => d.DocId == docid).FirstOrDefaultAsync();
            await LogDocTradeSecretActivityByFileId(docDocument?.FileId ?? 0);
        }

        public async Task LogDocTradeSecretActivityByFileId(int fileId)
        {
            var docFolder = await _repository.DocFolders.AsNoTracking().Where(f => f.DocDocuments.Any(d => d.FileId == fileId)).FirstOrDefaultAsync();
            var tsReqLocator = await GetDocTradeSecretRequestLocator(docFolder?.DataKey, docFolder?.DataKeyValue);
            if (!string.IsNullOrEmpty(tsReqLocator))
            {
                var tsRequest = await _tradeSecretService.GetUserRequest(tsReqLocator);
                var requestId = (tsRequest != null && tsRequest.IsCleared) ? tsRequest.RequestId : 0;
                await _tradeSecretService.LogActivity(GetDocTradeSecretSource(docFolder?.DataKey), TradeSecretScreen.DocFile, fileId, TradeSecretActivityCode.Download, requestId);

                if (!(tsRequest?.IsCleared ?? false))
                    throw new UnauthorizedAccessException();
            }
        }

        public async Task LogDocTradeSecretActivityByFileIds(List<int> fileIds)
        {
            var docFolder = await _repository.DocFolders.AsNoTracking().Where(f => f.DocDocuments.Any(d => d.FileId == fileIds.FirstOrDefault())).FirstOrDefaultAsync();
            var tsReqLocator = await GetDocTradeSecretRequestLocator(docFolder?.DataKey, docFolder?.DataKeyValue);
            if (!string.IsNullOrEmpty(tsReqLocator))
            {
                var tsRequest = await _tradeSecretService.GetUserRequest(tsReqLocator);
                var requestId = (tsRequest != null && tsRequest.IsCleared) ? tsRequest.RequestId : 0;

                foreach (var fileId in fileIds)
                {
                    await _tradeSecretService.LogActivity(GetDocTradeSecretSource(docFolder?.DataKey), TradeSecretScreen.DocFile, fileId, TradeSecretActivityCode.Download, requestId);
                }

                if (!(tsRequest?.IsCleared ?? false))
                    throw new UnauthorizedAccessException();
            }
        }

        public async Task LogDocTradeSecretActivityByFileName(string fileName)
        {
            await LogDocTradeSecretActivityByFileId(GetFileId(fileName));
        }

        public async Task LogDocTradeSecretActivityByFileNames(List<string?> fileNames)
        {
            var fileIds = fileNames.Select(f => GetFileId(f ?? "0")).ToList();
            await LogDocTradeSecretActivityByFileIds(fileIds);
        }

        public async Task LogDocTradeSecretActivityByDriveItemId(string driveItemId)
        {
            var docFile = await _repository.DocFiles.AsNoTracking().Where(d => d.DriveItemId == driveItemId).FirstOrDefaultAsync();
            await LogDocTradeSecretActivityByFileId(docFile?.FileId ?? 0);
        }

        public async Task LogDocTradeSecretActivityByDriveItemIds(List<string?> driveItemIds)
        {
            var docFolder = await _repository.DocFolders.AsNoTracking().Where(f => f.DocDocuments.Any(d => d.DocFile.DriveItemId == driveItemIds.FirstOrDefault())).FirstOrDefaultAsync();
            var tsReqLocator = await GetDocTradeSecretRequestLocator(docFolder?.DataKey, docFolder?.DataKeyValue);
            if (!string.IsNullOrEmpty(tsReqLocator))
            {
                var tsRequest = await _tradeSecretService.GetUserRequest(tsReqLocator);
                var requestId = (tsRequest != null && tsRequest.IsCleared) ? tsRequest.RequestId : 0;

                foreach (var driveItemId in driveItemIds)
                {
                    var docFile = await _repository.DocFiles.AsNoTracking().Where(d => d.DriveItemId == driveItemId).FirstOrDefaultAsync();
                    await _tradeSecretService.LogActivity(GetDocTradeSecretSource(docFolder?.DataKey), TradeSecretScreen.DocFile, docFile?.FileId ?? 0, TradeSecretActivityCode.Download, requestId);
                }

                if (!(tsRequest?.IsCleared ?? false))
                    throw new UnauthorizedAccessException();
            }
        }

        /// <summary>
        /// Create trade secret request locator
        /// Return null if record is not trade secret
        /// </summary>
        /// <param name="parentKeyName"></param>
        /// <param name="parentKeyId"></param>
        /// <returns></returns>
        private async Task<string?> GetDocTradeSecretRequestLocator(string? parentKeyName, int? parentKeyId)
        {
            if (string.IsNullOrEmpty(parentKeyName))
                return null;

            switch (parentKeyName.ToLower())
            {
                case "invid":
                    var invention = await _repository.Inventions.AsNoTracking().Where(i => (i.IsTradeSecret ?? false) && i.InvId == parentKeyId).FirstOrDefaultAsync();
                    if (invention != null)
                        return _tradeSecretService.CreateLocator(TradeSecretScreen.Invention, invention.InvId);
                    break;
                case "appid":
                    var application = await _repository.CountryApplications.AsNoTracking().Where(ca => (ca.Invention.IsTradeSecret ?? false) && ca.AppId == parentKeyId).FirstOrDefaultAsync();
                    if (application != null)
                        return _tradeSecretService.CreateLocator(TradeSecretScreen.Invention, application.InvId);
                    break;
                // Removed during deep clean - DMS/Disclosure module removed
                // case "dmsid":
                //     var disclosure = await _repository.Disclosures.AsNoTracking().Where(d => (d.IsTradeSecret ?? false) && d.DMSId == parentKeyId).FirstOrDefaultAsync();
                //     if (disclosure != null)
                //         return _tradeSecretService.CreateLocator(TradeSecretScreen.DMSDisclosure, disclosure.DMSId);
                //     break;
            }

            return null;
        }

        private string GetDocTradeSecretSource(string? dataKey)
        {
            switch (dataKey?.ToLower())
            {
                case "invid":
                    return TradeSecretScreen.InventionDocuments;
                case "appid":
                    return TradeSecretScreen.CountryApplicationDocuments;
                case "dmsid":
                    return TradeSecretScreen.DisclosureDocuments;
            }

            return string.Empty;
        }

        private int GetFileId(string fileName)
        {
            var fileId = 0;
            int.TryParse(Path.GetFileNameWithoutExtension(fileName)?.Replace("_thumb", "", StringComparison.OrdinalIgnoreCase), out fileId);
            return fileId;
        }
        #endregion
    }
}
