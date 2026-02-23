using R10.Web.Helpers;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocVerificationCommDocPrintViewModel
    {
        [Display(Name = "Document")]
        public string? DocName { get; set; }
        [Display(Name = "Case Number")]
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }
        [Display(Name = "Responsible (Reporting)")]
        public string? RespReporting { get; set; }
        [Display(Name = "System")]
        public string? System { get; set; }
        [Display(Name = "Uploaded Date")]
        public DateTime? UploadedDate { get; set; }
    }

}
