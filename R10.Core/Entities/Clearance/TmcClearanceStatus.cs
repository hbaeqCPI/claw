using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Clearance
{
    public class TmcClearanceStatus : BaseEntity
    {
        public int ClearanceStatusId { get; set; }

        [Key]
        [Required]
        [StringLength(100)]
        [Display(Name = "Search Request Status")]
        public string ClearanceStatus { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Active?")]
        public bool ActiveSwitch { get; set; }

        [Display(Name = "Workflow Order")]
        public int? WorkflowOrder { get; set; }

        [Display(Name = "Generate Trademark?")]
        public bool GenerateTrademark { get; set; }

        [StringLength(25)]
        [Display(Name = "Group Name")]
        public string? GroupName { get; set; }

        public bool CPIStatus { get; set; }

        public List<TmcClearance>? Clearances { get; set; }
    }
}
