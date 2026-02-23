using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatAssignmentStatus : BaseEntity
    {
        public int AssignmentStatusID { get; set; }

        [Key]
        [Required]
        [StringLength(20)]
        [Display(Name = "Assignment Status")]
        public string AssignmentStatus { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Active?")]
        public bool ActiveSwitch { get; set; }
        public bool CPIAssignStatus { get; set; }

        public List<PatAssignmentHistory>? AssignmentsHistory { get; set; }
    }

}
