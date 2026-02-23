using Microsoft.EntityFrameworkCore;
using R10.Core;
using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
using R10.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace R10.Infrastructure.Data
{
    public class DataImportRepository : IDataImportRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public DataImportRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public DataTable  GetStructure(DataImportType type)
        {
            using (SqlDataAdapter darDetail = new SqlDataAdapter(type.TableLoader, _dbContext.Database.GetDbConnection() as SqlConnection))
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

        public async Task UpdateMappings(int importId, List<DataImportMapping> mappings)
        {
            //transfer old mappings            
            var oldMappings = await _dbContext.DataImportMappings.Where(m => m.ImportId == importId).ToListAsync();
            foreach (var item in oldMappings)
            {
                var newColumn = mappings.FirstOrDefault(m => m.YourField == item.YourField);
                if (newColumn != null)
                {
                    newColumn.CPIField = item.CPIField;
                }
            }

            await _dbContext.Database.ExecuteSqlRawAsync($"Delete From tblDataImportMapping Where ImportId={importId}");
            if (mappings.Any())
                _dbContext.DataImportMappings.AddRange(mappings);

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateErrors(int importId, List<DataImportError> errors, bool isUpdate) {
            await _dbContext.Database.ExecuteSqlRawAsync($"Delete From tblDataImportErrorLog Where ImportId={importId}");
            if (errors.Any())
            {
                if (isUpdate)
                {
                    await _dbContext.Database.ExecuteSqlRawAsync($"Update tblDataImportHistory Set Status='Update Failed' Where ImportId={importId}");
                }
                else
                {
                    await _dbContext.Database.ExecuteSqlRawAsync($"Update tblDataImportHistory Set Status='Import Failed' Where ImportId={importId}");
                }
                
                _dbContext.DataImportErrors.AddRange(errors);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task Import(DataImportType type,DataTable table, int importId, string options, string userName)
        {
            using (SqlCommand cmd = new SqlCommand(type.TableLoader))
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
