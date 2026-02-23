using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ProductPatentViewModel : BaseEntity
    {

        public int AppId { get; set; }

        public string? RelatedPatCaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? RelatedPatCountry { get; set; }

        [Display(Name = "Sub Case")]
        public string? RelatedPatSubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? RelatedPatCaseType { get; set; }

        [Display(Name = "Status")]
        public string? RelatedPatStatus { get; set; }

        [Display(Name = "Title")]
        public string? AppTitle { get; set; }

        [Display(Name = "Application No.")]
        public string? RelatedPatAppNumber { get; set; }

        [Display(Name = "Patent No.")]
        public string? PatNumber { get; set; }

        [Display(Name = "Cost")]
        public decimal CostAmount { get; set; }

        [Display(Name = "Expiration Date")]
        public DateTime? ExpDate { get; set; }

    }
}
