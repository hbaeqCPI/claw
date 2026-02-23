using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class FormPMSActionViewModel
    {
        public int ParentId { get; set; }
        public int ActId { get; set; }
        public int DDId { get; set; }

        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [Display(Name = "Action Due")]
        public string? ActionDue { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Indicator")]
        public string? Indicator { get; set; }

        [Display(Name = "Date Taken")]
        public DateTime? DateTaken { get; set; }

        public bool IsEqualDueDate { get; set; }
    }
}
