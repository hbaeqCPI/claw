using R10.Core.Entities.Trademark;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    public class AgentCEFee : AgentCEFeeDetail
    {
        public Agent? Agent { get; set; }
        public CurrencyType? SharedCurrencyType { get; set; }

        [NotMapped]
        [Display(Name = "System Type")]
        public string? SystemTypeName { get; set; }

        [NotMapped]
        [Display(Name = "Fee Factors")]
        public string? CostFactors { get; set; }
    }

    public class AgentCEFeeDetail : BaseEntity
    {
        [Key]
        public int FeeID { get; set; }        

        public int AgentID { get; set; }                      
        
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "System Type")]
        public string? SystemType { get; set; }

        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Currency Type")]        
        public string? CurrencyType { get; set; }

        [Required]
        [Display(Name = "Fee Type")]        
        public string CostType { get; set; }

        public string? CostFormula { get; set; }
        public int? CostFactor1 { get; set; }
        public decimal? CostFactor2 { get; set; }
        public int? CostFactor3 { get; set; }        
        public string? OriginatingLanguage { get; set; }
        public AgentCETranslationType TranslationType { get; set; }
    }

    public enum AgentCETranslationType
    {
        [Display(Name = "Pages")]
        Pages,
        [Display(Name = "Words")]
        Words
    }
}
