namespace R10.Web.Api.Models
{
    public class TmkCostTrackingData : CostTrackingData
    {
        //tmk info
        public string? TrademarkName { get; set; }
        public string? CaseType { get; set; }
        public string? TrademarkStatus { get; set; }
        public string? AppNumber { get; set; }
        public DateTime? FilDate { get; set; }
    }
}
