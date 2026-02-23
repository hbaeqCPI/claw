using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels.ReportScheduler
{
    public class ScheduleViewModel
    {
        public int TaskId { get; set; }

        [Display(Name="Schedule")]
        public string? Schedule { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Trigger")]
        public string? Trigger { get; set; }

        [Display(Name = "Next Run Time")]
        public string? NextRunTime { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Date Created")]
        public string? DateCreated { get; set; }

        [Display(Name = "User Id")]
        public string? UserId { get; set; }
    }
}
