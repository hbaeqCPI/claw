using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventorAwardsMassUpdateViewModel
    {
        public int? MassUpdateInventorId { get; set; }
        public int? MassUpdateAppId { get; set; }
        public int? MassUpdateInvId { get; set; }
        public string? Controller { get; set; } = "PatInventorAward";
        public DateTime? MassUpdatePaymentDate { get; set; }
        public string? MassUpdateAwardType { get; set; }
        public DateTime? MassUpdateAwardDateFrom { get; set; }
        public DateTime? MassUpdateAwardDateTo { get; set; }
        public string? MassUpdatePaymentDateOption { get; set; }
        public DateTime? MassUpdateSpecificPaymentDate { get; set; }
        public string? MassUpdateInventionInventorOnly { get; set; }
    }
}
