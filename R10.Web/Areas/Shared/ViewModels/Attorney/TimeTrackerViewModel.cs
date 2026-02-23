using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class TimeTrackerViewModel : TimeTracker
    {
        public string? TimeTrackerClientCode { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }
        public string? Title { get; set; }
        public string? CaseType { get; set; }
        public string? Status { get; set; }
        public string? ApplicationNumber { get; set; }
        public DateTime? FilDate { get; set; }
    }

    public class TimeTrackerExportToExcelViewModel
    {
        public bool Exported { get; set; }
        [Display(Name = "System Type")]
        public string SystemType { get; set; }
        [Display(Name = "LabelCaseNumber")]
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }
        [Display(Name = "LabelClient")]
        public string? TimeTrackerClientCode { get; set; }
        public string? Title { get; set; }
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }
        public string? Status { get; set; }
        [Display(Name = "Application Number")]
        public string? ApplicationNumber { get; set; }
        [Display(Name = "Filling Date")]
        public DateTime? FilDate { get; set; }
        public decimal Duration { get; set; }
        [Display(Name = "Entry Date")]
        public DateTime EntryDate { get; set; }
        public string? Description { get; set; }
    }

}
