using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using R10.Core.Entities.Patent;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionInventorRemunerationInventorInfoViewModel : PatInventorInv
    {
        public string? Inventor { get; set; }
        [Display(Name = "Employee Title")]
        public string? Position { get; set; }
        //public decimal? Percentage { get; set; }
        public int? SumABC { get; set; }
        [Display(Name = "% of Ownership")]
        public int? InventorPosition { get; set; }
        [NotMapped]
        public bool IsDirty { get; set; }
    }

    public class RemunerationInventorInfoExportViewModel
    {
        [Display(Name = "Inventor")]
        public string? Inventor { get; set; }
        [Display(Name = "Employee Title")]
        public string? Position { get; set; }
        [Display(Name = "Claimed Date")]
        public DateTime? ClaimedDate { get; set; }
        [Display(Name = "% of Ownership")]
        public int? InventorPosition { get; set; }
        [Display(Name = "% of Invention")]
        public double? Percentage { get; set; }
        [Display(Name = "A")]
        public int? PositionA { get; set; }
        [Display(Name = "B")]
        public int? PositionB { get; set; }
        [Display(Name = "C")]
        public int? PositionC { get; set; }
        [Display(Name = "Buying Rights Amount")]
        public double? BuyingRightsAmount { get; set; }
        [Display(Name = "Buying Rights Date")]
        public DateTime? BuyingRightsDate { get; set; }
        [Display(Name = "Initial Payment")]
        public double? InitialPayment { get; set; }
        [Display(Name = "Initial Payment Date")]
        public DateTime? InitialPaymentDate { get; set; }
        [Display(Name = "Paid By Lump Sum?")]
        public Boolean PaidByLumpSum { get; set; }
        [Display(Name = "Lump Sum Amount")]
        public double? LumpSumAmount { get; set; }
        [Display(Name = "Lump Sum Paid Date")]
        public DateTime? LumpSumPaidDate { get; set; }
        [Display(Name = "Remarks")]
        public string? RemunerationRemarks { get; set; }
    }
}
