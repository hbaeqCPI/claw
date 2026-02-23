using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient.Server;
using R10.Core;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using R10.Core.Entities.Documents;

namespace R10.Infrastructure.Data.Trademark
{
    public class TmkTrademarkRepository : EFRepository<TmkTrademark>, ITmkTrademarkRepository
    {

        public TmkTrademarkRepository(ApplicationDbContext dbContext) : base(dbContext) { }
        
        #region Trademark Main
        public async Task<TmkTrademark> AddAsync(TmkTrademark trademark, TmkTrademarkModifiedFields enteredDateFields, DateTime dateCreated)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                if (trademark.OwnerID > 0)
                {
                    var existing = await _dbContext.TmkOwners.FirstOrDefaultAsync(o => o.TmkID == trademark.OwnerID);
                    if (existing != null)
                    {
                        existing.OwnerID = (int)trademark.OwnerID;
                        existing.UpdatedBy = trademark.UpdatedBy;
                        existing.LastUpdate = trademark.LastUpdate;
                    }
                    else
                        _dbContext.TmkOwners.Add(new TmkOwner
                        {
                            OwnerID = (int)trademark.OwnerID,
                            CreatedBy = trademark.CreatedBy,
                            UpdatedBy = trademark.UpdatedBy,
                            DateCreated = trademark.DateCreated,
                            LastUpdate = trademark.LastUpdate
                        });
                    trademark.OwnerID = null;
                }

