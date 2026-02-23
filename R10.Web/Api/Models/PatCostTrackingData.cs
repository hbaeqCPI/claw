namespace R10.Web.Api.Models
{
    public class PatCostTrackingData : CostTrackingData
    {
        //app info
        public string? AppTitle { get; set; }
        public string? CaseType { get; set; }
        public string? ApplicationStatus { get; set; }
        public string? AppNumber { get; set; }
        public DateTime? FilDate { get; set; }
    }
}
