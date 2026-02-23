using R10.Core.Entities.Patent;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatWorkflowActionViewModel : PatWorkflowAction
    {
        [Required, Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [Required, Display(Name = "Action Value")]
        public string? ActionValue { get; set; }
    }

    public class PatWorkflowCreateActionViewModel
    {
        public bool Generate { get; set; }
        public int ActionTypeId { get; set; }
        public string? ActionType { get; set; }

        [Required]
        public DateTime BaseDate { get; set; }
    }
}
