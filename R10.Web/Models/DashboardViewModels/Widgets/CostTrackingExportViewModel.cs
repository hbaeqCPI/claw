using System.ComponentModel.DataAnnotations;

namespace R10.Web.Models.DashboardViewModels
{
    public class CostTrackingExportViewModel
    {
        [Display(Name = "Case Number")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Cost Type")]
        public string? CostType { get; set; }

        [Display(Name = "Invoice Date")]
        public DateTime? InvoiceDate { get; set; }

        [Display(Name = "Invoice Amount")]
        public decimal InvoiceAmount { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime? PayDate { get; set; }
    }
}