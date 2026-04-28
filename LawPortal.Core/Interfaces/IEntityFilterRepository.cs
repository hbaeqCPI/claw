using LawPortal.Core.Entities;
using LawPortal.Core.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface IEntityFilterRepository
    {
        IQueryable<CPiUserEntityFilter> UserEntityFilters { get; }
        IQueryable<CPiUserSystemRole> CPiUserSystemRoles { get; }
        IQueryable<CPiRespOffice> CPiRespOffices { get; }

        Task<List<CPiUserEntityFilter>> GetUserEntityFilters(string userId);
        Task<List<CPiRespOffice>> GetUserRespOffices(string userId, string systemId, List<string> roles);
    }
}
