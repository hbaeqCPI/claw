using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class DashboardSettingViewModel
    {
        public int? CatId { get; set; }
        public List<int>? WidgetIds { get; set; }
    }

    public class WidgetSettingViewModel
    {
        public int? Id { get; set; }
        public string? Setting { get; set; }
        public string? Title { get; set; }
    }
}
