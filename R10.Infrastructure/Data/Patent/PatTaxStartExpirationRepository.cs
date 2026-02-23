using System;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Interfaces;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using SystemType = R10.Core.Entities.SystemType;
using Microsoft.Data.SqlClient.Server;

namespace R10.Infrastructure.Data
{

    public class PatTaxStartExpirationRepository : IPatTaxStartExpirationRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public PatTaxStartExpirationRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CountryApplication> GetCountryApplicationToCompute(int appId)
        {
            var app = await _dbContext.CountryApplications.AsNoTracking().FirstOrDefaultAsync(c => c.AppId == appId);
            return app;
        }

        public async Task<PatCountryLawTaxInfoDTO.UserResponse> CanComputeExpirationBeforeIssue(CountryApplication app)
        {
                var canCompute = await _dbContext.PatCountryLaws.AnyAsync(c => c.Country == app.Country && c.CaseType == app.CaseType && c.CalcExpirBeforeIssue);
                return canCompute
                    ? PatCountryLawTaxInfoDTO.UserResponse.JustOk
                    : PatCountryLawTaxInfoDTO.UserResponse.NotNeeded;
        }

        public async Task<PatCountryLawTaxInfoDTO> ComputeTaxStart(int appId)
        {
            var taxStartInfo = new PatCountryLawTaxInfoDTO();
            var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.Int) });
            record.SetInt32(0, appId);

            var appIds = new List<SqlDataRecord> { record };
            using (SqlCommand cmd = new SqlCommand("procPatCL_CalcTaxStart"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                
                cmd.Parameters.AddWithValue("@AppIds", appIds).SqlDbType = SqlDbType.Structured;
                cmd.Parameters.AddWithValue("@AutoUpdate", 0);
                var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                if (reader.Read())
                {
                    var pos = reader.GetOrdinal("EarliestTaxStartDate");

                    if (!reader.IsDBNull(pos))
                    {
                        taxStartInfo.ExpTaxDate = reader.GetDateTime(pos);
                    }

                    taxStartInfo.ExpTaxNoBaseDateCount = reader.GetInt32(reader.GetOrdinal("NoOfTaxStartDatesNoBaseDate"));
                    taxStartInfo.ExpTaxDateCount = reader.GetInt32(reader.GetOrdinal("NoOfTaxStartDates"));
                    taxStartInfo.MissingBasedOnDateLawList = reader.GetString(reader.GetOrdinal("MissingAllBasedOn"));
                }

                cmd.Connection?.Close();
            }

            return taxStartInfo;
        }

        public async Task<PatCountryLawTaxInfoDTO> ComputeExpiration(int appId)
        {
            var expireInfo = new PatCountryLawTaxInfoDTO();
            var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.Int) });
            record.SetInt32(0, appId);

            var appIds = new List<SqlDataRecord> { record };
            using (SqlCommand cmd = new SqlCommand("procPatCL_CalcExpiration"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@AppIds", appIds).SqlDbType = SqlDbType.Structured;
                cmd.Parameters.AddWithValue("@AutoUpdate",0);
                var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                if (reader.Read())
                {
                    var pos = reader.GetOrdinal("GreatestExpireDate");

                    if (!reader.IsDBNull(pos))
                    {
                        expireInfo.ExpTaxDate = reader.GetDateTime(pos);
                    }

                    expireInfo.ExpTaxNoBaseDateCount = reader.GetInt32(reader.GetOrdinal("NoOfExpireDatesNoBaseDate"));
                    expireInfo.ExpTaxDateCount = reader.GetInt32(reader.GetOrdinal("NoOfExpireDates"));
                    expireInfo.MissingBasedOnDateLawList = reader.GetString(reader.GetOrdinal("MissingAllBasedOn"));

                    reader.NextResult();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                var expirationDate = reader.GetDateTime(0);
                                expireInfo.ExpirationDates.Add(expirationDate);
                            }
                        }
                    }
                }

                cmd.Connection?.Close();
            }

            return expireInfo;
        }

        public async Task UpdateTaxInfo(TaxInfoUpdateType updateType, int appId, DateTime? taxStartDate, DateTime? expireDate, string updatedBy)
        {
            var app = await _dbContext.CountryApplications.FirstOrDefaultAsync(c => c.AppId == appId);
            if (app != null)
            {
                if (updateType == TaxInfoUpdateType.Both)
                {
                    app.ExpDate = expireDate;
                    app.TaxStartDate = taxStartDate;
                }
                else if (updateType == TaxInfoUpdateType.ExpireDate)
                {
                    app.ExpDate = expireDate;
                }
                else
                {
                    app.TaxStartDate = taxStartDate;
                }

                app.UpdatedBy = updatedBy;
                await _dbContext.SaveChangesAsync();
            }

            
        }
    }
}


