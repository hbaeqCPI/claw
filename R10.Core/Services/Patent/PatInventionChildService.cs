using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class PatInventionChildService<T> : ChildEntityService<Invention, T> where T : BaseEntity
    {
        protected readonly IInventionService _inventionService;

        public PatInventionChildService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, IInventionService inventionService) : base(cpiDbContext, user)
        {
            _inventionService = inventionService;
        }

        public override IQueryable<T> QueryableList
        {
            get
            {
                var queryableList = base.QueryableList;

                if (_user.HasRespOfficeFilter(SystemType.Patent) || _user.HasEntityFilter())
                    queryableList = queryableList.Where(a => _inventionService.QueryableList.Any(i => i.InvId == EF.Property<int>(a, "InvId")));

                return queryableList;
            }
        }

        public override async Task<bool> Update(object key, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted)
        {
            if (updated.Any() || added.Any())
                await _inventionService.ValidatePermission((int)key, CPiPermissions.FullModify);

            if (deleted.Any())
                await _inventionService.ValidatePermission((int)key, CPiPermissions.CanDelete);

            return await base.Update(key, userName, updated, added, deleted);
        }

        protected async Task<Invention> ValidateInvention(int invId)
        {
            var invention = await _inventionService.GetByIdAsync(invId);
            Guard.Against.NoRecordPermission(invention != null);

            return invention;
        }

        protected async Task ValidatePermission(List<string> roles, string respOffice)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Patent, roles, respOffice));
        }
    }
}
