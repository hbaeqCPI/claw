
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DataQuerySearchResultViewModel
    {
        public int QueryId { get; set; }

        [Display(Name = "Query Name")]
        public string? QueryName { get; set; }
        

    }
}
