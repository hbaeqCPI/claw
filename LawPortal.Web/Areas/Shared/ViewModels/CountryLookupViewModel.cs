using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Areas.Shared.ViewModels
{
    public class CountryLookupViewModel
    {
        public int CountryID { get; set; }

        [Display(Name ="Country")]
        public string? Country { get; set; }

        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }
        
        public string? DesCaseType { get; set; }
    }
}
