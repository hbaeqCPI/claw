using R10.Web.Helpers;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocVerificationActionDocPrintViewModel
    {
        [Display(Name = "Base Date")]
        public DateTime? BaseDate { get; set; }
        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }
        [Display(Name = "Document")]
        public string? DocNames { get; set; }
        [Display(Name = "Case Number")]
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }
        public string? Status { get; set; }
        [Display(Name = "Application Number")]
        public string? AppNumber { get; set; }
        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }
        public string? System { get; set; }
        [Display(Name = "Due Dates")]
        public string? DueDates { get; set; }
    }

}



