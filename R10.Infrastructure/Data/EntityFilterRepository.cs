using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data
{
    public class EntityFilterRepository : IEntityFilterRepository
    {
        protected readonly DbContext _dbContext;

        public EntityFilterRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<CPiUserEntityFilter> UserEntityFilters => _dbContext.Set<CPiUserEntityFilter>().AsNoTracking();
        public IQueryable<CPiUserSystemRole> CPiUserSystemRoles => _dbContext.Set<CPiUserSystemRole>().AsNoTracking();
        public IQueryable<CPiRespOffice> CPiRespOffices => _dbContext.Set<CPiRespOffice>().AsNoTracking();

        public async Task<List<CPiUserEntityFilter>> GetUserEntityFilters(string userId)
        {
            return await this.UserEntityFilters.Where(f => f.UserId == userId).ToListAsync();
        }

        public async Task<List<CPiRespOffice>> GetUserRespOffices(string userId, string systemId,List<string> roles)
        {
            return await CPiRespOffices.Where(ro => ro.UserSystemRoles.Any(r => r.UserId == userId && r.SystemId == systemId && (!roles.Any() || roles.Contains(r.RoleId)) && r.RespOffice == ro.RespOffice)).ToListAsync();
        }
    }
}
