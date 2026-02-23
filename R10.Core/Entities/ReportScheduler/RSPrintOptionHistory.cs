using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSPrintOptionHistory
    {
        [Key]
        public int OptionHistoryId { get; set; }

        [Required]
        public int LogId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Option")]
        public string? OptionName { get; set; }

        [StringLength(150)]
        [Display(Name = "Option Alias")]
        public string? OptionAlias { get; set; }

        [Required]
        [Display(Name = "Print")]
        public bool OptionValue { get; set; }
    }
}
