using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatApplicationStatus : BaseEntity
    {
        public int StatusId { get; set; }

        [Key]
        [Required]
        [StringLength(15)]
        [Display(Name = "Application Status")]
        public string ApplicationStatus { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Active?")]
        public bool ActiveSwitch { get; set; } = true;

        public bool CPIAppStatus { get; set; }
    }

}
