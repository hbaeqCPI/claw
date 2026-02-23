using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatIndicator : BaseEntity
    {
        public int IndicatorId { get; set; }

        [Key]
        [Required]
        [StringLength(20)]
        [Display(Name = "Indicator")]
        public string? Indicator { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "CPI Indicator")]
        public bool CPIIndicator { get; set; } = false;

        [Display(Name = "Foreign Filing Indicator")]
        public bool FFIndicator { get; set; }

        public List<PatActionParameter>? ActionParameters { get; set; }
        public List<PatDueDate>? DueDates { get; set; }
        public List<PatDueDateInv>? DueDateInvs { get; set; }
    }
}
