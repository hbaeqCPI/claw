using R10.Core.DTOs;
using R10.Core.Entities.DMS;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.DMS
{
    public interface IDMSReviewService : IChildEntityService<Disclosure, DMSReview>
    {
        Task<(int DMSReviewId, CPiEntityType ReviewerType, int ReviewerId, byte[] tStamp)> Update(DMSReview review);

        Task UpdateValuation(DMSValuation valuation);

        Task DeleteValuation(DMSValuation valuation);
    }
}
