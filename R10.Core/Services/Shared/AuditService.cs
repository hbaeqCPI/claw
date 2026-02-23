using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Interfaces;

namespace R10.Core.Services.Shared
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _auditRepository;
        public AuditService(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }
        public async Task<AuditLogPagedResult> GetAuditLogHeader(AuditSearchDTO searchCriteria)
        {
            var result = await _auditRepository.GetAuditLogHeader(searchCriteria);
            return result;
        }

        public async  Task<List<AuditKeyDTO>> GetAuditKey(string systemType, int audTrailId)
        {
            var result = await _auditRepository.GetAuditKey(systemType,audTrailId);
            return result;
        }

        public async Task<List<AuditDetailDTO>> GetAuditLogDetail(string systemType, int audTrailId)
        {
            var result = await _auditRepository.GetAuditLogDetail(systemType, audTrailId);
            return result;
        }

        public async Task<List<LookupDTO>> GetAuditLookup(string systemType, string dataType)
        {
            return await _auditRepository.GetAuditLookup(systemType, dataType);
        }

        public async Task<List<AuditReportDTO>> GetAuditReport(AuditSearchDTO searchCriteria)
        {
            var result = await _auditRepository.GetAuditReport(searchCriteria);
            return result;
        }

        public async Task<List<LookupDTO>> GetAvailableSystemTypes()
        {
            return await _auditRepository.GetAvailableSystemTypes();
        }
    }
}
