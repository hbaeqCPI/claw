using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Core.Queries.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient.Server;

namespace R10.Infrastructure.Data
{

    public class QuickDocketRepository : IQuickDocketRepository
    {
        protected readonly ApplicationDbContext _dbContext;
        public QuickDocketRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<QuickDocketDTO>> GetQuickDocket(QuickDocketSearchCriteriaDTO criteria)
        {
            var sql = SqlHelper.BuildSql("procSysQuickDocket", criteria);
            var parameters = SqlHelper.BuildSqlParameters(criteria);

            var result = await _dbContext.QuickDocketDTO.FromSqlRaw(sql, parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task UpdateQuickDocket(QuickDocketUpdateCriteriaDTO criteria)
        {
            var ddIds = new List<SqlDataRecord>();
            foreach (var item in criteria.RecIds)
            {
                var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.VarChar,500) });
                record.SetString(0, item);
                ddIds.Add(record);
            }

            using (SqlCommand cmd = new SqlCommand("procSysQuickDocketUpdate"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.FillParamValues(criteria);
                cmd.Parameters.RemoveAt("@DDIds");
                cmd.Parameters.AddWithValue("@DDIds", ddIds).SqlDbType = SqlDbType.Structured;
                await cmd.ExecuteNonQueryAsync();
            }

        }

