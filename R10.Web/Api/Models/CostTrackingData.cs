namespace R10.Web.Api.Models
{
    public class CostTrackingData
    {
        public int CostTrackId { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? CountryName { get; set; }
        public string? SubCase { get; set; }

        public string? CostType { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? InvoiceNumber { get; set; }
        public decimal InvoiceAmount { get; set; }
        public DateTime? PayDate { get; set; }

        //entities
        public string? AgentCode { get; set; }
        public string? AgentName { get; set; }

        public string? Remarks { get; set; }

        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}
