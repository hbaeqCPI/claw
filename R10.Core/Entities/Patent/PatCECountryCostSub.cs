using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class PatCECountryCostSub : PatCECountryCostSubDetail
    {
        [Key]
        public int SubId { get; set; }
        public int CCId { get; set; }

        [Display(Name = "Calculate?")]
        public bool SActiveSwitch { get; set; }

        [Display(Name = "CPI Cost")]
        public bool SCPICost { get; set; }

        public PatCECountryCostChild? PatCECountryCostChild { get; set; }

        [NotMapped]
        public string? SDataTypeDisplay { get; set; }
    }

    public class PatCostEstimatorCostSub : PatCECountryCostSubDetail
    {
        [Key]
        public int CESubId { get; set; }
        public int CECCId { get; set; }
        public int SubId { get; set; }
        public int KeyId { get; set; }        

        public PatCostEstimatorCostChild? PatCostEstimatorCostChild { get; set; }
        public List<PatCostEstimatorCountryCost>? PatCostEstimatorCountryCosts { get; set; }

        [NotMapped]
        public int CCId { get; set; }

        [NotMapped]
        public int CECostId { get; set; }
    }

    public class PatCECountryCostSubDetail : BaseEntity
    {
        [Required]
        public string SDescription { get; set; }

        [Required]
        public CECostDataType SDataType { get; set; }

        [Display(Name = "Value")]
        public string? SDefaultValue { get; set; }

        [Display(Name = "Alt Value")]
        public string? SAltValue { get; set; }

        [Display(Name = "Operator")]
        public string? SOpts { get; set; }

        [Display(Name = "Alt Operator")]
        public string? SAltOpts { get; set; }

        [Display(Name = "Default Cost")]
        public decimal SCost { get; set; }

        [Display(Name = "Alternate Cost")]
        public decimal SAltCost { get; set; }

        [Display(Name = "Multiplier Cost")]
        public decimal SMultCost { get; set; }

        public int SOrderOfEntry { get; set; }

        [Display(Name = "Use Cost Factors?")]
        public bool SUseCostFactor { get; set; }
        public string? SCostFormula { get; set; }
        public int? SCostFactor1 { get; set; }
        public decimal? SCostFactor2 { get; set; }
        public int? SCostFactor3 { get; set; }
        public PatCETranslationType STranslationType { get; set; }
    }
}