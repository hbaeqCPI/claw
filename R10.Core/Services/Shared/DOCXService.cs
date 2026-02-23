using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Interfaces;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;

namespace R10.Core.Services.Shared
{
    public class DOCXService : IDOCXService
    {
        private readonly IApplicationDbContext _repository;
        private readonly string docxFeatureCode = "DOCX";
        private long lastDOCXSessionId = 0;
        private int recordCount = 0;

        public DOCXService(IApplicationDbContext repository)
        {
            _repository = repository;
        }

        #region DOCX Main CRUD
        public IQueryable<DOCXMain> DOCXesMain => _repository.DOCXesMain.AsNoTracking();

        public async Task<DOCXMain> GetDOCXMainById(int docxId)
        {
            return await DOCXesMain.SingleOrDefaultAsync(l => l.DOCXId == docxId);
        }

        public async Task Add(DOCXMain docx)
        {
            _repository.DOCXesMain.Add(docx);
            await _repository.SaveChangesAsync();
        }

        public async Task Update(DOCXMain docx)
        {
            _repository.DOCXesMain.Update(docx);
            await _repository.SaveChangesAsync();
        }

        public async Task Delete(DOCXMain docx)
        {
            _repository.DOCXesMain.Remove(docx);
            await _repository.SaveChangesAsync();
        }
        #endregion

        #region Children Update
        private async Task UpdateChild<T1,T2>(string userName, T1 mainRecord, IEnumerable<T2> updated, IEnumerable<T2> added, IEnumerable<T2> deleted) where T1 : BaseEntity where T2: BaseEntity
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
        public async Task<List<DOCXFieldListDTO>> GetFieldList(int docxId, string sortColumn, string sortDirection)
        {
            var result = await _repository.DOCXFieldListDTO.FromSqlInterpolated($"procDOCX_FieldList @DOCXId={docxId},  @SortColumn={sortColumn},  @SortDirection={sortDirection}")
                            .AsNoTracking().ToListAsync();
            return result;
        }
        #endregion

        #region User Data
        public IQueryable<DOCXUserData> DOCXUserData => _repository.DOCXUserData.AsNoTracking();
        public async Task<bool> UserDataUpdate(int docxId, string userName, IEnumerable<DOCXUserData> updatedUserData, IEnumerable<DOCXUserData> newUserData, IEnumerable<DOCXUserData> deletedUserData)
        {
            if (newUserData.Any())
            {
                foreach (var item in newUserData)
                {
                    item.DOCXId = docxId;
                }
            }
            var docxMain = await GetDOCXMainById(docxId);

            //await UpdateChild<BaseEntity>(userName, docxMain, updatedUserData, newUserData, deletedUserData);
            await UpdateChild(userName, docxMain, updatedUserData, newUserData, deletedUserData);
            return true;
        }
        #endregion

        #region Record Source
        public IQueryable<DOCXRecordSource> DOCXRecordSources => _repository.DOCXRecordSources.AsNoTracking();
        public IQueryable<DOCXDataSource> DOCXDataSources => _repository.DOCXDataSources.AsNoTracking();

        public async Task<DOCXRecordSource> GetRecordSourceById(int recSourceId)
        {
            return await DOCXRecordSources.SingleOrDefaultAsync(rs => rs.RecSourceId == recSourceId);
        }

        public async Task<DOCXRecordSource> GetRecordSourceById(int dataSourceId, int docxId)
        {
            return await DOCXRecordSources.SingleOrDefaultAsync(rs => rs.DataSourceId == dataSourceId && rs.DOCXId == docxId);
        }

        public async Task<bool> ValidParentRecord(int parentRecSourceId, int docxId)
        {
            var docxRecordSource = await DOCXRecordSources.SingleOrDefaultAsync(rs => rs.RecSourceId == parentRecSourceId && rs.DOCXId == docxId);
            return !(docxRecordSource is null);
        }

        public async Task<List<FamilyTreeDTO>> GetFamilyTree(int docxId, int? parentId)
        {
            var familyTree = await _repository.FamilyTreeDTO.FromSqlInterpolated($"Exec procDOCX_TV @DOCXId={docxId}, @ParentId={parentId}")
                                                   .AsNoTracking().ToListAsync();
            return familyTree;
        }

        public async Task<bool> RecordSourceUpdate (int docxId, string userName, IEnumerable<DOCXRecordSource> updatedRecordSource, IEnumerable<DOCXRecordSource> newRecordSource, IEnumerable<DOCXRecordSource> deletedRecordSource)
        {
          
            var docxMain = await GetDOCXMainById(docxId);
            //await UpdateChild<BaseEntity>(userName, docxMain, updatedRecordSource, newRecordSource, deletedRecordSource);
            await UpdateChild(userName, docxMain, updatedRecordSource, newRecordSource, deletedRecordSource);

            return true;
        }
        #endregion

