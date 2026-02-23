using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.GeneralMatter
{
    public class GMMatterPatentService : GMMatterChildService<GMMatterPatent>, IChildEntityService<GMMatter, GMMatterPatent>
    {
        private readonly IInventionService _inventionService;
        private readonly ICountryApplicationService _countryApplicationService;

        public GMMatterPatentService(
            IInventionService inventionService,
            ICountryApplicationService countryApplicationService,
            ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user, 
            IGMMatterService matterService) : base(cpiDbContext, user, matterService)
        {
            _inventionService = inventionService;
            _countryApplicationService = countryApplicationService;
        }

        public override IQueryable<GMMatterPatent> QueryableList
        {
            get
            {
                var queryableList = _cpiDbContext.GetRepository<GMMatterPatent>().QueryableList;

                //use patent respOffice and entity filters
                if (_user.HasRespOfficeFilter(SystemType.Patent) || _user.HasEntityFilter())
                    queryableList = queryableList.Where(mp => mp.AppId == null ? 
                        _inventionService.QueryableList.Any(i => i.InvId == mp.InvId) :
                        _countryApplicationService.CountryApplications.Any(ca => ca.AppId == mp.AppId));

                return queryableList;
            }
        }
    }
}
