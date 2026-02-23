using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSReportType : BaseEntity
    {
        [Key]
        public int ReportId { get; set; }

        [StringLength(50)]
        [Display(Name = "Report Name")]
        public string ReportName { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Is Enabled")]
        public bool IsEnabled { get; set; }

        [Display(Name = "Multiple Sort")]
        public bool MultipleSort { get; set; }

        public List<RSCriteriaControl>? RSCriteriaControls { get; set; }
        public List<RSPrintOptionControl>? RSPrintOptionControls { get; set; }
        public List<RSMain>? RSMains { get; set; }
    }
}
