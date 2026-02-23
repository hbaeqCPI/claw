using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSCriteriaControl : BaseEntity
    {
        [Key]
        public int CriteriaId { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Field Name")]
        public string? FieldName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Field Type")]
        public string? FieldType { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Field Alias")]
        public string? FieldAlias { get; set; }

        [Display(Name = "Default Field")]
        public bool DefaultField { get; set; }

        [StringLength(50)]
        [Display(Name = "Default Value")]
        public string? DefaultValue { get; set; }

        [Display(Name = "Is Multiple")]
        public bool IsMultiple { get; set; }
        public RSReportType? RSReportType { get; set; }
    }
}
