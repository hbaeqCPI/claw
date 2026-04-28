using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface ICostTrackingService<T> : IEntityService<T> where T : class
    {
        Task<bool> CanModifyAgent(int agentId);
    }
}
