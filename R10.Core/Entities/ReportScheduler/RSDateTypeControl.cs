using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSDateTypeControl : BaseEntity
    {
        [Key]
        public int DateTypeId { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        public int DateType { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Date Type")]
        public string DateTypeName { get; set; }

        [Display(Name = "Default Date Type")]
        public bool DefaultDateType { get; set; }
    }
}
