using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;

namespace R10.Core.Services.Shared
{
    public class LetterService : ILetterService
    {
        private readonly IApplicationDbContext _repository;
        private readonly ISystemSettings<PatSetting> _patSettings;
        private readonly string letterFeatureCode = "Let";
        private long lastLetterSessionId = 0;
        private int recordCount = 0;


        public LetterService(IApplicationDbContext repository, 
            ISystemSettings<PatSetting> patSettings
            )
        {
            _repository = repository;
            _patSettings = patSettings;
        }

        #region Letter Main CRUD
        public IQueryable<LetterMain> LettersMain => _repository.LettersMain.AsNoTracking();

        public IQueryable<LetterMain> FilteredLettersMain 
        {
            get
            {
                var letters = _repository.LettersMain.AsNoTracking();

                if (!_patSettings.GetSetting().Result.IsInventorRemunerationOn)
                {
                    letters = letters.Where(c => !c.TemplateFile.StartsWith("Pat-Remuneration"));
                }

                if (!_patSettings.GetSetting().Result.IsInventorFRRemunerationOn)
                {
                    letters = letters.Where(c => !c.TemplateFile.StartsWith("Pat-FRRemuneration"));
                }

                return letters;
            }
        } 

        public async Task<LetterMain> GetLetterMainById(int letId)
        {
            return await LettersMain.SingleOrDefaultAsync(l => l.LetId == letId);
        }

        public async Task Add(LetterMain letter)
        {
            _repository.LettersMain.Add(letter);
            await _repository.SaveChangesAsync();
        }

        public async Task Update(LetterMain letter)
        {
            _repository.LettersMain.Update(letter);
            await _repository.SaveChangesAsync();
        }

        public async Task Delete(LetterMain letter)
        {
            _repository.LettersMain.Remove(letter);
            await _repository.SaveChangesAsync();
        }

        public IQueryable<LetterTag> LetterTags => _repository.LetterTags.AsNoTracking();
        #endregion

        #region Children Update
        private async Task UpdateChild<T1, T2>(string userName, T1 mainRecord, IEnumerable<T2> updated, IEnumerable<T2> added, IEnumerable<T2> deleted) where T1 : BaseEntity where T2 : BaseEntity
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                mainRecord.UpdatedBy = userName;
                mainRecord.LastUpdate = DateTime.Now;

                // update parent stamp fields
                var parent = _repository.Set<T1>().Attach(mainRecord);
                parent.Property(c => c.UpdatedBy).IsModified = true;
                parent.Property(c => c.LastUpdate).IsModified = true;

                foreach (var item in updated)
                {
                    item.UpdatedBy = userName;
                    item.LastUpdate = mainRecord.LastUpdate;
                }

                foreach (var item in added)
                {
                    item.CreatedBy = mainRecord.UpdatedBy;
                    item.DateCreated = mainRecord.LastUpdate;
                    item.UpdatedBy = mainRecord.UpdatedBy;
                    item.LastUpdate = mainRecord.LastUpdate;
                }

                var dbSet = _repository.Set<T2>();

                if (updated.Any())
                    dbSet.UpdateRange(updated);

                if (added.Any())
                    dbSet.AddRange(added);

                if (deleted.Any())
                    dbSet.RemoveRange(deleted);
                await _repository.SaveChangesAsync();

