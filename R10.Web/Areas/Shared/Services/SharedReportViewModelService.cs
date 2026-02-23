using AutoMapper;
using R10.Core.Queries.Shared;
using R10.Web.Interfaces.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Web.Interfaces;
using R10.Core.Interfaces.Shared;
using R10.Core.Interfaces;

namespace R10.Web.Areas.Shared.Services
{
    public class SharedReportViewModelService : ISharedReportViewModelService
    {
        private readonly ISharedReportRepository _sharedReportRepository;

        private readonly IMapper _mapper;

        private const int QuickDocket_SettingId = 2;

        public SharedReportViewModelService(
                                ISharedReportRepository sharedReportRepository,
                                IMapper mapper)
        {
            _sharedReportRepository = sharedReportRepository;
            _mapper = mapper;
        }

        public IQueryable<SharedReportActionTypeLookupDTO> GetCombinedActionTypes
        {
            get { return _sharedReportRepository.CombinedActionTypes; }
        }

        public IQueryable<SharedReportActionDueLookupDTO> GetCombinedActionDues
        {
            get { return _sharedReportRepository.CombinedActionDues; }
        }

        public IQueryable<SharedReportIndicatorLookupDTO> GetCombinedIndicators
        {
            get { return _sharedReportRepository.CombinedIndicators; }
        }

        public IQueryable<SharedReportCountryLookupDTO> GetCombinedCountries
        {
            get { return _sharedReportRepository.CombinedCountries; }
        }

        public IQueryable<SharedReportAreaLookupDTO> GetCombinedAreas
        {
            get { return _sharedReportRepository.CombinedAreas; }
        }

        public IQueryable<SharedReportClientLookupDTO> GetCombinedClients
        {
            get { return _sharedReportRepository.CombinedClients; }
        }

        public IQueryable<SharedReportOwnerLookupDTO> GetCombinedOwners
        {
            get { return _sharedReportRepository.CombinedOwners; }
        }

        public IQueryable<SharedReportAttorneyLookupDTO> GetCombinedAttorneys
        {
            get { return _sharedReportRepository.CombinedAttorneys; }
        }

        public IQueryable<SharedReportStatusLookupDTO> GetCombinedStatuses
        {
            get { return _sharedReportRepository.CombinedStatuses; }
        }

        public IQueryable<SharedReportCaseTypeLookupDTO> GetCombinedCaseTypes
        {
            get { return _sharedReportRepository.CombinedCaseTypes; }
        }

        public IQueryable<SharedReportResponsibleOfficeLookupDTO> GetCombinedResponsibleOffices
        {
            get { return _sharedReportRepository.CombinedResponsibleOffices; }
        }

        public IQueryable<SharedReportCaseNumberLookupDTO> GetCombinedCaseNumbers
        {
            get { return _sharedReportRepository.CombinedCaseNumbers; }
        }

        public IQueryable<SharedReportCostTypeLookupDTO> GetCombinedCostTypes
        {
            get { return _sharedReportRepository.CombinedCostTypes; }
        }

        public IQueryable<SharedReportAgentLookupDTO> GetCombinedAgents
        {
            get { return _sharedReportRepository.CombinedAgents; }
        }
    }
}
