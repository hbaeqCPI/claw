using R10.Core.Entities.Patent;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels.Report
{
    public class TradeSecretMasterListReportViewModel
    {
        public int Id { get; set; }
        public string? CaseNumber { get; set; }
        public string? Title { get; set; }
        public string? Sys { get; set; }
        public DateTime? TradeSecretDate { get; set; }
        public string? TradeSecretDate_Fmt { get; set; }
        public DateTime? LastViewDate { get; set; }
        public string? LastViewDate_Fmt { get; set; }
        public string? Inventors { get; set; }
        public string? AbstractConcat { get; set; }
        public List<AbstractExport>? Abstracts { get; set; }
    }
}
