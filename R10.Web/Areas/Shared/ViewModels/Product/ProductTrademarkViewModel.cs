using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ProductTrademarkViewModel : BaseEntity
    {

        public int TmkId { get; set; }

        public string? RelatedTmkCaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? RelatedTmkCountry { get; set; }

        [Display(Name = "Sub Case")]
        public string? RelatedTmkSubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? RelatedTmkCaseType { get; set; }

        [Display(Name = "Status")]
        public string? RelatedTmkStatus { get; set; }

        [Display(Name = "Trademark Name")]
        public string? TrademarkName { get; set; }

        [Display(Name = "Application No.")]
        public string? RelatedTmkAppNumber { get; set; }

        [Display(Name = "Registration No.")]
        public string? RegNumber { get; set; }

        [Display(Name = "Cost")]
        public decimal CostAmount { get; set; }

        [Display(Name = "Next Renewal Date")]
        public DateTime? NextRenewalDate { get; set; }
    }
}
