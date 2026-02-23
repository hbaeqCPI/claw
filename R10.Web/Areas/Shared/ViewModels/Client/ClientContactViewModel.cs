using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ClientContactViewModel : ClientContact
    {
        [Required(ErrorMessage = "Contact is required.")]
        [Display(Name = "Contact")]
        public new int? ContactID { get; set; }
        public string? ContactName { get; set; }

        public bool? IsDMSReviewer { get; set; }
        public bool? IsAMSDecisionMaker { get; set; }
        public bool? IsTmkSearchReviewer { get; set; }
        public bool? IsPatClearanceReviewer { get; set; }
        public bool? IsRMSDecisionMaker { get; set; }
        public bool? IsFFDecisionMaker { get; set; }

        public string? GenAllLettersDescription { get; set; }
        public string? LetterSendAsDescription { get; set; }
    }
}