                var result = await base.AddAsync(trademark);
                if (AnyActionFieldsModified(enteredDateFields))
                {
                    await GenerateCountryLawActions(trademark.TmkId, trademark.UpdatedBy, enteredDateFields, "ScrAdd",dateCreated);
                }
                scope.Complete();
                return result;
            }
        }

        public async Task<int> UpdateAsync(TmkTrademark trademark, TmkTrademarkModifiedFields modifiedFields, DateTime? dateCreated,bool isMultipleOwnersOn)
        {
            var delegationId = 0;
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                if (!isMultipleOwnersOn)
                {
                    //Trademark is not using TmkTrademark.OwnerID to store owner data but
                    //TmkTrademark.OwnerID is used when editing trademark record manually from the screen.
                    //When updating trademark record using background services, set trademark.OwnerID = 0 so TmkOwner table checking is ignored.
                    if (trademark.OwnerID > 0)
                    {
                        var existing = await _dbContext.TmkOwners.FirstOrDefaultAsync(o => o.TmkID == trademark.TmkId);
                        if (existing != null)
                        {
                            existing.OwnerID = (int)trademark.OwnerID;
                            existing.UpdatedBy = trademark.UpdatedBy;
                            existing.LastUpdate = trademark.LastUpdate;
                        }
                        else
                            _dbContext.TmkOwners.Add(new TmkOwner
                            {
                                TmkID = trademark.TmkId,
                                OwnerID = (int)trademark.OwnerID,
                                CreatedBy = trademark.CreatedBy,
                                UpdatedBy = trademark.UpdatedBy,
                                DateCreated = trademark.DateCreated,
                                LastUpdate = trademark.LastUpdate
                            });
                    }
                    else if (trademark.OwnerID == null)
                    {
                        var existing = await _dbContext.TmkOwners.FirstOrDefaultAsync(o => o.TmkID == trademark.TmkId);
                        if (existing != null)
                        {
                            _dbContext.TmkOwners.Remove(existing);
                        }
                    }
                }
                trademark.OwnerID = null;

                await base.UpdateAsync(trademark);
                _dbContext.Entry(trademark).State = EntityState.Detached;

                if (AnyActionFieldsModified(modifiedFields))
                {
                    delegationId = await GenerateCountryLawActions(trademark.TmkId, trademark.UpdatedBy, modifiedFields, "ScrUpd", dateCreated);
                }

                await SyncToDesignatedTrademarks(trademark, modifiedFields);
                scope.Complete();
            }
            return delegationId;
        }

        public TmkTrademarkRenewalFields GetTrademarkRenewal(TmkTrademarkRenewalParameters param)
        {
            using (SqlCommand cmd = new SqlCommand("procTmkGenRenewalDate"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters.RemoveAt("@CalcRenewalDate");
                cmd.Parameters.RemoveAt("@EqualRenewalDate");
                cmd.Parameters.RemoveAt("@NoOfRenDates");
                cmd.FillParamValues(param);
                cmd.Parameters.Add("@CalcRenewalDate", System.Data.SqlDbType.DateTime).Direction = System.Data.ParameterDirection.Output;
                cmd.Parameters.Add("@EqualRenewalDate", System.Data.SqlDbType.Bit).Direction = System.Data.ParameterDirection.Output;
                cmd.Parameters.Add("@NoOfRenDates", System.Data.SqlDbType.Int).Direction = System.Data.ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                var retVal = new TmkTrademarkRenewalFields
                {
                    NoOfRenDates = (int)cmd.Parameters["@NoOfRenDates"].Value,
                    EqualRenewalDate = (bool)cmd.Parameters["@EqualRenewalDate"].Value
                };
                if (retVal.NoOfRenDates == 1)
                    retVal.CalcRenewalDate = (DateTime)cmd.Parameters["@CalcRenewalDate"].Value;

                return retVal;

            }
        }

        public async Task UpdateChild<T>(TmkTrademark trademark, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                trademark.UpdatedBy = userName;
                trademark.LastUpdate = DateTime.Now;
                var parent = _dbContext.TmkTrademarks.Attach(trademark);
                parent.Property(c => c.UpdatedBy).IsModified = true;
                parent.Property(c => c.LastUpdate).IsModified = true;

                foreach (var item in updated)
                {
                    item.UpdatedBy = trademark.UpdatedBy;
                    item.LastUpdate = trademark.LastUpdate;
                }

                foreach (var item in added)
                {
                    item.CreatedBy = trademark.UpdatedBy;
                    item.DateCreated = trademark.LastUpdate;
                    item.UpdatedBy = trademark.UpdatedBy;
                    item.LastUpdate = trademark.LastUpdate;
                }
                var dbSet = _dbContext.Set<T>();
                if (updated.Any())
                    dbSet.UpdateRange(updated);

                if (added.Any())
                    dbSet.AddRange(added);

                if (deleted.Any())
                    dbSet.RemoveRange(deleted);
                await _dbContext.SaveChangesAsync();

                await SyncChildToDesignatedTrademarks(trademark,new TmkTrademarkModifiedFields(), typeof(T));
                scope.Complete();
            }
        }

        
        public async Task<Tuple<string, string, string,string>> CopyTrademark(int oldTmkId, string newCaseNumber, string newSubCase, List<int> countryIds,
                                       bool copyCaseInfo, bool copyRemarks, bool copyAssignments, bool copyGoods, bool copyImages, bool copyKeywords, 
                                       bool copyDesCountries, bool copyLicenses, bool copyRelatedCases, string createdBy, string relationship, bool copyProducts, bool copyOwners)
        {
            var ids = new List<SqlDataRecord> { };
            foreach (var countryId in countryIds)
            {
                var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.Int) });
                record.SetInt32(0, countryId);
                ids.Add(record);
            }
            using (SqlCommand cmd = new SqlCommand("procTmkCopyTrademark"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters["@OldTmkId"].Value =  oldTmkId;
                cmd.Parameters["@NewCaseNumber"].Value = newCaseNumber;
                cmd.Parameters["@NewSubCase"].Value = newSubCase;
                cmd.Parameters["@CopyCaseInfo"].Value = copyCaseInfo;
                cmd.Parameters["@CopyRemarks"].Value = copyRemarks;
                cmd.Parameters["@CopyAssignments"].Value = copyAssignments;
                cmd.Parameters["@CopyGoods"].Value = copyGoods;
                cmd.Parameters["@CopyImages"].Value = copyImages;
                cmd.Parameters["@CopyKeywords"].Value = copyKeywords;
                cmd.Parameters["@CopyDesCountries"].Value = copyDesCountries;
                cmd.Parameters["@CopyLicenses"].Value = copyLicenses;
                cmd.Parameters["@CopyProducts"].Value = copyProducts;
                cmd.Parameters["@CopyRelatedCases"].Value = copyRelatedCases;
                cmd.Parameters["@CopyOwners"].Value = copyOwners; 
                cmd.Parameters["@CreatedBy"].Value = createdBy;
                cmd.Parameters["@Relationship"].Value = relationship;
                

                cmd.Parameters.RemoveAt("@CountryIds");
                cmd.Parameters.AddWithValue("@CountryIds", ids).SqlDbType = SqlDbType.Structured;

                cmd.Parameters["@AddedRecords"].Value = string.Empty;
                cmd.Parameters["@ExistingRecords"].Value = string.Empty;
                cmd.Parameters["@NoCLRecords"].Value = string.Empty;
                cmd.Parameters["@AddedRecordTmkIds"].Value = string.Empty;
                

                await cmd.ExecuteNonQueryAsync();

                var addedRecords = (string)cmd.Parameters["@AddedRecords"].Value;
                var existingRecords = (string)cmd.Parameters["@ExistingRecords"].Value;
                var noCLRecords = (string)cmd.Parameters["@NoCLRecords"].Value;
                var addedRecordTmkIds = (string)cmd.Parameters["@AddedRecordTmkIds"].Value;

                return Tuple.Create(addedRecords, existingRecords, noCLRecords, addedRecordTmkIds);
            }
        }

        public async Task AddCustomFieldsAsCopyFields()
        {
            await _dbContext.Database.ExecuteSqlAsync(@$"Insert Into tblTmkTrademarkCopySetting(FieldDesc,FieldName,[Copy],UserName)
                                                 Select cfs.ColumnLabel,cfs.ColumnName,0,cs.UserName from tblSysCustomFieldSetting cfs 
                                                 Cross Join(Select Distinct UserName From tblTmkTrademarkCopySetting) cs
                                                 Where cfs.TableName='tblTmkTrademark' and cfs.Visible = 1 
                                                 And Not Exists(Select 1 From tblTmkTrademarkCopySetting ecs Where ecs.FieldName=cfs.ColumnName and isnull(ecs.UserName,'')=isnull(cs.UserName,''))
                                                 Order By cfs.OrderOfEntry");
            
            await _dbContext.Database.ExecuteSqlAsync(@$"Delete ecs From tblTmkTrademarkCopySetting ecs 
                    Where ecs.FieldName like 'CustomField%' And FieldName Not In(Select cfs.ColumnName from tblSysCustomFieldSetting cfs 
                    Where cfs.TableName='tblTmkTrademark' and cfs.Visible = 1) ");
        }

        #endregion

        #region Family/Designation
        public async Task<bool> CanHaveDesignatedCountry(string country, string caseType)
        {
            return await _dbContext.TmkDesCaseTypes.AnyAsync(dc => dc.IntlCode == country && dc.CaseType == caseType);
        }
        #endregion

        #region Defaults, Action Gen, etc.

        protected async Task AutoAddDefaults(TmkTrademark trademark)
        {
            await AutoAddDefaultAgent(trademark);
        }

        protected async Task AutoAddDefaultAgent(TmkTrademark trademark)
        {
            if (trademark.AgentID == null)
            {
                var agentId = await _dbContext.TmkCountryLaws
                    .Where(cl => cl.Country == trademark.Country && cl.CaseType == trademark.CaseType)
                    .Select(cl => cl.DefaultAgent).FirstOrDefaultAsync();
                if (agentId != null && agentId > 0)
                    trademark.AgentID = agentId;
            }
        }
        public bool AnyActionFieldsModified(TmkTrademarkModifiedFields modifiedFields)
        {
            return (modifiedFields.IsChgCtryCaseType || modifiedFields.IsChgFilDate || modifiedFields.IsChgPubDate ||
                    modifiedFields.IsChgRegDate || modifiedFields.IsChgPriDate || modifiedFields.IsChgAllowDate ||
                    modifiedFields.IsChgLastRenDate || modifiedFields.IsChgNextRenDate || modifiedFields.IsChgParentFilDate);
        }

        protected async Task<int> GenerateCountryLawActions(int tmkId, string updatedBy, TmkTrademarkModifiedFields modifiedFields, string caller, DateTime? dateCreated)
        {
            var tmkParams = BuildLawParameters(tmkId, modifiedFields);
            var delegationId = 0;
            using (SqlCommand cmd = new SqlCommand("procTmkGenCountryLawActions"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters.RemoveAt("@TmkIds");
                cmd.Parameters.AddWithValue("@TmkIds", tmkParams).SqlDbType = SqlDbType.Structured;
                cmd.Parameters["@CreatedBy"].Value = updatedBy;
                cmd.Parameters["@Caller"].Value = caller;
                cmd.Parameters.RemoveAt("@DelegationId");
                cmd.Parameters.Add("@DelegationId", SqlDbType.Int).Direction = System.Data.ParameterDirection.Output;

                if (dateCreated.HasValue)
                    cmd.Parameters["@DateCreated"].Value = dateCreated;

                await cmd.ExecuteNonQueryAsync();
                delegationId = (int)cmd.Parameters["@DelegationId"].Value;
                return delegationId;
            }
        }

        protected async Task SyncToDesignatedTrademarks(TmkTrademark trademark, TmkTrademarkModifiedFields modifiedFields)
        {
            if (await _dbContext.TmkCountryLaws.AnyAsync(cl => cl.Country == trademark.Country && cl.CaseType == trademark.CaseType && cl.AutoUpdtDesTmkRecs == 1))
            {
                var tmkParams = BuildLawParameters(trademark.TmkId, modifiedFields);
                using (SqlCommand cmd = new SqlCommand("procTmkDesCtryUpdateFromParent"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                    if (cmd.Connection.State == ConnectionState.Closed)
                        cmd.Connection.Open();

                    SqlCommandBuilder.DeriveParameters(cmd);
                    cmd.Parameters.RemoveAt("@TmkIds");
                    cmd.Parameters.AddWithValue("@TmkIds", tmkParams).SqlDbType = SqlDbType.Structured;
                    cmd.Parameters["@UpdatedBy"].Value = trademark.UpdatedBy;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task SyncChildToDesignatedTrademarks(TmkTrademark trademark, TmkTrademarkModifiedFields modifiedFields, Type childType)
        {
            if (await _dbContext.TmkCountryLaws.AnyAsync(cl => cl.Country == trademark.Country && cl.CaseType == trademark.CaseType && cl.AutoUpdtDesTmkRecs == 1))
            {
                var tmkParams = BuildLawParameters(trademark.TmkId, modifiedFields);
                using (SqlCommand cmd = new SqlCommand("procTmkDesCtryUpdateFromParent"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                    if (cmd.Connection?.State == ConnectionState.Closed)
                        cmd.Connection.Open();

                    SqlCommandBuilder.DeriveParameters(cmd);
                    cmd.Parameters.RemoveAt("@TmkIds");
                    cmd.Parameters.AddWithValue("@TmkIds", tmkParams).SqlDbType = SqlDbType.Structured;
                    cmd.Parameters["@UpdatedBy"].Value = trademark.UpdatedBy;
                    cmd.Parameters["@UpdateChildOnly"].Value= true;
                    
                    if (childType == typeof(TmkAssignmentHistory))
                        cmd.Parameters["@UpdateAssignment"].Value = true;
                    else if (childType == typeof(TmkLicensee))
                        cmd.Parameters["@UpdateLicensee"].Value = true;
                    else if (childType == typeof(DocDocument) || childType == typeof(DocFolder))
                        cmd.Parameters["@UpdateImage"].Value = true;
                    else if (childType == typeof(TmkKeyword))
                        cmd.Parameters["@UpdateKeyword"].Value = true;
                    else if (childType == typeof(TmkTrademarkClass))
                        cmd.Parameters["@UpdateGoods"].Value = true;
                    else if (childType == typeof(PatRelatedTrademark))
                        cmd.Parameters["@UpdateRelatedPatent"].Value = true;
                    else if (childType == typeof(TmkOwner))
                        cmd.Parameters["@UpdateOwner"].Value = true;
                    else if (childType == typeof(TmkProduct))
                        cmd.Parameters["@UpdateProduct"].Value = true;
                    await cmd.ExecuteNonQueryAsync();

                }
            }
        }

        private List<SqlDataRecord> BuildLawParameters (int tmkId, TmkTrademarkModifiedFields modifiedFields)
        {
            var record = new SqlDataRecord(
                    new SqlMetaData("TmkId", SqlDbType.Int),
                    new SqlMetaData("IsChgCtryCaseType", SqlDbType.Bit),
                    new SqlMetaData("IsChgFilDate", SqlDbType.Bit),
                    new SqlMetaData("IsChgPubDate", SqlDbType.Bit),
                    new SqlMetaData("IsChgRegDate", SqlDbType.Bit),
                    new SqlMetaData("IsChgPriDate", SqlDbType.Bit),
                    new SqlMetaData("IsChgAllowDate", SqlDbType.Bit),
                    new SqlMetaData("IsChgLastRenDate", SqlDbType.Bit),
                    new SqlMetaData("IsChgNextRenDate", SqlDbType.Bit),
                    new SqlMetaData("IsChgParentFilDate", SqlDbType.Bit));

            record.SetInt32(0, tmkId);
            record.SetBoolean(1, modifiedFields.IsChgCtryCaseType);
            record.SetBoolean(2, modifiedFields.IsChgFilDate);
            record.SetBoolean(3, modifiedFields.IsChgPubDate);
            record.SetBoolean(4, modifiedFields.IsChgRegDate);
            record.SetBoolean(5, modifiedFields.IsChgPriDate);
            record.SetBoolean(6, modifiedFields.IsChgAllowDate);
            record.SetBoolean(7, modifiedFields.IsChgLastRenDate);
            record.SetBoolean(8, modifiedFields.IsChgNextRenDate);
            record.SetBoolean(9, modifiedFields.IsChgParentFilDate);
            return new List<SqlDataRecord> { record };
    }


        #endregion

        #region Actions
        public async Task<List<ActionTabDTO>> GetActions(int tmkId, ActionDisplayOption actionDisplayOption)
        {
            var sql = "Select * From vwTmkTrademarkAction Where TmkId = {0}";

            if (actionDisplayOption == ActionDisplayOption.Open) sql += " And DateTaken Is Null";
            else if (actionDisplayOption == ActionDisplayOption.Close) sql += " And DateTaken Is Not Null";
            sql += " Order By DueDate, ActionDue;";

            var actions = await _dbContext.ActionTabDTO.FromSqlRaw(sql, tmkId).AsNoTracking().ToListAsync();
            return actions;
        }
        public async Task ActionsUpdate(int tmkId, string userName, IEnumerable<TmkDueDate> updatedActions, IEnumerable<TmkDueDate> deletedActions)
        {
            // note, this really a due date update but it is done on the action tab of the main screen
            foreach (var item in deletedActions)
            {
                _dbContext.Set<TmkDueDate>().Remove(item);
            }

            foreach (var item in updatedActions)
            {
                _dbContext.Entry(item).State = EntityState.Modified;
            }

            // there may be several actions to be updated - get the unique actid
            var actIds = (deletedActions.Select(d => d.ActId).Union(updatedActions.Select(u => u.ActId))).Distinct();

            foreach (var actId in actIds)
            {
                var actionDue = _dbContext.TmkActionDues.FirstOrDefault(a => a.ActId == actId);
                if (actionDue != null)
                {
                    actionDue.UpdatedBy = userName;
                    actionDue.LastUpdate = DateTime.Now;
                }
            }

            var trademark = _dbContext.TmkTrademarks.FirstOrDefault(t => t.TmkId == tmkId);
            if (trademark != null)
            {
                trademark.UpdatedBy = userName;
                trademark.LastUpdate = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task ActionDelete(TmkDueDate deletedAction)
        {
            _dbContext.Set<TmkDueDate>().Remove(deletedAction);
            var actionDue = _dbContext.TmkActionDues.FirstOrDefault(a => a.ActId == deletedAction.ActId);
            if (actionDue != null)
            {
                actionDue.UpdatedBy = deletedAction.UpdatedBy;
                actionDue.LastUpdate = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task CheckChildlessActionDue(IEnumerable<int> affectedIds)
        {
            if (!affectedIds.Any()) return;

            var actIds = new List<SqlDataRecord> {};

            foreach (var actId  in affectedIds)
            { 
                var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.Int) });
                record.SetInt32(0, actId);
                actIds.Add(record);
            }
            using (SqlCommand cmd = new SqlCommand("procWebTmkActionNoChildCheck"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@ActIds", actIds).SqlDbType = SqlDbType.Structured;
                await cmd.ExecuteNonQueryAsync();
            }
        }
        public async Task<List<DelegationEmailDTO>> GetDelegationEmails(int delegationId)
        {
            var list = await _dbContext.DelegationEmailDTO.FromSqlInterpolated($@"Select Distinct DelegationId,AssignedBy,AssignedTo,FirstName,LastName From
                                (Select ddd.DelegationId,ddd.CreatedBy as AssignedBy,u.Email as AssignedTo,u.FirstName,u.LastName From tblCPiGroups g Inner Join tblCPiUserGroups ug on g.Id=ug.GroupId Inner Join tblCPIUsers u on u.Id=ug.UserId Inner Join tblTmkDueDateDelegation ddd on ddd.GroupId=ug.GroupId Union
                                 Select ddd.DelegationId,ddd.CreatedBy as AssignedBy,u.Email as AssignedTo,u.FirstName,u.LastName From  tblCPIUsers u  Inner Join tblTmkDueDateDelegation ddd on ddd.UserId=u.Id
                                ) t Where t.DelegationId={delegationId}").AsNoTracking().ToListAsync();
            return list;
        }

        public async Task MarkDelegationasEmailed(int delegationId)
        {
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"Update tblTmkDueDateDelegation Set NotificationSent=1 Where DelegationId={delegationId}");
        }

        public async Task<List<LookupIntDTO>> GetDelegatedDdIds(int action, int[] recIds)
        {
            var ids = new List<SqlDataRecord>();
            var result = new List<LookupIntDTO>();
            foreach (var item in recIds)
            {
                var record = new SqlDataRecord(new SqlMetaData[] { 
                    new SqlMetaData("Id", SqlDbType.Int),
                    new SqlMetaData("DueDate", SqlDbType.DateTime2)
                });
                record.SetInt32(0, item);
                record.SetDBNull(1);
                ids.Add(record);
            }
            using (SqlCommand cmd = new SqlCommand("procTmkDelegatedTask"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@Ids", ids).SqlDbType = SqlDbType.Structured;
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result.Add(new LookupIntDTO { Value = reader.GetInt32(0) });
                    }
                }
                cmd.Connection?.Close();
            }
            return result;
        }

        public async Task<List<DelegationEmailDTO>> GetDeletedDelegationEmails(int delegationId)
        {
            var list = await _dbContext.DelegationEmailDTO.FromSqlInterpolated($"exec procTmkDelegatedTask @action = 3, @delegationid = {delegationId}").AsNoTracking().ToListAsync();
            return list;
        }

        public async Task<DelegationDetailDTO> GetDeletedDelegation(int delegationId)
        {
            var delegation = await _dbContext.DelegationDetailDTO.FromSqlInterpolated($"Select DataKeyValue as DelegationId, dddLog.ActID,dddLog.DDID,dddLog.GroupId,dddLog.UserId,dddLog.NotificationSent,ParentActId,ParentId    From tblDeleteLog d WITH (NOLOCK) cross apply openjson(d.record) With(ActId int '$.ActId',DDId int '$.DDId',GroupId int '$.GroupId',UserId nvarchar(450) '$.UserId',NotificationSent int '$.NotificationSent',ParentActId int '$.ParentActId',ParentId int '$.ParentId') as dddlog Where DataKey='DelegationId' and SystemType='T' And DataKeyValue={delegationId}").AsNoTracking().FirstOrDefaultAsync();
            return delegation;
        }

        public async Task<List<LookupIntDTO>> GetDuedateChangedDelegationIds(int action, List<TmkDueDate> updated)
        {
            var ids = new List<SqlDataRecord>();
            var result = new List<LookupIntDTO>();
            foreach (var item in updated)
            {
                var record = new SqlDataRecord(new SqlMetaData[] {
                    new SqlMetaData("Id", SqlDbType.Int),
                    new SqlMetaData("DueDate", SqlDbType.DateTime2)
                });
                record.SetInt32(0, item.DDId);
                record.SetDateTime(1, item.DueDate);
                ids.Add(record);
            }
            using (SqlCommand cmd = new SqlCommand("procTmkDelegatedTask"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@Ids", ids).SqlDbType = SqlDbType.Structured;
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result.Add(new LookupIntDTO { Value = reader.GetInt32(0) });
                    }
                }
                cmd.Connection?.Close();
            }
            return result;
        }

        #endregion

        #region Costs
        public async Task<List<TmkCostTrack>> GetCosts(int tmkId)
        {
            return await _dbContext.TmkCostTracks.Where(c => c.TmkId == tmkId).AsNoTracking().ToListAsync();
        }
        public async Task CostsUpdate(int tmkId, string userName, IEnumerable<TmkCostTrack> updatedCostTracks, IEnumerable<TmkCostTrack> deletedCostTracks)
        {
            // note, this really a due date update but it is done on the action tab of the main screen
            foreach (var item in deletedCostTracks)
            {
                _dbContext.Set<TmkCostTrack>().Remove(item);
            }

            foreach (var item in updatedCostTracks)
            {
                _dbContext.Entry(item).State = EntityState.Modified;
            }

            var trademark = _dbContext.TmkTrademarks.FirstOrDefault(t => t.TmkId == tmkId);
            if (trademark != null)
            {
                trademark.UpdatedBy = userName;
                trademark.LastUpdate = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task CostDelete(TmkCostTrack deletedCostTrack)
        {
            _dbContext.Set<TmkCostTrack>().Remove(deletedCostTrack);

            var trademark = _dbContext.TmkTrademarks.FirstOrDefault(t => t.TmkId == deletedCostTrack.TmkId);
            if (trademark != null)
            {
                trademark.UpdatedBy = deletedCostTrack.UpdatedBy;
                trademark.LastUpdate = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
        }
        #endregion

        #region Licensees
        public async Task<List<TmkLicensee>> GetLicensees(int tmkId)
        {
            return await _dbContext.TmkLicensees.Where(c => c.TmkId == tmkId).AsNoTracking().ToListAsync();
        }

        public async Task LicenseesUpdate(int tmkId, string userName, IEnumerable<TmkLicensee> updatedLicensees, IEnumerable<TmkLicensee> newLicensees, IEnumerable<TmkLicensee> deletedLicensees)
        {
            foreach (var item in deletedLicensees)
            {
                _dbContext.Set<TmkLicensee>().Remove(item);
            }

            foreach (var item in updatedLicensees)
            {
                _dbContext.Entry(item).State = EntityState.Modified;
            }

            foreach (var item in newLicensees)
            {
                item.TmkId = tmkId;
                _dbContext.Add(item);
            }

            var trademark = _dbContext.TmkTrademarks.FirstOrDefault(a => a.TmkId == tmkId);
            if (trademark != null)
            {
                trademark.UpdatedBy = userName;
                trademark.LastUpdate = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task LicenseeDelete(TmkLicensee deletedLicensee)
        {
            _dbContext.Set<TmkLicensee>().Remove(deletedLicensee);
            var application = _dbContext.TmkTrademarks.FirstOrDefault(a => a.TmkId == deletedLicensee.TmkId);
            if (application != null)
            {
                application.UpdatedBy = deletedLicensee.UpdatedBy;
                application.LastUpdate = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
        }
        public IQueryable<TmkLicensee> TmkLicensees => _dbContext.TmkLicensees.AsNoTracking();
        #endregion

        #region Products
           public IQueryable<TmkProduct> TmkProducts => _dbContext.TmkProducts.AsNoTracking();
        #endregion

        #region Family Tree View

        public async Task<IEnumerable<FamilyTreeDTO>> GetFamilyTree(string paramType, string paramValue, string paramParent)
        {
            var familyTree = await _dbContext.FamilyTreeDTO.FromSqlInterpolated($"Exec procTmkTV @ParamType={paramType}, @ParamValue={paramValue}, @ParamParent={paramParent}")
                                                   .AsNoTracking().ToListAsync();
            return familyTree;
        }

        public async Task<FamilyTreeTmkDTO> GetNodeDetails(string paramType, string paramValue)
        {
            var treeDetail = (await _dbContext.FamilyTreeTmkDTO.FromSqlInterpolated($"Exec procTmkTVDetail @ParamType={paramType}, @ParamValue={paramValue}").ToListAsync()).FirstOrDefault();

            return treeDetail;
        }

        public void UpdateParent(int childTmkId, int newParentId, string parentInfo, string userName)
        {

            using (SqlCommand cmd = new SqlCommand("procTmkTVUpdate"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters["@ChildId"].Value = childTmkId;
                cmd.Parameters["@NewParentId"].Value = newParentId;
                cmd.Parameters["@ParentInfo"].Value = parentInfo;
                cmd.Parameters["@UpdatedBy"].Value = userName;

                cmd.ExecuteNonQuery();
            }
        }
        #endregion





    }
}
