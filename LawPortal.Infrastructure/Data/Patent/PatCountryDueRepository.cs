using Microsoft.EntityFrameworkCore;
using LawPortal.Core;
using LawPortal.Core.Interfaces;
using System.Data;
using Microsoft.Data.SqlClient;

namespace LawPortal.Infrastructure.Data
{
    public class PatCountryDueRepository : IPatCountryDueRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public PatCountryDueRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    
        public async Task GenerateCountryLawActions(CountryLawRetroParam criteria)
        {
            using (SqlCommand cmd = new SqlCommand("procPatCL_Gen_ActionsRetro"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters.RemoveAt("@ClientCode");
                cmd.Parameters.RemoveAt("@AttorneyCode");
                cmd.Parameters.RemoveAt("@Status");
                cmd.FillParamValues(criteria);

                if (criteria.ClientCode != null && criteria.ClientCode.Any()) { 
                    var clientCodes = "|" + String.Join("|", criteria.ClientCode) + "|";
                    cmd.Parameters.AddWithValue("@ClientCode", clientCodes);
                }

                if (criteria.AttorneyCode != null && criteria.AttorneyCode.Any())
                {
                    var attyCodes = "|" + String.Join("|", criteria.AttorneyCode) + "|";
                    cmd.Parameters.AddWithValue("@AttorneyCode", attyCodes);
                }

                if (criteria.Status != null && criteria.Status.Any())
                {
                    var appStatuses = "|" + String.Join("|", criteria.Status) + "|";
                    cmd.Parameters.AddWithValue("@Status", appStatuses);
                }

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
