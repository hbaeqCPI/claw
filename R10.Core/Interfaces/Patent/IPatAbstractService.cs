using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.Patent
{
    public interface IPatAbstractService : IChildEntityService<Invention, PatAbstract>
    {
        Task<List<PatAbstract>> GetPatAbstracts(int invId);
    }
}
