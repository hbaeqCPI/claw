using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class PatInventorAppService: IMultipleEntityService<PatInventorApp>
    {
        private readonly ICountryApplicationService _countryApplicationService;
        private readonly IApplicationDbContext _repository;
        ClaimsPrincipal _user;

        public PatInventorAppService(
            IApplicationDbContext repository,
            ClaimsPrincipal user,
            ICountryApplicationService countryApplicationService)
        {
            _repository = repository;
            _countryApplicationService = countryApplicationService;
            _user = user;
        }

        public IQueryable<PatInventorApp> QueryableListWithEntityFilter
        {
            get
            {
                if (_countryApplicationService.IsInventorRequired)
                    return _countryApplicationService.QueryableChildList<PatInventorApp>().Where(EntityFilter());

                return _countryApplicationService.QueryableChildList<PatInventorApp>();
            }
        }

        private Expression<Func<PatInventorApp, bool>> EntityFilter()
        {
            return i => _repository.CPiUserEntityFilters.Any(ef => ef.UserId == _user.GetUserIdentifier() && ef.EntityId == i.InventorID);
        }

        public  async Task<bool> Update(object key, string userName, IEnumerable<PatInventorApp> updated, IEnumerable<PatInventorApp> added, IEnumerable<PatInventorApp> deleted)
        {
            int appId = (int)key;

            foreach (var item in deleted)
            {
                item.InventorAppInventor = null;
                await ValidateEntityFilter(item.InventorID);
            }

            if (_countryApplicationService.IsInventorRequired && deleted.Any())
            {
                if (await QueryableListWithEntityFilter.CountAsync(o => o.AppId == appId) <= deleted.Count())
                    Guard.Against.Null(null, "Inventor");
            }

            foreach (var item in updated)
            {
                Guard.Against.Null(item.InventorAppInventor?.Inventor, "Inventor");

                item.InventorID = item.InventorAppInventor.InventorID;
                item.InventorAppInventor = null;
                await ValidateEntityFilter(item.InventorID);
            }

            foreach (var item in added)
            {
                Guard.Against.Null(item.InventorAppInventor?.Inventor, "Inventor");

                item.AppId = appId;
                item.InventorID = item.InventorAppInventor.InventorID;
                item.InventorAppInventor = null;
                await ValidateEntityFilter(item.InventorID);
            }

            if (added.Any())
            {
                var startIndex = await GetNextOrderOfEntry(appId);
                foreach (var item in added.AsEnumerable().Reverse())
                {
                    item.OrderOfEntry = startIndex++;
                }
            }
            await _countryApplicationService.UpdateChild(appId, userName, updated, added, deleted);
            return true;
        }

        private async Task<int> GetNextOrderOfEntry(int appId)
        {
            int lastOrderOfEntry = 0;
            try
            {
                lastOrderOfEntry = await _countryApplicationService.QueryableChildList<PatInventorApp>().Where(o => o.AppId == appId).MaxAsync(o => o.OrderOfEntry);
            }
            catch { }

            return lastOrderOfEntry + 1;
        }

        public async Task Reorder(int id, string userName, int newIndex)
        {
            var appInventor = await QueryableListWithEntityFilter.SingleOrDefaultAsync(o => o.InventorIDApp == id);
            Guard.Against.NoRecordPermission(appInventor != null);

            int appId = appInventor.AppId;
            int oldIndex = appInventor.OrderOfEntry;

            await ValidateEntityFilter(appInventor.InventorID);

            List<PatInventorApp> appInventors = new List<PatInventorApp>();
            if (oldIndex > newIndex)
            {
                appInventors = await _countryApplicationService.QueryableChildList<PatInventorApp>().Where(w => w.AppId == appId && w.OrderOfEntry >= newIndex && w.OrderOfEntry < oldIndex).ToListAsync();
                appInventors.ForEach(m => m.OrderOfEntry = m.OrderOfEntry + 1);
            }
            else
            {
                appInventors = await _countryApplicationService.QueryableChildList<PatInventorApp>().Where(w => w.AppId == appId && w.OrderOfEntry <= newIndex && w.OrderOfEntry > oldIndex).ToListAsync();
                appInventors.ForEach(m => m.OrderOfEntry = m.OrderOfEntry - 1);
            }
            appInventor.OrderOfEntry = newIndex;
            appInventors.Add(appInventor);
            await _countryApplicationService.UpdateChild(appId, userName, appInventors, new List<PatInventorApp>(), new List<PatInventorApp>());
        }

       

        private async Task ValidateEntityFilter(int entityId)
        {
            if (_countryApplicationService.IsInventorRequired) {
                var allowed = await _repository.CPiUserEntityFilters.AnyAsync(f => f.UserId == _user.GetUserIdentifier() && f.EntityId == entityId);
                Guard.Against.NoFieldPermission(allowed, "Inventor");
            }
            
        }
    }
}
