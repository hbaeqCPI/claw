using R10.Web.Helpers;
using System.ComponentModel.DataAnnotations;
using static R10.Web.Helpers.ImageHelper;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DocVerificationActionDocZoomViewModel
    {
        public int ActId { get; set; }
        public int ParentId { get; set; }
        public int InvId { get; set; }

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
        [Display(Name = "Base Date")]
        public DateTime? BaseDate { get; set; }
        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        public bool CanVerifyAction { get; set; }
        public string? CreatedBy { get; set; }
        [Display(Name = "Action Verified By")]
        public string? VerifiedBy { get; set; }
        [Display(Name = "Action Verified Date")]
        
        public DateTime? DateVerified { get; set; }
        [StringLength(450)]
        public string? VerifierId { get; set; }
        
        public string? DocName { get; set; }
        public string? System { get; set; }
        public string? ScreenCode { get; set; }
        public string? DocFileName { get; set; }
        public CPiSavedFileType FileType { get; set; }
    }
}