using R10.Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.AMS.ViewModels
{
    /// <summary>
    /// Stub view model for AMS Due List (AMS module removed).
    /// Retained for GridAnnuities shared component compatibility.
    /// </summary>
    public class AMSDueListViewModel : BaseEntity
    {
        public int DueId { get; set; }
        public int AnnId { get; set; }
        public string? CPIClientCode { get; set; }

        [Display(Name = "No.")]
        public string? AnnuityNoDue { get; set; }
        [Display(Name = "Due Date")]
        public DateTime? AnnuityDueDate { get; set; }
        [Display(Name = "Amount")]
        public decimal AnnuityCost { get; set; }
        [Display(Name = "Cost to Expire")]
        public decimal CostToExpiration { get; set; }

        public string? ClientInstructionType { get; set; }
        public string? ClientInstruction { get; set; }
        public DateTime? ClientInstructionDate { get; set; }

        [Display(Name = "CPI Instruction")]
        public string? CPIInstructionType { get; set; }
        [Display(Name = "CPI Payment Date")]
        public DateTime? CPIPaymentDate { get; set; }
        [Display(Name = "CPI Receipt Post Date")]
        public DateTime? CPIReceiptPostDate { get; set; }
        [Display(Name = "Paid Thru")]
        public string? PaidThru { get; set; }
        [Display(Name = "Mark Deleted")]
        public bool IgnoreRecord { get; set; }
        public string? AnnuityCostFlag { get; set; }
    }
}
