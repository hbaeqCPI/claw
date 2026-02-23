using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class WidgetViewModel
    {
        public int Id { get; set; }
        public int WidgetId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int NextId { get; set; }
        public string ViewName { get; set; }
        public object Data { get; set; }
        public string[] SeriesColors { get; set; }
        public string Icon { get; set; }
        public bool CanExpand { get; set; }
        public bool CanExportExcel { get; set; }
        public bool CanExportPDF { get; set; }
        public int ViewMode { get; set; }
        public JObject UserSettings { get; set; }
        public JObject WidgetSettings { get; set; }
        public string Template { get; set; }
        public string LabelTemplate { get; set; }
        public string TooltipTemplate { get; set; }
        public string Policy { get; set; }
        public string Error { get; set; }
        public bool IsDetail { get; set; } = false;
        public bool CanDrillDown { get; set; }
        public bool CanExportPpt { get; set; }
        public string? CreatorId { get; set; }
        public bool CanEmail { get; set; }
        public bool CanCopy { get; set; }
        public bool CanEditTitle { get; set; }
        public int RowSpan { get; set; }
    }
}
