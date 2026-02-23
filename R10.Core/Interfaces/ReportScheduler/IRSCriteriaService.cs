using R10.Core.Entities.ReportScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IRSCriteriaService : IEntityService<RSCriteria>
    {
        IQueryable<RSCriteria> RSCriterias { get; }
        IQueryable<RSCriteriaControl> RSCriteriaControls { get; }
        bool DeleteRSCriteriaById(int schedCritId);

        bool DeleteRSCriteriaByTaskId(int taskId);

        bool UpdateRSCriteria(RSCriteria rSCriteria);

        bool AddRSCriteria(RSCriteria rSCriteria);

        IQueryable<RSCriteria> GetRSCriterias(int taksId);

        RSCriteria GetRSCriteriaById(int schedCritId);

        Task<bool> Update(object key, string userName,
            IEnumerable<RSCriteria> updated,
            IEnumerable<RSCriteria> added,
            IEnumerable<RSCriteria> deleted);
    }
}
