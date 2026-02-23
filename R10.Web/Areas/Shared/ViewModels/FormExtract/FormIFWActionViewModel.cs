using R10.Core.DTOs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class FormIFWActionViewModel
    {
        public int IFWId { get; set; }
        public int DocTypeId { get; set; }
        public int MapHdrId { get; set; }
        public string? PMSActionType { get; set; } = "";

        public FormIFWActionDueDTO? ActionDue { get; set; }
    }

    public class FormIFWOtherInfoViewModel
    {
        public int IFWId { get; set; }
        public int AppId { get; set; }

        [Display (Name = "Application Remarks")]
        public string? AppRemarks { get; set; }
    }

    // new action map
    public class FormIFWActViewModel
    {
        public string? ActionType { get; set; }
        public List<FormIFWDueDateViewModel>? DueDateList { get; set;}
    }

    public class FormIFWDueDateViewModel
    {
        [Key]
        public int ActParamId { get; set; }

        [Display(Name = "Action Due")]
        public string? ActionDue { get; set; }

        public int Yr { get; set; }

        public int Mo { get; set; }

        public int Dy { get; set; }

        [Display(Name = "Indicator")]
        public string? Indicator { get; set; }

        [NotMapped]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

    }
}
