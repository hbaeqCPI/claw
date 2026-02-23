using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Clearance;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class TmcClearanceChildService<T> : ChildEntityService<TmcClearance, T> where T : BaseEntity
    {
        protected readonly ITmcClearanceService _clearanceService;

        public TmcClearanceChildService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, ITmcClearanceService clearanceService) : base(cpiDbContext, user)
        {
            _clearanceService = clearanceService;
        }

        public override IQueryable<T> QueryableList
        {
            get
            {
                var queryableList = base.QueryableList;

                if (_user.HasEntityFilter())
                    queryableList = queryableList.Where(a => _clearanceService.QueryableList.Any(i => i.TmcId == EF.Property<int>(a, "TmcId")));

                return queryableList;
            }
        }

        public override async Task<bool> Update(object key, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted)
        {
            if (updated.Any() || added.Any())
                await _clearanceService.ValidatePermission((int)key);

            if (deleted.Any())
                await _clearanceService.ValidatePermission((int)key);

            return await base.Update(key, userName, updated, added, deleted);
        }

        protected async Task ValidateRole(List<string> roles)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.SearchRequest, roles, null)); //Clearance HAS NO RESPOFC
        }

        protected async Task<TmcClearance> ValidateClearance(int tmcId)
        {
            var clearance = await _clearanceService.GetByIdAsync(tmcId);
            Guard.Against.NoRecordPermission(clearance != null);
            return clearance;
        }
    }
}