using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LawPortal.Core.Entities.Patent;
using LawPortal.Core.Identity;

namespace LawPortal.Core.Interfaces
{
    public interface IEntityService<T> : IBaseService<T>
    {
        IQueryable<CPiUserEntityFilter> UserEntityFilters { get; }
        IQueryable<CPiUserSystemRole> CPiUserSystemRoles { get; }
        Task<bool> EntityFilterAllowed(int entityId);
        Task<bool> ValidatePermission(string systemId, List<string> roles, string respOffice);
        Task<bool> ValidateRespOffice(string systemId, string respOffice, List<string>? roles = null);
        Task<List<CPiUserEntityFilter>> GetEntityFilters();
        Task<List<CPiRespOffice>> GetRespOffices(string systemId, List<string> roles);
        Task<int> GetUserEntityId();
        Task<string> GetLanguage(string locale);
    }
}