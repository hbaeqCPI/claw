using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LawPortal.Core.Entities;
using LawPortal.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Web.Services
{
    public class CPiMenuPageManager : ICPiMenuPageManager
    {
        private readonly ICPiDbContext _cpiDbContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _claimsPrincipal;

        public CPiMenuPageManager(
            ICPiDbContext cpiDbContext,
            IAuthorizationService authorizationService, 
            ClaimsPrincipal claimsPrincipal)
        {
            _cpiDbContext = cpiDbContext;
            _authorizationService = authorizationService;
            _claimsPrincipal = claimsPrincipal;
        }

        public IQueryable<CPiMenuPage> MenuPages => _cpiDbContext.GetRepository<CPiMenuPage>().QueryableList;

        public async Task<List<CPiMenuPage>> GetAllowedMenuPagesByAreaAsync(string areaId)
        {
            var menuPages = (await MenuPages.ToListAsync())
                                        .Where(p => p.Area.ToUpper() == "SHARED" || p.Area.ToUpper() == areaId.ToUpper() || string.IsNullOrEmpty(p.Area))
                                        .OrderBy(p => p.Area).ThenBy(p => p.Name)
                                        .ToList();
            var allowedItems = new List<CPiMenuPage>();
            foreach (var item in menuPages)
            {
                if (item.Policy == "*" || (await _authorizationService.AuthorizeAsync(_claimsPrincipal, item.Policy)).Succeeded)
                {
                    allowedItems.Add(item);
                }
            }

            return allowedItems;
        }

        public async Task<CPiMenuPage> GetMenuPageByIdAsync(int id)
        {
            return await MenuPages.Where(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<CPiMenuPage> SaveMenuPage(CPiMenuPage page)
        {
            var repository = _cpiDbContext.GetRepository<CPiMenuPage>();

            if (page.Id == 0)
                repository.Add(page);
            else
                repository.Update(page);

            await _cpiDbContext.SaveChangesAsync();
            return page;
        }

        public async Task RemoveMenuPage(CPiMenuPage page)
        {
            _cpiDbContext.GetRepository<CPiMenuPage>().Delete(page);
            await _cpiDbContext.SaveChangesAsync();
        }
    }
}
