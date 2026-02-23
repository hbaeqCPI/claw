namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventorAwardsSearchViewModel
    {
        public int SearchInventorId { get; set; }
        public string? SearchSystemType { get; set; }
        public string? SearchCaseNumber { get; set; }
        public string? SearchCountry { get; set; }
        public string? SearchSubCase { get; set; }
        public decimal? SearchAmountFrom { get; set; }
        public decimal? SearchAmountTo { get; set; }
        public string? SearchAwardType { get; set; }
        public DateTime? SearchAwardDateFrom { get; set; }
        public DateTime? SearchAwardDateTo { get; set; }
        public DateTime? SearchPaymentDateFrom { get; set; }
        public DateTime? SearchPaymentDateTo { get; set; }
    }
}
