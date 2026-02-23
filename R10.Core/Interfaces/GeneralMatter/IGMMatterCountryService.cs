using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IGMMatterCountryService : IChildEntityService<GMMatter, GMMatterCountry>
    {
        IQueryable<GMAreaCountry> GMMatterCountryAreas { get; }
    }
}