        #region DOCX Filter
        public IQueryable<DOCXRecordSourceFilter> DOCXRecordSourceFilters => _repository.DOCXRecordSourceFilters.AsNoTracking();
        public IQueryable<DOCXRecordSourceFilterUser> DOCXRecordSourceFiltersUser => _repository.DOCXRecordSourceFiltersUser.AsNoTracking();
        
        public async Task<bool> DOCXRecordSourceFilterUpdate(string userName, 
            IEnumerable<DOCXRecordSourceFilter> updatedFilterData, IEnumerable<DOCXRecordSourceFilter> newFilterData, IEnumerable<DOCXRecordSourceFilter> deletedFilterData)
        {
            int recSourceId = 0;
            if (newFilterData.Any())
                recSourceId = newFilterData.First().RecSourceId;
            else if (updatedFilterData.Any())
                recSourceId = updatedFilterData.First().RecSourceId;
            else if (deletedFilterData.Any())
            {
                recSourceId = deletedFilterData.First().RecSourceId;
                if (recSourceId == 0) //fix recSourceId might not be passing
                {
                    var docxFilterId = deletedFilterData.First().DOCXFilterId;
                    recSourceId = await DOCXRecordSourceFilters.Where(f => f.DOCXFilterId == docxFilterId).Select(f => f.RecSourceId).FirstOrDefaultAsync();
                }
            }

            var recordSource = await GetRecordSourceById(recSourceId);
            //await UpdateChild<BaseEntity>(userName, recordSource, updatedFilterData, newFilterData, deletedFilterData);
            await UpdateChild(userName, recordSource, updatedFilterData, newFilterData, deletedFilterData);
            return true;
        }

        public async Task<bool> DOCXRecordSourceFilterUserUpdate(int docxId, string userName, string userEmail,
            IEnumerable<DOCXRecordSourceFilterUser> updatedFilterData, IEnumerable<DOCXRecordSourceFilterUser> newFilterData, IEnumerable<DOCXRecordSourceFilterUser> deletedFilterData)
        {
            int recSourceId = 0;
            if (newFilterData.Any())
            {
                recSourceId = newFilterData.First().RecSourceId;
                foreach (var item in newFilterData)
                { 
                    item.UserEmail = userEmail;
                    item.DOCXId = docxId;
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
                    recSourceId = await DOCXRecordSourceFiltersUser.Where(f => f.UserFilterId == userFilterId).Select(f => f.RecSourceId).FirstOrDefaultAsync();
                }              
            }               

            var recordSource = await GetRecordSourceById(recSourceId);
            //await UpdateChild<BaseEntity>(userName, recordSource, updatedFilterData, newFilterData, deletedFilterData);
            await UpdateChild(userName, recordSource, updatedFilterData, newFilterData, deletedFilterData);
            return true;
        }

        public async Task<List<LookupDTO>> GetFilterFieldsList(int recSourceId)
        {
            var result = await _repository.DOCXFilterLookUpDTO.FromSqlInterpolated($"procDOCX_RecFilter @Action=10, @RecSourceId={recSourceId}").AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<DOCXFilterDataDTO> GetFilterDataList(int recSourceId, string fieldName, int pageNo, int pageSize, string filterData)
        {
            //SqlParameter countParam = new SqlParameter
            //{
            //    ParameterName = "@RecordCount",
            //    SqlDbType = System.Data.SqlDbType.Int,
            //    Direction = System.Data.ParameterDirection.Output
            //};

            //var result = await _repository.DOCXFilterLookUpDTO.FromSqlRaw($"procDOCX_RecFilterData @RecSourceId, @FieldName, @PageNo, @PageSize, @FilterData, @RecordCount Output",
            //                    new SqlParameter("@RecSourceId", recSourceId), new SqlParameter("@FieldName", fieldName),
            //                    new SqlParameter("@PageNo", pageNo), new SqlParameter("@PageSize", pageSize),
            //                    new SqlParameter("@FilterData", filterData),
            //                    countParam)
            //                    .AsNoTracking().ToListAsync();
            //int countReturned = (int)countParam.Value;
            //DOCXFilterDataDTO retVal = new DOCXFilterDataDTO();
            //retVal.Data = result;
            //retVal.RecordCount = countReturned;

            //return retVal;

            var connectionString = _repository.Database.GetDbConnection().ConnectionString;

            using (SqlDataAdapter da = new SqlDataAdapter("procDOCX_RecFilterData", new SqlConnection(connectionString)))
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
                DOCXFilterDataDTO retVal = new DOCXFilterDataDTO();
                retVal.Data = result;
                retVal.RecordCount = recordCount;
                return retVal;
            }
        }

