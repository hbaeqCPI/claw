using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Trademark
{
    public class TmkCEFeeDetail : BaseEntity
    {
        [Key]
        public int FeeDetailId { get; set; }
        public int FeeSetupId { get; set; }
        
        public int OrderOfEntry { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(3)]
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }        

        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Effective Start")]
        public DateTime? EffFromDate { get; set; }

        [Display(Name = "Effective End")]
        public DateTime? EffToDate { get; set; }

        [Display(Name = "CPI Cost")]
        public bool IsCPICost { get; set; }

        [Display(Name = "Currency Type")]        
        public string? CurrencyType { get; set; }


        public TmkCEFee? TmkCEFee { get; set; }
        public CurrencyType? SharedCurrencyType { get; set; }
    }
}
