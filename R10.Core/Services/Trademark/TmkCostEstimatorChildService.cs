using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using System.Security.Claims;

namespace R10.Core.Services
{
    public class TmkCostEstimatorChildService<T> : ChildEntityService<TmkCostEstimator, T> where T : BaseEntity
    {
        protected readonly ITmkCostEstimatorService _costEstimatorService;

        public TmkCostEstimatorChildService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, ITmkCostEstimatorService costEstimatorService) : base(cpiDbContext, user)
        {
            _costEstimatorService = costEstimatorService;
        }

        public override IQueryable<T> QueryableList
        {
            get
            {
                var queryableList = base.QueryableList;

                return queryableList;
            }
        }

        public override async Task<bool> Update(object key, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted)
        {
            await _costEstimatorService.ValidatePermission((int)key, CPiPermissions.CostEstimatorModify);

            return await base.Update(key, userName, updated, added, deleted);
        }
    }
}
