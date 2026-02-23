using R10.Core.Entities.GeneralMatter;
using R10.Core.Exceptions;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.GeneralMatter
{
    public class GMMatterCountryService : GMMatterChildService<GMMatterCountry>, IGMMatterCountryService
    {
        public GMMatterCountryService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, IGMMatterService matterService) : base(cpiDbContext, user, matterService)
        {
        }

        public IQueryable<GMAreaCountry> GMMatterCountryAreas => _cpiDbContext.GetRepository<GMAreaCountry>().QueryableList
                                                                            .Where(a => QueryableList.Any(c => c.GMCountry.Country == a.Country));
    }
}
