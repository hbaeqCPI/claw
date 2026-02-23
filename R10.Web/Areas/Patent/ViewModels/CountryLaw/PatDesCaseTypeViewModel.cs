using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;

namespace R10.Web.Areas.Patent.ViewModels.CountryLaw
{
    public class PatDesCaseTypeViewModel :PatDesCaseType
    {
        [Display(Name = "Country Name")]
        public string? DesCountryName { get; set; }

    }

}
