using R10.Core.Queries.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces.Shared
{
    public interface ISharedReportViewModelService
    {
        //QuickDocketSearchCriteriaViewModel GetSearchCriteria(CPiUser user, List<string> systems, QuickDocketDefaultSettingsViewModel defaultSettings);

        //List<QuickDocketViewModel> GetQuickDocket(QuickDocketSearchCriteriaViewModel criteria);

        //List<QuickDocketViewModel> GetQuickDocket(List<QueryFilterViewModel> filters);

        //List<QuickDocketSchedulerViewModel> GetQuickDocketScheduler(QuickDocketSearchCriteriaViewModel criteria);

        //Task<List<QuickDocketLookUpDTO>> GetPickListData(string property, string text, string systemType);

        IQueryable<SharedReportActionTypeLookupDTO> GetCombinedActionTypes { get; }

        IQueryable<SharedReportActionDueLookupDTO> GetCombinedActionDues { get; }

        IQueryable<SharedReportIndicatorLookupDTO> GetCombinedIndicators { get; }

        IQueryable<SharedReportCountryLookupDTO> GetCombinedCountries { get; }

        IQueryable<SharedReportAreaLookupDTO> GetCombinedAreas { get; }

        IQueryable<SharedReportClientLookupDTO> GetCombinedClients { get; }

        IQueryable<SharedReportOwnerLookupDTO> GetCombinedOwners { get; }

        IQueryable<SharedReportAttorneyLookupDTO> GetCombinedAttorneys { get; }

        IQueryable<SharedReportStatusLookupDTO> GetCombinedStatuses { get; }

        IQueryable<SharedReportCaseTypeLookupDTO> GetCombinedCaseTypes { get; }

        IQueryable<SharedReportResponsibleOfficeLookupDTO> GetCombinedResponsibleOffices { get; }

        IQueryable<SharedReportCaseNumberLookupDTO> GetCombinedCaseNumbers { get; }

        IQueryable<SharedReportCostTypeLookupDTO> GetCombinedCostTypes { get; }

        IQueryable<SharedReportAgentLookupDTO> GetCombinedAgents { get; }
    }
}
