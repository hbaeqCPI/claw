using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Trademark
{
    public class TmkCECountryCostChild : TmkCECountryCostChildDetail
    {
        [Key]
        public int CCId { get; set; }
        public int CostId { get; set; }

        [Display(Name = "Calculate?")]
        public bool CActiveSwitch { get; set; }

        [Display(Name = "CPI Cost")]
        public bool CCPICost { get; set; }


        public TmkCECountryCost? TmkCECountryCost { get; set; }
        public CurrencyType? TmkCurrencyType { get; set; }
        public List<TmkCECountryCostSub>? TmkCECountryCostSubs { get; set; }

        [NotMapped]
        public string? CDataTypeDisplay { get; set; }

        [NotMapped]
        public int OldCCId { get; set; }
    }

    public class TmkCostEstimatorCostChild : TmkCECountryCostChildDetail
    {
        [Key]
        public int CECCId { get; set; }
        public int CECostId { get; set; }
        public int CCId { get; set; }
        public int KeyId { get; set; }

        public double? ExchangeRate { get; set; }
        public double? AllowanceRate { get; set; }

        public TmkCostEstimatorCost? TmkCostEstimatorCost { get; set; }
        public List<TmkCostEstimatorCostSub>? TmkCostEstimatorCostSubs { get; set; }
        public List<TmkCostEstimatorCountryCost>? TmkCostEstimatorCountryCosts { get; set; }

        [NotMapped]
        public int CostId { get; set; }
    }

    public class TmkCECountryCostChildDetail : BaseEntity
    {
        [Required]
        public string CDescription { get; set; }

        [Required]
        public TmkCECostDataType CDataType { get; set; }

        [Display(Name = "Value")]
        public string? CDefaultValue { get; set; }

        [Display(Name = "Alt Value")]
        public string? CAltValue { get; set; }

        [Display(Name = "Operator")]
        public string? COpts { get; set; }

        [Display(Name = "Alt Operator")]
        public string? CAltOpts { get; set; }

        [Display(Name = "Default Cost")]
        public decimal CCost { get; set; }

        [Display(Name = "Alternate Cost")]
        public decimal CAltCost { get; set; }

        [Display(Name = "Multiplier Cost")]
        public decimal CMultCost { get; set; }

        public int COrderOfEntry { get; set; }

        [Display(Name = "Currency Type")]
        public string? CurrencyType { get; set; }
    }
}