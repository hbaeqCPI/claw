using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class EntityService<T> : BaseService<T>, IEntityService<T> where T : class
    {
        protected readonly ClaimsPrincipal _user;

        public EntityService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext)
        {
            _user = user;
            UserId = _user.GetUserIdentifier();
        }
        protected string UserId { get; }

        public IQueryable<CPiUserEntityFilter> UserEntityFilters => _cpiDbContext.GetEntityFilterRepository().UserEntityFilters;

        public IQueryable<CPiUserSystemRole> CPiUserSystemRoles => _cpiDbContext.GetEntityFilterRepository().CPiUserSystemRoles;

        public async Task<bool> EntityFilterAllowed(int entityId)
        {
            return await _cpiDbContext.GetEntityFilterRepository().UserEntityFilters.AnyAsync(f => f.UserId == UserId && f.EntityId == entityId);
        }

        public async Task<bool> ValidatePermission(string systemId, List<string> roles, string respOffice)
        {
            if (_user.IsAdmin())
                return true;

            if (_user.HasRespOfficeFilter(systemId))
                return await _cpiDbContext.GetEntityFilterRepository().CPiUserSystemRoles.AnyAsync(r => r.UserId == UserId && r.SystemId == systemId && roles.Contains(r.RoleId) && r.RespOffice == respOffice);
            else
                return await _cpiDbContext.GetEntityFilterRepository().CPiUserSystemRoles.AnyAsync(r => r.UserId == UserId && r.SystemId == systemId && roles.Contains(r.RoleId));
        }

        public async Task<List<CPiUserEntityFilter>> GetEntityFilters()
        {
            return await _cpiDbContext.GetEntityFilterRepository().GetUserEntityFilters(UserId);
        }

        public async Task<List<CPiRespOffice>> GetRespOffices(string systemId, List<string> roles)
        {
            var respOffices = new List<CPiRespOffice>();
            var system = await _cpiDbContext.GetRepository<CPiSystem>().GetByIdAsync(systemId);

            if (system == null) return respOffices;

            if (_user.HasRespOfficeFilter(systemId))
                respOffices = await _cpiDbContext.GetEntityFilterRepository().GetUserRespOffices(UserId, systemId,roles);
            else
                respOffices =  await _cpiDbContext.GetEntityFilterRepository().CPiRespOffices.ToListAsync();

            var systemType = (system.SystemType ?? "-").ToCharArray().FirstOrDefault();
            return respOffices.Where(ro => string.IsNullOrEmpty(ro.SystemTypes) || ro.SystemTypes.Contains(systemType)).ToList();
        }

        public async Task<bool> ValidateRespOffice(string systemId, string respOffice, List<string>? roles = null)
        {
            roles = roles ?? CPiPermissions.FullModify;

            if (_user.HasRespOfficeFilter(systemId))
                return await _cpiDbContext.GetEntityFilterRepository().CPiUserSystemRoles.AnyAsync(r => r.UserId == UserId && r.SystemId == systemId && roles.Contains(r.RoleId) && r.RespOffice == respOffice);

            return await _cpiDbContext.GetEntityFilterRepository().CPiRespOffices.AnyAsync(ro => ro.RespOffice == respOffice);
        }

        public async Task<int> GetUserEntityId()
        {
            switch (_user.GetUserType()) {
                case CPiUserType.ContactPerson:
                case CPiUserType.Inventor:
                case CPiUserType.Attorney:
                    return await UserEntityFilters.Where(ef => ef.UserId == UserId).Select(ef => ef.EntityId).FirstOrDefaultAsync();
                default:
                    return 0;
            }
        }

        public async Task<string> GetLanguage(string locale)
        {
            return await _cpiDbContext.GetRepository<Language>().QueryableList.Where(l => l.LanguageCulture.ToLower() == locale.ToLower()).Select(l => l.LanguageName).FirstOrDefaultAsync();
        }
    }
}
