using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LawPortal.Core.Entities.Patent;

namespace LawPortal.Web.Areas.Patent.ViewModels.CountryLaw
{
    public class PatDesCaseTypeViewModel :PatDesCaseType
    {
        [Display(Name = "Country Name")]
        public string? DesCountryName { get; set; }

    }

}
