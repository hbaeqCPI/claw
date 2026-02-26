using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;

namespace R10.Web.Areas.Admin.Services
{
    public class ActionIndicatorService : IActionIndicatorService
    {
        private readonly ICPiDbContext _cpiDbContext;

        public ActionIndicatorService(ICPiDbContext cpiDbContext)
        {
            _cpiDbContext = cpiDbContext;
        }

        public async Task<List<string>> GetActionIndicators()
        {
            var patIndicators = await _cpiDbContext.GetReadOnlyRepositoryAsync<PatIndicator>().QueryableList.Select(i => i.Indicator ?? "").ToListAsync();
            var tmkIndicators = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmkIndicator>().QueryableList.Select(i => i.Indicator ?? "").ToListAsync();

            return patIndicators.Union(tmkIndicators).ToList().Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }

    public interface IActionIndicatorService
    {
        Task<List<string>> GetActionIndicators();
    }
}
