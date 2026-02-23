using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Services.Shared
{
    public class DataQueryService : IDataQueryService
    {
        private readonly IApplicationDbContext _repository;

        public DataQueryService(IApplicationDbContext repository)
        {
            _repository = repository;
        }

        #region DQMain CRUD
        public IQueryable<DataQueryMain> DataQueriesMain => _repository.DataQueriesMain.AsNoTracking();
        public IQueryable<DataQueryAllowedFunction> DataQueryAllowedFunction => _repository.DataQueryAllowedFunctions.AsNoTracking();
        public IQueryable<DataQueryTag> DataQueryTags => _repository.DataQueryTags.AsNoTracking();

        public async Task<DataQueryMain> GetByIdAsync(int queryId)
        {
            return await DataQueriesMain.SingleOrDefaultAsync(q => q.QueryId == queryId);
        }

        public async Task<DataQueryMain> GetByNameAsync(string queryName)
        {
            return await DataQueriesMain.SingleOrDefaultAsync(q => q.QueryName.Equals(queryName));
        }

        public async Task Add(DataQueryMain query)
        {
            _repository.DataQueriesMain.Add(query);
            await _repository.SaveChangesAsync();
        }

        public async Task Update(DataQueryMain query)
        {
            _repository.DataQueriesMain.Update(query);
            await _repository.SaveChangesAsync();
        }

        public async Task Delete(DataQueryMain query)
        {
            _repository.DataQueriesMain.Remove(query);
            await _repository.SaveChangesAsync();
        }

        #endregion

        #region DQ Metadata
        public async Task <List<DQMetadataDTO>> GetDQMetadata(string userEmail)
        {
            var metadata = await _repository.DQMetadataDTO.FromSqlInterpolated($"procDQ_MetaData @UserEmail={userEmail}")
                                .AsNoTracking().ToListAsync();
            return metadata;
        }

        public async Task<List<DQMetaRelationsDTO>> GetDQMetadataRelations(string userEmail)
        {
            var relations = await _repository.DQMetaRelationsDTO.FromSqlInterpolated($"procDQ_MetaDataRelationship @UserEmail={userEmail}")
                                .AsNoTracking().ToListAsync();
            return relations;
        }

        #endregion

        #region DQ Run Query

        private int recordCount;

        //public async Task<DataTable> RunQuery(string sql, string sortExpr, int page, int pageSize, string userName, bool hasEntityFilterOn, bool hasRespOfficeOn )
        public DataTable RunQuery(string sql, string sourceTables, string sourceTablesWithAlias, string selectList, string sortExpr, int page, int pageSize, string userName, bool hasEntityFilterOn, bool hasRespOfficeOn )
        {
            using (SqlDataAdapter da = new SqlDataAdapter("procDQ_RunQuery", new SqlConnection(GetSqlConnectionString())))
            {
                DataTable dt = new DataTable();
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.CommandTimeout = 0;
                da.SelectCommand.Parameters.AddWithValue("@SQL", sql);
                da.SelectCommand.Parameters.AddWithValue("@SourceTables", sourceTables);
                da.SelectCommand.Parameters.AddWithValue("@SourceTablesWithAlias", sourceTablesWithAlias);
                da.SelectCommand.Parameters.AddWithValue("@SelectList", selectList);
                da.SelectCommand.Parameters.AddWithValue("@SortExpr", sortExpr);
                da.SelectCommand.Parameters.AddWithValue("@Page", page);
                da.SelectCommand.Parameters.AddWithValue("@PageSize", pageSize);
                da.SelectCommand.Parameters.AddWithValue("@UserName", userName);
                da.SelectCommand.Parameters.AddWithValue("@HasEntityFilterOn", hasEntityFilterOn);
                da.SelectCommand.Parameters.AddWithValue("@HasRespOfficeOn", hasRespOfficeOn);

                SqlParameter recCountParam = new SqlParameter("@RowCount", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                da.SelectCommand.Parameters.Add(recCountParam);

                da.Fill(dt);

                recordCount = (int)recCountParam.Value;
                return dt;
            }

            // async version below
            //var conn = GetSqlConnection();
            //conn.Open();
            //using (SqlCommand cmd = conn.CreateCommand())
            //{
            //    DataTable dt = new DataTable();
            //    cmd.CommandType = CommandType.StoredProcedure;
            //    cmd.CommandText = "procDQ_RunQuery";
            //    cmd.Parameters.AddWithValue("@SQL", sql);
            //    cmd.Parameters.AddWithValue("@SortExpr", sortExpr);
            //    cmd.Parameters.AddWithValue("@Page", page);
            //    cmd.Parameters.AddWithValue("@PageSize", pageSize);
            //    SqlParameter recCountParam = new SqlParameter("@RowCount", SqlDbType.Int)
            //    {
            //        Direction = ParameterDirection.Output
            //    };
            //    cmd.Parameters.Add(recCountParam);
            //    var reader = await cmd.ExecuteReaderAsync();
            //    dt.Load(reader);
            //    if (recCountParam.Value != DBNull.Value)
            //        recordCount = (int)recCountParam.Value;
            //    return dt;
            //}
        }

        public DataTable RunCRQuery(string sql, string userId, bool hasEntityFilterOn, bool hasRespOfficeOn)
        {
            using (SqlDataAdapter da = new SqlDataAdapter("procCR_RunQuery", new SqlConnection(GetSqlConnectionString())))
            {
                DataTable dt = new DataTable();
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@SQL", sql);
                da.SelectCommand.Parameters.AddWithValue("@UserId", userId);
                da.SelectCommand.Parameters.AddWithValue("@HasEntityFilterOn", hasEntityFilterOn);
                da.SelectCommand.Parameters.AddWithValue("@HasRespOfficeOn", hasRespOfficeOn);

                da.Fill(dt);
                return dt;
            }
        }

        public int RunQueryCount ()
        {
            return recordCount;
        }

        //private SqlConnection GetSqlConnection()
        //{
        //    return _repository.Database.GetDbConnection() as SqlConnection;
        //}

        private string GetSqlConnectionString()
        {
            return _repository.Database.GetDbConnection().ConnectionString;
        }
        #endregion

    }
}
