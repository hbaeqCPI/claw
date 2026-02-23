using R10.Core.Entities.Trademark; 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkWorkflowDetailViewModel : TmkWorkflow
    {
        [Display(Name = "Trigger")]
        public string? Trigger { get; set; }

        [Display(Name = "Trigger Value")]
        public string? TriggerValue { get; set; }

        public string? ScreenName { get; set; }
        public bool ShowScreenField { get; set; }
    }
}