        public async Task<List<QuickDocketDeDocketBatchUpdateResultDTO>> UpdateQuickDocketDeDocketBatch(QuickDocketUpdateCriteriaDTO criteria)
        {
            var ddIds = new List<SqlDataRecord>();
            foreach (var item in criteria.RecIds)
            {
                var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.VarChar, 500) });
                record.SetString(0, item);
                ddIds.Add(record);
            }

            using (SqlCommand cmd = new SqlCommand("procSysQuickDocketUpdate"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.FillParamValues(criteria);
                cmd.Parameters.RemoveAt("@DDIds");
                cmd.Parameters.AddWithValue("@DDIds", ddIds).SqlDbType = SqlDbType.Structured;

                var reader = await cmd.ExecuteReaderAsync();
                var result = new List<QuickDocketDeDocketBatchUpdateResultDTO>();

                while (reader.Read())
                {
                    var file = new QuickDocketDeDocketBatchUpdateResultDTO
                    {
                        DedocketId = (int)reader["DedocketId"],
                        SystemType = (string)reader["SystemType"],
                        ActId = (int)reader["ActId"],
                        Instruction = (string)reader["Instruction"],
                        ParentId = (int)reader["ParentId"]
                    };
                    result.Add(file);
                }
                return result;
            }

        }

        public IQueryable<QuickDocketSchedulerDTO> GetQuickDocketScheduler(QuickDocketSearchCriteriaDTO criteria)
        {
            var sql = SqlHelper.BuildSql("procSysQuickDocket", criteria);
            var parameters = SqlHelper.BuildSqlParameters(criteria);
            var result = _dbContext.QuickDocketSchedulerDTO.FromSqlRaw(sql, parameters.ToArray()).AsNoTracking().ToList();
            return result.AsQueryable();
        }

        public async Task<List<QDActionTypeLookupDTO>> CombinedActionTypes(string systemType) {
            var query = BuildLookupQuery(systemType, QDLookup.ActionType);
            var result = await _dbContext.QDActionTypeLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDActionTypeLookupDTO>> CombinedDefaultActionTypes(string systemType)
        {
            var query = BuildLookupDefaultQuery(systemType, QDLookupDefault.ActionType);
            var result = await _dbContext.QDActionTypeLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDActionDueLookupDTO>> CombinedActionDues(string systemType)
        {
            var query = BuildLookupQuery(systemType, QDLookup.ActionDue);
            var result = await _dbContext.QDActionDueLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDActionDueLookupDTO>> CombinedDefaultActionDues(string systemType)
        {
            var query = BuildLookupDefaultQuery(systemType, QDLookupDefault.ActionDue);
            var result = await _dbContext.QDActionDueLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDCaseNumberLookupDTO>> CombinedCaseNumbers(string systemType)
        {
            var query = BuildLookupQuery(systemType, QDLookup.CaseNumber);
            var result = await _dbContext.QDCaseNumberLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDCaseNumberLookupDTO>> CombinedDefaultCaseNumbers(string systemType)
        {
            var query = BuildLookupDefaultQuery(systemType, QDLookupDefault.CaseNumber);
            var result = await _dbContext.QDCaseNumberLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDCaseTypeLookupDTO>> CombinedCaseTypes(string systemType)
        {
            var query = BuildLookupQuery(systemType, QDLookup.CaseType);
            var result = await _dbContext.QDCaseTypeLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDRespOfficeLookupDTO>> CombinedRespOffices(string systemType)
        {
            var query = BuildLookupQuery(systemType, QDLookup.RespOffice);
            var result = await _dbContext.QDRespOfficeLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDCaseTypeLookupDTO>> CombinedDefaultCaseTypes(string systemType)
        {
            var query = BuildLookupDefaultQuery(systemType, QDLookupDefault.CaseType);
            var result = await _dbContext.QDCaseTypeLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDRespOfficeLookupDTO>> CombinedDefaultRespOffices(string systemType)
        {
            var query = BuildLookupDefaultQuery(systemType, QDLookupDefault.RespOffice);
            var result = await _dbContext.QDRespOfficeLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDClientRefLookupDTO>> CombinedClientRefs(string systemType)
        {
            var query = BuildLookupQuery(systemType, QDLookup.ClientRef);
            var result = await _dbContext.QDClientRefLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDDeDocketInstructionLookupDTO>> CombinedDeDocketInstructions(string systemType)
        {
            var query = BuildLookupQuery(systemType, QDLookup.DeDocketInstruction);
            var result = await _dbContext.QDDeDocketInstructionLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDDeDocketInstructedByLookupDTO>> CombinedDeDocketInstructedBy(string systemType)
        {
            var query = BuildLookupQuery(systemType, QDLookup.DeDocketInstructedBy);
            var result = await _dbContext.QDDeDocketInstructedByLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDClientRefLookupDTO>> CombinedDefaultClientRefs(string systemType)
        {
            var query = BuildLookupDefaultQuery(systemType, QDLookupDefault.ClientRef);
            var result = await _dbContext.QDClientRefLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDStatusLookupDTO>> CombinedStatuses(string systemType)
        {
            var query = BuildLookupQuery(systemType, QDLookup.Status);
            var result = await _dbContext.QDStatusLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDStatusLookupDTO>> CombinedDefaultStatuses(string systemType)
        {
            var query = BuildLookupDefaultQuery(systemType, QDLookupDefault.Status);
            var result = await _dbContext.QDStatusLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDTitleLookupDTO>> CombinedTitles(string systemType)
        {
            var query = BuildLookupQuery(systemType, QDLookup.Title); 
            var result = await _dbContext.QDTitleLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDTitleLookupDTO>> CombinedDefaultTitles(string systemType)
        {
            var query = BuildLookupDefaultQuery(systemType, QDLookupDefault.Title);
            var result = await _dbContext.QDTitleLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDIndicatorLookupDTO>> CombinedIndicators(string systemType)
        {
            var query = BuildLookupQuery(systemType, QDLookup.Indicator);
            var result = await _dbContext.QDIndicatorLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDIndicatorLookupDTO>> CombinedDefaultIndicators(string systemType)
        {
            var query = BuildLookupDefaultQuery(systemType, QDLookupDefault.Indicator);
            var result = await _dbContext.QDIndicatorLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDCountryLookupDTO>> CombinedCountries(string systemType)
        {
            var query = BuildLookupQuery(systemType, QDLookup.Country);
            var result = await _dbContext.QDCountryLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDCountryLookupDTO>> CombinedDefaultCountries(string systemType)
        {
            var query = BuildLookupDefaultQuery(systemType, QDLookupDefault.Country);
            var result = await _dbContext.QDCountryLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDClientLookupDTO>> GetClientList(string systemType)
        {
            var query = BuildLookupQuery(systemType, QDLookup.Client);
            var result = await _dbContext.QDClientLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDAgentLookupDTO>> GetAgentList(string systemType)
        {
            var query = BuildLookupQuery(systemType, QDLookup.Agent);
            var result = await _dbContext.QDAgentLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDOwnerLookupDTO>> GetOwnerList(string systemType)
        {
            var query = BuildLookupQuery(systemType, QDLookup.Owner);
            var result = await _dbContext.QDOwnerLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<QDAttorneyLookupDTO>> GetAttorneyList(string systemType)
        {
            var query = BuildLookupQuery(systemType, QDLookup.Attorney);
            var result = await _dbContext.QDAttorneyLookupDTO.FromSqlRaw(query.Sql, query.Parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        private string GenerateSql(string storedProcName, QuickDocketSearchCriteriaDTO entity)
        {
            string sql = storedProcName;
            foreach (PropertyInfo property in entity.GetType().GetProperties())
            {
                sql = sql + " @" + property.Name + ",";
            }
            return sql.Substring(0, sql.Length - 1);
        }

        private LookupQuery BuildLookupQuery(string systemType, QDLookup action)
        {
            var criteria = new { action = (int)action, systemType };
            var sql = SqlHelper.BuildSql("procSysQuickDocketLookUp", criteria);
            var parameters = SqlHelper.BuildSqlParameters(criteria);
            return new LookupQuery { Sql = sql, Parameters = parameters };
        }

        private LookupQuery BuildLookupDefaultQuery(string systemType, QDLookupDefault action)
        {
            var criteria = new { action = (int)action, systemType };
            var sql = SqlHelper.BuildSql("procSysQuickDocketLookUpDefault", criteria);
            var parameters = SqlHelper.BuildSqlParameters(criteria);
            return new LookupQuery { Sql = sql, Parameters = parameters };
        }
    }

    public enum QDLookup {
        RespOffice = 1,
        ActionType,
        ActionDue,
        Responsible,
        Attorney,
        CaseNumber,
        Country,
        CaseType,
        Client,
        ClientRef,
        Owner,
        Indicator,
        Agent,
        Title,
        Status,
        DeDocketInstruction,
        DeDocketInstructedBy,
    }

    public enum QDLookupDefault
    {
        RespOffice = 1,
        ActionType,
        ActionDue,
        CaseNumber,
        Country,
        CaseType,
        ClientRef,
        Indicator,
        Title,
        Status
    }

    public class LookupQuery {
        public string Sql { get; set; }
        public List<SqlParameter> Parameters { get; set; }
    }
}
