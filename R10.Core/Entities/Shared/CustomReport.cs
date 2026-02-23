using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class CustomReport : CustomReportDetail
    {
    }
    public class CustomReportDetail : BaseEntity
    {
        [Key]
        public int ReportId { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Report Name is Required.")]
        [Display(Name = "Report Name")]
        public string? ReportName { get; set; }

        public string? Remarks { get; set; }

        public bool IsShared { get; set; }
        public bool IsEditable { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }
        public int? QueryId { get; set; }

        [Display(Name = "Report File")]
        public string? ReportFile { get; set; }
    }
}
