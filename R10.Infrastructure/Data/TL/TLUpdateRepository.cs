using Microsoft.EntityFrameworkCore;
using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using R10.Core.DTOs;
using Microsoft.Data.SqlClient.Server;
using R10.Core.Services;

namespace R10.Infrastructure.Data
{
    public class TLUpdateRepository : ITLUpdateRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public TLUpdateRepository(ApplicationDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        #region Biblio
        public async Task<List<TLNumberFormatDTO>> GetNumbersToStandardize()
        {
            var list = await _dbContext.TLNumberFormatDTO
                .FromSqlRaw($"procTLMarkBiblio @Action=1").AsNoTracking().ToListAsync();
            return list;
        }


        public async Task SaveStandardNumber(List<TLNumberFormatDTO> numbers)
        {
            var records = new List<SqlDataRecord>();
            foreach (var data in numbers)
            {
                var record = new SqlDataRecord(new SqlMetaData[]  {
                    new SqlMetaData("TLTmkId", SqlDbType.Int),
                    new SqlMetaData("TMSStdAppNo", SqlDbType.VarChar,20),
                    new SqlMetaData("TMSStdPubNo", SqlDbType.VarChar,20),
                    new SqlMetaData("TMSStdRegNo", SqlDbType.VarChar,20)
                });

                record.SetValue(0, data.TLTmkId);
                record.SetValue(1, data.TMSStdAppNo);
                record.SetValue(2, data.TMSStdPubNo);
                record.SetValue(3, data.TMSStdRegNo);
                records.Add(record);
            }

            using (SqlCommand cmd = new SqlCommand("procTLMarkBiblio"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@Action", 2);
                cmd.Parameters.AddWithValue("@TLSearch", records).SqlDbType = SqlDbType.Structured;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task MarkBiblioDiscrepancies()
        {
            await _dbContext.Database.ExecuteSqlRawAsync($"procTLMarkBiblio @Action=0, @MarkUpdateFlags=1");
        }

        public async Task<List<TLCompareGoodsDTO>> CompareGoods(int tlTmkId)
        {
            var list = await _dbContext.TLCompareGoodsDTO
                .FromSqlRaw($"procTLCompareGoods @TLTmkId={tlTmkId}").AsNoTracking().ToListAsync();
            return list;
        }

        public async Task<int> UpdateBiblioRecord(int tlTmkId, string updatedBy)
        {
            var parameters = SqlHelper.BuildSqlParameters(new {TLTmkId= tlTmkId, UpdatedBy=updatedBy });
            parameters.Add(new SqlParameter {
                ParameterName = "Result",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            });
            await _dbContext.Database.ExecuteSqlRawAsync("exec procTLUpdateBiblio @TLTmkId,@UpdatedBy, @Result Out", parameters.ToArray());
            var result = (int)parameters[2].Value;
            return result;
        }

        public async Task<int> UpdateBiblioRecords(TLUpdateCriteria criteria) {
            using (SqlCommand cmd = new SqlCommand("procTLUpdateBiblio"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.FillParamValues(criteria);
                cmd.Parameters.RemoveAt("@Result");
                var resultParam = new SqlParameter
                {
                    ParameterName = "Result",
                    DbType = DbType.Int32,
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(resultParam);
                await cmd.ExecuteNonQueryAsync();
                var result = (int)resultParam.Value;
                return result;
            }
        }

        public async Task<bool> UndoBiblio(int jobId, int tmkId, int logId, string updatedBy)
        {
            using (SqlCommand cmd = new SqlCommand("procTLPTO_UpdLogBiblioUndo"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@JobId", jobId);
                cmd.Parameters.AddWithValue("@TmkId", tmkId);
                cmd.Parameters.AddWithValue("@LogId", logId);
                cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                cmd.Parameters.Add("@Return_Value", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                await cmd.ExecuteNonQueryAsync();
                var result = (int)cmd.Parameters["@Return_Value"].Value;
                return result > 0;
            }
        }

        public async Task<List<TLUpdateWorkflow>> GetUpdateWorkflowRecs(int jobId, int tlTmkId)
        {
            var ids = new List<SqlDataRecord>();
            var result = new List<TLUpdateWorkflow>();
            
            using (SqlCommand cmd = new SqlCommand("procTLUpdateWorkflow"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@JobId", jobId);
                cmd.Parameters.AddWithValue("@TLTmkId", tlTmkId);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result.Add(new TLUpdateWorkflow { 
                            TmkId = reader.GetInt32(0),
                            OldTrademarkStatus = reader.GetString(1),
                            TriggerDate = reader.IsDBNull(2) ? (DateTime?) null: (DateTime?)reader.GetDateTime(2)
                    });
                    }
                }
                cmd.Connection?.Close();
            }
            return result;
        }

        #endregion

        #region TrademarkName
        public async Task<bool> UpdateTrademarkNameRecord(int tlTmkId, string updatedBy)
        {
            var parameters = SqlHelper.BuildSqlParameters(new { TLTmkId = tlTmkId, UpdatedBy = updatedBy });
            parameters.Add(new SqlParameter
            {
                ParameterName = "Result",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            });
            await _dbContext.Database.ExecuteSqlRawAsync("exec procTLUpdateTrademarkName @TLTmkId,@UpdatedBy, @Result Out", parameters.ToArray());
            var result = (int)parameters[2].Value;
            return result > 0;
        }

        public async Task<bool> UpdateTrademarkNameRecords(TLUpdateCriteria criteria)
        {
            using (SqlCommand cmd = new SqlCommand("procTLUpdateTrademarkName"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.FillParamValues(criteria);
                cmd.Parameters.RemoveAt("@Result");
                var resultParam = new SqlParameter
                {
                    ParameterName = "Result",
                    DbType = DbType.Int32,
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(resultParam);
                await cmd.ExecuteNonQueryAsync();
                var result = (int)resultParam.Value;
                return result > 0;
            }
        }
        public async Task<bool> UndoTrademarkName(int jobId, int tmkId, int logId, string updatedBy)
        {
            using (SqlCommand cmd = new SqlCommand("procTLPTO_UpdLogTmkNameUndo"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@JobId", jobId);
                cmd.Parameters.AddWithValue("@TmkId", tmkId);
                cmd.Parameters.AddWithValue("@LogId", logId);
                cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                cmd.Parameters.Add("@Return_Value", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                await cmd.ExecuteNonQueryAsync();
                var result = (int)cmd.Parameters["@Return_Value"].Value;
                return result > 0;
            }
        }
        #endregion

        #region Actions
        public async Task<bool> UpdateActionRecords(TLUpdateCriteria criteria)
        {
            using (SqlCommand cmd = new SqlCommand("procTLUpdateAction"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.FillParamValues(criteria);
                cmd.Parameters.RemoveAt("@Result");
                var resultParam = new SqlParameter
                {
                    ParameterName = "Result",
                    DbType = DbType.Int32,
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(resultParam);
                await cmd.ExecuteNonQueryAsync();
                var result = (int)resultParam.Value;
                return result > 0;
            }
        }

        public async Task<bool> UndoActions(int jobId, int tmkId, int logId, string updatedBy)
        {
            using (SqlCommand cmd = new SqlCommand("procTLPTO_UpdLogActionUndo"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@JobId", jobId);
                cmd.Parameters.AddWithValue("@TmkId", tmkId);
                cmd.Parameters.AddWithValue("@LogId", logId);
                cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                cmd.Parameters.Add("@Return_Value", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                await cmd.ExecuteNonQueryAsync();
                var result = (int)cmd.Parameters["@Return_Value"].Value;
                return result > 0;
            }
        }
        #endregion

        #region Goods
        public async Task<bool> UndoGoods(int jobId, int tmkId, int logId, string updatedBy)
        {
            using (SqlCommand cmd = new SqlCommand("procTLPTO_UpdLogGoodsUndo"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@JobId", jobId);
                cmd.Parameters.AddWithValue("@TmkId", tmkId);
                cmd.Parameters.AddWithValue("@LogId", logId);
                cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                cmd.Parameters.Add("@Return_Value", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                await cmd.ExecuteNonQueryAsync();
                var result = (int)cmd.Parameters["@Return_Value"].Value;
                return result > 0;
            }
        }
        #endregion



    }
}
