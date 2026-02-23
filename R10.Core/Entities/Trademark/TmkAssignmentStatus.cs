using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkAssignmentStatus : BaseEntity
    {
        public int AssignmentStatusId { get; set; }

        [Key]
        [StringLength(20)]
        [Display(Name = "Assignment Status")]
        public string? AssignmentStatus { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Active?")]
        public bool ActiveSwitch { get; set; }

        public bool CPIAssignStatus { get; set; } = false;
        public List<TmkAssignmentHistory>? AssignmentsHistory { get; set; }
    }
}