        public async Task<LookupDTO> FilterDataValueMapper(int recSourceId, string fieldName, string value)
        {
            var result = await _repository.DOCXFilterLookUpDTO.FromSqlInterpolated($"procDOCX_RecFilter @Action=20, @RecSourceId={recSourceId}, @FieldName={fieldName}, @Value={value}")
                            .AsNoTracking().FirstOrDefaultAsync();
            return result;
        }


        #endregion

        #region Main Screen DOCX Popup
        public async Task<SystemScreen> GetSystemScreen(int id)
        {
            var systemScreen = await _repository.SystemScreens.Where(e => e.ScreenId == id).AsNoTracking().FirstOrDefaultAsync();
            return systemScreen;
        }

        public async Task<SystemScreen> GetSystemScreen(string systemType, string screenCode)
        {
            var systemScreen = await _repository.SystemScreens.Where(e => e.SystemType == systemType && e.ScreenCode == screenCode && e.FeatureType == docxFeatureCode)
                                    .AsNoTracking().FirstOrDefaultAsync();
            return systemScreen;
        }

        //public async Task<List<DOCXContactDTO>> GetDOCXContacts(int docxId, string userEmail)
        //{
        //    var result = await _repository.DOCXContactDTO.FromSqlInterpolated($"procDOCX_PopupContacts @DOCXId={docxId}, @UserEmail={userEmail}").AsNoTracking().ToListAsync();
        //    return result;
        //}

