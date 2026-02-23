using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatTaxYearViewModel: PatTaxYear
    {
        [Display(Name = "Exchange Rate")]
        public double? ExchangeRate { get; set; }

        [Display(Name = "Tax Cost")]
        public double? TaxCost { get; set; }
    }
}
