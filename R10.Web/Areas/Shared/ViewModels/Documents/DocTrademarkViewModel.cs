using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocTrademarkViewModel
    {
        public int TmkId { get; set; }

        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Mark Type")]
        public string? MarkType { get; set; }

        public string? ClientName { get; set; }

        public string? OwnerName { get; set; }

        public string? AgentName { get; set; }        

        [Display(Name = "Attorney 1")]
        public string? Attorney1 { get; set; }
        [Display(Name = "Attorney 2")]
        public string? Attorney2 { get; set; }
        [Display(Name = "Attorney 3")]
        public string? Attorney3 { get; set; }
        [Display(Name = "Attorney 4")]
        public string? Attorney4 { get; set; }
        [Display(Name = "Attorney 5")]
        public string? Attorney5 { get; set; }

        [Display(Name = "Trademark Name")]
        public string? TrademarkName { get; set; }

        [Display(Name = "Status")]
        public string? TrademarkStatus { get; set; }

        [Display(Name = "Status Date")]
        public DateTime? TrademarkStatusDate { get; set; }

        [Display(Name = "Priority Country")]
        public string? PriCountry { get; set; }

        [Display(Name = "Priority No.")]
        public string? PriNumber { get; set; }

        [Display(Name = "Priority Date")]
        public DateTime? PriDate { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNumber { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

        [Display(Name = "Registration No.")]
        public string? RegNumber { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime? RegDate { get; set; }

        [Display(Name = "Last Renewal Date")]
        public DateTime? LastRenewalDate { get; set; }

        [Display(Name = "Next Renewal Date")]
        public DateTime? NextRenewalDate { get; set; }

        [Display(Name = "Last Renewal No.")]       
        public string? LastRenewalNumber { get; set; }

        [Display(Name = "Parent Application Number")]
        public string? ParentAppNumber { get; set; }

        [Display(Name = "Parent Filing Date")]
        public DateTime? ParentFilDate { get; set; }
    }
}
