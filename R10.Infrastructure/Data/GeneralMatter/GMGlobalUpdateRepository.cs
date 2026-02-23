using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Interfaces.GeneralMatter;
using Microsoft.Data.SqlClient.Server;

namespace R10.Infrastructure.Data.GeneralMatter
{
    public class GMGlobalUpdateRepository : IGMGlobalUpdateRepository
    {
        protected readonly ApplicationDbContext _dbContext;

        public GMGlobalUpdateRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<IList<GlobalUpdateLookupDTO>> GetFromData(string updateField)
        {
            var result = await _dbContext.GlobalUpdateLookupDTO.FromSqlInterpolated($"procGMGlobalUpdate_DataFrom @UpdateField={updateField}").AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<IList<GlobalUpdateLookupDTO>> GetToData(string updateField)
        {
            var result = await _dbContext.GlobalUpdateLookupDTO.FromSqlInterpolated($"procGMGlobalUpdate_DataTo @UpdateField={updateField}").AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<(IList<GMGlobalUpdatePreviewDTO>,int)> GetPreviewList(GMGlobalUpdateCriteriaDTO searchCriteria, int page, int pageSize)
        {
            var sql = SqlHelper.BuildSql("procGMGlobalUpdate_RecordsAffected", searchCriteria);
            var parameters = SqlHelper.BuildSqlParameters(searchCriteria);

            // append output param @RowCount
            sql += ", @Page, @PageSize, @RowCount Output";
            parameters.Add(new SqlParameter { ParameterName = "Page", SqlDbType = SqlDbType.Int, Value = page });
            parameters.Add(new SqlParameter { ParameterName = "PageSize", SqlDbType = SqlDbType.Int, Value = pageSize });

            var recordCountParam = new SqlParameter
            {
                ParameterName = "RowCount", 
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            parameters.Add(recordCountParam);

            var result = await _dbContext.GMGlobalUpdatePreviewDTO.FromSqlRaw(sql, parameters.ToArray()).AsNoTracking().ToListAsync();
            var recordCount = (int) recordCountParam.Value;

            return (result, recordCount);
        }

        public async Task<int> RunUpdate (GMGlobalUpdateCriteriaDTO searchCriteria)
        {
            var keyIds = new List<SqlDataRecord>();
            foreach (var item in searchCriteria.KeyIds)
            {
                var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.Int) });
                record.SetInt32(0, item);
                keyIds.Add(record);
            }

            //var sql = SqlHelper.BuildSql("procGMGlobalUpdate", searchCriteria);
            //sql += ", @RowCount Output";

            //var parameters = SqlHelper.BuildSqlParameters(searchCriteria);
            //var recordCountParam = new SqlParameter
            //{
            //    ParameterName = "RowCount",
            //    SqlDbType = SqlDbType.Int,
            //    Direction = ParameterDirection.Output
            //};
            //parameters.Add(recordCountParam);

            //await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
            //var recordCount = (int) recordCountParam.Value;

            //return recordCount;

            using (SqlCommand cmd = new SqlCommand("procGMGlobalUpdate"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters.RemoveAt("@KeyIds");
                //cmd.Parameters.RemoveAt("@DataKeyIds");
                cmd.Parameters.RemoveAt("@RowCount");
                cmd.FillParamValues(searchCriteria);
                cmd.Parameters.AddWithValue("@KeyIds", keyIds).SqlDbType = SqlDbType.Structured;
                //cmd.Parameters.AddWithValue("@DataKeyIds", dataKeyIds).SqlDbType = SqlDbType.Structured;

                var recordCountParam = new SqlParameter
                {
                    ParameterName = "RowCount",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(recordCountParam);
                await cmd.ExecuteNonQueryAsync();

                var recordCount = (int) recordCountParam.Value;
                 return recordCount;

            }
        }

        public IQueryable<GMGlobalUpdateLog> GMGlobalUpdateLogs  => _dbContext.GMGlobalUpdateLog.AsNoTracking();

    }
}
