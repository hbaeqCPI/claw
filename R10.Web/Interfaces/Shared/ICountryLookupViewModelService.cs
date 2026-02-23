using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface ICountryLookupViewModelService
    {
        string CountrySource { get; }
        IQueryable<CountryLookupViewModel> Countries { get; }
    }
}
