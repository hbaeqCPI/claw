using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.AMS;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.AMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.AMS
{
    public class AMSMainChildService<T> : ChildEntityService<AMSMain, T> where T : BaseEntity
    {
        protected readonly IAMSMainService _amsMainService;

        public AMSMainChildService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, IAMSMainService amsMainService) : base(cpiDbContext, user)
        {
            _amsMainService = amsMainService;
        }

        public override IQueryable<T> QueryableList
        {
            get
            {
                var queryableList = base.QueryableList;

                if (_user.HasRespOfficeFilter(SystemType.AMS) || _user.HasEntityFilter())
                    queryableList = queryableList.Where(a => _amsMainService.QueryableList.Any(m => m.AnnID == EF.Property<int>(a, "AnnID")));

                return queryableList;
            }
        }

        public override async Task<bool> Update(object key, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted)
        {
            //TODO:
            //  Validation is no longer necessary since controller action already has authorization policy ??
            //  Child entity may have separate permission from parent (i.e. Product)
            //if (updated.Any() || added.Any())
            //    await _amsMainService.ValidatePermission((int)key, CPiPermissions.FullModify);

            //if (deleted.Any())
            //    await _amsMainService.ValidatePermission((int)key, CPiPermissions.CanDelete);

            return await base.Update(key, userName, updated, added, deleted);
        }

        protected async Task<AMSMain> ValidateAMSMain(int annId)
        {
            var amsMain = await _amsMainService.GetByIdAsync(annId);
            Guard.Against.NoRecordPermission(amsMain != null);

            return amsMain;
        }

        protected async Task ValidatePermission(List<string> roles, string respOffice)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.AMS, roles, respOffice));
        }
    }
}
