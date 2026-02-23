using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatInventorInv: PatInventorInvDetail
    {
        public PatInventor? InventorInvInventor { get; set; }

        public Invention? InventorInvInvention { get; set; }
        public List<PatIRDistribution>? Distributions { get; set; }
        public PatIRRemuneration? Remuneration { get; set; }
        public PatIREmployeePosition? EmployeePosition { get; set; }

        public PatIRFRRemuneration? FRRemuneration { get; set; }
        public List<PatIRFRDistribution>? FRDistributions { get; set; }

    }

    public class PatInventorInvDetail : BaseEntity
    {
        [Key]
        public int InventorInvID { get; set; }

        public int InvId { get; set; }
        public int? RemunerationId { get; set; }

        public int InventorID { get; set; }

        public int? PositionId { get; set; }
        [Display(Name = "A")]
        public int? PositionA { get; set; }
        [Display(Name = "B")]
        public int? PositionB { get; set; }
        [Display(Name = "C")]
        public int? PositionC { get; set; }

        public int OrderOfEntry { get; set; }

        public string? Remarks { get; set; }
        [Display(Name = "% of Invention")]
        [Range(0, 100, ErrorMessage = "Enter number between 0 to 100")]
        public double? Percentage { get; set; }
        [Display(Name = "Paid By Lump Sum?")]
        public Boolean PaidByLumpSum { get; set; }
        [Display(Name = "Lump Sum Amount")]
        public double? LumpSumAmount { get; set; }
        [Display(Name = "Lump Sum Paid Date")]
        public DateTime? LumpSumPaidDate { get; set; }

        
        [Display(Name = "Initial Payment")]
        public double? InitialPayment { get; set; }
        public bool? IsApplicant { get; set; }
        public bool EligibleForBasicAward { get; set; }
        [Display(Name = "Remarks")]
        public string? RemunerationRemarks { get; set; }

        [Display(Name = "Buying Rights Amount")]
        public double? BuyingRightsAmount { get; set; }

        [Display(Name = "Buying Rights Date")]
        public DateTime? BuyingRightsDate { get; set; }

        public int? FRRemunerationId { get; set; }
        [Display(Name = "Invention Report Award")]
        public double? FRFirstPayment { get; set; }
        [Display(Name = "First filing Award")]
        public double? FRSecondPayment { get; set; }
        [Display(Name = "Use (first sell) Award")]
        public double? FRThirdPayment { get; set; }
        [Display(Name = "Payment Date")]
        public DateTime? FRFirstPaymentDate { get; set; }
        [Display(Name = "Payment Date")]
        public DateTime? FRSecondPaymentDate { get; set; }
        [Display(Name = "Payment Date")]
        public DateTime? FRThirdPaymentDate { get; set; }

        [Display(Name = "Claimed Date")]
        public DateTime? ClaimedDate { get; set; }
        [Display(Name = "Initial Payment Date")]
        public DateTime? InitialPaymentDate { get; set; }

    }
}
