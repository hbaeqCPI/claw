using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using R10.Core.Entities;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Web.Services
{
    public class CPiMenuItemManager : ICPiMenuItemManager
    {
        private readonly ICPiDbContext _cpiDbContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _claimsPrincipal;
        private readonly IMemoryCache _cache;

        public CPiMenuItemManager(
            ICPiDbContext cpiDbContext,
            IAuthorizationService authorizationService, 
            ClaimsPrincipal claimsPrincipal,
            IMemoryCache cache)
        {
            _cpiDbContext = cpiDbContext;
            _authorizationService = authorizationService;
            _claimsPrincipal = claimsPrincipal;
            _cache = cache;
        }

        public IQueryable<CPiMenuItem> MenuItems => _cpiDbContext.GetRepository<CPiMenuItem>().QueryableList;

        public async Task<List<CPiMenuItem>> GetUserMenuItemsByParentIdAsync(string parentId)
        {
            List<CPiMenuItem> menuItems;
            var cacheKey = "MenuItems";
            if (!_cache.TryGetValue(cacheKey, out menuItems))
            {
                menuItems = await MenuItems.Where(m => m.IsEnabled).Include(p => p.Page).ToListAsync();

                _cache.Set(cacheKey, menuItems, new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
                    SlidingExpiration = TimeSpan.FromMinutes(20)
                }.SetSize(1));
            }

            return await GetAllowedMenuItems(menuItems.Where(m => m.IsEnabled && m.ParentId.ToLower() == parentId.ToLower()).OrderBy(m => m.SortOrder).ToList());
        }

        public async Task<List<CPiMenuItem>> GetMenuItemsByParentIdAsync(string parentId)
        {
            var menuItems = await MenuItems.Where(m => m.ParentId == parentId).OrderBy(m => m.SortOrder).Include(p => p.Page).ToListAsync();

            return await GetAllowedMenuItems(menuItems);
        }

        public async Task<CPiMenuItem> GetMenuItemByIdAsync(string id)
        {
            var menuItem = await MenuItems.Where(m => m.Id == id).FirstOrDefaultAsync();

            return menuItem;
        }

        private async Task<List<CPiMenuItem>> GetAllowedMenuItems(List<CPiMenuItem> menuItems)
        {
            List<CPiMenuItem> allowedItems = new List<CPiMenuItem>();
            foreach (CPiMenuItem item in menuItems)
            {
                //menu item policy overrides menu page policy
                //var policy = item.Page == null ? item.Policy : item.Page.Policy;
                var policy = item.Policy;
                if (!string.IsNullOrEmpty(policy) && (policy == "*" || (await _authorizationService.AuthorizeAsync(_claimsPrincipal, policy)).Succeeded))
                {
                    allowedItems.Add(item);
                }
            }

            return allowedItems;
        }

        public async Task<int> GetNextSortOrder(string parentId)
        {
            var menuItem = await MenuItems.Where(m => m.ParentId == parentId).OrderBy(m => m.SortOrder).LastOrDefaultAsync();

            return (menuItem == null ? 0 : menuItem.SortOrder + 1);
        }

        public async Task<CPiMenuItem> SaveMenuItem(CPiMenuItem menuItem)
        {
            var repository = _cpiDbContext.GetRepository<CPiMenuItem>();

            if (string.IsNullOrEmpty(menuItem.Id))
                repository.Add(menuItem);
            else
                repository.Update(menuItem);

            await _cpiDbContext.SaveChangesAsync();
            return menuItem;
        }

        public async Task RemoveMenuItem(CPiMenuItem menuItem)
        {
            var repository = _cpiDbContext.GetRepository<CPiMenuItem>();

            repository.Delete(menuItem);

            var menuItems = await MenuItems.Where(m => m.ParentId == menuItem.Id).ToListAsync();
            repository.Delete(menuItems);

            await _cpiDbContext.SaveChangesAsync();

            //reseed sort order
            await SortMenuItems(menuItem.ParentId);
        }

        public async Task RemoveMenuItems(List<CPiMenuItem> menuItems)
        {
            _cpiDbContext.GetRepository<CPiMenuItem>().Delete(menuItems);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task MoveMenuItem(CPiMenuItem menuItem, int newIndex, string newParentId)
        {
            int oldIndex = menuItem.SortOrder;
            string oldParentId = menuItem.ParentId;

            List<CPiMenuItem> menuItems = new List<CPiMenuItem>();

            if (oldParentId == newParentId)
            {
                if (oldIndex > newIndex)
                {
                    menuItems = await MenuItems.Where(m => m.ParentId == newParentId && m.SortOrder >= newIndex && m.SortOrder < oldIndex).ToListAsync();
                    menuItems.ForEach(m => m.SortOrder = m.SortOrder + 1);
                }
                else
                {
                    menuItems = await MenuItems.Where(m => m.ParentId == newParentId && m.SortOrder <= newIndex && m.SortOrder > oldIndex).ToListAsync();
                    menuItems.ForEach(m => m.SortOrder = m.SortOrder - 1);
                }
            }
            else
            {
                var oldMenuItems = await MenuItems.Where(m => m.ParentId == oldParentId && m.SortOrder > oldIndex && m.Id != menuItem.Id).ToListAsync();
                oldMenuItems.ForEach(m => m.SortOrder = m.SortOrder - 1);
                menuItems.AddRange(oldMenuItems);

                var newMenuItems = await MenuItems.Where(m => m.ParentId == newParentId && m.SortOrder >= newIndex).ToListAsync();
                newMenuItems.ForEach(m => m.SortOrder = m.SortOrder + 1);
                menuItems.AddRange(newMenuItems);
            }

            menuItem.ParentId = newParentId;
            menuItem.SortOrder = newIndex;
            menuItems.Add(menuItem);

            _cpiDbContext.GetRepository<CPiMenuItem>().Update(menuItems);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task SortMenuItems(string parentId)
        {
            int sortOrder = 0;

            var menuItems = await MenuItems.Where(m => m.ParentId == parentId).OrderBy(m => m.SortOrder).ToListAsync();
            menuItems.ForEach(m => m.SortOrder = sortOrder++);

            _cpiDbContext.GetRepository<CPiMenuItem>().Update(menuItems);
            await _cpiDbContext.SaveChangesAsync();

        }
    }
}
