using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class QuickEmailCopyViewModel
    {
        [Required]
        public int QESetupID { get; set; }
        [Required]
        public string? TemplateName { get; set; }
        //public int OldQESetupID { get; set; }
        public bool CopyMainInfo { get; set; } = true;
        public bool CopyLayout { get; set; } = true;
        public bool CopyRecipients { get; set; } = true;
        public bool CopyRemarks { get; set; } = true;
    }
}
