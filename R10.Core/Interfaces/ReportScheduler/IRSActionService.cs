using R10.Core.Entities.ReportScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IRSActionService : IEntityService<RSAction>
    {
        IQueryable<RSAction> RSActions { get; }

        IQueryable<RSActionType> RSActionTypes { get; }

        IQueryable<RSOrderByControl> RSOrderByControls { get; }

        bool DeleteRSActionById(int actionId);

        bool DeleteRSActionByTaskId(int taskId);

        bool UpdateRSAction(RSAction rSAction);

        bool AddRSAction(RSAction rSAction);

        IQueryable<RSAction> GetRSActions(int taksId);

        RSAction GetRSActionById(int actionId);

        Task<bool> Update(object key, string userName,
            IEnumerable<RSAction> updated,
            IEnumerable<RSAction> added,
            IEnumerable<RSAction> deleted);
        Task ActionUpdate(RSAction rSAction);
    }
}
