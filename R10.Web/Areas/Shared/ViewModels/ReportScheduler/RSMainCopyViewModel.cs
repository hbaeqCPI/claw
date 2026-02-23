using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels.ReportScheduler
{
    public class RSMainCopyViewModel
    {
        public int CopyTaskId { get; set; }

        [Required]
        [Display(Name = "Schedule Name")]
        public string? CopyScheduleName { get; set; }

        [Display(Name = "Settings")]
        public bool CopySettings { get; set; }

        [Display(Name = "Actions")]
        public bool CopyActions { get; set; }

        [Display(Name = "Criteria")]
        public bool CopyCriteria { get; set; }

        [Display(Name = "Print Options")]
        public bool CopyPrintOptions { get; set; }
    }
}
