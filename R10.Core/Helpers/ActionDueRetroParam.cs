using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core
{    public class ActionDueRetroParam
    {
        public int ActionTypeID { get; set; }

        public int ActParamId { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Case Type(s)")]
        public List<string>? CaseTypes { get; set; }

        [Display(Name = "Status")]
        public List<string>? Statuses { get; set; }

        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [Display(Name = "Action Due")]
        public string? ActionDue { get; set; }

        [Display(Name = "Base Date")]
        [Required]
        public DateTime BaseDate { get; set; }

        [Display(Name = "Only generate actions that are due after this date")]
        public DateTime? DueDateCutOff { get; set; }

        public bool ActiveOnly { get; set; } = true;

        [Display(Name = "Filing Date")]
        public DateTime? FilDateFrom { get; set; }
        [Display(Name = "To")]
        public DateTime? FilDateTo { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDateFrom { get; set; }
        [Display(Name = "To")]
        public DateTime? PubDateTo { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDateFrom { get; set; }
        [Display(Name = "To")]
        public DateTime? IssDateTo { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime? RegDateFrom { get; set; }
        [Display(Name = "To")]
        public DateTime? RegDateTo { get; set; }

        [Display(Name = "Effective Open Date")]
        public DateTime? EffectiveOpenDateFrom { get; set; }
        [Display(Name = "To")]
        public DateTime? EffectiveOpenDateTo { get; set; }

        [Display(Name = "Termination/End Date")]
        public DateTime? TerminationEndDateFrom { get; set; }
        [Display(Name = "To")]
        public DateTime? TerminationEndDateTo { get; set; }

        [Display(Name = "Status Date")]
        public DateTime? StatusDateFrom { get; set; }
        [Display(Name = "To")]
        public DateTime? StatusDateTo { get; set; }

        [Display(Name = "Disclosure Date")]
        public DateTime? DisclosureDateFrom { get; set; }
        [Display(Name = "To")]
        public DateTime? DisclosureDateTo { get; set; }
    }
}
