using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities.Trademark;

namespace R10.Core.Interfaces.Trademark
{
    public interface ITmkGlobalUpdateRepository
    {
        Task<IList<GlobalUpdateLookupDTO>> GetFromData(string updateField);
        Task<IList<GlobalUpdateLookupDTO>> GetToData(string updateField);
        Task<(IList<TmkGlobalUpdatePreviewDTO>, int)> GetPreviewList(TmkGlobalUpdateCriteriaDTO searchCriteria, int page, int pageSize);
        Task <int> RunUpdate(TmkGlobalUpdateCriteriaDTO searchCriteria);

        IQueryable<TmkGlobalUpdateLog> TmkGlobalUpdateLogs { get; }

    }
}
