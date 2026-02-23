using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.DMS
{
    public class DMSDisclosureStatus : BaseEntity
    {
        public int DisclosureStatusId { get; set; }

        [Key]
        [StringLength(20)]
        [Display(Name = "Disclosure Status")]
        public string DisclosureStatus { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        [Display(Name = "Workflow Order")]
        public int? WorkflowOrder { get; set; }

        [Display(Name = "Default Status?")]
        public bool IsDefault { get; set; }

        //Reviewer Can Review?
        [Display(Name = "Can Review?")]
        public bool CanReview { get; set; }

        [Display(Name = "Can Preview?")]
        public bool CanPreview { get; set; }

        //Inventor Can Submit?
        [Display(Name = "Can Submit?")]
        public bool CanSubmit { get; set; }

        [Display(Name = "Lock Record?")]
        public bool LockRecord { get; set; }

        [StringLength(25)]
        [Display(Name = "Group Name")]
        public string? GroupName { get; set; }

        public bool CPIDiscStatus { get; set; }

        //Allow copy/generate a new Patent Clearance record with DMS data
        [Display(Name = "Can Copy to Patent Clearance?")]
        public bool CanCopyToClearance { get; set; }

        [Display(Name = "Number of days permitted")]
        public int DaysLimit { get; set; }

        public List<Disclosure>? Disclosures { get; set; }
    }
}
