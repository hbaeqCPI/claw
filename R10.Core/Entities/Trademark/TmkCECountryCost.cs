using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Trademark
{
    public class TmkCECountryCost : TmkCECountryCostDetail
    {
        [Key]
        public int CostId { get; set; }
        public int CECountryId { get; set; }

        [Display(Name = "Calculate?")]
        public bool ActiveSwitch { get; set; }

        [Display(Name = "CPI Cost")]
        public bool CPICost { get; set; }

        public TmkCECountrySetup? TmkCECountrySetup { get; set; }
        public List<TmkCECountryCostChild>? TmkCECountryCostChilds { get; set; }
        public TmkCEStage? TmkCEStage { get; set; }

        [NotMapped]
        public string? DataTypeDisplay { get; set; }
    }

    public class TmkCostEstimatorCost : TmkCECountryCostDetail
    {
        [Key]
        public int CECostId { get; set; }        
        public int CostId { get; set; }
        public int CECountryId { get; set; }
        public int KeyId { get; set; }

        public TmkCECountrySetup? TmkCECountrySetup { get; set; }
        public List<TmkCostEstimatorCountryCost>? TmkCostEstimatorCountryCosts { get; set; }
        public List<TmkCostEstimatorCostChild>? TmkCostEstimatorCostChilds { get; set; }
    }

    public class TmkCECountryCostDetail : BaseEntity
    {
        [Required]
        public string Description { get; set; }

        [Required]
        public TmkCECostDataType DataType { get; set; }

        [Display(Name = "Value")]
        public string? DefaultValue { get; set; }

        [Display(Name = "Operator")]
        public string? Opts { get; set; }

        [Display(Name = "Default Cost")]
        public decimal Cost { get; set; }

        [Display(Name = "Alternate Cost")]
        public decimal AltCost { get; set; }

        [Display(Name = "Multiplier Cost")]
        public decimal MultCost { get; set; }

        public int OrderOfEntry { get; set; }

        [Required]
        [Display(Name = "Cost Type")]
        public string? CostType { get; set; }

        [Required]
        [Display(Name = "Stage")]
        public string? Stage { get; set; }

        [Display(Name = "Mark Type")]
        public string? MarkType { get; set; }

        [Display(Name = "Use Cost Factors?")]
        public bool UseCostFactor { get; set; }
        public string? CostFormula { get; set; }
        public int? CostFactor1 { get; set; }
        public decimal? CostFactor2 { get; set; }
        public int? CostFactor3 { get; set; }
        public TmkCETranslationType TranslationType { get; set; }
    }

    public enum TmkCETranslationType
    {
        [Display(Name = "Pages")]
        Pages,
        [Display(Name = "Words")]
        Words
    }

    public enum TmkCECostDataType
    {
        [Display(Name = "String")]
        String,
        [Display(Name = "Date")]
        Date,
        [Display(Name = "Date Range")]
        DateRange,
        [Display(Name = "Numeric")]
        Numeric,
        [Display(Name = "Boolean")]
        Boolean,
        [Display(Name = "Selection")]
        Selection,
        [Display(Name = "Numeric Range")]
        NumericRange,
    }
}