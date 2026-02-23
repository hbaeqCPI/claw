using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSCriteriaHistory
    {
        [Key]
        public int CritHistoryId { get; set; }

        [Required]
        public int LogId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Field")]
        public string? FieldName { get; set; }

        [StringLength(20)]
        [Display(Name = "Condition")]
        public string? Condition { get; set; }

        [StringLength(100)]
        [Display(Name = "Criteria")]
        public string? FieldValue { get; set; }

        [StringLength(50)]
        [Display(Name = "Special")]
        public string? Special { get; set; }

        public int? ParamOrder { get; set; }
    }
}
