using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class AgentContactViewModel : AgentContact
    {
        [Required(ErrorMessage = "Contact is required.")]
        [Display(Name = "Contact")]
        public new int? ContactID { get; set; }
        public string? ContactName { get; set; }

        public string? GenAllLettersDescription { get; set; }
        public string? LetterSendAsDescription { get; set; }
    }
}
