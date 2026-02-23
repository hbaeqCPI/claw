using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.DMS;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class DMSDisclosureChildService<T> : ChildEntityService<Disclosure, T> where T : BaseEntity
    {
        protected readonly IDisclosureService _disclosureService;

        public DMSDisclosureChildService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, IDisclosureService disclosureService) : base(cpiDbContext, user)
        {
            _disclosureService = disclosureService;
        }

        public override IQueryable<T> QueryableList
        {
            get
            {
                var queryableList = base.QueryableList;

                if (_user.HasEntityFilter())
                    queryableList = queryableList.Where(a => _disclosureService.QueryableList.Any(i => i.DMSId == EF.Property<int>(a, "DMSId")));

                return queryableList;
            }
        }

        public override async Task<bool> Update(object key, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted)
        {            
            //KEYWORD - DEFAULT INVENTORS ONLY
            //QUESTION - DEFAULT INVENTORS OR MODIFY USERS ONLY
            //RELATED INVENTION - NO GRID UPDATES (SHOULD NOT INHERIT DISCLOSURE CHILD SERVICE?)
            
            Type objectType = typeof(T);

            if (((added.Any() || updated.Any() || deleted.Any()) && objectType == typeof(DMSCombined)))
            {
                Guard.Against.NoRecordPermission(await _disclosureService.IsUserReviewerInDMS((int)key) || await ValidatePermission(SystemType.DMS, CPiPermissions.FullModify, null));
            }
            else if (((added.Any() || updated.Any() || deleted.Any()) && objectType != typeof(DMSDiscussion)))
            {
                Guard.Against.NoRecordPermission(await _disclosureService.IsUserDefaultInventor((int)key) || await ValidatePermission(SystemType.DMS, CPiPermissions.FullModify, null));
            }            

            return await base.Update(key, userName, updated, added, deleted);
        }

        protected async Task<Disclosure> ValidateDisclosure(int dmsId)
        {
            var disclosure = await _disclosureService.GetByIdAsync(dmsId);
            Guard.Against.NoRecordPermission(disclosure != null);

            return disclosure;
        }

        protected async Task ValidatePermission(List<string> roles)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.DMS, roles, null)); //DMS HAS NO RESPOFC
        }

    }
}
