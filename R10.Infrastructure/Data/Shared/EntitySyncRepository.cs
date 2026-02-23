using Microsoft.EntityFrameworkCore;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;

namespace R10.Infrastructure.Data
{
    public class EntitySyncRepository:  IEntitySyncRepository
    {

        protected readonly ApplicationDbContext _dbContext;
        public EntitySyncRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SyncEntities(int[] ids, int syncType, string userName) {
            
            var entityIds = new List<SqlDataRecord>();
            foreach (var item in ids)
            {
                var record = new SqlDataRecord(new SqlMetaData[] { new SqlMetaData("Id", SqlDbType.Int) });
                record.SetInt32(0, item);
                entityIds.Add(record);
            }

            using (SqlCommand command = new SqlCommand("procSysClientOwnerSyncUpdate"))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@Action", syncType);
                command.Parameters.AddWithValue("@EntityIDs", entityIds).SqlDbType = SqlDbType.Structured;
                command.Parameters.AddWithValue("@UserId", userName);
                
                command.Connection = new SqlConnection(_dbContext.Database.GetDbConnection().ConnectionString);
                command.Connection.Open();
                await command.ExecuteNonQueryAsync();
            }
        }

       
    }
}