        public bool UpdatePopupFilter(int docxId, string fieldName, string operand, string userEmail, string userName)
        {
            var connectionString = _repository.Database.GetDbConnection().ConnectionString;
            using (SqlCommand cmd = new SqlCommand("procDOCX_PopupFilterUpdate"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = new SqlConnection(connectionString);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@DOCXId", docxId);
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

        public DataSet GenerateDOCXData(int docxId, bool includeGenerated, bool isLog, string returnType,// IEnumerable<DOCXEntityContactDTO> selectedContacts,
                                                string userEmail, bool hasRespOffice, bool hasEntityFilter, int id)
        {
            var ds = GetDOCXDataSet(docxId, includeGenerated, isLog, returnType, userEmail, hasRespOffice, hasEntityFilter, id);

            // process ds: tables: 0=TableName; 1=rows of TableLink; 3=Session Id; 4->n=ctual tables
            var tableCount = ds.Tables.Count;
            if (tableCount < 4)
            {
                throw new Exception("GenerateDOCXData Error: Insufficient table data!");
            }

            ds.Tables[0].TableName = "xxyyzzTableName";
            ds.Tables[1].TableName = "xxyyzzTableLink";
            ds.Tables[2].TableName = "xxyyzzSession";

            // get session id for logging
            lastDOCXSessionId = (long)ds.Tables[2].Rows[0]["SessionId"];

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
            return lastDOCXSessionId;
        }

        private DataSet GetDOCXDataSet(int docxId, bool includeGenerated, bool isLog, string returnType,// IEnumerable<DOCXEntityContactDTO> selectedContacts,
                                                    string userEmail, bool hasRespOffice, bool hasEntityFilter, int id)
        {
            //var contactParam = BuildContactParameters(selectedContacts);
            var connectionString = _repository.Database.GetDbConnection().ConnectionString;
            using (SqlCommand cmd = new SqlCommand("procDOCX_Gen"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = new SqlConnection(connectionString);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                //cmd.Parameters.RemoveAt("@EntityContacts");
                //if (selectedContacts.Count() > 0)
                //    cmd.Parameters.AddWithValue("@EntityContacts", contactParam).SqlDbType = SqlDbType.Structured;
                //else
                //    cmd.Parameters.AddWithValue("@EntityContacts", null);

                cmd.Parameters["@DOCXId"].Value = docxId;
                cmd.Parameters["@IncludeGenerated"].Value = includeGenerated;
                cmd.Parameters["@IsLog"].Value = isLog;
                cmd.Parameters["@ReturnType"].Value = returnType;
                cmd.Parameters["@UserEmail"].Value = userEmail;
                cmd.Parameters["@HasRespOfficeOn"].Value = hasRespOffice;
                cmd.Parameters["@HasEntityFilterOn"].Value = hasEntityFilter;
                cmd.Parameters["@Id"].Value = id;

                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;

                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds;
            }
        }

        //private List<SqlDataRecord> BuildContactParameters(IEnumerable<DOCXEntityContactDTO> entityContacts)
        //{
        //    var contactList = new List<SqlDataRecord>();
        //    foreach (DOCXEntityContactDTO contact in entityContacts)
        //    {
        //        var record = new SqlDataRecord(
        //            new SqlMetaData("EntityId", SqlDbType.Int),
        //            new SqlMetaData("ContactId", SqlDbType.Int));
                
        //        record.SetInt32(0, contact.EntityId);
        //        record.SetInt32(1, contact.ContactId);
        //        contactList.Add(record);

        //    }
        //    return contactList;
        //}

        public int LogDOCX(long sessionId, string systemType, int docxId, string docxFile, string userName, string? itemId, string? signatory)
        {
            var connectionString = _repository.Database.GetDbConnection().ConnectionString;
            using (SqlCommand cmd = new SqlCommand("procDOCX_Log"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = new SqlConnection(connectionString);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@SessionId", sessionId);
                cmd.Parameters.AddWithValue("@SystemType", systemType);
                cmd.Parameters.AddWithValue("@DOCXId", docxId);
                cmd.Parameters.AddWithValue("@DOCXFile", docxFile);
                cmd.Parameters.AddWithValue("@GenBy", userName);
                cmd.Parameters.Add("@DOCXLogId", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@ItemId", itemId);
                cmd.Parameters.AddWithValue("@Signatory", signatory);
                cmd.ExecuteNonQuery();
                var docxLogId = (int)cmd.Parameters["@DOCXLogId"].Value;
                return docxLogId;
            }
            
        }

        #endregion

        #region Setup Preview Screen
        public DataTable PreviewDOCXData(int docxId, bool includeGenerated, string userEmail, bool hasRespOffice, bool hasEntityFilter,
                                                string sortExpr, int page, int pageSize = 0)
        {
            var connectionString = _repository.Database.GetDbConnection().ConnectionString;

            using (SqlDataAdapter da = new SqlDataAdapter("procDOCX_Preview", new SqlConnection(connectionString)))
            {
                DataTable dt = new DataTable();
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@DOCXId", docxId);
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
        public int PreviewDOCXCount()
        {
            return recordCount;
        }

        #endregion

        #region DOCX Logs
        public IQueryable<DOCXLog> DOCXLogs => _repository.DOCXLogs.AsNoTracking();
        //public IQueryable<DOCXLogDetail> DOCXLogDetails => _repository.DOCXLogDetails.AsNoTracking();
        #endregion

        #region USPTO
        public IQueryable<DOCXUSPTOHeader> DOCXUSPTOHeaders => _repository.DOCXUSPTOHeaders.AsNoTracking();
        public IQueryable<DOCXUSPTOHeaderKeyword> DOCXUSPTOHeaderKeywords => _repository.DOCXUSPTOHeaderKeywords.AsNoTracking();

        public async Task<List<DOCXUSPTOHeaderKeywordDTO>> GetUSPTOHeaderKeywordList()
        {
            var headers = await DOCXUSPTOHeaders.ToListAsync();
            var keywords = await DOCXUSPTOHeaderKeywords.ToListAsync();
            var result = new List<DOCXUSPTOHeaderKeywordDTO>();

            if (headers == null || keywords == null) return result;

            foreach (var header in headers)
            {
                var headerKeyword = new DOCXUSPTOHeaderKeywordDTO()
                {
                    HeaderKeyword = header.HeaderName,
                    IsHeader = true
                };
                result.Add(headerKeyword);

                var headerKeywords = keywords.Where(k => k.HId == header.HId).Select(k => new DOCXUSPTOHeaderKeywordDTO()
                {
                    HeaderKeyword = k.KeywordName,
                    IsHeader = false
                });
                result.AddRange(headerKeywords);
            }

            //var result = await _repository.DOCXUSPTOHeaderKeywordDTO.FromSqlInterpolated($"procDOCX_GetUSPTOHeaderKeywordList")
            //                .AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<DOCXUSPTOHeaderKeywordExcelDTO>> GetUSPTOHeaderKeywordExcelList()
        {
            var headers = await DOCXUSPTOHeaders.ToListAsync();
            var keywords = await DOCXUSPTOHeaderKeywords.ToListAsync();
            var result = new List<DOCXUSPTOHeaderKeywordExcelDTO>();
            var i = 1;

            if (headers == null || keywords == null) return result;

            foreach (var header in headers.OrderBy(h => h.HId))
            {
                i = 1;

                foreach (var keyword in keywords.Where(k => k.HId == header.HId).OrderBy(k => k.KId))
                {
                    var headerKeyword = new DOCXUSPTOHeaderKeywordExcelDTO()
                    {
                        Header = i == 1 ? header.HeaderName : "",
                        Keyword = keyword.KeywordName
                    };

                    result.Add(headerKeyword);
                    i++;
                }

            }

            return result;
        }

        #endregion

    }
}
