using System.ComponentModel.DataAnnotations;

namespace LawPortal.Core.Entities
{
    public partial class CurrencyType : BaseEntity
    {
        public int KeyID { get; set; }

        [Key]
        [Required]
        [StringLength(3)]
        [Display(Name = "Currency Type")]
        public string?  CurrencyTypeCode { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string?  Description { get; set; }

        [Display(Name = "Exchange Rate")]
        public double? ExchangeRate { get; set; }

        [StringLength(10)]
        [Display(Name = "Symbol")]
        public string? Symbol { get; set; }

        [Display(Name = "Allowance Rate")]
        public double? AllowanceRate { get; set; }

        [StringLength(20)]
        [Display(Name = "Exchange Rate Updated By")]
        public string? ExRateUpdatedBy { get; set; }
        public DateTime? ExRateLastUpdate { get; set; }

        // Exchange Rates
        // US Dollar
        public double? USD_ExRate { get; set; }
        public DateTime? USD_ExRateLastUpdate { get; set; }

        // Euro
        public double? EUR_ExRate { get; set; }
        public DateTime? EUR_ExRateLastUpdate { get; set; }

        // British Pound
        public double? GBP_ExRate { get; set; }
        public DateTime? GBP_ExRateLastUpdate { get; set; }

        // Danish Krone
        public double? DKK_ExRate { get; set; }
        public DateTime? DKK_ExRateLastUpdate { get; set; }

        public bool CPICurrencyType { get; set; }
    }


}
