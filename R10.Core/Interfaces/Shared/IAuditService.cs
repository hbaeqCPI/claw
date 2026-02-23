using R10.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IAuditService
    {
        Task<AuditLogPagedResult> GetAuditLogHeader(AuditSearchDTO searchCriteria);

        Task<List<AuditKeyDTO>> GetAuditKey(string systemType, int audTrailId);

        Task<List<AuditDetailDTO>> GetAuditLogDetail(string systemType, int audTrailId);

        Task<List<LookupDTO>> GetAuditLookup(string systemType, string dataType);

        Task<List<AuditReportDTO>> GetAuditReport(AuditSearchDTO searchCriteria);

        Task<List<LookupDTO>> GetAvailableSystemTypes();
    }
}
