using R10.Core.Entities.ReportScheduler;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels.ReportScheduler
{
    public class RSPrintOptionViewModel:RSPrintOption
    {
        public int parentId { get; set; }

        [Display(Name = "Option")]
        public string? OptionAlias { get; set; }
    }
}
