using Microsoft.EntityFrameworkCore;
using R10.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core;
using System.Data;
using Microsoft.Data.SqlClient.Server;
using Microsoft.Data.SqlClient;
using System.Reflection.PortableExecutable;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace R10.Infrastructure.Data
{

    public class SharePointRepository : ISharePointRepository
    {
        private readonly ApplicationDbContext _dbContext;
        
        public SharePointRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<string>> SyncToDocumentTablesSave(string userName, DateTime? lastModifiedDateTime, List<SharePointSyncDTO> sharePointSyncItems, bool fromCopy=false,
                                                                 bool mainRecordOnly=false, bool singleNode = false)
        {
            var result = new List<string>();
            var records = new List<SqlDataRecord>();
            foreach (var item in sharePointSyncItems)
            {
                var record = new SqlDataRecord(new SqlMetaData[] { 
                    new SqlMetaData("Id", SqlDbType.VarChar,255),
                    new SqlMetaData("Name", SqlDbType.VarChar,255),
                    new SqlMetaData("Key", SqlDbType.VarChar,255),
                    new SqlMetaData("DocLibrary", SqlDbType.VarChar,255),
                    new SqlMetaData("DocLibraryFolder", SqlDbType.VarChar,255),

                    new SqlMetaData("IsImage", SqlDbType.Bit),
                    new SqlMetaData("Author", SqlDbType.NVarChar,255),
                    new SqlMetaData("IsPrivate", SqlDbType.Bit),
                    new SqlMetaData("DocUrl", SqlDbType.VarChar,4000),
                    new SqlMetaData("Remarks", SqlDbType.NVarChar,4000),
                    new SqlMetaData("IsDefault", SqlDbType.Bit),
                    new SqlMetaData("IsPrintOnReport", SqlDbType.Bit),
                    new SqlMetaData("Tags", SqlDbType.NVarChar,4000),
                    new SqlMetaData("IsVerified", SqlDbType.Bit),
                    new SqlMetaData("IncludeInWorkflow", SqlDbType.Bit),
                    new SqlMetaData("IsActRequired", SqlDbType.Bit),
                    new SqlMetaData("Source", SqlDbType.NVarChar,20),
                    new SqlMetaData("CheckAct", SqlDbType.Bit),
                    new SqlMetaData("SendToClient", SqlDbType.Bit)
                });
                record.SetString(0, item.Id);
                record.SetString(1, item.Name);
                record.SetString(2, fromCopy ? item.ParentId : item.Key);
                record.SetString(3, item.DocLibrary);
                record.SetString(4, item.DocLibraryFolder);
                record.SetBoolean(5, item.IsImage);
                record.SetString(6, item.Author);
                record.SetBoolean(7, item.IsPrivate);
                record.SetString(8, item.DocUrl ?? "");
                record.SetString(9, item.Remarks ?? "");
                record.SetBoolean(10, item.IsDefault);
                record.SetBoolean(11, item.IsPrintOnReport);
                record.SetString(12, item.Tags ?? "");
                record.SetBoolean(13, item.IsVerified);
                record.SetBoolean(14, item.IncludeInWorkflow);
                record.SetBoolean(15, item.IsActRequired);
                record.SetString(16, item.Source ?? "");
                record.SetBoolean(17, item.CheckAct);
                record.SetBoolean(18, item.SendToClient);
                records.Add(record);
            }

            var name = fromCopy ? "procSharePointToDocCopySource" : "procSharePointToAzureBlobPrepare";
            using (SqlCommand cmd = new SqlCommand(name))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                
                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                cmd.CommandTimeout = 0;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@CreatedBy", userName);
                cmd.Parameters.AddWithValue("@List", records).SqlDbType = SqlDbType.Structured;

                if (!fromCopy) {
                    cmd.Parameters.AddWithValue("@LastModifiedDateTime", lastModifiedDateTime);
                    cmd.Parameters.AddWithValue("@MainRecordOnly", mainRecordOnly);
                    cmd.Parameters.AddWithValue("@SingleNode", singleNode);
                }
                //await cmd.ExecuteNonQueryAsync();

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                }
            }
            return result;
        }


        public async Task<List<SharePointToAzureBlobSyncDTO>> GetSharePointToAzureBlobList(string docLibrary)
        {
            var result = await _dbContext.SharePointToAzureBlobSyncDTO.FromSqlInterpolated($"procSharePointToAzureBlobProcess @DocLibrary={docLibrary}").AsNoTracking().ToListAsync();
            return result;
        }

        public async Task MarkSharePointToAzureBlobAsProcessed(int logId)
        {
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"Update tblSharePointToAzureBlobSyncLog Set Processed=1 Where LogId={logId}");
        }

        public async Task SyncToDocumentTablesUpdateDelete(string docLibrary, string systemType, List<SharePointSyncDTO> sharePointSyncItems)
        {
            var records = new List<SqlDataRecord>();
            foreach (var item in sharePointSyncItems)
            {
                var record = new SqlDataRecord(new SqlMetaData[] {
                    new SqlMetaData("Id", SqlDbType.VarChar,255),
                    new SqlMetaData("Name", SqlDbType.VarChar,255),
                    new SqlMetaData("Key", SqlDbType.VarChar,255),
                    new SqlMetaData("DocLibrary", SqlDbType.VarChar,255),
                    new SqlMetaData("DocLibraryFolder", SqlDbType.VarChar,255),
                    new SqlMetaData("IsImage", SqlDbType.Bit),
                    new SqlMetaData("Author", SqlDbType.NVarChar,255),
                    new SqlMetaData("IsPrivate", SqlDbType.Bit),
                    new SqlMetaData("DocUrl", SqlDbType.VarChar,4000),
                    new SqlMetaData("Remarks", SqlDbType.NVarChar,4000),
                    new SqlMetaData("IsDefault", SqlDbType.Bit),
                    new SqlMetaData("IsPrintOnReport", SqlDbType.Bit),
                    new SqlMetaData("Tags", SqlDbType.NVarChar,4000),
                    new SqlMetaData("IsVerified", SqlDbType.Bit),
                    new SqlMetaData("IncludeInWorkflow", SqlDbType.Bit),
                    new SqlMetaData("IsActRequired", SqlDbType.Bit),
                    new SqlMetaData("Source", SqlDbType.NVarChar,20),
                    new SqlMetaData("CheckAct", SqlDbType.Bit),
                    new SqlMetaData("SendToClient", SqlDbType.Bit)
                });
                record.SetString(0, item.Id);
                record.SetString(1, item.Name ?? "");
                //record.SetString(2, item.Key);
                //record.SetString(3, item.DocLibrary);
                //record.SetString(4, item.DocLibraryFolder);
                //record.SetBoolean(5, item.IsImage);
                //record.SetString(6, item.Author);
                record.SetBoolean(7, item.IsPrivate);
                record.SetString(8, item.DocUrl ?? "");
                record.SetString(9, item.Remarks ?? "");
                record.SetBoolean(10, item.IsDefault);
                record.SetBoolean(11, item.IsPrintOnReport);
                record.SetString(12, item.Tags ?? "");
                record.SetBoolean(13, item.IsVerified);
                record.SetBoolean(14, item.IncludeInWorkflow);
                record.SetBoolean(15, item.IsActRequired);
                record.SetString(16, item.Source ?? "");
                record.SetBoolean(17, item.CheckAct);
                record.SetBoolean(18, item.SendToClient);
                records.Add(record);
            }

            using (SqlCommand cmd = new SqlCommand("procSharePointToAzureBlobCleanup"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;

                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                cmd.CommandTimeout = 0;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@SystemType", systemType);
                cmd.Parameters.AddWithValue("@DocLibrary", docLibrary);
                cmd.Parameters.AddWithValue("@List", records).SqlDbType = SqlDbType.Structured;
                await cmd.ExecuteNonQueryAsync();
            }
        }


        public async Task SyncToDocumentTablesCopy(string author, List<SharePointSyncCopyDTO> sharePointSyncItems)
        {
            var records = new List<SqlDataRecord>();
            foreach (var item in sharePointSyncItems)
            {
                var record = new SqlDataRecord(new SqlMetaData[] {
                    new SqlMetaData("DocLibrary", SqlDbType.VarChar,255),
                    new SqlMetaData("Screen", SqlDbType.VarChar,255),
                    new SqlMetaData("DataKey", SqlDbType.VarChar,20),
                    new SqlMetaData("ParentId", SqlDbType.Int),
                    new SqlMetaData("RecordId", SqlDbType.Int),
                    new SqlMetaData("ScreenCode", SqlDbType.VarChar,20),
                    new SqlMetaData("SourceDriveItemId", SqlDbType.VarChar,255),
                    new SqlMetaData("NewDriveItemId", SqlDbType.VarChar,255),
                    new SqlMetaData("NewFileName", SqlDbType.VarChar,255),
                    
                });

                record.SetString(0, item.DocLibrary);
                record.SetString(1, item.Screen);
                record.SetString(2, item.DataKey ?? "");
                record.SetInt32(3, item.ParentId);
                record.SetInt32(4, item.RecordId);
                record.SetString(5, "");
                record.SetString(6, item.SourceDriveItemId ?? "");
                record.SetString(7, item.NewDriveItemId ?? "");
                record.SetString(8, item.NewFileName ?? "");
                
                records.Add(record);
            }

            using (SqlCommand cmd = new SqlCommand("procSharePointToDocCopy"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;

                cmd.Connection = _dbContext.Database.GetDbConnection() as SqlConnection;
                cmd.CommandTimeout = 0;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@Author", author);
                cmd.Parameters.AddWithValue("@List", records).SqlDbType = SqlDbType.Structured;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<string> GetDocLibraryLastSync(string docLibrary)
        {
            var optionSubkey = $"SharePointLastSync-{docLibrary}";
            var result = _dbContext.Database.SqlQuery<string>($"Select Top 1 OptionValue  as [Value] From tblPubOptions Where Optionkey='General' and OptionSubkey={optionSubkey}").FirstOrDefault();
            return result;
        }
    }
}


