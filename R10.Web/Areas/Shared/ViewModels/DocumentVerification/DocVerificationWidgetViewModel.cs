using R10.Core.DTOs;
using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocketingTaskDistributionViewModel
    {
        [Display(Name = "Name")]
        public string? Name { get; set; }
        [Display(Name = "No. of tasks assigned")]
        public int NoOfAssigned { get; set; }
        [Display(Name = "No. of tasks completed")]
        public int NoOfCompleted { get; set; }
        [Display(Name = "No. of tasks outstanding")]
        public int NoOfOutstanding { get; set; }
        [Display(Name = "Percentage of completion")]
        public decimal CompletePercent { get; set; }
    }

    public class DocVerificationWidgetInfo
    {
        public string? WidgetId { get; set; }
        public string? Settings { get; set; }
    }
}
