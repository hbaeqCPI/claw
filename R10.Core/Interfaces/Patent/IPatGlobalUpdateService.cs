using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;

namespace R10.Core.Interfaces.Patent
{
    public interface IPatGlobalUpdateService 
    {
        Task<List<LookupDTO>> GetUpdateFields();

        Task<IList<GlobalUpdateLookupDTO>> GetFromData(string updateField);
        Task<IList<GlobalUpdateLookupDTO>> GetToData(string updateField);
        Task<(IList<PatGlobalUpdatePreviewDTO>,int)> GetPreviewList(PatGlobalUpdateCriteriaDTO searchCriteria, int page, int pageSize);
        Task <int> RunUpdate(PatGlobalUpdateCriteriaDTO searchCriteria);

        IQueryable<PatGlobalUpdateLog> PatGlobalUpdateLogs { get; }
    }
}
