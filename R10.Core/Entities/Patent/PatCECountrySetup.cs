using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class PatCECountrySetup: BaseEntity
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

        [StringLength(8)]
        [Display(Name = "Entity Status")]
        public string? EntityStatus { get; set; }

        [Display(Name = "Currency Type")]
        [Required]
        public string? CurrencyType { get; set; }

        [Display(Name = "Default?")]
        public bool IsDefault { get; set; }

        [Display(Name = "Fees as of")]
        public DateTime? FeesEffDate { get; set; }

        public PatCaseType? PatCaseType { get; set; }
        public PatCountry? PatCountry { get; set; }
        public CurrencyType? PatCurrencyType { get; set; }
        
        public List<PatCECountryCost>? PatCECountryCosts { get; set; }
        public List<PatCostEstimatorCost>? PatCostEstimatorCosts { get; set; }


        [NotMapped]
        public string? CopyOptions { get; set; }
    }
    
}
