using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.PatClearance
{
    public class PacClearanceStatus : BaseEntity
    {
        public int ClearanceStatusId { get; set; }

        [Key]
        [Required]
        [StringLength(100)]
        [Display(Name = "Clearance Status")]
        public string ClearanceStatus { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Active?")]
        public bool ActiveSwitch { get; set; }

        [Display(Name = "Workflow Order")]
        public int? WorkflowOrder { get; set; }

        //Allow copy/generate a new Invention Disclosure record with Patent Clearance data
        [Display(Name = "Can Copy to Invention Disclosure?")]
        public bool CanCopyToDisclosure { get; set; }

        [StringLength(25)]
        [Display(Name = "Group Name")]
        public string? GroupName { get; set; }

        public bool CPIStatus { get; set; }

        public List<PacClearance>? Clearances { get; set; }
    }
}
