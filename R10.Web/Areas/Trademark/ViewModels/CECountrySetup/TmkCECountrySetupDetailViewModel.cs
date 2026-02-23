using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Trademark;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkCECountrySetupDetailViewModel:TmkCECountrySetup
    {       

        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }
               
        [Display(Name = "Description")]
        public string? CaseTypeDescription { get; set; }

        [Display(Name = "Exchange Rate")]
        public double? ExchangeRate { get; set; }

        public bool HasCPICost { get; set; }

    }
}
