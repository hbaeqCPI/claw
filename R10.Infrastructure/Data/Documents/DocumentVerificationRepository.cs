using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Globalization;

namespace R10.Infrastructure.Data
{

    public class DocumentVerificationRepository : IDocumentVerificationRepository
    {
        protected readonly ApplicationDbContext _dbContext;
        public DocumentVerificationRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<DocumentVerificationNewDTO>> GetDocVerificationNewDocs(DocumentVerificationSearchCriteriaDTO criteria)
        {
            var sql = SqlHelper.BuildSql("procSysDocumentVerificationNewView", criteria);
            var parameters = SqlHelper.BuildSqlParameters(criteria);

            var result = await _dbContext.DocumentVerificationNewDTO.FromSqlRaw(sql, parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<DocumentVerificationDTO>> GetDocVerificationDocuments(DocumentVerificationSearchCriteriaDTO criteria)
        {
            var sql = SqlHelper.BuildSql("procSysDocumentVerification", criteria);
            var parameters = SqlHelper.BuildSqlParameters(criteria);

            var result = await _dbContext.DocumentVerificationDTO.FromSqlRaw(sql, parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<DocumentVerificationActionDTO>> GetDocVerificationActions(DocumentVerificationSearchCriteriaDTO criteria)
        {
            var sql = SqlHelper.BuildSql("procSysDocumentVerification", criteria);
            var parameters = SqlHelper.BuildSqlParameters(criteria);

            var result = await _dbContext.DocumentVerificationActionDTO.FromSqlRaw(sql, parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        #region Export
        public async Task<List<DocumentVerificationNewDTO>> GetDocVerificationNewDocExport(string ids)
        {
            var sql = "exec procWebSharedDocVerificationNewDocPrintScreen @IDs, @ReportFormat, @UserID, @SettingLanguageCode";
            var parameters = new[]
            {
                new SqlParameter("@IDs", ids ?? (object)DBNull.Value),
                new SqlParameter("@ReportFormat", DBNull.Value),
                new SqlParameter("@UserID", DBNull.Value),
                new SqlParameter("@SettingLanguageCode", "")
            };

            var result = await _dbContext.DocumentVerificationNewDTO.FromSqlRaw(sql, parameters).AsNoTracking().ToListAsync();
            return result;
        }
        
        public async Task<List<DocumentVerificationCommunicationDTO>> GetDocVerificationCommunications(DocumentVerificationSearchCriteriaDTO criteria)
        {
            var sql = SqlHelper.BuildSql("procSysDocumentVerification", criteria);
            var parameters = SqlHelper.BuildSqlParameters(criteria);

            var result = await _dbContext.DocumentVerificationCommunicationDTO.FromSqlRaw(sql, parameters.ToArray()).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<DocumentVerificationDTO>> GetDocVerificationDocExport(string ids)
        {
            var sql = "exec procWebSharedDocVerificationDocPrintScreen @IDs, @ReportFormat, @UserID, @SettingLanguageCode";
            var parameters = new[]
            {
                new SqlParameter("@IDs", ids ?? (object)DBNull.Value),
                new SqlParameter("@ReportFormat", DBNull.Value),
                new SqlParameter("@UserID", DBNull.Value),
                new SqlParameter("@SettingLanguageCode", "")
            };

            var result = await _dbContext.DocumentVerificationDTO.FromSqlRaw(sql, parameters).AsNoTracking().ToListAsync();
            return result;
        }

        public async Task<List<DocumentVerificationActionDTO>> GetDocVerificationActionDocExport(string ids)
        {
            var sql = "exec procWebSharedDocVerificationActionDocPrintScreen @IDs, @ReportFormat, @UserID, @SettingLanguageCode";
            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            var parameters = new[]
            {
                new SqlParameter("@IDs", ids ?? (object)DBNull.Value),
                new SqlParameter("@ReportFormat", DBNull.Value),
                new SqlParameter("@UserID", DBNull.Value),
                new SqlParameter("@SettingLanguageCode", cultureInfo.Name)
            };

            var result = await _dbContext.DocumentVerificationActionDTO.FromSqlRaw(sql, parameters).AsNoTracking().ToListAsync();
            return result;
        }
        public async Task<List<DocumentVerificationCommunicationDTO>> GetDocVerificationCommunicationsDocExport(string ids)
        {
            var sql = "exec procWebSharedDocVerificationCommDocPrintScreen @IDs, @ReportFormat, @UserID, @SettingLanguageCode";
            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            var parameters = new[]
            {
                new SqlParameter("@IDs", ids ?? (object)DBNull.Value),
                new SqlParameter("@ReportFormat", DBNull.Value),
                new SqlParameter("@UserID", DBNull.Value),
                new SqlParameter("@SettingLanguageCode", cultureInfo.Name)
            };

            var result = await _dbContext.DocumentVerificationCommunicationDTO.FromSqlRaw(sql, parameters).AsNoTracking().ToListAsync();
            return result;
        }
        #endregion
    }
}
