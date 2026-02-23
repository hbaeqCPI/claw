using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{

    public interface IGMMatterAttorneyService : IMultipleEntityService<GMMatter, GMMatterAttorney>
    {
        Task<bool> Update(object key, string userName, IEnumerable<GMMatterAttorney> updated, IEnumerable<GMMatterAttorney> added, IEnumerable<GMMatterAttorney> deleted,
            List<string> canModifyRoles, List<string> canDeleteRoles);
    }
}
