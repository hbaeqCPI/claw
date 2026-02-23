using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.GeneralMatter
{
    public class GMMatterChildService<T> : ChildEntityService<GMMatter, T> where T : BaseEntity
    {
        protected readonly IGMMatterService _matterService;

        public GMMatterChildService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, IGMMatterService matterService) : base(cpiDbContext, user)
        {
            _matterService = matterService;
        }

        public override IQueryable<T> QueryableList
        {
            get
            {
                var queryableList = base.QueryableList;

                if (_user.HasRespOfficeFilter(SystemType.GeneralMatter) || _user.HasEntityFilter())
                    queryableList = queryableList.Where(a => _matterService.QueryableList.Any(m => m.MatId == EF.Property<int>(a, "MatId")));

                return queryableList;
            }
        }

        public override async Task<bool> Update(object key, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted)
        {
            //TODO:
            //  Validation is no longer necessary since controller action already has authorization policy ??
            //  Child entity may have separate permission from parent (i.e. Product)
            //if (updated.Any() || added.Any())
            //    await _matterService.ValidatePermission((int)key, CPiPermissions.FullModify);

            //if (deleted.Any())
            //    await _matterService.ValidatePermission((int)key, CPiPermissions.CanDelete);

            return await  base.Update(key, userName, updated, added, deleted);
        }

        protected async Task<GMMatter> ValidateMatter(int matId)
        {
            var matter = await _matterService.GetByIdAsync(matId);
            Guard.Against.NoRecordPermission(matter != null);

            return matter;
        }

        protected async Task ValidatePermission(List<string> roles, string respOffice)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.GeneralMatter, roles, respOffice));
        }
    }
}
