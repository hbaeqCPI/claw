using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core;
using System.Transactions;
using Microsoft.Data.SqlClient.Server;
using System.Data;
using Microsoft.Data.SqlClient;
using R10.Core.Entities;
using R10.Core.Interfaces.Patent;
using R10.Core.Entities.Documents;
using System;
using R10.Core.Exceptions;
using R10.Core.Entities.Shared;
using System.Security.Claims;
using R10.Core.Interfaces.Shared;
using R10.Core.Helpers;

namespace R10.Infrastructure.Data.Patent
{

    public class CountryApplicationRepository : ICountryApplicationRepository
    {
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly IPatIDSRepository _idsRepository;
        private readonly ICPiDbContext _cpiDbContext;
        private readonly ClaimsPrincipal _user;
        private readonly ITradeSecretService _tradeSecretService;

        public CountryApplicationRepository(ICPiDbContext cpiDbContext,
            IPatIDSRepository idsRepository, ISystemSettings<PatSetting> settings,
            ClaimsPrincipal user, ITradeSecretService tradeSecretService)
        {
            _settings = settings;
            _idsRepository = idsRepository;
            _cpiDbContext = cpiDbContext;
            _user = user;
            _tradeSecretService = tradeSecretService;
        }

        private IQueryable<CountryApplication> QueryableList => _cpiDbContext.GetRepository<CountryApplication>().QueryableList;

        public async Task<CountryApplication> Add(CountryApplication application, PatIDSRelatedCasesInfo idsInfo, ApplicationModifiedFields modifiedFields, DateTime dateCreated,
                                                  bool hasRelatedCasesMassCopy,string? sessionKey)
        {
            //await AutoAddDefaults(application);

            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                _cpiDbContext.GetRepository<CountryApplication>().Add(application);
                await CheckPriority(application, modifiedFields);
                await AddSingleOwner(application);
                var tsActivity = await SetTradeSecret(application);
                await _cpiDbContext.SaveChangesAsync();

                if (!string.IsNullOrEmpty(tsActivity.ActivityCode))
                {
                    await CreateTradeSecretActivityLog(application.InvId, application.AppId, tsActivity.ActivityCode, tsActivity.AuditLogs);
                    await _cpiDbContext.SaveChangesAsync();
                }

                if (AnyActionFieldsModified(modifiedFields))
                {
                    await GenerateCountryLawActions(new int[] { application.AppId }, application.UpdatedBy, modifiedFields, dateCreated, sessionKey);
                }

                if (idsInfo != null)
                {
                    idsInfo.AppId = application.AppId;
                    await _idsRepository.SaveIDSInfo(idsInfo);
                }
                if (hasRelatedCasesMassCopy)
                {
                    await RelatedCasesMassCopy(application.AppId, application.InvId, application.CreatedBy);
                }
                scope.Complete();
                return application;
            }
        }

        public async Task Update(CountryApplication application, PatIDSRelatedCasesInfo idsInfo, ApplicationModifiedFields modifiedFields, DateTime dateCreated, bool hasRelatedCasesMassCopy, string? sessionKey)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                //possible CL action based on priority (like CN ORD Request for Examination)
                var generateCLFromPriority = await _cpiDbContext.GetRepository<PatPriority>().QueryableList.AnyAsync(p => p.ParentAppId == application.AppId && p.FilDate != application.FilDate);

                _cpiDbContext.GetRepository<CountryApplication>().Update(application);

                await CheckPriority(application, modifiedFields);
                await AddSingleOwner(application);

                var tsActivity = await SetTradeSecret(application);
                if (!string.IsNullOrEmpty(tsActivity.ActivityCode))
                    await CreateTradeSecretActivityLog(application.InvId, application.AppId, tsActivity.ActivityCode, tsActivity.AuditLogs);

                await _cpiDbContext.SaveChangesAsync();
                _cpiDbContext.Detach(application);

                if (AnyActionFieldsModified(modifiedFields))
                {
                    await GenerateCountryLawActions(new int[] { application.AppId }, application.UpdatedBy, modifiedFields, dateCreated, sessionKey);
                }

                await SyncToDesignatedApplications(application, modifiedFields);

                if (idsInfo != null)
                {
                    await _idsRepository.SaveIDSInfo(idsInfo);
                }

                if (generateCLFromPriority)
                {
                    await GenerateCountryLawFromPriority(application.InvId, application.UpdatedBy);
                }

                //should only be true here if casenumber was modified (new family)
                if (hasRelatedCasesMassCopy)
                {
                    await RelatedCasesMassCopy(application.AppId, application.InvId, application.CreatedBy);
                }

