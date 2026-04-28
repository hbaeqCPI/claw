using Microsoft.EntityFrameworkCore;
using LawPortal.Core.DTOs;
using LawPortal.Core.Entities.Documents;
using LawPortal.Core.Entities.Shared;
using LawPortal.Core.Interfaces;
using LawPortal.Core.Helpers;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Security.Claims;
using LawPortal.Core.Identity;
using LawPortal.Core.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using LawPortal.Core.Services.Shared;
using System.Reflection.Metadata.Ecma335;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Data.SqlClient.Server;

namespace LawPortal.Core.Services.Documents
{
    public class DocumentService : IDocumentService
    {
        private readonly IApplicationDbContext _repository;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly ICPiUserSettingManager _userSettingManager;
        private readonly ClaimsPrincipal _user;
        private readonly ILoggerService<Log> _errorLogger;

        private const string _defaultFolderName = "Incoming Email";

        public DocumentService(
            IApplicationDbContext repository,
            ISystemSettings<DefaultSetting> settings,
            ICPiUserSettingManager userSettingManager,
            ClaimsPrincipal user,
            ILoggerService<Log> errorLogger
            )
        {
            _repository = repository;
            _settings = settings;
            _userSettingManager = userSettingManager;
            _user = user;
            _errorLogger = errorLogger;
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
        public IQueryable<DocFileSignatureRecipient> DocFileSignatureRecipients => _repository.DocFileSignatureRecipients.AsNoTracking();

        public IQueryable<DocVerification> DocVerifications => _repository.DocVerifications.AsNoTracking();
        public IQueryable<DocResponsibleDocketing> DocRespDocketings => _repository.DocRespDocketings.AsNoTracking();
        public IQueryable<DocResponsibleReporting> DocRespReportings => _repository.DocRespReportings.AsNoTracking();
        public IQueryable<DocQuickEmailLog> DocQuickEmailLogs => _repository.DocQuickEmailLogs.AsNoTracking();


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

                // PatActionDues and TmkActionDues DbSets removed - verification clearing handled elsewhere
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

            // PatActionDues and TmkActionDues DbSets removed - verification clearing handled elsewhere

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
            // PatActionDues and TmkActionDues DbSets removed - verification action verify is no longer handled here
            await Task.CompletedTask;
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
            // Workflow action generation removed during debloat (ICountryApplicationService/ITmkTrademarkService deleted)
            await Task.CompletedTask;
            return 0;
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

        }


        public async Task SetEnvelopeId(int fileId, string envelopeId)
        {
            await _repository.DocFileSignatures.Where(f => f.FileId == fileId).ExecuteUpdateAsync(f => f.SetProperty(p => p.EnvelopeId, p => envelopeId));
        }
        public async Task SetEnvelopeIdForSharePointFile(string itemId, string envelopeId)
        {
            await _repository.SharePointFileSignatures.Where(f => f.DriveItemId==itemId).ExecuteUpdateAsync(f => f.SetProperty(p => p.EnvelopeId, p => envelopeId));
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
            // Patent entity DbSets (Inventions, CountryApplications, PatActionDues, PatCostTracks, etc.) removed
            await Task.CompletedTask;
        }

        private async Task UpdateTrademarkParentStamp(DocFolder folder)
        {
            // Trademark entity DbSets (TmkTrademarks, TmkActionDues, TmkCostTracks) removed
            await Task.CompletedTask;
        }

        private async Task UpdateGeneralMatterParentStamp(DocFolder folder)
        {
            await Task.CompletedTask;
        }

        private async Task UpdateDMSParentStamp(DocFolder folder)
        {
            await Task.CompletedTask;
        }

        private async Task UpdatePatClearanceParentStamp(DocFolder folder)
        {
            await Task.CompletedTask;
        }

        private async Task UpdateTmkSearchParentStamp(DocFolder folder)
        {
            await Task.CompletedTask;
        }

        private void UpdateStamp<T>(string userName, T entity) where T : BaseEntity
        {
            entity.UpdatedBy = userName;
            entity.LastUpdate = DateTime.Now;
        }

        private async Task SyncChildToDesignatedRecords(DocFolder folder)
        {
            // Designation sync removed during debloat (ICountryApplicationService/ITmkTrademarkService deleted)
            await Task.CompletedTask;
        }

        #endregion


        #region Gmail Email
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
            // All parent lookup DbSets (PatActionDues, PatCostTracks, PatActionDueInvs, PatCostTrackInvs, CountryApplications, TmkActionDues, TmkCostTracks) removed
            await Task.CompletedTask;
            return string.Empty;
        }


        public async Task<string> GenerateFolderName(string documentLink)
        {
            var documentLinkArray = documentLink.Split("|");

            // All entity DbSets (Inventions, CountryApplications, PatActionDues, PatCostTracks, etc.) removed
            // Return fallback folder name
            await Task.CompletedTask;
            return $"{documentLinkArray[2]}.{documentLinkArray[3]}";
        }

        public async Task<(string? ClientCode, string? ClientName, string? MatterNumber)> GetClientMatter(string documentLink)
        {
            // All entity DbSets (Inventions, CountryApplications, TmkTrademarks) removed
            await Task.CompletedTask;
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

    }
}
