using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class RemunerationRevenueViewModel
    {
        [Display(Name = "Year")]
        public int? Year { get; set; }
        public double? Stage { get; set; }

        [Display(Name = "Total Revenue")]
        public double? Revenue { get; set; }
        [Display(Name = "Reduction Rate")]
        public double Reduction { get; set; }
        [Display(Name = "Reduced Revenue")]
        public double? ReducedRevenue { get; set; }
    }

    public class RevenueForRemunerationViewModel
    {
        public double RevenueForRemuneration { get; set; }
        public double TotalRevenueThisYear { get; set; }
    }
}
