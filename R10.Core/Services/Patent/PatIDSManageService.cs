using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using static System.Net.Mime.MediaTypeNames;

namespace R10.Core.Services
{
    public class PatIDSManageService : IPatIDSManageService
    {
        private readonly IInventionService _inventionService;
        private readonly ICountryApplicationRepository _countryAppRepository;
        private readonly ICountryApplicationService _applicationService;
        private readonly IApplicationDbContext _repository;

        private readonly ClaimsPrincipal _user;

        public PatIDSManageService(IApplicationDbContext repository,
            ICountryApplicationRepository countryAppRepository, IInventionService inventionService,
            ICountryApplicationService applicationService,  ClaimsPrincipal user)
        {
            _repository = repository;
            _countryAppRepository = countryAppRepository;
            _inventionService = inventionService;
            _applicationService = applicationService;
            _user = user;
        }

        public IQueryable<PatIDSManageDTO> IDSManageCases
        {
            get
            {
                var idsManageCases = _repository.Set<PatIDSManageDTO>().AsNoTracking();
                if (_user.HasRespOfficeFilter(SystemType.IDS))
                    idsManageCases = idsManageCases.Where(RespOfficeFilter());

                if (_user.HasEntityFilter())
                    idsManageCases = idsManageCases.Where(EntityFilter());

                if (_user.RestrictExportControl())
                    idsManageCases = idsManageCases.Where(ca => !(ca.ExportControl ?? false));
                if (!_user.CanAccessPatTradeSecret())
                    idsManageCases = idsManageCases.Where(ca => _repository.Inventions.AsNoTracking().Any(i => !(i.IsTradeSecret ?? false) && i.InvId == ca.InvId));
                return idsManageCases;
            }
        }

        public async Task IDSUpdateFilDate(int appId, string filDateType, string recordType, DateTime? filDate, DateTime? specificFilDate, bool consideredByExaminer)
        {
            await _countryAppRepository.IDSUpdateFilDate(appId, filDateType, recordType, _user.GetUserName(), filDate, specificFilDate, consideredByExaminer);
        }

        public async Task UpdateConsideredByExaminer(int appId, string filDateType, string recordType, DateTime? filDateFrom, DateTime? filDateTo, DateTime? specificFilDate) {
            await _countryAppRepository.UpdateConsideredByExaminer(appId, filDateType, recordType, filDateFrom, filDateTo, specificFilDate, _user.GetUserName());
        }

        protected Expression<Func<PatIDSManageDTO, bool>> RespOfficeFilter()
        {
            return a => _repository.CPiUserSystemRoles.AsNoTracking().Any(r =>
                r.UserId == _user.GetUserIdentifier() && r.SystemId == SystemType.IDS &&
                a.RespOffice == r.RespOffice);
        }

        protected Expression<Func<PatIDSManageDTO, bool>> EntityFilter()
        {
            var userIdentifier = _user.GetUserIdentifier();
            var userEntityFilters = _repository.CPiUserEntityFilters.AsNoTracking();

            switch (_user.GetEntityFilterType())
            {
                case CPiEntityType.Client:
                    return a => _inventionService.QueryableList.Any(i => i.InvId == a.InvId);

                case CPiEntityType.Agent:
                    return a => _applicationService.CountryApplications.Any(ca=> ca.AppId==a.AppId);

                case CPiEntityType.Owner:
                    return a => _applicationService.CountryApplications.Any(ca => ca.AppId == a.AppId);

                case CPiEntityType.Attorney:
                    return a => _inventionService.QueryableList.Any(i => i.InvId == a.InvId);

                case CPiEntityType.Inventor:
                    return a => _applicationService.CountryApplications.Any(ca => ca.AppId == a.AppId);
            }

            return null;
        }


        public IQueryable<PatKeyword> InventionKeywords =>
            _repository.PatKeywords.AsNoTracking().Where(k => IDSManageCases.Any(a => a.InvId == k.Invention.InvId));

        public IQueryable<PatInventorApp> PatInventorsApps => _repository.PatInventorsApp.AsNoTracking(); //no need to filter
    }
}
