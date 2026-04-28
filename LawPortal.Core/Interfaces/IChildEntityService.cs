using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface IChildEntityService<T1, T2> : IBaseService<T2>
    {
        //IQueryable<T2> QueryableList { get; }

        Task<bool> Update(object key, string userName,
            IEnumerable<T2> updated,
            IEnumerable<T2> added,
            IEnumerable<T2> deleted);

        Task<bool> EntityFilterAllowed(int entityId);
        Task<bool> ValidatePermission(string systemId, List<string> roles, string respOffice);
        Task<List<CPiUserEntityFilter>> GetEntityFilters();
    }
}