                //when EP grant, cascade grant info to validated countries
                if (application.Country.ToLower() == "ep" && "granted,issued".Contains(application.ApplicationStatus.ToLower()) &&
                    (modifiedFields.IsChgIssDate || modifiedFields.IsChgPatNumber))
                {
                    await UpdateEPDesignatedCountries(application.AppId, application.UpdatedBy);
                    await InsertEPDesignatedCountriesOwner(application.AppId, application.UpdatedBy);
                }
                scope.Complete();
            }
        }

        public async Task Delete(CountryApplication application)
        {
            var canDeleteTradeSecret = await CanDeleteTradeSecret(application.AppId);
            Guard.Against.UnAuthorizedAccess(canDeleteTradeSecret.Allowed);

            _cpiDbContext.GetRepository<CountryApplication>().Delete(application);

            if (canDeleteTradeSecret.InvId > 0)
                await CreateTradeSecretActivityLog(canDeleteTradeSecret.InvId, application.AppId, TradeSecretActivityCode.Delete, CreateAuditLogs(await QueryableList.SingleOrDefaultAsync(ca => ca.AppId == application.AppId), null));

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task UpdateChild<T>(CountryApplication application, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                application.UpdatedBy = userName;
                application.LastUpdate = DateTime.Now;
                var parent = _cpiDbContext.GetRepository<CountryApplication>().Attach(application);
                parent.Property(c => c.UpdatedBy).IsModified = true;
                parent.Property(c => c.LastUpdate).IsModified = true;

                foreach (var item in updated)
                {
                    item.UpdatedBy = application.UpdatedBy;
                    item.LastUpdate = application.LastUpdate;
                }

                foreach (var item in added)
                {
                    item.CreatedBy = application.UpdatedBy;
                    item.DateCreated = application.LastUpdate;
                    item.UpdatedBy = application.UpdatedBy;
                    item.LastUpdate = application.LastUpdate;
                }
                var dbSet = _cpiDbContext.GetRepository<T>();
                if (updated.Any())
                    dbSet.Update(updated);

                if (added.Any())
                    dbSet.Add(added);

                if (deleted.Any())
                    dbSet.Delete(deleted);

                await _cpiDbContext.SaveChangesAsync();

                await SyncChildToDesignatedApplications(application.AppId, application.Country, application.CaseType, userName, typeof(T));
                scope.Complete();
            }
        }

        public async Task SyncChildToDesignatedApplications(int appId, string country, string caseType, string userName, Type childType)
        {
            var process = await _cpiDbContext.GetRepository<PatCountryLaw>().QueryableList.AnyAsync(cl => cl.Country == country && cl.CaseType == caseType && cl.AutoUpdtDesPatRecs == 1);
            if (process)
            {
                using (SqlCommand cmd = new SqlCommand("procPatDesCtryUpdateFromParent"))
                {
                    var connectionString = _cpiDbContext.GetDbConnection().ConnectionString;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Connection = new SqlConnection(connectionString);
                    if (cmd.Connection?.State == ConnectionState.Closed)
                        cmd.Connection.Open();

                    cmd.Parameters.AddWithValue("@AppId", appId);
                    cmd.Parameters.AddWithValue("@UpdateChildOnly", true);
                    cmd.Parameters.AddWithValue("@UpdatedBy", userName);

                    if (childType == typeof(PatAssignmentHistory))
                        cmd.Parameters.AddWithValue("@UpdateAssignment", true);
                    else if (childType == typeof(PatInventorApp))
                        cmd.Parameters.AddWithValue("@UpdateInventor", true);
                    else if (childType == typeof(PatOwnerApp))
                        cmd.Parameters.AddWithValue("@UpdateOwner", true);
                    else if (childType == typeof(PatLicensee))
                        cmd.Parameters.AddWithValue("@UpdateLicensee", true);
                    else if (childType == typeof(DocDocument) || childType == typeof(DocFolder))
                        cmd.Parameters.AddWithValue("@UpdateImage", true);
                    else if (childType == typeof(PatRelatedTrademark))
                        cmd.Parameters.AddWithValue("@UpdateRelatedTrademark", true);
                    else if (childType == typeof(PatRelatedCase))
                        cmd.Parameters.AddWithValue("@UpdateRelatedCase", true);
                    else if (childType == typeof(PatProduct))
                        cmd.Parameters.AddWithValue("@UpdateProduct", true);
                    else if (childType == typeof(PatSubjectMatter))
                        cmd.Parameters.AddWithValue("@UpdateSubjectMatter", true);
                    await cmd.ExecuteNonQueryAsync();

                }
            }
        }

        public async Task CopyCountryApplication(int oldAppId, int newAppId, bool copyImages, bool copyAssignments,
            bool copyInventors, bool copyLicenses, bool copyOwners, bool copyCosts, bool copyIDS, bool copyRelatedCases,
            bool copyRelatedTrademarks, bool copyInventorAward, bool copyProducts, bool copyTerminalDisclaimer, string userName)
        {
            await _cpiDbContext.ExecuteSqlInterpolatedAsync($"procPatAppCopy @OldAppId={oldAppId},@NewAppId={newAppId},@CreatedBy={userName},@CopyImages={copyImages},@CopyAssignments={copyAssignments},@CopyInventors={copyInventors},@CopyLicenses={copyLicenses},@CopyOwners={copyOwners},@CopyCosts={copyCosts},@CopyIDS={copyIDS},@CopyRelatedCases={copyRelatedCases},@CopyRelatedTrademarks={copyRelatedTrademarks},@CopyInventorAward={copyInventorAward},@CopyProducts={copyProducts},@CopyTerminalDisclaimer={copyTerminalDisclaimer}");
        }

        public async Task GenerateCountryLawFromPriority(int invId, string userName) {
            var patCountryDues = _cpiDbContext.GetRepository<PatCountryDue>().QueryableList;
            var appIds = await QueryableList.Where(ca => ca.InvId == invId && patCountryDues.Any(cd => cd.Country == ca.Country && cd.CaseType == ca.CaseType && cd.BasedOn == "Priority")).Select(ca => ca.AppId).ToListAsync();
            if (appIds.Count > 0) {
                var modifiedFields = new ApplicationModifiedFields { IsChgPriorityDate = true };
                var dateCreated = DateTime.Now;
                await GenerateCountryLawActions(appIds.ToArray(), userName, modifiedFields, dateCreated);
            }
        }

        public async Task UpdateExpirationDate(List<PatTerminalDisclaimerChildDTO> children, string updatedBy)
        {
            if (children.Any()) {
                foreach (var child in children)
                {
                    if (child.NewExpDate != null)
                        await _cpiDbContext.ExecuteSqlRawAsync($"Update tblPatCountryApplication Set ExpDate = '{child.NewExpDate}',UpdatedBy='{updatedBy}',LastUpdate=getdate() Where Appid={child.AppId} ");
                    else
                        await _cpiDbContext.ExecuteSqlRawAsync($"Update tblPatCountryApplication Set ExpDate = null,UpdatedBy='{updatedBy}',LastUpdate=getdate() Where Appid={child.AppId} ");
                }
            }
        }


        public async Task<List<PatActionMultipleBasedOnDTO>> GetActionsWithMultipleBasedOn(int appId,string? sessionKey)
        {
            var list = await _cpiDbContext.Query<PatActionMultipleBasedOnDTO>().FromSqlInterpolated($@"Select LogId,AppId,ActionType,ActionDue,BasedOn,BaseDate,DueDate,Accept From tblPatActionMultipleBasedOn Where SessionKey={sessionKey} And AppId={appId}").AsNoTracking().ToListAsync();
            return list;
        }

        public async Task GenerateActionsWithMultipleBasedOn(List<PatActionMultipleBasedOnSelectionDTO> list,string? createdBy)
        {
            var logIds = new List<SqlDataRecord>();
            foreach (var item in list)
            {
                var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("LogId", SqlDbType.Int), new SqlMetaData("Accept", SqlDbType.Bit) });
                record.SetInt32(0, item.LogId);
                record.SetBoolean(1, item.Accept);
                logIds.Add(record);
            }

            using (SqlCommand cmd = new SqlCommand("procPatCL_MultipleBasedOn"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _cpiDbContext.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@CreatedBy", createdBy);
                cmd.Parameters.AddWithValue("@LogIds", logIds).SqlDbType = SqlDbType.Structured;
                await cmd.ExecuteNonQueryAsync();
            }
        }



        protected async Task CheckPriority(CountryApplication application, ApplicationModifiedFields modifiedFields)
        {
            var settings = await _settings.GetSetting();
            var caseTypes = settings.PrioCaseTypeAutoPopulate;

            if (caseTypes.Contains(application.CaseType) && modifiedFields.IsChgCaseType || modifiedFields.IsChgFilDate)
                modifiedFields.IsChgPriorityDate = true;
        }

        protected bool AnyActionFieldsModified(ApplicationModifiedFields modifiedFields)
        {
            return (modifiedFields.IsChgCaseType || modifiedFields.IsChgFilDate || modifiedFields.IsChgPubDate ||
                    modifiedFields.IsChgIssDate || modifiedFields.IsChgParentFilDate || modifiedFields.IsChgParentIssDate ||
                    modifiedFields.IsChgPCTDate || modifiedFields.IsChgPriorityDate);
        }

        protected async Task GenerateCountryLawActions(int[] affectedAppIds, string userId, ApplicationModifiedFields modifiedFields, DateTime dateCreated,string? sessionKey="")
        {
            var appIds = new List<SqlDataRecord>();
            foreach (var item in affectedAppIds)
            {
                var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.Int) });
                record.SetInt32(0, item);
                appIds.Add(record);
            }

            using (SqlCommand cmd = new SqlCommand("procPatCL_CalcActions"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _cpiDbContext.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters.RemoveAt("@AppIds");
                cmd.FillParamValues(modifiedFields);
                cmd.Parameters["@UpdatedBy"].Value = userId;
                cmd.Parameters["@DateCreated"].Value = dateCreated;
                cmd.Parameters["@SessionKey"].Value = sessionKey;

                cmd.Parameters.AddWithValue("@AppIds", appIds).SqlDbType = SqlDbType.Structured;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        protected async Task SyncToDesignatedApplications(CountryApplication application, ApplicationModifiedFields modifiedFields)
        {
            if (await _cpiDbContext.GetRepository<PatCountryLaw>().QueryableList.AnyAsync(cl => cl.Country == application.Country && cl.CaseType == application.CaseType && cl.AutoUpdtDesPatRecs == 1))
            {
                using (SqlCommand cmd = new SqlCommand("procPatDesCtryUpdateFromParent"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Connection = _cpiDbContext.GetDbConnection() as SqlConnection;
                    if (cmd.Connection?.State == ConnectionState.Closed)
                        cmd.Connection.Open();

                    SqlCommandBuilder.DeriveParameters(cmd);
                    cmd.FillParamValues(modifiedFields);
                    cmd.Parameters["@AppId"].Value = application.AppId;
                    cmd.Parameters["@UpdatedBy"].Value = application.UpdatedBy;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        protected async Task AddSingleOwner(CountryApplication application)
        {
            if (application.OwnerID > 0)
            {
                var existing = await _cpiDbContext.GetRepository<PatOwnerApp>().QueryableList.FirstOrDefaultAsync(o => o.AppId == application.AppId);
                if (existing != null)
                {
                    existing.OwnerID = (int)application.OwnerID;
                    existing.UpdatedBy = application.UpdatedBy;
                    existing.LastUpdate = application.LastUpdate;
                }
                else
                    _cpiDbContext.GetRepository<PatOwnerApp>().Add(new PatOwnerApp
                    {
                        AppId = application.AppId,
                        OwnerID = (int)application.OwnerID,
                        CreatedBy = application.CreatedBy,
                        UpdatedBy = application.UpdatedBy,
                        DateCreated = application.DateCreated,
                        LastUpdate = application.LastUpdate
                    });
                application.OwnerID = null;
            }
        }

        public async Task RelatedCasesMassCopy(int appId, int invId, string? createdBy)
        {
            var ids = new List<SqlDataRecord>();
            var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("AppId", SqlDbType.Int), new SqlMetaData("InvId", SqlDbType.Int) });
            record.SetInt32(0, appId);
            record.SetInt32(1, invId);
            ids.Add(record);

            using (SqlCommand cmd = new SqlCommand("procPatRelatedCasesMassCopyFamily"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _cpiDbContext.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                cmd.Parameters.AddWithValue("@Ids", ids).SqlDbType = SqlDbType.Structured;
                cmd.Parameters.AddWithValue("@CreatedBy", createdBy);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task AddCustomFieldsAsCopyFields()
        {
            await _cpiDbContext.ExecuteSqlAsync(@$"Insert Into tblPatCountryApplicationCopySetting(FieldDesc,FieldName,[Copy],UserName)
                                                 Select cfs.ColumnLabel,cfs.ColumnName,0,cs.UserName from tblSysCustomFieldSetting cfs 
                                                 Cross Join(Select Distinct UserName From tblPatCountryApplicationCopySetting) cs
                                                 Where cfs.TableName='tblPatCountryApplication' and cfs.Visible = 1 
                                                 And Not Exists(Select 1 From tblPatCountryApplicationCopySetting ecs Where ecs.FieldName=cfs.ColumnName and isnull(ecs.UserName,'')=isnull(cs.UserName,''))
                                                 Order By cfs.OrderOfEntry");

            await _cpiDbContext.ExecuteSqlAsync(@$"Delete ecs From tblPatCountryApplicationCopySetting ecs 
                    Where ecs.FieldName like 'CustomField%' And FieldName Not In(Select cfs.ColumnName from tblSysCustomFieldSetting cfs 
                    Where cfs.TableName='tblPatCountryApplication' and cfs.Visible = 1) ");
        }

        public async Task UpdateEPDesignatedCountries(int parentAppId, string updatedBy)
        {
            await _cpiDbContext.ExecuteSqlInterpolatedAsync($"Update ca Set ca.PatNumber=parent.PatNumber, ca.IssDate = parent.IssDate,ca.UpdatedBy={updatedBy},ca.LastUpdate=getdate() from tblPatCountryApplication ca Inner Join tblPatDesignatedCountry desCtry on desCtry.GenAppId=ca.AppId Inner Join tblPatCountryApplication parent on parent.AppId=desCtry.AppId And parent.AppId={parentAppId}");
        }

        public async Task InsertEPDesignatedCountriesOwner(int parentAppId, string updatedBy)
        {
            await _cpiDbContext.ExecuteSqlInterpolatedAsync($"Insert Into tblPatOwnerApp ( AppId, OwnerID, OrderOfEntry, Remarks,[Percentage],CreatedBy,UpdatedBy,DateCreated,LastUpdate ) Select desCtry.GenAppId, srcOwner.OwnerID, srcOwner.OrderOfEntry, srcOwner.Remarks,srcOwner.[Percentage],{updatedBy},{updatedBy},getdate(),getdate() From tblPatOwnerApp srcOwner Inner Join tblPatCountryApplication parent on parent.AppId=srcOwner.AppId Inner Join tblPatDesignatedCountry desCtry on desCtry.AppId=parent.AppId Inner Join tblPatCountryApplication child on child.AppId=desCtry.GenAppId Left Join tblPatOwnerApp ex on ex.AppId=desCtry.GenAppId and ex.OwnerID=srcOwner.OwnerID Where srcOwner.AppId={parentAppId} and ex.AppId is null");
        }

        #region Designation
        public async Task<bool> CanHaveDesignatedCountry(string country, string caseType)
        {
            return await _cpiDbContext.GetRepository<PatDesCaseType>().QueryableList.AnyAsync(dc => dc.IntlCode == country && dc.CaseType == caseType);
        }

        public async Task<object[]> GetSelectableDesignatedCountries(string country, string caseType, int appId)
        {
            var list = await _cpiDbContext.Query<LookupDescDTO>().FromSqlInterpolated($@"Select DesCountry as Value, DesCaseType as Text, CountryName as Description From
                                (Select Distinct dc.DesCountry, dc.DesCaseType,c.CountryName,
                                row_number() over(partition by dc.DesCountry order by dc.DefaultCaseType desc) as rowno
                                From tblPatDesCaseType AS dc
                                Inner Join tblPatCountry c on c.Country=dc.DesCountry
                                Where((dc.[IntlCode] = {country}) AND(dc.[CaseType] = {caseType}))
                                And Not Exists(Select 1 From tblPatDesignatedCountry AS d Where(d.AppId = {appId}) AND(d.DesCountry = dc.DesCountry))
                                ) t Where rowno = 1").AsNoTracking().ToListAsync();
            return list.Select(item => new { DesCountry = item.Value, DesCaseType = item.Text, CountryName = item.Description }).ToArray();

        }

        public async Task<string[]> GetSelectableDesignatedCaseTypes(string country, string caseType, string desCountry)
        {
            return await _cpiDbContext.GetRepository<PatDesCaseType>().QueryableList.Where(dc => dc.IntlCode == country && dc.CaseType == caseType && dc.DesCountry == desCountry)
                .Select(dc => dc.DesCaseType).Distinct().ToArrayAsync();
        }

        public async Task<List<PatParentCaseDTO>> GetPossibleFamilyReferences(int appId, string caseNumber)
        {
            var list = await _cpiDbContext.Query<PatParentCaseDTO>()
                .FromSqlInterpolated($"procPatFamilyGetPossibleReferences @AppId={appId},@CaseNumber={caseNumber}").AsNoTracking()
                .ToListAsync();
            return list;
        }

        public async Task<List<PatParentCaseDTO>> GetAllPossibleTerminalDisclaimer(int appId)
        {
            var list = await _cpiDbContext.Query<PatParentCaseDTO>()
                .FromSqlInterpolated($"Select AppId as ParentId,ca.CaseNumber + '/' + ca.Country + case when IsNull(ca.SubCase,'') = '' then '/' else '/' + ca.SubCase + '/' end + ca.CaseType AS ParentCase,ca.PatNumber From tblPatCountryApplication ca Inner Join tblPatApplicationStatus st on st.ApplicationStatus=ca.ApplicationStatus where st.ActiveSwitch=1 and ca.PatNumber > '' and ca.AppId <> {appId} Order By ca.CaseNumber, ca.Country, ca.SubCase, ca.CaseType").AsNoTracking()
                .ToListAsync();
            return list;
        }

        public async Task<int> GetActiveTerminalDisclaimerAppId(int appId) {
            var terminalDisclaimer = await _cpiDbContext.Query<LookupIntDTO>().FromSqlInterpolated($@"Select TerminalDisclaimerAppId as Value From 
                      (Select td.AppId,td.TerminalDisclaimerAppId,ca.ExpDate,
                      Row_Number() Over (Partition By td.AppId Order By ca.ExpDate) as RowNum
                      From tblPatTerminalDisclaimer td Inner Join tblPatCountryApplication ca on td.TerminalDisclaimerAppId=ca.AppId) td
                      Where AppId={appId} and RowNum=1").FirstOrDefaultAsync();
            //        From tblPatTerminalDisclaimer td Inner Join tblPatCountryApplication ca on td.TerminalDisclaimerAppId=ca.AppId   Where ca.ExpDate is not null) td
            return terminalDisclaimer != null ? terminalDisclaimer.Value : 0;
        }


        public async Task<List<PatTerminalDisclaimerChildDTO>> GetTerminalDisclaimerChildren(int appId)
        {
            var list = await _cpiDbContext.Query<PatTerminalDisclaimerChildDTO>()
                .FromSqlInterpolated($@"Select ca.AppId,CaseNumber, Country, SubCase, CaseType, ExpDate,td.NewExpDate from tblPatCountryApplication ca
                                        Left Join 
                                    (Select td.AppId,Min(ca2.ExpDate) as NewExpDate From tblPatTerminalDisclaimer td Inner Join tblPatCountryApplication ca2 on td.TerminalDisclaimerAppId=ca2.AppId
                                    Inner Join tblPatApplicationStatus st on st.ApplicationStatus=ca2.ApplicationStatus Where st.ActiveSwitch=1 and ca2.ExpDate is not null Group By td.AppId) as td on td.AppId=ca.AppId
                                    Where Exists(Select 1 From tblPatTerminalDisclaimer td where td.AppId=ca.AppID and td.TerminalDisclaimerAppId={appId})").AsNoTracking().ToListAsync();
            return list;
        }

        public async Task DesignateCountries(int appId, bool fromCountryLaw, string createdBy)
        {
            var affected = await _cpiDbContext.ExecuteSqlInterpolatedAsync($"procPatDesCtry_Gen @AppId={appId}, @DesigFromCountryLaw={fromCountryLaw},@CreatedBy={createdBy}");
        }

        public async Task GenerateApplications(int parentAppId, string desCountries, string updatedBy)
        {
            await _cpiDbContext.ExecuteSqlInterpolatedAsync($"procPatDesCtry_Gen_Apps @AppId={parentAppId}, @DesCountries={desCountries},@UpdatedBy={updatedBy}");
        }

        public async Task<List<PatDesignatedCountry>> GetSelectableCountries(int appId)
        {
            return await _cpiDbContext.GetRepository<PatDesignatedCountry>().QueryableList.Where(d => d.AppId == appId && d.GenApp).Select(d => new PatDesignatedCountry { DesCountry = d.DesCountry, CountryName = d.Country.CountryName, GenAppId = d.GenAppId }).ToListAsync();
        }

        public async Task MarkDesCountriesWithExistingApps(int appId)
        {
            await _cpiDbContext.ExecuteSqlRawAsync($"Update d Set d.GenApp=0 From tblPatDesignatedCountry d Inner Join tblPatCountryApplication ca on d.GenCaseNumber=ca.CaseNumber and d.DesCountry=ca.Country and d.GenSubCase=ca.Subcase Where d.AppId ={appId}");
        }
        #endregion

        #region Related Cases
        public async Task<List<PatRelatedCaseDTO>> GetRelatedCases(int appId)
        {
            var list = await _cpiDbContext.Query<PatRelatedCaseDTO>()
                .FromSqlInterpolated($"procPatRelatedCases @AppId={appId}").AsNoTracking().ToListAsync();
            return list;
        }
        #endregion

        #region IDS
        public async Task IDSUpdateFilDate(int appId, string filDateType, string recordType, string userName, DateTime? filDate, DateTime? specificFilDate, bool consideredByExaminer)
        {
            await _cpiDbContext.ExecuteSqlInterpolatedAsync($"procPatIDSUpdateFilDate @AppId={appId},@FilDateType={filDateType},@RecordType={recordType},@UpdatedBy={userName},@FilDate={filDate},@SpecificFilDate={specificFilDate},@ConsideredByExaminer={consideredByExaminer}");
        }

        public async Task UpdateConsideredByExaminer(int appId, string filDateType, string recordType, DateTime? filDateFrom, DateTime? filDateTo, DateTime? specificFilDate, string userName) {
            await _cpiDbContext.ExecuteSqlInterpolatedAsync($"procPatIDSUpdateConsideredByExaminer @AppId={appId},@FilDateType={filDateType},@RecordType={recordType},@UpdatedBy={userName},@FilDateFrom={filDateFrom},@FilDateTo={filDateTo},@SpecificFilDate={specificFilDate}");
        }
        
        #endregion

            #region Family Tree View

        public async Task<IEnumerable<FamilyTreeDTO>> GetFamilyTree(string paramType, string paramValue, string paramParent)
        {
            var familyTree = await _cpiDbContext.Query<FamilyTreeDTO>().FromSqlInterpolated($"Exec procPatTV @ParamType={paramType}, @ParamValue={paramValue}, @ParamParent={paramParent}")
                                                    .AsNoTracking().ToListAsync();
            return familyTree;
        }

        public FamilyTreePatDTO GetNodeDetails(string paramType, string paramValue)
        {
            var treeDetail = _cpiDbContext.Query<FamilyTreePatDTO>().FromSqlInterpolated($"Exec procPatTVDetail @ParamType={paramType}, @ParamValue={paramValue}")
                                                   .AsNoTracking().AsEnumerable().FirstOrDefault();
            return treeDetail;
        }


        public void UpdateParent(int childAppId, int newParentId, string parentInfo, string userName)
        {
            // note: this should NOT be run async, else it will not update (somehow the update seems to get rolled back if run async)
            using (SqlCommand cmd = new SqlCommand("procPatTVUpdate"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _cpiDbContext.GetDbConnection() as SqlConnection;
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@ChildId", childAppId);
                cmd.Parameters.AddWithValue("@NewParentId", newParentId);
                cmd.Parameters.AddWithValue("@ParentInfo", parentInfo);
                cmd.Parameters.AddWithValue("@UpdatedBy", userName);

                cmd.ExecuteNonQuery();
            }
        }

        public async Task<List<FamilyTreeParentCaseDTO>> GetPossibleFamilyTreeReferences(int appId, string caseNumber)
        {
            var list = await _cpiDbContext.Query<FamilyTreeParentCaseDTO>()
                .FromSqlInterpolated($"procPatFamilyTreeGetPossibleReferences @AppId={appId},@CaseNumber={caseNumber}").AsNoTracking()
                .ToListAsync();
            return list;
        }

        #endregion
        #region Action

        public async Task<List<DelegationEmailDTO>> GetDelegationEmails(int delegationId)
        {
            var list = await _cpiDbContext.Query<DelegationEmailDTO>().FromSqlInterpolated($@"Select Distinct DelegationId,AssignedBy,AssignedTo,FirstName,LastName From
                                (Select ddd.DelegationId,ddd.CreatedBy as AssignedBy,u.Email as AssignedTo,u.FirstName,u.LastName From tblCPiGroups g Inner Join tblCPiUserGroups ug on g.Id=ug.GroupId Inner Join tblCPIUsers u on u.Id=ug.UserId Inner Join tblPatDueDateDelegation ddd on ddd.GroupId=ug.GroupId Union
                                 Select ddd.DelegationId,ddd.CreatedBy as AssignedBy,u.Email as AssignedTo,u.FirstName,u.LastName From  tblCPIUsers u  Inner Join tblPatDueDateDelegation ddd on ddd.UserId=u.Id
                                ) t Where t.DelegationId={delegationId}").AsNoTracking().ToListAsync();
            return list;
        }

        public async Task MarkDelegationasEmailed(int delegationId)
        {
            await _cpiDbContext.ExecuteSqlInterpolatedAsync($"Update tblPatDueDateDelegation Set NotificationSent=1 Where DelegationId={delegationId}");
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
            using (SqlCommand cmd = new SqlCommand("procPatDelegatedTask"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _cpiDbContext.GetDbConnection() as SqlConnection;
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
        public async Task<List<DelegationEmailDTO>> GetDeletedDelegationEmails(int delegationId) {
            var list = await _cpiDbContext.Query<DelegationEmailDTO>().FromSqlInterpolated($"exec procPatDelegatedTask @action = 3, @delegationid = {delegationId}").AsNoTracking().ToListAsync();
            return list;
        }

        public async Task<DelegationDetailDTO> GetDeletedDelegation(int delegationId)
        {
            var delegation = await _cpiDbContext.Query<DelegationDetailDTO>().FromSqlInterpolated($"Select DataKeyValue as DelegationId, dddLog.ActID,dddLog.DDID,dddLog.GroupId,dddLog.UserId,dddLog.NotificationSent,ParentActId,ParentId    From tblDeleteLog d WITH (NOLOCK) cross apply openjson(d.record) With(ActId int '$.ActId',DDId int '$.DDId',GroupId int '$.GroupId',UserId nvarchar(450) '$.UserId',NotificationSent int '$.NotificationSent',ParentActId int '$.ParentActId',ParentId int '$.ParentId') as dddlog Where DataKey='DelegationId' and SystemType='P' And DataKeyValue={delegationId}").AsNoTracking().FirstOrDefaultAsync();
            return delegation;
        }

        public async Task<List<LookupIntDTO>> GetDuedateChangedDelegationIds(int action, List<PatDueDate> updated) {
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
            using (SqlCommand cmd = new SqlCommand("procPatDelegatedTask"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _cpiDbContext.GetDbConnection() as SqlConnection;
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
        #region Unitary Patent
        public async Task<int> ShouldShowUnitaryPatentFields(int action, string country, string caseType, int appId)
        {
            using (SqlCommand cmd = new SqlCommand("procPatShowUnitaryPatentFields"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _cpiDbContext.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@Country", country);
                cmd.Parameters.AddWithValue("@CaseType", caseType);
                cmd.Parameters.AddWithValue("@AppId", appId);
                cmd.Parameters.Add("@Result", SqlDbType.Int).Direction = ParameterDirection.Output;

                await cmd.ExecuteNonQueryAsync();
                var result = (int)cmd.Parameters["@Result"].Value;
                return result;
            }
        }

        public async Task<List<PatDesignationDTO>> GetDesignatedCountries(int appId)
        {
            var designatedCountries = await _cpiDbContext.Query<PatDesignationDTO>().FromSqlInterpolated($"Declare @OutputParam int Exec procPatShowUnitaryPatentFields @Action=4,@AppId={appId},@Result=@OutputParam output").AsNoTracking().ToListAsync();
            return designatedCountries;
        }
        #endregion

        #region Trade Secret
        private async Task<(string? ActivityCode, Dictionary<string, string?[]>? AuditLogs)> SetTradeSecret(CountryApplication countryApplication)
        {
            var activityCode = string.Empty;

            if (!_user.CanAccessPatTradeSecret())
                return (null, null);

            var current = await QueryableList.Include(ca => ca.Invention).Where(ca => ca.AppId == countryApplication.AppId).SingleOrDefaultAsync();
            var currentTradeSecret = current?.TradeSecret ?? new CountryApplicationTradeSecret();
            var invention = current?.Invention ?? (await _cpiDbContext.GetRepository<Invention>().QueryableList.Where(i => i.CaseNumber == countryApplication.CaseNumber).SingleOrDefaultAsync());

            if (invention != null && (invention.IsTradeSecret ?? false))
            {
                var isTSCleared = await _tradeSecretService.IsUserCleared(TradeSecretScreen.Invention, countryApplication.InvId);

                //set defaults that are normally handled by insert trigger (tblPatCountryApplication_I_Trig)
                if (countryApplication.AppId == 0 && string.IsNullOrEmpty(countryApplication.AppTitle))
                    countryApplication.AppTitle = invention.TradeSecret?.InvTitle;

                //only cleared full modify users can edit trade secret fields
                if (!(_user.CanEditPatTradeSecretFields() && isTSCleared))
                    countryApplication.RestoreTradeSecret(currentTradeSecret, true);

                countryApplication.TradeSecret = countryApplication.CreateTradeSecret(new CountryApplicationTradeSecret());

                if (countryApplication.AppId == 0) { 
                    activityCode = TradeSecretActivityCode.Create;
                }
                else if (countryApplication.TradeSecret.AppTitle != current?.TradeSecret?.AppTitle){
                    activityCode = TradeSecretActivityCode.Update;
                }
            }

            //create activity log
            if (!string.IsNullOrEmpty(activityCode))
                return (activityCode, CreateAuditLogs(current, countryApplication));

            return (null, null);
        }

        /// <summary>
        /// Returns Allowed = true and InvId > 0 if invention is a trade secret and user has delete clearance
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        private async Task<(bool Allowed, int InvId)> CanDeleteTradeSecret(int appId)
        {
            var tradeSecret = (await QueryableList.Where(ca => ca.AppId == appId).Select(ca => new { ca.InvId, ca.Invention.IsTradeSecret }).SingleOrDefaultAsync());
            var isTradeSecret = false;

            if (tradeSecret != null)
            {
                isTradeSecret = tradeSecret.IsTradeSecret ?? false;

                if (isTradeSecret && _user.CanAccessPatTradeSecret())
                {
                    //only cleared admins can delete trade secret
                    var isTSCleared = await _tradeSecretService.IsUserCleared(TradeSecretScreen.Invention, tradeSecret.InvId);
                    return (_user.IsPatTradeSecretAdmin() && isTSCleared, tradeSecret.InvId);
                }
            }

            return (!isTradeSecret, 0);
        }

        private async Task<TradeSecretActivity> CreateTradeSecretActivityLog(int invId, int appId, string activityCode, Dictionary<string, string?[]>? auditLogs)
        {
            var tsRequest = await _tradeSecretService.GetUserRequest(_tradeSecretService.CreateLocator(TradeSecretScreen.Invention, invId));
            var tsActivity = _tradeSecretService.CreateActivity(TradeSecretScreen.CountryApplication, TradeSecretScreen.CountryApplication, appId, activityCode, tsRequest?.RequestId ?? 0, auditLogs);

            return tsActivity;
        }

        private Dictionary<string, string?[]> CreateAuditLogs(CountryApplication? oldValues, CountryApplication? newValues)
        {
            var auditLogs = new Dictionary<string, string?[]>();

            if (newValues?.TradeSecret?.AppTitle != oldValues?.TradeSecret?.AppTitle)
                auditLogs.Add("AppTitle", [oldValues?.TradeSecret?.AppTitle, newValues?.TradeSecret?.AppTitle]);

            return auditLogs;
        }
        #endregion
    }
}
