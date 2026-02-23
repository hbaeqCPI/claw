using R10.Core.Entities.ReportScheduler;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels.ReportScheduler
{
    public class RSActionHistoryViewModel:RSActionHistory
    {
        [Display(Name = "Setting")]
        public string? Setting { get; set; }
    }
}
