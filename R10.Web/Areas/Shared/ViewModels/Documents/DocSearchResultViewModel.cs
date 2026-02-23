using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocSearchResultViewModel
    {
        public string? DataKey { get; set; }
        public int DataKeyValue { get; set; }

    }

    public class DocumentInventionResultsViewModel : DocSearchResultViewModel
    {
        public int InvId { get; set; }

        public string? CaseNumber { get; set; }

        [Display(Name = "Family Number")]
        public string? FamilyNumber { get; set; }

        [Display(Name = "Disclosure Status")]
        public string? DisclosureStatus { get; set; }

        [Display(Name = "Title")]
        public string? InvTitle { get; set; }

        public string? ClientCode { get; set; }
    }

    public class DocumentCtryAppResultsViewModel : DocSearchResultViewModel
    {
        public int AppId { get; set; }

        public string? CaseNumber { get; set; }

        [Display(Name ="Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Status")]
        public string? ApplicationStatus { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Patent No.")]
        public string? PatNumber { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

        //[Display(Name = "Title")]
        //public string? AppTitle { get; set; }

    }

    public class DocumentActionResultsViewModel : DocSearchResultViewModel
    {
        public int ActId { get; set; }
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [Display(Name = "Base Date")]
        public DateTime BaseDate { get; set; }
    }

    public class DocumentCostResultsViewModel : DocSearchResultViewModel
    {

        public int CostTrackId { get; set; }
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Cost Type")]
        public string? CostType { get; set; }

        [Display(Name = "Invoice Date")]
        public DateTime? InvoiceDate { get; set; }

        [Display(Name = "Invoice Amount")]
        public decimal InvoiceAmount { get; set; }

    }

    public class DocumentTrademarkResultsViewModel : DocSearchResultViewModel
    {
        public int TmkId { get; set; }

        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Status")]
        public string? TrademarkStatus { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Registration No.")]
        public string? RegNumber { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime? RegDate { get; set; }        

    }

    public class DocumentGeneralMatterResultsViewModel : DocSearchResultViewModel
    {
        public int MatId { get; set; }

        public string? CaseNumber { get; set; }       

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Matter Type")]
        public string? MatterType { get; set; }

        [Display(Name = "Status")]
        public string? MatterStatus { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        public string? ClientCode { get; set; }

        [Display(Name = "Title")]
        public string? MatterTitle { get; set; }

    }

}
