using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMIndicator : BaseEntity
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

        public List<GMDueDate>? DueDates { get; set; }

    }
}
