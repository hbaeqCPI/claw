using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class CustomReportViewModel : ReportBaseViewModel
    {
        public int ReportId { get; set; }

        [StringLength(100)]
        [Display(Name = "Report Name")]
        public string? ReportName { get; set; }

        [StringLength(20)]
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [StringLength(20)]
        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Date Created")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Last Update")]
        public DateTime? LastUpdate { get; set; }

        public string? Remarks { get; set; }

        public bool IsShared { get; set; }
        public bool IsEditable { get; set; }
    }
}
