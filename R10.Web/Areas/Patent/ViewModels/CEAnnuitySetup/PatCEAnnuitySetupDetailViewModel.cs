using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatCEAnnuitySetupDetailViewModel:PatCEAnnuitySetup
    {       

        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }

        [Display(Name = "Exchange Rate")]
        public double? ExchangeRate { get; set; }

        public bool HasCPICost { get; set; }
    }
}
