using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels.Report
{
    public class TradeSecretReportCriteriaViewModel : ReportBaseViewModel
    {
        public int ReportOption { get; set; }       
        public string? token { get; set; }
        public bool PrintPatent { get; set; }
        public bool PrintDMS { get; set; }
    }
}
