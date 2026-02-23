using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocActionViewModel 
    {
        public string? CaseNumber { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        public string? ClientName { get; set; }

        public string? Status { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

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

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [Display(Name = "Base Date")]
        public DateTime BaseDate { get; set; }
    }

    public class DocPatActViewModel: DocActionViewModel
    {
        [Display(Name = "Patent No.")]
        public string? PatNumber { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

        [Display(Name = "Application Title")]
        public string? AppTitle { get; set; }

    }

    public class DocTmkActViewModel : DocActionViewModel
    {
        [Display(Name = "Registration No.")]
        public string? RegNumber { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime? RegDate { get; set; }

        [Display(Name = "Trademark Name")]
        public string? TrademarkName { get; set; }
    }

    public class DocGMActViewModel
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

        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [Display(Name = "Base Date")]
        public DateTime BaseDate { get; set; }
    }

}