                scope.Complete();
            }
        }
        #endregion

        #region Field List
        public async Task<List<LetterFieldListDTO>> GetFieldList(int letId, string sortColumn, string sortDirection)
        {
            var result = await _repository.LetterFieldListDTO.FromSqlInterpolated($"procLet_FieldList @LetId={letId},  @SortColumn={sortColumn},  @SortDirection={sortDirection}")
                            .AsNoTracking().ToListAsync();
            return result;
        }        
        #endregion

        #region User Data
        public IQueryable<LetterUserData> LetterUserData => _repository.LetterUserData.AsNoTracking();
        public async Task<bool> UserDataUpdate(int letId, string userName, IEnumerable<LetterUserData> updatedUserData, IEnumerable<LetterUserData> newUserData, IEnumerable<LetterUserData> deletedUserData)
        {
            if (newUserData.Any())
            {
                foreach (var item in newUserData)
                {
                    item.LetId = letId;
                }
            }
            var letterMain = await GetLetterMainById(letId);

            //await UpdateChild<BaseEntity>(userName, letterMain, updatedUserData, newUserData, deletedUserData);
            await UpdateChild(userName, letterMain, updatedUserData, newUserData, deletedUserData);
            return true;
        }
        #endregion

        #region Record Source
        public IQueryable<LetterRecordSource> LetterRecordSources => _repository.LetterRecordSources.AsNoTracking();
        public IQueryable<LetterDataSource> LetterDataSources => _repository.LetterDataSources.AsNoTracking();

        public IQueryable<LetterDataSource> FilteredLetterDataSources
        {
            get
            {
                var dataSources = _repository.LetterDataSources.AsNoTracking();

                if (!_patSettings.GetSetting().Result.IsInventorRemunerationOn)
                {
                    dataSources = dataSources.Where(c => !c.DataSourceDescMain.StartsWith("DE Remuneration"));
                }

                if (!_patSettings.GetSetting().Result.IsInventorFRRemunerationOn)
                {
                    dataSources = dataSources.Where(c => !c.DataSourceDescMain.StartsWith("FR Remuneration"));
                }

                if (!_patSettings.GetSetting().Result.IsInventionCostTrackingOn)
                {
                    dataSources = dataSources.Where(c => !c.DataSourceDescMain.StartsWith("Patent Invention Cost Tracking"));
                }

                if (!_patSettings.GetSetting().Result.IsInventionActionOn)
                {
                    dataSources = dataSources.Where(c => !(c.DataSourceDescMain.Contains("Invention Action") || c.DataSourceDescMain.Contains("Invention Due") || c.DataSourceDescMain.Contains("Invention DeDocket Instruction")));
                }

                return dataSources;
            }
        }

        public async Task<LetterRecordSource> GetRecordSourceById(int recSourceId)
        {
            return await LetterRecordSources.SingleOrDefaultAsync(rs => rs.RecSourceId == recSourceId);
        }

        public async Task<LetterRecordSource> GetRecordSourceById(int dataSourceId, int letId)
        {
            return await LetterRecordSources.SingleOrDefaultAsync(rs => rs.DataSourceId == dataSourceId && rs.LetId == letId);
        }        

        public async Task<List<FamilyTreeDTO>> GetFamilyTree(int letId, int? parentId)
        {
            var familyTree = await _repository.FamilyTreeDTO.FromSqlInterpolated($"Exec procLet_TV @LetId={letId}, @ParentId={parentId}")
                                                   .AsNoTracking().ToListAsync();
            return familyTree;
        }
        public async Task<bool> ValidParentRecord(int parentRecSourceId, int letId)
        {
            var letterRecordSources = await LetterRecordSources.SingleOrDefaultAsync(rs => rs.RecSourceId == parentRecSourceId && rs.LetId == letId);
            return !(letterRecordSources is null);
        }

        public async Task<bool> RecordSourceUpdate (int letId, string userName, IEnumerable<LetterRecordSource> updatedRecordSource, IEnumerable<LetterRecordSource> newRecordSource, IEnumerable<LetterRecordSource> deletedRecordSource)
        {

            var letterMain = await GetLetterMainById(letId);
            //await UpdateChild<BaseEntity>(userName, letterMain, updatedRecordSource, newRecordSource, deletedRecordSource);
            await UpdateChild(userName, letterMain, updatedRecordSource, newRecordSource, deletedRecordSource);

            return true;
        }
        #endregion

        #region Letter Filter
        public IQueryable<LetterRecordSourceFilter> LetterRecordSourceFilters => _repository.LetterRecordSourceFilters.AsNoTracking();
        public IQueryable<LetterRecordSourceFilterUser> LetterRecordSourceFiltersUser => _repository.LetterRecordSourceFiltersUser.AsNoTracking();

        public async Task<bool> LetterRecordSourceFilterUpdate(string userName,
            IEnumerable<LetterRecordSourceFilter> updatedFilterData, IEnumerable<LetterRecordSourceFilter> newFilterData, IEnumerable<LetterRecordSourceFilter> deletedFilterData)
        {
            int recSourceId = 0;
            if (newFilterData.Any())
                //recSourceId = newFilterData.First().LetterRecordSource.RecSourceId; //correct automapper
                recSourceId = newFilterData.First().RecSourceId;
            else if (updatedFilterData.Any())
                recSourceId = updatedFilterData.First().RecSourceId;
            else if (deletedFilterData.Any())
            {
                recSourceId = deletedFilterData.First().RecSourceId;
                if (recSourceId == 0) //fix recSourceId might not be passing
                {
                    var docxFilterId = deletedFilterData.First().LetFilterId;
                    recSourceId = await LetterRecordSourceFilters.Where(f => f.LetFilterId == docxFilterId).Select(f => f.RecSourceId).FirstOrDefaultAsync();
                }
            }

            var recordSource = await GetRecordSourceById(recSourceId);
            //await UpdateChild<BaseEntity>(userName, recordSource, updatedFilterData, newFilterData, deletedFilterData);
            await UpdateChild(userName, recordSource, updatedFilterData, newFilterData, deletedFilterData);
            return true;
        }

        public async Task<bool> LetterRecordSourceFilterUserUpdate(int letId, string userName, string userEmail,
            IEnumerable<LetterRecordSourceFilterUser> updatedFilterData, IEnumerable<LetterRecordSourceFilterUser> newFilterData, IEnumerable<LetterRecordSourceFilterUser> deletedFilterData)
        {
            int recSourceId = 0;
            if (newFilterData.Any())
            {
                //recSourceId = newFilterData.First().LetterRecordSource.RecSourceId; //correct automapper
                recSourceId = newFilterData.First().RecSourceId;
                foreach (var item in newFilterData)
                {
                    item.UserEmail = userEmail;
                    item.LetId = letId;
                    item.FilterSource = "U";
                }
            }
            if (updatedFilterData.Any())
            {
                recSourceId = updatedFilterData.First().RecSourceId;
                foreach (var item in updatedFilterData)
                {
                    item.UserEmail = userEmail;
                }
            }
            if (deletedFilterData.Any())
            {
                recSourceId = deletedFilterData.First().RecSourceId;
                if (recSourceId == 0) //fix recSourceId might not be passing
                {
                    var userFilterId = deletedFilterData.First().UserFilterId;
                    recSourceId = await LetterRecordSourceFiltersUser.Where(f => f.UserFilterId == userFilterId).Select(f => f.RecSourceId).FirstOrDefaultAsync();
                }
            }

            var recordSource = await GetRecordSourceById(recSourceId);
            //await UpdateChild<BaseEntity>(userName, recordSource, updatedFilterData, newFilterData, deletedFilterData);
            await UpdateChild(userName, recordSource, updatedFilterData, newFilterData, deletedFilterData);
            return true;
        }        

        public async Task<List<LookupDTO>> GetFilterFieldsList(int recSourceId)
        {
            var result = await _repository.LetterFilterLookUpDTO.FromSqlInterpolated($"procLet_RecFilter @Action=10, @RecSourceId={recSourceId}").AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<LetterFilterDataDTO> GetFilterDataList(int recSourceId, string fieldName, int pageNo, int pageSize, string filterData)
        {
            //SqlParameter countParam = new SqlParameter
            //{
            //    ParameterName = "@RecordCount",
            //    SqlDbType = System.Data.SqlDbType.Int,
            //    Direction = System.Data.ParameterDirection.Output
            //};

            //var result = await _repository.LetterFilterLookUpDTO.FromSqlRaw($"procLet_RecFilterData @RecSourceId, @FieldName, @PageNo, @PageSize, @FilterData, @RecordCount Output",
            //                    new SqlParameter("@RecSourceId", recSourceId), new SqlParameter("@FieldName", fieldName),
            //                    new SqlParameter("@PageNo", pageNo), new SqlParameter("@PageSize", pageSize),
            //                    new SqlParameter("@FilterData", filterData),
            //                    countParam)
            //                    .AsNoTracking().ToListAsync();
            //int countReturned = (int)countParam.Value;
            //LetterFilterDataDTO retVal = new LetterFilterDataDTO();
            //retVal.Data = result;
            //retVal.RecordCount = countReturned;

            //return retVal;
            var connectionString = _repository.Database.GetDbConnection().ConnectionString;

            using (SqlDataAdapter da = new SqlDataAdapter("procLet_RecFilterData", new SqlConnection(connectionString)))
            {
                DataTable dt = new DataTable();
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@RecSourceId", recSourceId);
                da.SelectCommand.Parameters.AddWithValue("@FieldName", fieldName);
                da.SelectCommand.Parameters.AddWithValue("@PageNo", pageNo);
                da.SelectCommand.Parameters.AddWithValue("@PageSize", pageSize);
                da.SelectCommand.Parameters.AddWithValue("@FilterData", filterData);

                SqlParameter recCountParam = new SqlParameter("@RecordCount", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                da.SelectCommand.Parameters.Add(recCountParam);

                da.Fill(dt);
                var result = dt.AsEnumerable().Select(dr => new LookupDTO()
                {
                    Text = dr.Field<string>("Text"),
                    Value = dr.Field<string>("Value")
                }).ToList();
                recordCount = (int)recCountParam.Value;
                LetterFilterDataDTO retVal = new LetterFilterDataDTO();
                retVal.Data = result;
                retVal.RecordCount = recordCount;
                return retVal;
            }
        }

        public async Task<LookupDTO> FilterDataValueMapper(int recSourceId, string fieldName, string value)
        {
            var result =  _repository.LetterFilterLookUpDTO.FromSqlInterpolated($"procLet_RecFilter @Action=20, @RecSourceId={recSourceId}, @FieldName={fieldName}, @Value={value}")
                            .AsNoTracking().AsEnumerable().FirstOrDefault();
            return result;
        }


        #endregion

        #region Main Screen Letter Popup
        public async Task<SystemScreen> GetSystemScreen(int id)
        {
            var systemScreen = await _repository.SystemScreens.Where(e => e.ScreenId == id).AsNoTracking().FirstOrDefaultAsync();
            return systemScreen;
        }

        public async Task<SystemScreen> GetSystemScreen(string systemType, string screenCode)
        {
            var systemScreen = await _repository.SystemScreens.Where(e => e.SystemType == systemType && e.ScreenCode == screenCode && e.FeatureType == letterFeatureCode)
                                    .AsNoTracking().FirstOrDefaultAsync();
            return systemScreen;
        }

        public async Task<List<LetterContactDTO>> GetLetterContacts(int letId, string userEmail)
        {
            var result = await _repository.LetterContactDTO.FromSqlInterpolated($"procLet_PopupContacts @LetId={letId}, @UserEmail={userEmail}").AsNoTracking().ToListAsync();
            return result;
        }

        public bool UpdatePopupFilter(int letId, string fieldName, string operand, string userEmail, string userName)
        {
            var connectionString = _repository.Database.GetDbConnection().ConnectionString;
            using (SqlCommand cmd = new SqlCommand("procLet_PopupFilterUpdate"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = new SqlConnection(connectionString);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@LetId", letId);
                cmd.Parameters.AddWithValue("@UserEmail", userEmail);
                cmd.Parameters.AddWithValue("@FieldName", fieldName);
                cmd.Parameters.AddWithValue("@Operator", "=");
                cmd.Parameters.AddWithValue("@Operand1", operand);
                cmd.Parameters.AddWithValue("@CreatedBy", userName);
                cmd.Parameters.AddWithValue("@DateCreated", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
            return true;
        }

        public DataSet GenerateLetterData(int letId, bool includeGenerated, bool isLog, string returnType, IEnumerable<LetterEntityContactDTO> selectedContacts,
                                                string userEmail, bool hasRespOffice, bool hasEntityFilter, string? previewSelection)
        {
            var ds = GetLetterDataSet(letId, includeGenerated, isLog, returnType, selectedContacts, userEmail, hasRespOffice, hasEntityFilter, previewSelection);

            // process ds: tables: 0=TableName; 1=rows of TableLink; 3=Session Id; 4->n=ctual tables
            var tableCount = ds.Tables.Count;
            if (tableCount < 4)
            {
                throw new Exception("GenerateLetterData Error: Insufficient table data!");
            }

            ds.Tables[0].TableName = "xxyyzzTableName";
            ds.Tables[1].TableName = "xxyyzzTableLink";
            ds.Tables[2].TableName = "xxyyzzSession";

            // get session id for logging
            lastLetterSessionId = (long)ds.Tables[2].Rows[0]["SessionId"];

            // establish table names of actual data table (index starts at 3)
            string tableNames = ds.Tables["xxyyzzTableName"].Rows[0]["TableName"].ToString();
            string[] aTables = tableNames.Split("|");
            for (var i = 3; i < tableCount; i++)
            {
                ds.Tables[i].TableName = aTables[i - 3];
            }

            // establish table relationship
            ds.EnforceConstraints = false;
            foreach (DataRow row in ds.Tables["xxyyzzTableLink"].Rows)
            {
                string[] xLink = row["TableLink"].ToString().Split("|");                 // parent|child|linkfield
                var parentTable = xLink[0];
                var childTable = xLink[1];
                var linkField = xLink[2];
                string relName = parentTable + childTable;
                ds.Relations.Add(relName, ds.Tables[parentTable].Columns[linkField], ds.Tables[childTable].Columns[linkField]);
            }

            // remove header tables
            ds.Tables.Remove("xxyyzzTableName");
            ds.Tables.Remove("xxyyzzTableLink");
            ds.Tables.Remove("xxyyzzSession");

            return ds;
        }

        public long GetSessionId() {
            return lastLetterSessionId;
        }

        private DataSet GetLetterDataSet(int letId, bool includeGenerated, bool isLog, string returnType, IEnumerable<LetterEntityContactDTO> selectedContacts,
                                                    string userEmail, bool hasRespOffice, bool hasEntityFilter, string? previewSelection)
        {
            var contactParam = BuildContactParameters(selectedContacts);
            var connectionString = _repository.Database.GetDbConnection().ConnectionString;
            using (SqlCommand cmd = new SqlCommand("procLet_Gen"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = new SqlConnection(connectionString);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters.RemoveAt("@EntityContacts");
                if (selectedContacts.Count() > 0)
                    cmd.Parameters.AddWithValue("@EntityContacts", contactParam).SqlDbType = SqlDbType.Structured;
                else
                    cmd.Parameters.AddWithValue("@EntityContacts", null);

                cmd.Parameters["@LetId"].Value = letId;
                cmd.Parameters["@IncludeGenerated"].Value = includeGenerated;
                cmd.Parameters["@IsLog"].Value = isLog;
                cmd.Parameters["@ReturnType"].Value = returnType;
                cmd.Parameters["@UserEmail"].Value = userEmail;
                cmd.Parameters["@HasRespOfficeOn"].Value = hasRespOffice;
                cmd.Parameters["@HasEntityFilterOn"].Value = hasEntityFilter;
                cmd.Parameters["@PreviewSelection"].Value = previewSelection;

                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;

                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds;
            }
        }

        private List<SqlDataRecord> BuildContactParameters(IEnumerable<LetterEntityContactDTO> entityContacts)
        {
            var contactList = new List<SqlDataRecord>();
            foreach (LetterEntityContactDTO contact in entityContacts)
            {
                var record = new SqlDataRecord(
                    new SqlMetaData("EntityId", SqlDbType.Int),
                    new SqlMetaData("ContactId", SqlDbType.Int));
                
                record.SetInt32(0, contact.EntityId);
                record.SetInt32(1, contact.ContactId);
                contactList.Add(record);

            }
            return contactList;
        }

        public int LogLetter(long sessionId, string systemType, int letId, string letFile, string userName)
        {
            var connectionString = _repository.Database.GetDbConnection().ConnectionString;
            using (SqlCommand cmd = new SqlCommand("procLet_Log"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = new SqlConnection(connectionString);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@SessionId", sessionId);
                cmd.Parameters.AddWithValue("@SystemType", systemType);
                cmd.Parameters.AddWithValue("@LetId", letId);
                cmd.Parameters.AddWithValue("@LetFile", letFile);
                cmd.Parameters.AddWithValue("@GenBy", userName);
                cmd.Parameters.Add("@LetLogId", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                var letLogId = (int)cmd.Parameters["@LetLogId"].Value;
                return letLogId;
            }
            
        }
        public async Task LogItemId(int letLogId, string itemId) {
            await _repository.LetterLogs.Where(l => l.LetLogId == letLogId).ExecuteUpdateAsync(setters=> setters.SetProperty(l=> l.ItemId,itemId));
        }

        public async Task<List<LookupIntDTO>> GetDataKeyValuesToLog(long sessionId)
        {
            var result = await _repository.LookupIntDTO.FromSqlInterpolated($"Select Distinct DataKeyValue as Value From tmpLet_GenLog Where sessionid={sessionId}").AsNoTracking().ToListAsync();
            return result;
        }

        #endregion

        #region Setup Preview Screen
        public DataTable PreviewLetterData(int letId, bool includeGenerated, string userEmail, bool hasRespOffice, bool hasEntityFilter,
                                                string sortExpr, int page, int pageSize = 0)
        {
            var connectionString = _repository.Database.GetDbConnection().ConnectionString;

            using (SqlDataAdapter da = new SqlDataAdapter("procLet_Preview", new SqlConnection(connectionString)))
            {
                DataTable dt = new DataTable();
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@LetId", letId);
                da.SelectCommand.Parameters.AddWithValue("@IncludeGenerated", includeGenerated);
                da.SelectCommand.Parameters.AddWithValue("@UserEmail", userEmail);
                da.SelectCommand.Parameters.AddWithValue("@HasRespOfficeOn", hasRespOffice);
                da.SelectCommand.Parameters.AddWithValue("@HasEntityFilterOn", hasEntityFilter);
                da.SelectCommand.Parameters.AddWithValue("@SortExpr", sortExpr);
                da.SelectCommand.Parameters.AddWithValue("@Page", page);
                da.SelectCommand.Parameters.AddWithValue("@PageSize", pageSize);

                SqlParameter recCountParam = new SqlParameter("@RowCount", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                da.SelectCommand.Parameters.Add(recCountParam);

                da.Fill(dt);
                recordCount = (int)recCountParam.Value;
                return dt;
            }
        }
        public int PreviewLetterCount()
        {
            return recordCount;
        }

        #endregion

        #region Letter Logs
        public IQueryable<LetterLog> LetterLogs => _repository.LetterLogs.AsNoTracking();
        public IQueryable<LetterLogDetail> LetterLogDetails => _repository.LetterLogDetails.AsNoTracking();
        #endregion

        #region Custom Fields
        public IQueryable<LetterCustomField> LetterCustomFields => _repository.LetterCustomFields.AsNoTracking();

        public async Task<LetterDataSource> GetDataSourceById(int dataSourceId)
        {
            return await LetterDataSources.SingleOrDefaultAsync(ds => ds.DataSourceId == dataSourceId);
        }

        public async Task<bool> LetterCustomFieldUpdate(int dataSourceId, string userName, string userEmail,
            IEnumerable<LetterCustomField> updatedData, IEnumerable<LetterCustomField> newData, IEnumerable<LetterCustomField> deletedData)
        {
            if (newData.Any())
            {
                //recSourceId = newFilterData.First().LetterRecordSource.RecSourceId; //correct automapper
                foreach (var item in newData)
                {
                    item.DataSourceId = dataSourceId;
                }
            }
            if (deletedData.Any())
            {
                dataSourceId = deletedData.First().DataSourceId;
            }

            var dataSource = await GetDataSourceById(dataSourceId);
            //await UpdateChild<BaseEntity>(userName, recordSource, updatedFilterData, newFilterData, deletedFilterData);
            await UpdateChild(userName, dataSource, updatedData, newData, deletedData);
            return true;
        }

        public async Task<List<LetterFieldListDTO>> GetDataSourceFieldList(int dataSourceId, string sortColumn, string sortDirection)
        {
            var result = await _repository.LetterFieldListDTO.FromSqlInterpolated($"procLet_DataSorceFieldList @DataSourceId={dataSourceId},  @SortColumn={sortColumn},  @SortDirection={sortDirection}, @DataType=DEFAULT")
                            .AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<LetterFieldListDTO>> GetDataSourceFieldList(int dataSourceId)
        {
            var result = await _repository.LetterFieldListDTO.FromSqlInterpolated($"procLet_DataSorceFieldList @DataSourceId={dataSourceId},  @SortColumn=DEFAULT,  @SortDirection=DEFAULT, @DataType=DEFAULT")
                            .AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<LetterFieldListDTO>> GetDataSourceDateFieldList(int dataSourceId)
        {
            var result = await _repository.LetterFieldListDTO.FromSqlInterpolated($"procLet_DataSorceFieldList @DataSourceId={dataSourceId},  @SortColumn=DEFAULT,  @SortDirection=DEFAULT, @DataType='datetime2'")
                            .AsNoTracking().ToListAsync();
            return result;
        }
        #endregion

    }
}
