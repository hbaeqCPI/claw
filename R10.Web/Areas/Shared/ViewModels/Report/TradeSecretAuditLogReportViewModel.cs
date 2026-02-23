using R10.Core.Entities.Patent;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels.Report
{
    public class TradeSecretAuditLogReportViewModel
    {
        public int ParentId { get; set; }
        public int AuditLogId { get; set; }
        public int ActivityId { get; set; }
        public string? ScreenId { get; set; }
        public int RecId { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }
        public string? Title { get; set; }
        public string? Sys { get; set; }
        public string? Inventors { get; set; }
        public string? AbstractConcat { get; set; }
        public string? UserId{ get; set; }
        public string? UpdatedBy{ get; set; }
        public string? UpdatedDate { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? UpdatedFields { get; set; }
        public List<AbstractExport>? Abstracts { get; set; }
    }
}
