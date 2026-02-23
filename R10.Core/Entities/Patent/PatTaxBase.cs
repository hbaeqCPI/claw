using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace R10.Core.Entities.Patent
{
    public class PatTaxBase: BaseEntity
    {
        [Key]
        public int TaxBID { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Required]
        [Display(Name = "Tax Schedule")]
        [StringLength(3)]
        public string TaxSchedule { get; set; }

        public int? AgentID { get; set; }

        [Display(Name = "Currency Type")]
        [StringLength(3)]
        public string? CurrencyType { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Please enter a value greater than {0}")]
        [Display(Name = "Exchange Rate")]
        public double? ExchangeRate { get; set; } = 1;

        public bool TaxYearZero { get; set; }

        public CurrencyType? PatCurrencyType { get; set; }

        public PatCountry? PatCountry { get; set; }

        public Agent? Agent { get; set; }

        public List<PatTaxYear>? PatTaxesYear { get; set; }
    }
}
