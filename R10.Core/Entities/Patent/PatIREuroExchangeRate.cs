using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatIREuroExchangeRate : BaseEntity
    {
        [Key]
        public int ExchangeId { get; set; }

        [StringLength(3)]
        [Required]
        [Display(Name = "Currency Type")]
        public string CurrencyType { get; set; }
        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Default Exchange Rate")]
        [Required]
        public double DefaultExchangeRate { get; set; }
        [Display(Name = "Use Default Exchange Rate")]
        public bool UseDefault { get; set; }
        public virtual ICollection<PatIREuroExchangeRateYearly>? PatIREuroExchangeRateYearlys { get; set; }
    }
}
