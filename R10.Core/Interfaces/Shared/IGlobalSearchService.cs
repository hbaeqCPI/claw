using R10.Core.DTOs;
using R10.Core.Entities.GlobalSearch;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using R10.Core.Identity;

namespace R10.Core.Interfaces
{
    public interface IGlobalSearchService
    {
        IQueryable<GSSystem> GSSystems { get; }
        IQueryable<GSScreen> GSScreens { get; }
        IQueryable<GSField> GSFields { get; }
        IQueryable<CPiSystem> CPiSystems { get; }

        Task<IEnumerable<GSSearchDTO>> RunGlobalSearchDB(string userName, bool hasRespOfficeOn, bool hasEntityFilterOn, GSParamDTO parameters);
        Task<IEnumerable<GSSearchDocDTO>> RunGlobalSearchDoc(string userName, bool hasRespOfficeOn, bool hasEntityFilterOn, List<GSDocParamDTO> parameters, IEnumerable<GSMoreFilter> screenMoreFilters);
        Task<IEnumerable<GSDownloadDTO>> GetDownloadDocInfo(List<GSDownloadParamDTO> parameters);

        Task LogGlobalSearch(string userName, string searchCriteria);
    }
}
