using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatCostEstimatorCountry: BaseEntity
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

        [Display(Name = "Entity Status")]
        public string? EntityStatus { get; set; }

        public DateTime? FeesEffDate { get; set; }
        public string? CurrencyType { get; set; }
        public double? ExchangeRate { get; set; }
        public double? AllowanceRate { get; set; }

        public PatCostEstimator? PatCostEstimator { get; set; }
        public PatCountry? PatCountry { get; set; }
    }
}
