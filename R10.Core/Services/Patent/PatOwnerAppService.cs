using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class PatOwnerAppService : IMultipleEntityService<PatOwnerApp>
    {
        private readonly ICountryApplicationService _countryApplicationService;
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly IApplicationDbContext _repository;
        private readonly ICountryApplicationRepository _countryAppRepository;
        ClaimsPrincipal _user;

        public PatOwnerAppService(
            IApplicationDbContext repository,
            ClaimsPrincipal user, 
            ICountryApplicationService countryApplicationService,
            ISystemSettings<PatSetting> settings,
            ICountryApplicationRepository countryAppRepository) 
        {
            _settings = settings;
            _repository = repository;
            _countryApplicationService = countryApplicationService;
            _user = user;
            _countryAppRepository = countryAppRepository;
        }

        public IQueryable<PatOwnerApp> QueryableListWithEntityFilter
        {
            get
            {
                if (_countryApplicationService.IsOwnerRequired)
                    return _countryApplicationService.QueryableChildList<PatOwnerApp>().Where(EntityFilter());

                return _countryApplicationService.QueryableChildList<PatOwnerApp>();
            }
        }

        private Expression<Func<PatOwnerApp, bool>> EntityFilter()
        {
            return o => _repository.CPiUserEntityFilters.Any(ef => ef.UserId == _user.GetUserIdentifier() && ef.EntityId == o.OwnerID);
        }

        public async Task<bool> Update(object key, string userName, IEnumerable<PatOwnerApp> updated, IEnumerable<PatOwnerApp> added, IEnumerable<PatOwnerApp> deleted)
        {
            int appId = (int)key;
            var settings = await _settings.GetSetting();
            var ownerLabel = settings.LabelOwner;

            foreach (var item in deleted)
            {
                item.Owner = null;
                await ValidateEntityFilter(item.OwnerID);
            }

            if (_countryApplicationService.IsOwnerRequired && deleted.Any())
            {
                if (await QueryableListWithEntityFilter.CountAsync(o => o.AppId == appId) <= deleted.Count())
                    Guard.Against.Null(null, ownerLabel);
            }

            foreach (var item in updated)
            {
                //Guard.Against.Null(item.Owner?.OwnerCode, ownerLabel);
                //item.OwnerID = item.Owner.OwnerID;
                item.Owner = null;
                await ValidateEntityFilter(item.OwnerID);
            }

            foreach (var item in added)
            {
                //Guard.Against.Null(item.Owner?.OwnerCode, ownerLabel);
                //item.OwnerID = item.Owner.OwnerID;
                item.AppId = appId;
                item.Owner = null;
                await ValidateEntityFilter(item.OwnerID);
            }

            if (added.Any())
            {
                var startIndex = await GetNextOrderOfEntry(appId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
            }

            var cascadeOwnersForEP = await _countryApplicationService.CountryApplications.AnyAsync(ca => ca.AppId == appId && ca.Country == "EP" && (ca.ApplicationStatus == "Granted" || ca.ApplicationStatus == "Issued"));
            if (cascadeOwnersForEP)
            {
                await _countryAppRepository.InsertEPDesignatedCountriesOwner(appId,userName);
            }
            await _countryApplicationService.UpdateChild(appId, userName, updated, added, deleted);
            return true;
        }

        private async Task<int> GetNextOrderOfEntry(int appId)
        {
            int? lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await _countryApplicationService.QueryableChildList<PatOwnerApp>().Where(o => o.AppId == appId).MaxAsync(o => o.OrderOfEntry);
            }
            catch { }

            return (lastOrderOfEntry ?? 0) + 1;
        }

        public async Task Reorder(int id, string userName, int newIndex)
        {
            var appOwner = await QueryableListWithEntityFilter.SingleOrDefaultAsync(o => o.OwnerAppID == id);
            Guard.Against.NoRecordPermission(appOwner != null);

            int appId = appOwner.AppId;
            int oldIndex = appOwner.OrderOfEntry ?? 0;
            await ValidateEntityFilter(appOwner.OwnerID);

            List<PatOwnerApp> appOwners = new List<PatOwnerApp>();
            if (oldIndex > newIndex)
            {
                appOwners = await _countryApplicationService.QueryableChildList<PatOwnerApp>().Where(w => w.AppId == appId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                appOwners.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                appOwners = await _countryApplicationService.QueryableChildList<PatOwnerApp>().Where(w => w.AppId == appId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                appOwners.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            appOwner.OrderOfEntry = newIndex;
            appOwners.Add(appOwner);
            await _countryApplicationService.UpdateChild(appId, userName, appOwners, new List<PatOwnerApp>(), new List<PatOwnerApp>());
        }


        private async Task ValidateEntityFilter(int entityId)
        {
            if (_countryApplicationService.IsOwnerRequired || _user.GetEntityFilterType() == Identity.CPiEntityType.Owner)
            {
                var settings = await _settings.GetSetting();
                var allowed = await _repository.CPiUserEntityFilters.AnyAsync(f => f.UserId == _user.GetUserIdentifier() && f.EntityId == entityId);
                Guard.Against.NoFieldPermission(allowed, settings.LabelOwner);
            }
        }
        
    }

   
}
