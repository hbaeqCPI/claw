using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatActionDueInvSearchResultViewModel
    {

        public int ActId { get; set; }
        public string? CaseNumber { get; set; }

        [Display(Name = "Status")]
        public string? DisclosureStatus { get; set; }

        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [Display(Name = "Base Date")]
        public DateTime BaseDate { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Date Created")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Last Update")]
        public DateTime? LastUpdate { get; set; }

        public List<PatDueDateInv>? DueDates { get; set; }

        [Display(Name = "Action Due")]
        public string? ActionDue { get; set; }

        [Display(Name = "Date Taken")]
        public DateTime? DateTaken { get; set; }

        public bool DueDateExtended { get; set; } = false;

    }
}
