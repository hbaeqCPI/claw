// using R10.Core.Entities.AMS; // Removed during deep clean
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class PatCEFeeDetail : BaseEntity
    {
        [Key]
        public int FeeDetailId { get; set; }
        public int FeeSetupId { get; set; }
        
        public int OrderOfEntry { get; set; }

        [StringLength(8)]
        [Display(Name = "Entity Status")]
        public string? EntityStatus { get; set; }

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
       

        public PatCEFee? PatCEFee { get; set; }
    }
}
