using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Data.SqlClient;
using R10.Core.Interfaces.GeneralMatter;
using R10.Core.Entities.GeneralMatter;

namespace R10.Infrastructure.Data.GeneralMatter
{
    public class GMCostTrackingImportRepository : IGMCostTrackingImportRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public GMCostTrackingImportRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public DataTable  GetStructure()
        {
            using (SqlDataAdapter darDetail = new SqlDataAdapter("procGmCostImport", _dbContext.Database.GetDbConnection() as SqlConnection))
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

        public async Task UpdateMappings(int importId, List<GMCostTrackingImportMapping> mappings)
        {
            //transfer old mappings            
            var oldMappings = await _dbContext.GMCostTrackingImportMappings.Where(m => m.ImportId == importId).ToListAsync();
            foreach (var item in oldMappings)
            {
                var newColumn = mappings.FirstOrDefault(m => m.YourField == item.YourField);
                if (newColumn != null)
                {
                    newColumn.CPIField = item.CPIField;
                }
            }

            await _dbContext.Database.ExecuteSqlRawAsync($"Delete From tblGMCostImportMapping Where ImportId={importId}");
            if (mappings.Any())
                _dbContext.GMCostTrackingImportMappings.AddRange(mappings);

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateErrors(int importId, List<GMCostTrackingImportError> errors) {
            await _dbContext.Database.ExecuteSqlRawAsync($"Delete From tblGMCostImportErrorLog Where ImportId={importId}");
            if (errors.Any())
            {
                await _dbContext.Database.ExecuteSqlRawAsync($"Update tblGMCostImportHistory Set Status='Import Failed' Where ImportId={importId}");
                _dbContext.GMCostTrackingImportErrors.AddRange(errors);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task Import(DataTable table, int importId, string options, string userName)
        {
            using (SqlCommand cmd = new SqlCommand("procGmCostImport"))
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
