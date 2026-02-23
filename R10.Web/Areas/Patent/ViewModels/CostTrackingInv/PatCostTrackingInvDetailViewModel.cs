using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatCostTrackingInvDetailViewModel : PatCostTrackInvDetail
    {
        [Display(Name = "Status")]
        public string? DisclosureStatus { get; set; }

        [Display(Name = "Title")]
        public string? InvTitle { get; set; }


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

        public string? OldCostType { get; set; }
    }
}