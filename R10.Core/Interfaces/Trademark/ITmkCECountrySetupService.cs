using R10.Core.Entities;
using R10.Core.Entities.Trademark;

namespace R10.Core.Interfaces
{
    public interface ITmkCECountrySetupService
    {
        IQueryable<TmkCECountrySetup> TmkCECountrySetups { get; }
        IQueryable<TmkCECountryCost> TmkCECountryCosts { get; }
        IQueryable<TmkCECountryCostChild> TmkCECountryCostChildren { get; }
        IQueryable<TmkCECountryCostSub> TmkCECountryCostSubs { get; }
        IQueryable<TmkCaseType> TmkCaseTypes { get; }

        Task AddCECountrySetup(TmkCECountrySetup countrySetup);
        Task UpdateCECountrySetup(TmkCECountrySetup countrySetup);
        Task DeleteCECountrySetup(TmkCECountrySetup countrySetup);

        Task CopyCECountrySetup(int oldCECountryId, int newCECountryId, string userName, bool copyCosts);

        Task UpdateChild(int parentId, string userName, IEnumerable<TmkCECountryCost> updated, IEnumerable<TmkCECountryCost> added, IEnumerable<TmkCECountryCost> deleted);
        Task ReorderCECountryCost(int id, string userName, int newIndex);

        Task UpdateCostChild(int parentId, string userName, IEnumerable<TmkCECountryCostChild> updated, IEnumerable<TmkCECountryCostChild> added, IEnumerable<TmkCECountryCostChild> deleted);
        Task ReorderCECostChild(int id, string userName, int newIndex);

        Task UpdateCostSub(int parentId, string userName, IEnumerable<TmkCECountryCostSub> updated, IEnumerable<TmkCECountryCostSub> added, IEnumerable<TmkCECountryCostSub> deleted);
        Task ReorderCECostSub(int id, string userName, int newIndex);
    }
}
