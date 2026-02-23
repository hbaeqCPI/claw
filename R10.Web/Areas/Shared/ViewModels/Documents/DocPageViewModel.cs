using R10.Core.DTOs;

namespace R10.Web.Models.PageViewModels
{
    public class DocPageViewModel : PageViewModel
    {
        public List<LookupDTO>? DocSystemList { get; set; }
        public DocDetailLink? DocDetailLink { get; set; }
    }

    public class DocDetailLink
    {
        public string? SystemType { get; set; }
        public string? ScreenCode { get; set; }
        public string? DataKey { get; set; }
        public int DataKeyValue { get; set; }

    }
}
