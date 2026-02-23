using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels.Report
{
    public class TradeSecretViolationsReportViewModel
    {
        public int ParentId { get; set; }
        public int ActivityId { get; set; }
        public string? ScreenId { get; set; }
        public int RecId { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }
        public string? Title { get; set; }
        public string? Sys { get; set; }
        public string? ActivityDate { get; set; }
        public string? Source { get; set; }
        public string? UserEmail { get; set; }
        public string? Activity { get; set; }
    }
}
