using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class PatCEAnnuitySetup: BaseEntity
    {
        [Key]
        public int CEAnnuityId { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        [Required]
        public string Country { get; set; }

        public string? CaseType { get; set; }

        [StringLength(8)]
        [Display(Name = "Entity Status")]
        public string? EntityStatus { get; set; }

        [Display(Name = "Fees as of")]
        public DateTime? FeesEffDate { get; set; }

        [Display(Name = "Currency Type")]
        [Required]
        public string? CurrencyType { get; set; }

        public CurrencyType? PatCurrencyType { get; set; }
        public PatCountry? PatCountry { get; set; }

        [NotMapped]
        public string? CopyOptions { get; set; }

        [NotMapped]
        public string[]? CaseTypeList { get; set; }

        public List<PatCEAnnuityCost>? PatCEAnnuityCosts { get; set; }
    }
    
}
