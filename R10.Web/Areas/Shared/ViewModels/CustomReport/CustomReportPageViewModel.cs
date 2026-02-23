using R10.Web.Models.PageViewModels;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class CustomReportPageViewModel : PageViewModel
    {
        public string? DetailPageId { get; set; }
        public string? ReportName { get; set; }
    }
}
