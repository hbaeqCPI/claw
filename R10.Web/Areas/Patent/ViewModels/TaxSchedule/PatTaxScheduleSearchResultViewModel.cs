using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatTaxScheduleSearchResultViewModel 
    {
        public int TaxBID { get; set; }

        [Display(Name = "Tax Schedule")]
        public string? TaxSchedule { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Currency")]
        public string? CurrencyType { get; set; }

        [Display(Name = "Agent")]
        public string? AgentCode { get; set; }

        [Display(Name = "Agent Name")]
        public string? AgentName { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Date Created")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Last Update")]
        public DateTime? LastUpdate { get; set; }
    }
}
