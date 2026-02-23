using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities.GeneralMatter;

namespace R10.Core.Interfaces.GeneralMatter
{
    public interface IGMGlobalUpdateService 
    {
        Task<List<LookupDTO>> GetUpdateFields();

        Task<IList<GlobalUpdateLookupDTO>> GetFromData(string updateField);
        Task<IList<GlobalUpdateLookupDTO>> GetToData(string updateField);
        Task<(IList<GMGlobalUpdatePreviewDTO>,int)> GetPreviewList(GMGlobalUpdateCriteriaDTO searchCriteria, int page, int pageSize);
        Task <int> RunUpdate(GMGlobalUpdateCriteriaDTO searchCriteria);

        IQueryable<GMGlobalUpdateLog> GMGlobalUpdateLogs { get; }
    }
}
