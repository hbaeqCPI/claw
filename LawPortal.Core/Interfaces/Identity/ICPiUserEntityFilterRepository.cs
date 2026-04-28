using LawPortal.Core.DTOs;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface ICPiUserEntityFilterRepository : IDisposable
    {
        IQueryable<CPiUserEntityFilter> UserEntityFilters { get; }
        IQueryable<EntityFilterDTO> EntityFilters { get; }

        Task CreateAsync(CPiUserEntityFilter userEntityFilter);
        Task DeleteAsync(CPiUserEntityFilter userEntityFilter);

        Task CreateAsync(List<CPiUserEntityFilter> userEntityFilters);
        Task DeleteAsync(List<CPiUserEntityFilter> userEntityFilters);

        Task<List<CPiUserEntityFilter>> GetUserEntityFilters(string userId);
    }
}
