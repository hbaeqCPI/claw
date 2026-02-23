using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;

namespace R10.Core.Interfaces
{
    public interface IPatCECountrySetupService
    {
        IQueryable<PatCECountrySetup> PatCECountrySetups { get; }
        IQueryable<PatCECountryCost> PatCECountryCosts { get; }
        IQueryable<PatCECountryCostChild> PatCECountryCostChildren { get; }
        IQueryable<PatCECountryCostSub> PatCECountryCostSubs { get; }
        IQueryable<PatCaseType> PatCaseTypes { get; }


        Task AddCECountrySetup(PatCECountrySetup countrySetup);
        Task UpdateCECountrySetup(PatCECountrySetup countrySetup);
        Task DeleteCECountrySetup(PatCECountrySetup countrySetup);
        Task CopyCECountrySetup(int oldCECountryId, int newCECountryId, string userName, bool copyCosts);

        Task UpdateChild(int parentId, string userName, IEnumerable<PatCECountryCost> updated, IEnumerable<PatCECountryCost> added, IEnumerable<PatCECountryCost> deleted);        
        Task ReorderCECountryCost(int id, string userName, int newIndex);

        Task UpdateCostChild(int parentId, string userName, IEnumerable<PatCECountryCostChild> updated, IEnumerable<PatCECountryCostChild> added, IEnumerable<PatCECountryCostChild> deleted);
        Task ReorderCECostChild(int id, string userName, int newIndex);

        Task UpdateCostSub(int parentId, string userName, IEnumerable<PatCECountryCostSub> updated, IEnumerable<PatCECountryCostSub> added, IEnumerable<PatCECountryCostSub> deleted);
        Task ReorderCECostSub(int id, string userName, int newIndex);

    }
}
