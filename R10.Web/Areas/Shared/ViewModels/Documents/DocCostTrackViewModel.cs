using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocPatCostViewModel : DocCostViewModel
    {
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        public string? ClientName { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        public string? Status { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Patent No.")]
        public string? PatNumber { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

        [Display(Name = "Application Title")]
        public string? AppTitle { get; set; }
    }

    public class DocPatCostInvViewModel : DocCostViewModel
    {
        public string? CaseNumber { get; set; }

        public string? ClientName { get; set; }

        public string? DisclosureStatus { get; set; }

        [Display(Name = "Invention Title")]
        public string? InvTitle { get; set; }
    }

    public class DocTmkCostViewModel : DocCostViewModel
    {
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        public string? ClientName { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        public string? Status { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Registration No.")]
        public string? RegNumber { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime? RegDate { get; set; }

        [Display(Name = "Trademark Name")]
        public string? TrademarkName { get; set; }
    }

    public class DocGMCostViewModel : DocCostViewModel
    {
        public string? CaseNumber { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        public string? ClientName { get; set; }

        [Display(Name = "Status")]
        public string? MatterStatus { get; set; }

        [Display(Name = "Matter Type")]
        public string? MatterType { get; set; }

        [Display(Name = "Matter Title")]
        public string? MatterTitle { get; set; }

        [Display(Name = "Effective Open Date")]
        public DateTime? EffectiveOpenDate { get; set; }

        [Display(Name = "Termination/End Date")]
        public DateTime? TerminationEndDate { get; set; }

        [Display(Name = "Result/Royalty Description")]
        public string? ResultRoyalty { get; set; }
    }

    public class DocCostViewModel
    {
        [Required, StringLength(30), Display(Name = "Cost Type")]
        public string? CostType { get; set; }

        [Required, Display(Name = "Invoice Date")]
        public DateTime InvoiceDate { get; set; }

        [Required, StringLength(50), Display(Name = "Invoice Number")]
        public string? InvoiceNumber { get; set; }

        [Display(Name = "Invoice Amount")]
        public decimal InvoiceAmount { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime? PayDate { get; set; }

        [Display(Name = "Currency Type")]
        public string? CurrencyType { get; set; }

        [Display(Name = "Exchange Rate")]
        public double? ExchangeRate { get; set; }

        [Display(Name = "Net Cost")]
        public double NetCost { get; set; }
    }
}
