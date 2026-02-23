using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Admin.ViewModels
{
    public class ScheduledTaskDetailViewModel : ScheduledTask
    {
        [Display(Name = "Base URL")]
        public string? BaseUrl { get; set; }

        [Display(Name = "Use Service Account")]
        public bool UseServiceAccount { get; set; }
    }
}
