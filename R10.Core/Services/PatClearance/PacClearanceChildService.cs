using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.PatClearance;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class PacClearanceChildService<T> : ChildEntityService<PacClearance, T> where T : BaseEntity
    {
        protected readonly IPacClearanceService _clearanceService;

        public PacClearanceChildService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, IPacClearanceService clearanceService) : base(cpiDbContext, user)
        {
            _clearanceService = clearanceService;
        }

        public override IQueryable<T> QueryableList
        {
            get
            {
                var queryableList = base.QueryableList;

                if (_user.HasEntityFilter())
                    queryableList = queryableList.Where(a => _clearanceService.QueryableList.Any(i => i.PacId == EF.Property<int>(a, "PacId")));

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
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.PatClearance, roles, null)); //Clearance HAS NO RESPOFC
        }

        protected async Task<PacClearance> ValidateClearance(int pacId)
        {
            var clearance = await _clearanceService.GetByIdAsync(pacId);
            Guard.Against.NoRecordPermission(clearance != null);
            return clearance;
        }
    }
}