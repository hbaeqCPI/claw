using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatTaxScheduleDetailViewModel : PatTaxBase
    {
        [Display(Name = "Agent")]
        public string? AgentCode { get; set; }

        [Display(Name = "AgentName")]
        public string? AgentName { get; set; }

        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }

        [Display(Name = "Currency Name")]
        public string? CurrencyName { get; set; }

    }
}
