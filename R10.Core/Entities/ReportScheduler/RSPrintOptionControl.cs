using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSPrintOptionControl : BaseEntity
    {
        [Key]
        public int ParamId { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Option Name")]
        public string OptionName { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Option Alias")]
        public string OptionAlias { get; set; }

        [Display(Name = "Default Value")]
        public bool DefaultValue { get; set; }
    }
}
