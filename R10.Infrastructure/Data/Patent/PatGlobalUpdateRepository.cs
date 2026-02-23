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
using Microsoft.Data.SqlClient.Server;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces.Patent;

namespace R10.Infrastructure.Data.Patent
{
    public class PatGlobalUpdateRepository : IPatGlobalUpdateRepository
    {
        protected readonly ApplicationDbContext _dbContext;

        public PatGlobalUpdateRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<IList<GlobalUpdateLookupDTO>> GetFromData(string updateField)
        {
            var result = await _dbContext.GlobalUpdateLookupDTO.FromSqlInterpolated($"procPatGlobalUpdate_DataFrom @UpdateField={updateField}").AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<IList<GlobalUpdateLookupDTO>> GetToData(string updateField)
        {
            var result = await _dbContext.GlobalUpdateLookupDTO.FromSqlInterpolated($"procPatGlobalUpdate_DataTo @UpdateField={updateField}").AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<(IList<PatGlobalUpdatePreviewDTO>, int)> GetPreviewList(PatGlobalUpdateCriteriaDTO searchCriteria, int page, int pageSize)
        {
            var sql = SqlHelper.BuildSql("procPatGlobalUpdate_RecordsAffected", searchCriteria);
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

            var result = await _dbContext.PatGlobalUpdatePreviewDTO.FromSqlRaw(sql, parameters.ToArray()).AsNoTracking().ToListAsync();
            var recordCount = (int)recordCountParam.Value;

            return (result, recordCount);
        }

        public async Task<int> RunUpdate(PatGlobalUpdateCriteriaDTO searchCriteria)
        {
            var keyIds = new List<SqlDataRecord>();

            if (searchCriteria.KeyIds != null) //because of update in client atty fields
            { 
                foreach (var item in searchCriteria.KeyIds)
                {
                    var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.Int), new SqlMetaData("DataKey", SqlDbType.VarChar, 500) });
                    record.SetInt32(0, item.Id);
                    record.SetString(1, item.DataKey);

                    keyIds.Add(record);
                }
            }

            var dataKeyIds = new List<SqlDataRecord>();
            if (searchCriteria.DataKeyIds != null) //because of update in client atty fields
            { 
                foreach (var item in searchCriteria.DataKeyIds)
                {
                    var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.NVarChar, SqlMetaData.Max) });
                    record.SetString(0, item);
                    dataKeyIds.Add(record);
                }
            }

            using (SqlCommand cmd = new SqlCommand("procPatGlobalUpdate"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters.RemoveAt("@KeyIds");
                cmd.Parameters.RemoveAt("@DataKeyIds");
                cmd.Parameters.RemoveAt("@RowCount");
                cmd.FillParamValues(searchCriteria);
                cmd.Parameters.AddWithValue("@KeyIds", keyIds.Any() ? keyIds : null).SqlDbType = SqlDbType.Structured;
                cmd.Parameters.AddWithValue("@DataKeyIds", dataKeyIds.Any() ? dataKeyIds : null).SqlDbType = SqlDbType.Structured;

                var recordCountParam = new SqlParameter
                {
                    ParameterName = "RowCount",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(recordCountParam);
                await cmd.ExecuteNonQueryAsync();

                var recordCount = (int)recordCountParam.Value;
                return recordCount;

            }
        }


        public IQueryable<PatGlobalUpdateLog> PatGlobalUpdateLogs => _dbContext.PatGlobalUpdateLog.AsNoTracking();

    }
}
