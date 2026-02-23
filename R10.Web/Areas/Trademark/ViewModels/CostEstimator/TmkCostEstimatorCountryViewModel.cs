using R10.Core.Entities.Trademark;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkCostEstimatorCountryViewModel : TmkCostEstimatorCountry
    {
        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }

        [Display(Name = "Estimated Cost")]
        public double EstimatedCost { get; set; }

        public double? ExchangeRate { get; set; }
    }
}
