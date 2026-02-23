using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocInventionViewModel
    {
        public int InvId { get; set; }

        public string? CaseNumber { get; set; }
        [Display(Name = "Family Number")]
        public string? FamilyNumber { get; set; }

        public string? ClientName { get; set; }
        public string? OwnerName { get; set; }

        [Display(Name = "Invention Title")]
        public string? InvTitle { get; set; }

        [Display(Name = "Attorney 1")]
        public string? Attorney1 { get; set; }
        [Display(Name = "Attorney 2")]
        public string? Attorney2 { get; set; }
        [Display(Name = "Attorney 3")]
        public string? Attorney3 { get; set; }

        [Display(Name = "Disclosure Status")]
        public string? DisclosureStatus { get; set; }

        [Display(Name = "Disclosure Date")]
        public DateTime? DisclosureDate { get; set; }
    }
}
