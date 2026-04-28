using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LawPortal.Core
{
    public class CountryLawRetroParam
    {
        public int CDueId { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Case Type")]
        public string CaseType { get; set; }

        [Display(Name = "Action Type")]
        public string ActionType { get; set; }

        [Display(Name = "Action Due")]
        public string ActionDue { get; set; }

        [Display(Name = "Based On")]
        public string BasedOn { get; set; }

        [Display(Name = "Only generate actions that are due after this date")]
        public string DueDateCutOff { get; set; }

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

        public string? UserName { get; set; }
        public bool HasRespOfficeOn { get; set; }
        public bool HasEntityFilterOn { get; set; }

        public string? CaseNumber { get; set; }

        [Display(Name = "Family Number")]
        public string? FamilyNumber { get; set; }

        public string[]? ClientCode { get; set; }

        public string[]? AttorneyCode { get; set; }
        public bool AttorneyFilter1 { get; set; }
        public bool AttorneyFilter2 { get; set; }
        public bool AttorneyFilter3 { get; set; }
        public bool AttorneyFilter4 { get; set; }
        public bool AttorneyFilter5 { get; set; }

        [Display(Name = "Status")]
        public string[]? Status { get; set; }
    }
}
