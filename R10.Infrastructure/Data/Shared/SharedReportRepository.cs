using Microsoft.EntityFrameworkCore;
using R10.Core.Interfaces.Shared;
using R10.Core.Queries.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R10.Core.DTOs;
using System.Reflection;

namespace R10.Infrastructure.Data
{
    public class SharedReportRepository : ISharedReportRepository
    {
        protected readonly ApplicationDbContext _dbContext;
        public SharedReportRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<SharedReportActionTypeLookupDTO> CombinedActionTypes => _dbContext.SharedReportActionTypeView.AsNoTracking();

        public IQueryable<SharedReportActionDueLookupDTO> CombinedActionDues => _dbContext.SharedReportActionDueView.AsNoTracking();

        public IQueryable<SharedReportStatusLookupDTO> CombinedStatuses => _dbContext.SharedReportStatusView.AsNoTracking();

        public IQueryable<SharedReportCountryLookupDTO> CombinedCountries => _dbContext.SharedReportCountryView.AsNoTracking();

        public IQueryable<SharedReportAreaLookupDTO> CombinedAreas => _dbContext.SharedReportAreaView.AsNoTracking();

        public IQueryable<SharedReportClientLookupDTO> CombinedClients => _dbContext.SharedReportClientView.AsNoTracking();

        public IQueryable<SharedReportOwnerLookupDTO> CombinedOwners => _dbContext.SharedReportOwnerView.AsNoTracking();

        public IQueryable<SharedReportAttorneyLookupDTO> CombinedAttorneys => _dbContext.SharedReportAttorneyView.AsNoTracking();

        public IQueryable<SharedReportCaseTypeLookupDTO> CombinedCaseTypes => _dbContext.SharedReportCaseTypeView.AsNoTracking();

        public IQueryable<SharedReportIndicatorLookupDTO> CombinedIndicators => _dbContext.SharedReportIndicatorView.AsNoTracking();

        public IQueryable<SharedReportResponsibleOfficeLookupDTO> CombinedResponsibleOffices => _dbContext.SharedReportResponsibleOfficeView.AsNoTracking();

        public IQueryable<SharedReportCostTypeLookupDTO> CombinedCostTypes => _dbContext.SharedReportCostTypeView.AsNoTracking();

        public IQueryable<SharedReportCaseNumberLookupDTO> CombinedCaseNumbers => _dbContext.SharedReportCaseNumberView.AsNoTracking();

        public IQueryable<SharedReportAgentLookupDTO> CombinedAgents => _dbContext.SharedReportAgentView.AsNoTracking();
    }
}
