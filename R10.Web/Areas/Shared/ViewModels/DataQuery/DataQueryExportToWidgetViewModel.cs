using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DataQueryExportToWidgetViewModel
    {
        public int QueryId { get; set; }
        [Display(Name = "Category (Group by Column)")]
        public string? Category { get; set; }
        [Display(Name = "Group (Group by Column2, leave empty if not used)")]
        public string? Group { get; set; }
        [Required]
        [Display(Name = "Chart Type")]
        public string? CustomWidgetType { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string? Title { get; set; }
        public bool CanExport { get; set; }
        [Display(Name = "No. of Categories to display. (Leave empty to display all)")]
        public int? RecordsLimit { get; set; }
        [Display(Name = "Count Column. (Leave empty if no Count Column in query)")]
        public string? CountColumn { get; set; }
        public bool SharedWidget { get; set; }
        public string? AvaliableCharts { get; set; }

    }
}
