using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Web.Areas.Trademark.ViewModels.CountryLaw
{
    public class TmkDesCaseTypeViewModel :TmkDesCaseType
    {
        [Display(Name = "Country Name")]
        public string? DesCountryName { get; set; }

    }
}
