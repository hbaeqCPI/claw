using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.GeneralMatter
{
    public class GMOtherPartyService : AuxService<GMOtherParty>, IEntityService<GMOtherParty>
    {
        protected readonly IGMMatterService _gMMatterService;

        public GMOtherPartyService(ICPiDbContext cpiDbContext, ClaimsPrincipal user,
            IGMMatterService gMMatterService) : base(cpiDbContext, user)
        {
            _gMMatterService = gMMatterService;
        }

        public override IQueryable<GMOtherParty> QueryableList
        {
            get
            {
                var otherParties = base.QueryableList;

                if (_user.IsAuxiliaryLimited(SystemType.GeneralMatter))
                    otherParties = otherParties.Where(o =>
                        _gMMatterService.QueryableList.Any(gm => gm.OtherParties.Any(gmo => gmo.OtherParty == o.OtherParty)));

                return otherParties;
            }
        }
    }
}
