using R10.Core.Entities.PatClearance;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IPacInventorService : IMultipleEntityService<PacClearance, PacInventor>
    {
        IQueryable<PatInventor> PatInventors { get; }
    }
}
