using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels.ReportScheduler
{
    public class RSHistoryViewModel
    {
        public int parentId { get; set; }

        public int LogId { get; set; }

        [Display(Name= "Schedule Name")]
        public string? ScheduleName { get; set; }

        [Display(Name = "Frequency")]
        public string? Frequency { get; set; }

        [Display(Name = "Action")]
        public string? Action { get; set; }

        [Display(Name = "Start Time")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy hh:mm tt}")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "End Time")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy hh:mm tt}")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Elapsed Time")]
        public TimeSpan? ElapsedTime { get; set; }

        [Display(Name = "Run Result")]
        public string? RunResult { get; set; }

    }
}
