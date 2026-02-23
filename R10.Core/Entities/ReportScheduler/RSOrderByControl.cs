using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSOrderByControl : BaseEntity
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        public int OrderBy { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Order By Name")]
        public string OrderByName { get; set; }

        [Display(Name = "Default Order")]
        public bool DefaultOrder { get; set; }
    }
}
