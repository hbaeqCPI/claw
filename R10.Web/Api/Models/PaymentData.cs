using System.ComponentModel.DataAnnotations;

namespace R10.Web.Api.Models
{
    public class PaymentData
    {
        public string? CPIClientCode { get; set; }

        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? CountryName { get; set; }
        public string? SubCase { get; set; }

        public string? CPICaseType { get; set; }
        public string? CPITitle { get; set; }

        public string? CPIAgent { get; set; }
        public string? CPIAgentRef { get; set; }
        public string? CPIClient { get; set; }
        public string? CPIClientRef { get; set; }
        public string? CPIAttorney { get; set; }

        public string? CPIAppNo { get; set; }
        public string? CPIPatNo { get; set; }
        public string? CPIPubNo { get; set; }

        public DateTime? AnnuityDueDate { get; set; }
        public string? AnnuityNoDue { get; set; }

        public DateTime? CPIPaymentDate { get; set; }
        public decimal CPIInvoiceAmount { get; set; }
        public DateTime? CPIReceiptPostDate { get; set; }
    }
}
