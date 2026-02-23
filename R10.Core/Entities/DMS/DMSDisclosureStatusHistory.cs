using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSDisclosureStatusHistory
    {
        [Key]
        public int LogID { get; set; }

        [Required]
        public int DMSId { get; set; }

        [StringLength(20)]
        [Required(ErrorMessage = "Disclosure Status is required.")]
        [Display(Name = "Disclosure Status")]
        public string? DisclosureStatus { get; set; }
        
        [Display(Name = "Disclosure Date")]
        public DateTime? DisclosureDate { get; set; }

        [Display(Name = "Disclosure Status Date")]
        public DateTime? DisclosureStatusDate { get; set; }

        [Display(Name = "Recommendation")]
        public string? Recommendation { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [StringLength(20)]
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }
        
        [Display(Name = "Date Changed")]
        public DateTime? DateChanged { get; set; }

        public DMSStatusHistoryChangeType? ChangeType { get; set; }

        public Disclosure? Disclosure { get; set; }
    }

    public enum DMSStatusHistoryChangeType
    {
        None,
        Status,
        DisclosureDate
    }
}
