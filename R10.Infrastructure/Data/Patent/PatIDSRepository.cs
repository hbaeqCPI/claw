using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient.Server;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces.Patent;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Patent
{
    public class PatIDSRepository : IPatIDSRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PatIDSRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region IDS RelatedCases
        public async Task SaveIDSInfo(PatIDSRelatedCasesInfo idsInfo)
        {
            var existing = await _dbContext.PatIDSRelatedCasesInfos.Where(i => i.AppId == idsInfo.AppId).AsNoTracking()
                .FirstOrDefaultAsync();
            if (existing != null)
            {
                idsInfo.CreatedBy = existing.CreatedBy;
                idsInfo.DateCreated = existing.DateCreated;
                idsInfo.tStamp = existing.tStamp;
            }
            _dbContext.Entry(idsInfo).State = existing != null ? EntityState.Modified : EntityState.Added;
            _dbContext.SaveChanges();
        }

        public async Task<List<PatIDSRelatedCase>> GetIDSRelatedCases(int appId)
        {
            var list = await _dbContext.PatIDSRelatedCases
                .FromSqlInterpolated($"procPatIDSRelatedCases @Action = 0,@AppId={appId}").AsNoTracking().ToListAsync();

            list.ForEach(item =>
            {
                if (item.FileId > 0)
                {
                    var docFile = _dbContext.DocFiles.FirstOrDefault(f => f.FileId == item.FileId);
                    if (docFile != null)
                    {
                        item.CurrentDocFile = docFile.UserFileName;
                    }
                }
                if (!string.IsNullOrEmpty(item.ExaminerDocDate))
                {
                    if (!string.IsNullOrEmpty(item.RelatedPatNumber))
                    {
                        item.ExaminerIssDate = item.ExaminerDocDate;
                        item.ExaminerPubDate = "";
                    }
                    else
                    {
                        item.ExaminerPubDate = item.ExaminerDocDate;
                        item.ExaminerIssDate = "";
                    }
                }
                else {
                    item.ExaminerIssDate = "";
                    item.ExaminerPubDate = "";
                }
            });

            list.ForEach(nonPatLiterature =>
            {
                if (nonPatLiterature.FileId > 0)
                {
                    var docFile = _dbContext.DocFiles.FirstOrDefault(f => f.FileId == nonPatLiterature.FileId);
                    if (docFile != null)
                    {
                        nonPatLiterature.CurrentDocFile = docFile.UserFileName;
                    }
                }
            });


            return list;
        }

        public async Task<PatIDSRelatedCase> GetIDSRelatedCase(int relatedCasesId)
        {
            var relatedCase = await _dbContext.PatIDSRelatedCases.FirstOrDefaultAsync(r => r.RelatedCasesId == relatedCasesId);
            if (relatedCase.RelatedAppId > 0)
            {
                var relatedInfo = await GetIDSApplicationInfo(relatedCase.RelatedAppId ?? 0);
                if (relatedInfo != null)
                {
                    relatedCase.RelatedPubNumber = relatedInfo.RelatedPubNumber;
                    relatedCase.RelatedPatNumber = relatedInfo.RelatedPatNumber;
                    relatedCase.RelatedPubDate = relatedInfo.RelatedPubDate;
                    relatedCase.RelatedIssDate = relatedInfo.RelatedIssDate;
                    relatedCase.RelatedFirstNamedInventor = relatedInfo.RelatedFirstNamedInventor;
                }
            }
            return relatedCase;
        }

        public async Task<List<PatIDSRelatedCase>> GetIDSRelatedCasesForStandardization() {
            var relatedCases = await _dbContext.PatIDSRelatedCases.Where(r=> 
            (
            (!string.IsNullOrEmpty(r.RelatedPubNumber) && string.IsNullOrEmpty(r.RelatedPubNumberStandard)) ||
            (!string.IsNullOrEmpty(r.RelatedPatNumber) && string.IsNullOrEmpty(r.RelatedPatNumberStandard))
            )
            ).AsNoTracking().Take(10000).ToListAsync();
            return relatedCases;
        }


        public async Task<List<CaseListDTO>> GetApplications(int appId, string caseNumber)
        {
            var cases = await _dbContext.CountryApplications.Where(c => c.AppId != appId && c.CaseNumber == caseNumber && !_dbContext.PatIDSRelatedCases.Any(r => r.AppId == appId && r.RelatedAppId == c.AppId))
                .Select(c => new CaseListDTO { Id = c.AppId, Country = c.Country + (c.SubCase != "" ? " - " : "") + c.SubCase }).ToListAsync();
            return cases;
        }

        public async Task<PatIDSRelatedCase> GetIDSApplicationInfo(int appId)
        {
            var relatedInfo = await _dbContext.CountryApplications.Where(c => c.AppId == appId)
                .Select(c => new PatIDSRelatedCase
                {
                    RelatedPubNumber = c.PubNumber,
                    RelatedPatNumber = c.PatNumber,
                    RelatedPubDate = c.PubDate,
                    RelatedIssDate = c.IssDate,
                    RelatedFirstNamedInventor = _dbContext.PatInventorsApp.Where(a => a.AppId == appId).OrderBy(a => a.OrderOfEntry).Select(a => a.InventorAppInventor.Inventor).FirstOrDefault()
                }).FirstOrDefaultAsync();

            return relatedInfo;
        }

        public async Task IDSRelatedCasesDelete(PatIDSRelatedCase deletedIdsRelatedCase)
        {
            _dbContext.Set<PatIDSRelatedCase>().Remove(deletedIdsRelatedCase);
            var application = await _dbContext.CountryApplications.FirstOrDefaultAsync(a => a.AppId == deletedIdsRelatedCase.AppId);
            if (application != null)
            {
                application.UpdatedBy = deletedIdsRelatedCase.UpdatedBy;
                application.LastUpdate = DateTime.Now;
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task IDSRelatedCasesUpdate(PatIDSRelatedCase idsRelatedCase)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                if (idsRelatedCase.RelatedCasesId > 0)
                {
                    _dbContext.Entry(idsRelatedCase).State = EntityState.Modified;
                }
                else
                {
                    await _dbContext.Set<PatIDSRelatedCase>().AddAsync(idsRelatedCase);
                }
                var application = await _dbContext.CountryApplications.FirstOrDefaultAsync(a => a.AppId == idsRelatedCase.AppId);
                if (application != null)
                {
                    application.UpdatedBy = idsRelatedCase.UpdatedBy;
                    application.LastUpdate = DateTime.Now;
                }
                await _dbContext.SaveChangesAsync();

                //if (idsRelatedCase.CopyToFamily)
                //{
                //    await CopyIDSRelatedCasesToFamily(idsRelatedCase.RelatedCasesId, idsRelatedCase.UpdatedBy,isNew);
                //}

                scope.Complete();
            }



        }

        public async Task IDSRelatedCasesUpdate(int appId, string userName, IEnumerable<PatIDSRelatedCase> updatedRelatedCases)
        {
            foreach (var item in updatedRelatedCases)
            {
                var updatedItem = await _dbContext.PatIDSRelatedCases.FirstOrDefaultAsync(r => r.RelatedCasesId == item.RelatedCasesId);
                if (updatedItem != null)
                {
                    updatedItem.RelatedDateFiled = item.RelatedDateFiled;
                    updatedItem.ActiveSwitch = item.ActiveSwitch;
                    updatedItem.UpdatedBy = item.UpdatedBy;
                    updatedItem.LastUpdate = item.LastUpdate;
                    _dbContext.Entry(updatedItem).State = EntityState.Modified;
                }
            }

            var application = _dbContext.CountryApplications.FirstOrDefault(a => a.AppId == appId);
            if (application != null)
            {
                application.UpdatedBy = userName;
                application.LastUpdate = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task IDSRelatedCasesSave(int appId, string userName, IEnumerable<PatIDSRelatedCase> relatedCases)
        {

            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var item in relatedCases)
                {
                    item.RelatedPubNumber = item.RelatedPubNumber ?? "";
                    item.RelatedPatNumber = item.RelatedPatNumber ?? "";
                }
                var updated = relatedCases.Where(r => r.RelatedCasesId > 0);
                foreach (var relatedCase in updated)
                {
                    var prev = await _dbContext.PatIDSRelatedCases.AsNoTracking().FirstOrDefaultAsync(r=> r.RelatedCasesId== relatedCase.RelatedCasesId);
                    if (prev != null) {
                        relatedCase.FileId = prev.FileId;
                        relatedCase.DocFilePath = prev.DocFilePath;
                        relatedCase.tStamp = prev.tStamp;
                    }
                }
                _dbContext.PatIDSRelatedCases.UpdateRange(updated);
                _dbContext.PatIDSRelatedCases.AddRange(relatedCases.Where(r => r.RelatedCasesId <= 0));

                var timeStamp = DateTime.Now;
                var application = _dbContext.CountryApplications.FirstOrDefault(a => a.AppId == appId);
                if (application != null)
                {
                    application.UpdatedBy = userName;
                    application.LastUpdate = timeStamp;
                }
                await _dbContext.SaveChangesAsync();
                scope.Complete();
            }

        }

        public async Task<List<LookupDTO>> GetCopyRefCaseNumberList(int excludeAppId)
        {
            var list = await _dbContext.CountryApplications.Where(c => c.AppId != excludeAppId && (c.IDSRelatedCases.Any() || c.RelatedCases.Any())).Select(c => new LookupDTO { Text = c.CaseNumber }).Distinct().ToListAsync();
            return list;
        }

        public async Task<List<LookupDTO>> GetCopyRefCountryList(int excludeAppId)
        {
            var list = await _dbContext.CountryApplications.Where(c => c.AppId != excludeAppId && (c.IDSRelatedCases.Any() || c.RelatedCases.Any())).Select(c => new LookupDTO { Text = c.Country }).Distinct().ToListAsync();
            return list;
        }

        public async Task<List<LookupDTO>> GetCopyRefSubCaseList(int excludeAppId)
        {
            var list = await _dbContext.CountryApplications.Where(c => c.AppId != excludeAppId && (c.IDSRelatedCases.Any() || c.RelatedCases.Any())).Select(c => new LookupDTO { Text = c.SubCase }).Distinct().ToListAsync();
            return list;
        }

        public async Task<List<LookupDTO>> GetCopyRefKeywordList(int excludeAppId)
        {
            var list = await _dbContext.PatKeywords.Where(k => k.Invention.CountryApplications.Any(c => c.AppId != excludeAppId && (c.IDSRelatedCases.Any() || c.RelatedCases.Any())))
                                                   .Select(k => new LookupDTO { Text = k.Keyword }).Distinct().ToListAsync();
            return list;
        }

        public async Task<List<LookupDTO>> GetCopyRefInventorList(int excludeAppId)
        {
            var list = await _dbContext.PatInventorsApp.Where(i => i.CountryApplication.IDSRelatedCases.Any(r => r.AppId != excludeAppId) || i.CountryApplication.RelatedCases.Any(r => r.AppId != excludeAppId))
                   .Select(i => new LookupDTO { Text = i.InventorAppInventor.Inventor }).Distinct().ToListAsync();
            return list;
        }

        public async Task<List<LookupDTO>> GetCopyRefArtUnitList(int excludeAppId)
        {
            var list = await _dbContext.PatIDSRelatedCasesInfos.Where(r => r.AppId != excludeAppId && r.CountryApplication.IDSRelatedCases.Any())
                    .Select(r => new LookupDTO { Text = r.GroupArtUnit }).Distinct().ToListAsync();
            return list;
        }

        public async Task<List<PatIDSCopyFamilyDTO>> GetCopyToFamilyList(int appId, int relatedCasesId, string relatedBy)
        {
            var list = await _dbContext.PatIDSCopyFamilyDTO
                .FromSqlRaw($"procPatIDSRelatedCases @Action = 1,@AppId={appId},@RelatedCasesId={relatedCasesId},@RelatedBy={relatedBy}").AsNoTracking().ToListAsync();

            return list;
        }

        // CopyIDSRelatedCases(int[], RTSIDSCrossCheckCopyDTO[], int[], string) removed during deep clean (RTS module deleted)


        public async Task SaveStandardizedReferences(List<PatIDSRelatedCase> relatedCases)
        {
            var definition = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("RelatedCasesID", SqlDbType.Int), new SqlMetaData("RelatedPubNumberStandard", SqlDbType.VarChar,20),
                new SqlMetaData("RelatedPubNumberStandardYear", SqlDbType.VarChar, 4), new SqlMetaData("RelatedPatNumberStandard", SqlDbType.VarChar, 20)});

            var standardRecs = relatedCases.Select(r =>
            {
                var record = definition;
                record.SetValue(0, r.RelatedCasesId);
                record.SetValue(1, r.RelatedPubNumberStandard);
                record.SetValue(2, r.RelatedPubNumberStandardYear);
                record.SetValue(3, r.RelatedPatNumberStandard);

                return record;
            });

            using (SqlCommand cmd = new SqlCommand("procIDS_SaveStandard"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = new SqlConnection(_dbContext.Database.GetDbConnection().ConnectionString);
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@IDSStandard", standardRecs).SqlDbType = SqlDbType.Structured;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // CopyIDSRelatedCases(RTSIDSCrossCheckCopyDTO[], int[], string) removed during deep clean (RTS module deleted)

        public async Task CopyIDSRelatedCasesToFamily(PatIDSCopyFamilyActionDTO selection, string userId)
        {
            var sourceIds = new List<SqlDataRecord>();
            foreach (var data in selection.RelatedCasesIds)
            {
                var record = new SqlDataRecord(new SqlMetaData[]  {
                    new SqlMetaData("Id", SqlDbType.Int)
                        });
                record.SetValue(0, data);
                sourceIds.Add(record);
            }

            var copyToIds = new List<SqlDataRecord>();
            foreach (var data in selection.Selection)
            {
                var record = new SqlDataRecord(new SqlMetaData[]  {
                    new SqlMetaData("AppId", SqlDbType.Int),
                    new SqlMetaData("GenAction", SqlDbType.Bit)
                        });
                record.SetValue(0, data.AppId);
                record.SetValue(1, data.GenAction);
                copyToIds.Add(record);
            }

            using (SqlCommand cmd = new SqlCommand("procPatIDSRelatedCasesCopyFamily"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = new SqlConnection(_dbContext.Database.GetDbConnection().ConnectionString);
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@SrcAppId", selection.AppId);
                cmd.Parameters.AddWithValue("@ActionToGenerate", selection.ActionToGenerate);
                cmd.Parameters.AddWithValue("@BaseDate", selection.BaseDate);
                cmd.Parameters.AddWithValue("@DueDate", selection.DueDate);
                cmd.Parameters.AddWithValue("@Indicator", selection.Indicator);
                cmd.Parameters.AddWithValue("@UpdatedBy", userId);
                cmd.Parameters.AddWithValue("@RelatedCasesIds", sourceIds).SqlDbType = SqlDbType.Structured;
                cmd.Parameters.AddWithValue("@AppIds", copyToIds).SqlDbType = SqlDbType.Structured;
                cmd.Parameters.AddWithValue("@Country", selection.Country);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task CopyIDSNonPatLiteratureToFamily(PatIDSCopyFamilyActionDTO selection, string userId)
        {
            var sourceIds = new List<SqlDataRecord>();
            foreach (var data in selection.RelatedCasesIds)
            {
                var record = new SqlDataRecord(new SqlMetaData[]  {
                    new SqlMetaData("Id", SqlDbType.Int)
                        });
                record.SetValue(0, data);
                sourceIds.Add(record);
            }

            var copyToIds = new List<SqlDataRecord>();
            foreach (var data in selection.Selection)
            {
                var record = new SqlDataRecord(new SqlMetaData[]  {
                    new SqlMetaData("AppId", SqlDbType.Int),
                    new SqlMetaData("GenAction", SqlDbType.Bit)
                        });
                record.SetValue(0, data.AppId);
                record.SetValue(1, data.GenAction);
                copyToIds.Add(record);
            }

            using (SqlCommand cmd = new SqlCommand("procPatIDSNonPatLiteratureCopyFamily"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = new SqlConnection(_dbContext.Database.GetDbConnection().ConnectionString);
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@ActionToGenerate", selection.ActionToGenerate);
                cmd.Parameters.AddWithValue("@BaseDate", selection.BaseDate);
                cmd.Parameters.AddWithValue("@DueDate", selection.DueDate);
                cmd.Parameters.AddWithValue("@Indicator", selection.Indicator);
                cmd.Parameters.AddWithValue("@UpdatedBy", userId);
                cmd.Parameters.AddWithValue("@NonPatLiteratureIDs", sourceIds).SqlDbType = SqlDbType.Structured;
                cmd.Parameters.AddWithValue("@AppIds", copyToIds).SqlDbType = SqlDbType.Structured;
                await cmd.ExecuteNonQueryAsync();

            }
        }

        public IQueryable<PatIDSRelatedCaseDTO> IDSRelatedCasesDTO => _dbContext.PatIDSRelatedCaseDTO;
        public IQueryable<PatIDSRelatedCase> IDSRelatedCases => _dbContext.PatIDSRelatedCases;
        public IQueryable<PatIDSRelatedCasesInfo> IDSRelatedCasesInfos => _dbContext.PatIDSRelatedCasesInfos;
        public IQueryable<PatIDSRelatedCaseCopyDTO> PatIDSRelatedCasesCopyDTO => _dbContext.PatIDSRelatedCaseCopyDTO;

        #endregion
        #region IDS NonPatent Literature
        public async Task<List<PatIDSNonPatLiterature>> GetNonPatLiteratures(int appId)
        {
            var list = await _dbContext.PatIDSNonPatLiteratures.Where(n => n.AppId == appId).AsNoTracking().ToListAsync();
            list.ForEach(nonPatLiterature =>
            {
                if (nonPatLiterature.FileId > 0)
                {
                    var docFile = _dbContext.DocFiles.FirstOrDefault(f => f.FileId == nonPatLiterature.FileId);
                    if (docFile != null) {
                        nonPatLiterature.CurrentDocFile = docFile.UserFileName;
                    }
                }
            });
            return list;
        }

        public async Task<PatIDSNonPatLiterature> GetNonPatLiterature(int nonPatLiteratureId)
        {
            var nonPatLiterature = await _dbContext.PatIDSNonPatLiteratures.FirstOrDefaultAsync(n => n.NonPatLiteratureId == nonPatLiteratureId);
            //if (nonPatLiterature.FileId > 0)
            //{
            //    nonPatLiterature.CurrentDocFile =
            //        (await _dbContext.FileHandler.FirstOrDefaultAsync(f => f.FileID == nonPatLiterature.FileId))
            //        .UserFileName;
            //}
            return nonPatLiterature;
        }

        public async Task NonPatLiteratureDelete(PatIDSNonPatLiterature deletedIdsNonPatLiterature)
        {
            _dbContext.Set<PatIDSNonPatLiterature>().Remove(deletedIdsNonPatLiterature);
            var application = await _dbContext.CountryApplications.FirstOrDefaultAsync(a => a.AppId == deletedIdsNonPatLiterature.AppId);
            if (application != null)
            {
                application.UpdatedBy = deletedIdsNonPatLiterature.UpdatedBy;
                application.LastUpdate = DateTime.Now;
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task NonPatLiteratureUpdate(PatIDSNonPatLiterature idsNonPatLiterature)
        {
            if (idsNonPatLiterature.NonPatLiteratureId > 0)
            {
                _dbContext.Entry(idsNonPatLiterature).State = EntityState.Modified;
            }
            else
            {
                await _dbContext.Set<PatIDSNonPatLiterature>().AddAsync(idsNonPatLiterature);
            }
            var application = await _dbContext.CountryApplications.FirstOrDefaultAsync(a => a.AppId == idsNonPatLiterature.AppId);
            if (application != null)
            {
                application.UpdatedBy = idsNonPatLiterature.UpdatedBy;
                application.LastUpdate = DateTime.Now;
            }
            await _dbContext.SaveChangesAsync();

        }

        public async Task NonPatLiteratureUpdate(int appId, string userName, IEnumerable<PatIDSNonPatLiterature> updatedNonPatLiteratures, IEnumerable<PatIDSNonPatLiterature> newNonPatLiteratures)
        {
            foreach (var item in updatedNonPatLiteratures)
            {
                var updatedItem = await _dbContext.PatIDSNonPatLiteratures.FirstOrDefaultAsync(n => n.NonPatLiteratureId == item.NonPatLiteratureId);
                if (updatedItem != null)
                {
                    updatedItem.NonPatLiteratureInfo = item.NonPatLiteratureInfo;
                    updatedItem.ReferenceSrc = item.ReferenceSrc;
                    updatedItem.ReferenceDate = item.ReferenceDate;
                    updatedItem.RelatedDateFiled = item.RelatedDateFiled;
                    updatedItem.HasTranslation = item.HasTranslation;
                    updatedItem.ActiveSwitch = item.ActiveSwitch;
                    updatedItem.UpdatedBy = item.UpdatedBy;
                    updatedItem.LastUpdate = item.LastUpdate;
                    updatedItem.Remarks = item.Remarks;
                    updatedItem.ConsideredByExaminer = item.ConsideredByExaminer;
                    _dbContext.Entry(updatedItem).State = EntityState.Modified;
                }
            }

            //foreach (var item in updatedNonPatLiteratures)
            //{
            //    _dbContext.Entry(item).State = EntityState.Modified;
            //}

            foreach (var item in newNonPatLiteratures)
            {
                item.AppId = appId;
                _dbContext.Add(item);
            }

            var application = _dbContext.CountryApplications.FirstOrDefault(a => a.AppId == appId);
            if (application != null)
            {
                application.UpdatedBy = userName;
                application.LastUpdate = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<LookupDTO>> GetCopyNonPatCaseNumberList(int excludeAppId)
        {
            var list = await _dbContext.CountryApplications.Where(c => c.AppId != excludeAppId && c.NonPatLiteratures.Any()).Select(c => new LookupDTO { Text = c.CaseNumber }).Distinct().ToListAsync();
            return list;
        }

        public async Task<List<LookupDTO>> GetCopyNonPatCountryList(int excludeAppId)
        {
            var list = await _dbContext.CountryApplications.Where(c => c.AppId != excludeAppId && c.NonPatLiteratures.Any()).Select(c => new LookupDTO { Text = c.Country }).Distinct().ToListAsync();
            return list;
        }

        public async Task<List<LookupDTO>> GetCopyNonPatSubCaseList(int excludeAppId)
        {
            var list = await _dbContext.CountryApplications.Where(c => c.AppId != excludeAppId && c.NonPatLiteratures.Any()).Select(c => new LookupDTO { Text = c.SubCase }).Distinct().ToListAsync();
            return list;
        }

        public async Task<List<LookupDTO>> GetCopyNonPatKeywordList(int excludeAppId)
        {
            var list = await _dbContext.PatKeywords.Where(k => k.Invention.CountryApplications.Any(c => c.AppId != excludeAppId && c.NonPatLiteratures.Any()))
                .Select(k => new LookupDTO { Text = k.Keyword }).Distinct().ToListAsync();
            return list;
        }

        public async Task<List<LookupDTO>> GetCopyNonPatInventorList(int excludeAppId)
        {
            var list = await _dbContext.PatInventorsApp.Where(i => i.CountryApplication.NonPatLiteratures.Any(r => r.AppId != excludeAppId))
                .Select(i => new LookupDTO { Text = i.InventorAppInventor.Inventor }).Distinct().ToListAsync();
            return list;
        }

        public async Task<List<LookupDTO>> GetCopyNonPatArtUnitList(int excludeAppId)
        {
            var list = await _dbContext.PatIDSRelatedCasesInfos.Where(r => r.AppId != excludeAppId && r.CountryApplication.NonPatLiteratures.Any())
                .Select(r => new LookupDTO { Text = r.GroupArtUnit }).Distinct().ToListAsync();
            return list;
        }

        public IQueryable<PatIDSNonPatLiterature> NonPatLiteratures => _dbContext.PatIDSNonPatLiteratures;
        public IQueryable<PatRelatedCase> RelatedCases => _dbContext.PatRelatedCases;
        public IQueryable<DocFile> DocFiles => _dbContext.DocFiles;

        public async Task CopyNonPatLiteratures(int appId, int[] from, string userId)
        {
            var structure = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.Int) });
            var fromIds = from.Select(f =>
            {
                var record = structure;
                record.SetValue(0, f);
                return record;
            });

            using (SqlCommand cmd = new SqlCommand("procPatNonPatLiteratureCopy"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = new SqlConnection(_dbContext.Database.GetDbConnection().ConnectionString);
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@AppId", appId);
                cmd.Parameters.AddWithValue("@CopyFrom", fromIds).SqlDbType = SqlDbType.Structured;
                cmd.Parameters.AddWithValue("@UpdatedBy", userId);
                await cmd.ExecuteNonQueryAsync();
            }
        }


        #endregion

        public async Task<List<PatIDSSearchInputDTO>> GetIDSDownloadList(int maxAttempts, string? appIds = "")
        {
            var list = await _dbContext.PatIDSSearchInputDTO
                .FromSqlRaw($"procPatIDSSearchCases @Action = 1,@MaxAttempts={maxAttempts},@Ids='{appIds}' ").AsNoTracking().ToListAsync();

            return list;
        }

        public async Task SaveIDSRelatedCaseDocs(List<PatIDSRelatedCase> patIDSRelatedCases)
        {            
            foreach (var item in patIDSRelatedCases)
            {
                await _dbContext.PatIDSRelatedCases.Where(d => d.RelatedCasesId == item.RelatedCasesId)
                                                        .ExecuteUpdateAsync(d => d.SetProperty(p => p.FileId, p => item.FileId)
                                                            .SetProperty(p => p.DocFilePath, p => item.DocFilePath)
                                                            .SetProperty(p => p.UpdatedBy, p => item.UpdatedBy)
                                                            .SetProperty(p => p.LastUpdate, p => item.LastUpdate));
            }            
        }

        public async Task<IDSTotalDTO> GetIDSTotal(int appId)
        {
            //var filedRelatedCasesCount = await _dbContext.Database.SqlQuery<int>($"Select Count(*) as Value  From tblPatIDSRelatedCases Where RelatedDateFiled is not null and ConsideredByExaminer=1 and MatchTypeUsed='Cited' and ActiveSwitch=1 and AppId={appId}").FirstOrDefaultAsync();
            //var filedRelatedCasesCount = await _dbContext.Database.SqlQuery<int>($"Select Count(*) as Value  From tblPatIDSRelatedCases Where RelatedDateFiled is not null and MatchTypeUsed='Cited' and ActiveSwitch=1 and AppId={appId}").FirstOrDefaultAsync();
            var filedRelatedCasesCount = await _dbContext.Database.SqlQuery<int>(@$"Select Count(*) as Value  From tblPatIDSRelatedCases ids Inner Join tblPatCountryApplication ca on ca.AppId=ids.AppId
                                        Where ids.RelatedDateFiled is not null and
					 (ca.FilDate is null or Datediff(m,ca.FilDate,ids.RelatedDateFiled) > 3 Or
					 ids.RelatedDateFiled > (Select Min(BaseDate) as FirstBaseDate from tblPatActionDue Where AppID={appId} And IsOfficeAction=1)
					 )
                    and ids.MatchTypeUsed='Cited' and ids.ActiveSwitch=1 and ids.AppId={appId}
                    And (ids.FromParent=0 Or
                    ca.CaseType Not in('CON','REI','CPA','RCE'))").FirstOrDefaultAsync();

            //var unfiledRelatedCasesCount = await _dbContext.Database.SqlQuery<int>($"Select Count(*) as Value  From tblPatIDSRelatedCases Where RelatedDateFiled is null and MatchTypeUsed='Cited' and ActiveSwitch=1 and AppId={appId}").FirstOrDefaultAsync();
            var unfiledRelatedCasesCount = await _dbContext.Database.SqlQuery<int>(@$"Select Count(*) as Value  From tblPatIDSRelatedCases ids Inner Join tblPatCountryApplication ca on ca.AppId=ids.AppId
                                        Where ids.RelatedDateFiled is null and ids.MatchTypeUsed='Cited' and ids.ActiveSwitch=1 and ids.AppId={appId}
                                        And (ids.FromParent=0 Or
                                            ca.CaseType Not in('CON','REI','CPA','RCE'))").FirstOrDefaultAsync();

            //var filedNPLCount = await _dbContext.Database.SqlQuery<int>($"Select Count(*) as Value  From tblPatAppNonPatLiterature Where RelatedDateFiled is not null and ActiveSwitch=1 and AppId={appId}").FirstOrDefaultAsync();
            var filedNPLCount = await _dbContext.Database.SqlQuery<int>(@$"Select Count(*) as Value  From tblPatAppNonPatLiterature ids Inner Join tblPatCountryApplication ca on ca.AppId=ids.AppId
                                        Where ids.RelatedDateFiled is not null and
					 (ca.FilDate is null or Datediff(m,ca.FilDate,ids.RelatedDateFiled) > 3 Or
					 ids.RelatedDateFiled > (Select Min(BaseDate) as FirstBaseDate from tblPatActionDue Where AppID={appId} And IsOfficeAction=1)
					 )
                     and ids.ActiveSwitch=1 and ids.AppId={appId}
                     And (ids.FromParent=0 Or
                      ca.CaseType Not in('CON','REI','CPA','RCE'))").FirstOrDefaultAsync();

            //var unfiledNPLCount = await _dbContext.Database.SqlQuery<int>($"Select Count(*) as Value  From tblPatAppNonPatLiterature Where RelatedDateFiled is null and ActiveSwitch=1 and AppId={appId}").FirstOrDefaultAsync();
            var unfiledNPLCount = await _dbContext.Database.SqlQuery<int>(@$"Select Count(*) as Value  From tblPatAppNonPatLiterature ids Inner Join tblPatCountryApplication ca on ca.AppId=ids.AppId
                                        Where ids.RelatedDateFiled is null and ids.ActiveSwitch=1 and ids.AppId={appId}
                                        And (ids.FromParent=0 Or
                                            ca.CaseType Not in('CON','REI','CPA','RCE'))").FirstOrDefaultAsync();

            var idsTotal = new IDSTotalDTO();
            idsTotal.AppId = appId;
            idsTotal.FiledCount = filedRelatedCasesCount + filedNPLCount;
            idsTotal.UnfiledCount = unfiledRelatedCasesCount + unfiledNPLCount;

            // Removed during deep clean
            // var rtsSearch = await _dbContext.RTSSearchRecords.FirstOrDefaultAsync(r => r.PMSAppId == appId);
            // if (rtsSearch != null)
            // {
            //
            //     idsTotal.PLAppId = rtsSearch.PLAppId;
            //     var filedXMLCount = await _dbContext.Database.SqlQuery<int>($"Select COALESCE(Sum(ReferenceCount+NPLCount),0) as Value  From tblPLSearchIDSCount Where PLAppId={rtsSearch.PLAppId}").FirstOrDefaultAsync();
            //     idsTotal.XMLCount = filedXMLCount;
            //     idsTotal.XMLCountLastUpdate = await _dbContext.Database.SqlQuery<DateTime?>($"Select Max(MailRoomDate) as Value  From tblPLSearchIDSCount Where PLAppId={rtsSearch.PLAppId}").FirstOrDefaultAsync();
            // }
            return idsTotal;
        }


        public async Task<int> GetIDSReferencesTotal(int appId)
        {
            var relatedCasesCount = await _dbContext.Database.SqlQuery<int>(@$"Select Count(*) as Value  From tblPatIDSRelatedCases ids Inner Join tblPatCountryApplication ca on ca.AppId=ids.AppId
                                        Where ids.MatchTypeUsed='Cited' and ids.ActiveSwitch=1 and ids.AppId={appId}
                                        And (ids.FromParent=0 Or
                                            ca.CaseType Not in('CON','REI','CPA','RCE') Or
                                            Datediff(m,ids.RelatedDateFiled,ca.FilDate) <=3 Or
	                                        ids.RelatedDateFiled < (Select Min(BaseDate) as FirstBaseDate from tblPatActionDue Where AppID={appId} And IsOfficeAction=1))").FirstOrDefaultAsync();
            return relatedCasesCount;
        }

        public async Task<bool> HasReferencesInStaging()
        {
            var result = await _dbContext.Database.SqlQuery<bool>($"Select Cast(Case When exists(Select 1 From tblPatAppRelatedCasesBackground Where Processed=0) then 1 else 0 end as bit) as Value").FirstOrDefaultAsync();
            return result;
        }

        public async Task LoadReferencesFromStaging()
        {
            await _dbContext.Database.ExecuteSqlRawAsync("exec procPatRelatedCasesMassCopyFamilyBackground");
        }

    }
}
