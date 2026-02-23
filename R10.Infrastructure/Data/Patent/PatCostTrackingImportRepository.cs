using Microsoft.EntityFrameworkCore;
using R10.Core;
using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
using R10.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using R10.Core.Interfaces.Patent;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data
{
    public class PatCostTrackingImportRepository : IPatCostTrackingImportRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public PatCostTrackingImportRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public DataTable  GetStructure()
        {
            using (SqlDataAdapter darDetail = new SqlDataAdapter("procPatCostImport", _dbContext.Database.GetDbConnection() as SqlConnection))
            {
                var dt = new DataTable();
                var cmd = darDetail.SelectCommand;
                cmd.CommandType = CommandType.StoredProcedure;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                darDetail.FillSchema(dt, SchemaType.Source);
                darDetail.Fill(dt);
                return dt;
            }
        }

        public async Task UpdateMappings(int importId, List<PatCostTrackingImportMapping> mappings)
        {
            //transfer old mappings            
            var oldMappings = await _dbContext.PatCostTrackingImportMappings.Where(m => m.ImportId == importId).ToListAsync();
            foreach (var item in oldMappings)
            {
                var newColumn = mappings.FirstOrDefault(m => m.YourField == item.YourField);
                if (newColumn != null)
                {
                    newColumn.CPIField = item.CPIField;
                }
            }

            await _dbContext.Database.ExecuteSqlRawAsync($"Delete From tblPatCostImportMapping Where ImportId={importId}");
            if (mappings.Any())
                _dbContext.PatCostTrackingImportMappings.AddRange(mappings);

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateErrors(int importId, List<PatCostTrackingImportError> errors) {
            await _dbContext.Database.ExecuteSqlRawAsync($"Delete From tblPatCostImportErrorLog Where ImportId={importId}");
            if (errors.Any())
            {
                await _dbContext.Database.ExecuteSqlRawAsync($"Update tblPatCostImportHistory Set Status='Import Failed' Where ImportId={importId}");
                _dbContext.PatCostTrackingImportErrors.AddRange(errors);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task Import(DataTable table, int importId, string options, string userName)
        {
            using (SqlCommand cmd = new SqlCommand("procPatCostImport"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();
                cmd.Parameters.AddWithValue("@Action", 1);
                cmd.Parameters.AddWithValue("@ImportId", importId);
                cmd.Parameters.AddWithValue("@Options", options);
                cmd.Parameters.AddWithValue("@CreatedBy", userName);
                cmd.Parameters.AddWithValue("@List", table).SqlDbType = SqlDbType.Structured;
                await cmd.ExecuteNonQueryAsync();

            }
        }
    }
}
