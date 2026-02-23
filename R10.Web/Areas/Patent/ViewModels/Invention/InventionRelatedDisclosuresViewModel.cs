using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionRelatedDisclosureViewModel 
    {
        public int KeyId { get; set; }
        public int DMSId { get; set; }
        public int? InvId { get; set; }

        [Display(Name= "Disclosure Number")]
        public string? DisclosureNumber { get; set; }

        [Display(Name = "Disclosure Status")]
        public string? DisclosureStatus { get; set; }

        [Display(Name = "Disclosure Date")]
        public DateTime? DisclosureDate { get; set; }

        [Display(Name = "Client")]
        public string? ClientCode { get; set; } //use clientcode for consistency

        public string? ClientName { get; set; }

        [Display(Name = "Recommendation")]
        public string? Recommendation { get; set; }

        [Display(Name = "Title")]
        public string? DiscTitle { get; set; }
        public byte[]? tStamp { get; set; }
    }
}
