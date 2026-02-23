using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Trademark;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class CECountryListViewModel
    {
        public string CountrySource { get; set;  }

        public List<CECopyCountry> CountryList;
        public List<CECopyCountryRange> CountryRangeList;
    }

    public class CECopyCountry
    {
        public int CECountryId { get; set; }
        public string? Country { get; set; }
        public string? CountryName { get; set; }
    }

    public class CECopyCountryRange
    {
        public string? RangeLabel { get; set; }
        public string? CountryStart { get; set; }
        public string? CountryEnd { get; set; }
    }
}
