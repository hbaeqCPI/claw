using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class RelatedSearchRequestViewModel
    {
        public int TmcId { get; set; }

        [Display(Name = "LabelCaseNumber")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Trademark(s)/Tagline")]
        public string? TrademarkTagline { get; set; }

        [Display(Name = "Requestor's Name")]
        public string? Requestor { get; set; }
        
        [Display(Name = "Date Requested")]
        public DateTime? DateRequested { get; set; }

        [Display(Name = "Status")]
        public string? ClearanceStatus { get; set; }
    }
}
