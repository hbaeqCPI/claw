using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSIndicator : BaseEntity
    {
        public int IndicatorId { get; set; }

        [Key]
        [StringLength(20)]
        [Display(Name = "Indicator")]
        public string Indicator { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public bool CPIIndicator { get; set; } = false;

        public List<DMSActionParameter>? DMSActionParameters { get; set; }
        public List<DMSDueDate>? DMSDueDates { get; set; }
    }
}
