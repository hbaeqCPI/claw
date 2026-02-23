using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSFeeDetail : BaseEntity
    {
        [Key]
        public int FeeDetailId { get; set; }

        [Required]
        public int FeeSetupId { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Setup Name")]
        public string FeeSetupName { get; set; }

        public int FeeOrder { get; set; }

        [StringLength(10)]
        public string? FeeType { get; set; }

        [StringLength(5)]
        public string? Country { get; set; }

        [StringLength(3)]
        public string? CaseType { get; set; }

        [StringLength(5)]
        public string? AnnuityNoDue { get; set; }

        public decimal Amount { get; set; }

        [Display(Name = "Effective Start")]
        public DateTime? EffFromDate { get; set; }

        [Display(Name = "Effective End")]
        public DateTime? EffToDate { get; set; }

        public bool PerFamily { get; set; }

        public AMSFee? AMSFee { get; set; }
    }
}
