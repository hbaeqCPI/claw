using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using R10.Core.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data
{
    public class EFSRepository : IEFSRepository
    {
        protected readonly ApplicationDbContext _dbContext;
        public EFSRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<EFSFormDTO>> GetForms(string systemType, string docType, string country, int recId)
        {
            var list = await _dbContext.EFSFormDTO.FromSqlInterpolated($"procSysEFSPrint @Action = 0, @SystemType = {systemType}, @Country = {country}, @DocType = {docType},@RecId = {recId}").AsNoTracking().ToListAsync();
            return list;

        }

        public async Task<List<LookupDTO>> GetSignatories(string systemType, int recId)
        {
            var list = await _dbContext.LookupDTO.FromSqlInterpolated($"procSysEFSPrint @Action = 2, @SystemType = {systemType},@RecId = {recId}").AsNoTracking().ToListAsync();
            return list;
        }

        public async Task<DataSet> GetPrintData(string docType, string subType, string signatory, int recId, int pageNo, int noOfPages, string userId)
        {
            var connectionString = _dbContext.Database.GetDbConnection().ConnectionString;
            var connection = new SqlConnection(connectionString);
            using (var da = new SqlDataAdapter("procSysEFSPrint",connection ))
            {
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.CommandTimeout = 0;
                da.SelectCommand.Connection = connection;
                if (da.SelectCommand.Connection?.State == ConnectionState.Closed)
                    da.SelectCommand.Connection.Open();

                da.SelectCommand.Parameters.AddWithValue("@Action", 1);
                da.SelectCommand.Parameters.AddWithValue("@DocType", docType);
                da.SelectCommand.Parameters.AddWithValue("@SubType", subType);
                da.SelectCommand.Parameters.AddWithValue("@Signatory", signatory);
                da.SelectCommand.Parameters.AddWithValue("@RecId", recId);
                da.SelectCommand.Parameters.AddWithValue("@PageNo", pageNo);
                da.SelectCommand.Parameters.AddWithValue("@NoOfPages", noOfPages);
                da.SelectCommand.Parameters.AddWithValue("@UserId", userId);

                var ds = new DataSet();
                da.Fill(ds);

                if (connection.State == ConnectionState.Open)
                    connection.Close();

                return ds;
            }


        }

        public async Task LogEFSDoc(string systemType, int efsDocId, string dataKey, int dataKeyValue, string efsFileName, string genBy, int pageNo, int pageCount,string? itemId, string? signatory)
        {
            var connectionString = _dbContext.Database.GetDbConnection().ConnectionString;
            var connection = new SqlConnection(connectionString);
            using (var cmd = new SqlCommand("procSysEFSLog", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                cmd.Connection = connection; ;
                if (cmd.Connection?.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                cmd.Parameters.AddWithValue("@SystemType", systemType);
                cmd.Parameters.AddWithValue("@EFSDocId", efsDocId);
                cmd.Parameters.AddWithValue("@DataKey", dataKey);
                cmd.Parameters.AddWithValue("@DataKeyValue", dataKeyValue);
                cmd.Parameters.AddWithValue("@EFSFileName", efsFileName);
                cmd.Parameters.AddWithValue("@GenBy", genBy);
                cmd.Parameters.AddWithValue("@PageNo", pageNo);
                cmd.Parameters.AddWithValue("@PageCount", pageCount);
                cmd.Parameters.AddWithValue("@ItemId", itemId);
                cmd.Parameters.AddWithValue("@Signatory", signatory);
                cmd.ExecuteNonQuery();

                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public async Task UpdateEFS(IList<EFS> updated, string userName)
        {
            if (updated.Any()) {
                var dbSet = _dbContext.EFS;
                foreach (var item in updated)
                {
                    //just update the field that we allow
                    var efs = await _dbContext.EFS.FirstOrDefaultAsync(e => e.EfsDocId == item.EfsDocId);
                    if (efs != null) {
                        efs.SignatureQESetupId = item.SignatureQESetupId;
                        efs.UpdatedBy = userName;
                        efs.LastUpdate = DateTime.Now;
                        dbSet.Update(efs);
                    }
                }
                await _dbContext.SaveChangesAsync();
            }
        }

        public IQueryable<EFS> QueryableList => _dbContext.EFS.AsNoTracking();
     
    }
}
