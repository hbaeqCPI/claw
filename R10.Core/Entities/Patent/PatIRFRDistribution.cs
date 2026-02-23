using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatIRFRDistribution : BaseEntity
    {
        [Key]
        public int DistributionId { get; set; }
        [Required]
        public int InventorInvID { get; set; }
        [Display(Name = "Year")]
        [Required]
        public int? Year { get; set; }

        [Display(Name = "Amount")]
        [Required]
        public double Amount { get; set; }

        [Display(Name = "Paid Date")]
        public DateTime? PaidDate { get; set; }
        public string? Remarks { get; set; }

        public PatInventorInv? InventorInv { get; set; }
        [Display(Name = "Override Amount")]
        public bool UseOverrideAmount { get; set; }
    }
}
