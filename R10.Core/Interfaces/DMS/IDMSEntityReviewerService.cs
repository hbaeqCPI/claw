using R10.Core.Entities.DMS;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.DMS
{
    public interface IDMSEntityReviewerService
    {
        IQueryable<DMSEntityReviewer> QueryableList { get; }

        Task<bool> Update(int entityId, DMSReviewerType entityType, string userName,
            IEnumerable<DMSEntityReviewer> updated,
            IEnumerable<DMSEntityReviewer> added,
            IEnumerable<DMSEntityReviewer> deleted);        

        Task<bool> IsDefaultReviewer(CPiEntityType reviewerType, int reviewerId);

        Task UpdateDefaultReviewer(CPiEntityType reviewerType, int reviewerId, string userName, bool isReviewer);
    }
}
