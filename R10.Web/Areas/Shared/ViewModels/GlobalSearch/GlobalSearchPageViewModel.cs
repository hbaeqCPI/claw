using R10.Core.DTOs;
using R10.Web.Areas.Shared.ViewModels;

namespace R10.Web.Models.PageViewModels
{
    public class GlobalSearchPageViewModel
    {
        public string? Title { get; set; }
        public string? PageId { get; set; }
        public List<GSSystemScreen>? SystemScreens { get; set; }

        public List<GSFieldListViewModel>? DataFilterFieldList { get; set; }
        public List<GSFieldListViewModel>? DocFilterFieldList { get; set; }

        public List<LookupDTO>? LogicalOperators { get; set; }

        public List<LookupDTO>? DocSearchMode { get; set; }
        public List<LookupDTO>? DocQueryType { get; set; }
    }

    public class GSSystemScreen
    {
        public LookupDTO System {get; set; }
        public List<LookupDTO> Screens { get; set; }

    }


}
