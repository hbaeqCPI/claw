using R10.Core.Entities;
using R10.Core.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
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
