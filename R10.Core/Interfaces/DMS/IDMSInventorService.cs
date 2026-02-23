using R10.Core.Entities.DMS;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.DMS
{
    public interface IDMSInventorService : IMultipleEntityService<Disclosure, DMSInventor>
    {
        IQueryable<PatInventor> PatInventors { get; }
    }
}
