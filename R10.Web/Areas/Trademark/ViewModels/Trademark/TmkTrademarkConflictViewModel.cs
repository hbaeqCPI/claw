using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkTrademarkConflictViewModel
    {
        public int ConflictId { get; set; }

        [Display(Name = "Conflict/Opp No.")]
        public string? ConflictOppNumber { get; set; }

        [Display(Name = "Other Party")]
        public string? OtherParty { get; set; }

        [Display(Name = "Other Party Mark")]
        public string? OtherPartyMark { get; set; }
    }
}
