using R10.Core.Entities.Trademark;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkWorkflowActionViewModel : TmkWorkflowAction
    {
        [Required, Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [Required, Display(Name = "Action Value")]
        public string? ActionValue { get; set; }
    }
}
