using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatCostTrackingDetailViewModel: PatCostTrackDetail
    {
        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Status")]
        public string? ApplicationStatus { get; set; }

        [Display(Name = "Application Title")]
        public string? AppTitle { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Agent")]
        public string? AgentCode { get; set; }

        [Display(Name = "Agent Name")]
        public string? AgentName { get; set; }

        public bool CanModifyAgent { get; set; } = true;

        public string? RespOffice { get; set; }
        public string? CopyOptions { get; set; }
        [Display(Name = "Billing Attorney")]
        public string? BillingAttorneyCode { get; set; }

        [Display(Name = "Billing Attorney Name")]
        public string? BillingAttorneyName { get; set; }

        [Display(Name = "Billing User Name")]
        public string? BillingUserName { get; set; }

        public string? OldCostType { get; set; }
    }
}
