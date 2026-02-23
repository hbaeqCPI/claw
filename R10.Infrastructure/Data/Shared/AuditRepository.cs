using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Interfaces;

namespace R10.Infrastructure.Data
{
    public class AuditRepository : EFRepository<AuditHeaderDTO>, IAuditRepository
    {
        public AuditRepository(ApplicationDbContext dbContext) : base(dbContext) { }


        public async Task<AuditLogPagedResult> GetAuditLogHeader(AuditSearchDTO searchCriteria)
        {
            var sql = SqlHelper.BuildSql("procAudWebLogHeader", searchCriteria);
            var parameters = SqlHelper.BuildSqlParameters(searchCriteria);
            //var result = await _dbContext.AuditHeaderDTO.FromSqlRaw(sql, parameters.ToArray()).AsNoTracking().ToListAsync();
            //return result;
            
            var auditLogPagedResult = new AuditLogPagedResult() { Data = new List<AuditHeaderDTO>(), TotalCount = 0 };

            using (SqlCommand cmd = new SqlCommand("procAudWebLogHeader"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                
                cmd.Parameters.AddRange(parameters.ToArray());
                
                using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    if (await reader.ReadAsync())
                    {
                        if (reader.HasRows && reader.GetName(0) == "TotalCount")
                            auditLogPagedResult.TotalCount = reader.GetInt32(0);
                        else
                            auditLogPagedResult.TotalCount = 0;
                    }
                    else
                        auditLogPagedResult.TotalCount = 0;

                    // 2. Advance to the next result set (the actual data)
                    await reader.NextResultAsync();

                    // 3. Read the paginated data
                    while (await reader.ReadAsync())
                    {
                        auditLogPagedResult.Data.Add(new AuditHeaderDTO
                        {
                            AudTrailId = reader.GetInt32(reader.GetOrdinal("AudTrailId")),
                            SystemType = reader.GetString(reader.GetOrdinal("SystemType")),
                            TableName = reader.GetString(reader.GetOrdinal("TableName")),
                            UserName = reader.GetString(reader.GetOrdinal("UserName")),
                            TranxDate = reader.GetDateTime(reader.GetOrdinal("TranxDate")),
                            TranxType = reader.GetString(reader.GetOrdinal("TranxType"))
                        });
                    }
                }

                cmd.Connection?.Close();
            }

            return auditLogPagedResult;
        }

        public async Task<List<AuditKeyDTO>> GetAuditKey(string systemType, int audTrailId)
        {
            var result = await _dbContext.AuditKeyDTO.FromSqlRaw("Exec procAudWebLogKey @SystemType, @AudTrailId",
                                    new SqlParameter("@SystemType", systemType), new SqlParameter("@AudTrailId", audTrailId))
                                    .AsNoTracking().ToListAsync();
            return result;
        }


        public async Task<List<AuditDetailDTO>> GetAuditLogDetail(string systemType, int audTrailId)
        {
            var result = await _dbContext.AuditDetailDTO.FromSqlRaw("Exec procAudWebLogDetail @SystemType, @AudTrailId",
                                    new SqlParameter("@SystemType", systemType), new SqlParameter("@AudTrailId", audTrailId))
                                    .AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<LookupDTO>> GetAuditLookup(string systemType, string dataType)
        {
            var result = await  _dbContext.AuditLookupDTO.FromSqlRaw("Exec procAudWebLookup @SystemType, @DataType",
                                                 new SqlParameter("@SystemType", systemType), new SqlParameter("@DataType", dataType))
                                                 .AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<AuditReportDTO>> GetAuditReport(AuditSearchDTO searchCriteria)
        {

            var sql = SqlHelper.BuildSql("procAudWebReport", searchCriteria);
            var parameters = SqlHelper.BuildSqlParameters(searchCriteria);
            var result = await _dbContext.AuditReportDTO.FromSqlRaw(sql, parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;


        }

        public async Task<List<LookupDTO>> GetAvailableSystemTypes()
        {
            var result = await _dbContext.AuditLookupDTO.FromSqlRaw("Select Distinct [SystemType] AS [Value], [SystemType] AS [Text] From tblAudTables Where IsInAudit = 1")
                                                 .AsNoTracking().ToListAsync();
            return result;
        }
    }
}
