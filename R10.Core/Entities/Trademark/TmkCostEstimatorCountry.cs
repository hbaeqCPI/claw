using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkCostEstimatorCountry : BaseEntity
    {
        [Key]
        public int EntityId { get; set; }

        public int KeyId { get; set; }

        public int? CECountryId { get; set; }

        [Required]
        [StringLength(5)]
        public string Country { get; set; }

        public string Source { get; set; }

        [Display(Name = "Case Type")]
        public string CaseType { get; set; }

        public DateTime? FeesEffDate { get; set; }
        public string? CurrencyType { get; set; }
        public double? ExchangeRate { get; set; }
        public double? AllowanceRate { get; set; }

        public TmkCostEstimator? TmkCostEstimator { get; set; }
        public TmkCountry? TmkCountry { get; set; }
    }
}
