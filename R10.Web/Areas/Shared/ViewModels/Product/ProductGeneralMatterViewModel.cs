using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ProductGeneralMatterViewModel : BaseEntity
    {

        public int MatId { get; set; }

        public string? RelatedGMCaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? RelatedGMCountry { get; set; }

        [Display(Name = "Sub Case")]
        public string? RelatedGMSubCase { get; set; }

        [Display(Name = "Matter Type")]
        public string? MatterType { get; set; }

        [Display(Name = "Status")]
        public string? RelatedGMStatus { get; set; }

        [Display(Name = "Title")]
        public string? MatterTitle { get; set; }

        [Display(Name = "Effective Open Date")]
        public DateTime? EffectiveOpenDate { get; set; }

        [Display(Name = "Termination/End Date")]
        public DateTime? TerminationEndDate { get; set; }

        [Display(Name = "Cost")]
        public decimal CostAmount { get; set; }

    }
}
