using R10.Core.Queries.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R10.Core.Interfaces.Shared
{
    public interface ISharedReportRepository
    {
        IQueryable<SharedReportActionTypeLookupDTO> CombinedActionTypes { get; }

        IQueryable<SharedReportActionDueLookupDTO> CombinedActionDues { get; }

        IQueryable<SharedReportStatusLookupDTO> CombinedStatuses { get; }

        IQueryable<SharedReportCountryLookupDTO> CombinedCountries { get; }

        IQueryable<SharedReportAreaLookupDTO> CombinedAreas { get; }

        IQueryable<SharedReportClientLookupDTO> CombinedClients { get; }

        IQueryable<SharedReportOwnerLookupDTO> CombinedOwners { get; }

        IQueryable<SharedReportAttorneyLookupDTO> CombinedAttorneys { get; }

        IQueryable<SharedReportCaseTypeLookupDTO> CombinedCaseTypes { get; } 

        IQueryable<SharedReportIndicatorLookupDTO> CombinedIndicators { get; }

        IQueryable<SharedReportResponsibleOfficeLookupDTO> CombinedResponsibleOffices { get; }

        IQueryable<SharedReportCostTypeLookupDTO> CombinedCostTypes { get; }

        IQueryable<SharedReportCaseNumberLookupDTO> CombinedCaseNumbers { get; }

        IQueryable<SharedReportAgentLookupDTO> CombinedAgents { get; }
    }
}
