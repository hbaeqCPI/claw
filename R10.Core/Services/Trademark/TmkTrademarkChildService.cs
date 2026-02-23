using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class TmkTrademarkChildService<T> : ChildEntityService<TmkTrademark, T> where T : BaseEntity
    {
        protected readonly ITmkTrademarkService _trademarkService;

        public TmkTrademarkChildService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, ITmkTrademarkService trademarkService) : base(cpiDbContext, user)
        {
            _trademarkService = trademarkService;
        }

        public override IQueryable<T> QueryableList
        {
            get
            {
                var queryableList = base.QueryableList;

                if (_user.HasRespOfficeFilter(SystemType.Trademark) || _user.HasEntityFilter())
                    //"TmkID" is case-sensitive.
                    //Only TmkOwnerService implements TmkTrademarkChildService and TmkOwner is using TmkID
                    queryableList = queryableList.Where(a => _trademarkService.TmkTrademarks.Any(t => t.TmkId == EF.Property<int>(a, "TmkID")));

                return queryableList;
            }
        }

        public override async Task<bool> Update(object key, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted)
        {
            //todo: validate trademark permission
            //if (updated.Any() || added.Any())
            //    await _trademarkService.ValidatePermission((int)key, CPiPermissions.FullModify);

            //if (deleted.Any())
            //    await _trademarkService.ValidatePermission((int)key, CPiPermissions.CanDelete);

            return await base.Update(key, userName, updated, added, deleted);
        }

        protected async Task<TmkTrademark> ValidateTrademark(int tmkId)
        {
            var invention = await _trademarkService.GetByIdAsync(tmkId);
            Guard.Against.NoRecordPermission(invention != null);

            return invention;
        }

        protected async Task ValidatePermission(List<string> roles, string respOffice)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Trademark, roles, respOffice));
        }
    }
}
