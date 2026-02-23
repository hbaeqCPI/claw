using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionRelatedInventionViewModel : BaseEntity
    {
        public int RelatedId { get; set; }
        public int InvId { get; set; }
        public int RelatedInvId { get; set; }

        [Required]
        public string? RelatedCaseNumber { get; set; }

        [Display(Name = "Title")]
        public string? InvTitle { get; set; }

        [Display(Name = "Disclosure Status")]
        public string? DisclosureStatus { get; set; }

        [Display(Name = "Disclosure Date")]
        public DateTime? DisclosureDate { get; set; }
    }
}
