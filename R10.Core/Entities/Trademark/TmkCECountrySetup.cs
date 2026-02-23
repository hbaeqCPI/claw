using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Trademark
{
    public class TmkCECountrySetup : BaseEntity
    {
        [Key]
        public int CECountryId { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        [Required]
        public string Country { get; set; }

        [StringLength(3)]
        [Display(Name = "Case Type")]
        [Required]
        public string? CaseType { get; set; }

        [Display(Name = "Currency Type")]
        [Required]
        public string? CurrencyType { get; set; }

        [Display(Name = "Default?")]
        public bool IsDefault { get; set; }

        [Display(Name = "Fees as of")]
        public DateTime? FeesEffDate { get; set; }

        public TmkCaseType? TmkCaseType { get; set; }
        public TmkCountry? TmkCountry { get; set; }
        public CurrencyType? TmkCurrencyType { get; set; }

        public List<TmkCECountryCost>? TmkCECountryCosts { get; set; }
        public List<TmkCostEstimatorCost>? TmkCostEstimatorCosts { get; set; }


        [NotMapped]
        public string? CopyOptions { get; set; }
    }

}
