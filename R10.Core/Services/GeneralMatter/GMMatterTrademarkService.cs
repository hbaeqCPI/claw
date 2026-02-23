using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
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
    public class GMMatterTrademarkService : GMMatterChildService<GMMatterTrademark>, IChildEntityService<GMMatter, GMMatterTrademark>
    {
        private readonly ITmkTrademarkService _trademarkService;

        public GMMatterTrademarkService(
            ITmkTrademarkService trademarkService,
            ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user, 
            IGMMatterService matterService) : base(cpiDbContext, user, matterService)
        {
            _trademarkService = trademarkService;
        }

        public override IQueryable<GMMatterTrademark> QueryableList
        {
            get
            {
                var queryableList = _cpiDbContext.GetRepository<GMMatterTrademark>().QueryableList;

                //use trademark respOffice and entity filters
                if (_user.HasRespOfficeFilter(SystemType.Trademark) || _user.HasEntityFilter())
                    queryableList = queryableList.Where(mp => _trademarkService.TmkTrademarks.Any(t => t.TmkId == mp.TmkId));

                return queryableList;
            }
        }
    }
}
