using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionInventorRemunerationMassUpdateViewModel
    {
        public int? MassUpdateRemunerationId { get; set; }
        public int? MassUpdateInvId { get; set; }
        public string? Controller { get; set; } = "PatInventorAward";
        [Required]
        [Display(Name = "Payment Date")]
        public DateTime? MassUpdatePaymentDate { get; set; }
        public string? MassUpdateAwardType { get; set; }
        [Required]
        [Display(Name = "Award Year From")]
        public int? MassUpdateAwardDateFrom { get; set; }
        [Required]
        [Display(Name = "Award Year To")]
        public int? MassUpdateAwardDateTo { get; set; }
        public string? MassUpdatePaymentDateOption { get; set; }
        public DateTime? MassUpdateSpecificPaymentDate { get; set; }
        public string? MassUpdatePaymentInventorOption { get; set; }
        public int? MassUpdateSpecificInventorID { get; set; }
    }
}
