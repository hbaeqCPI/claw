using System.ComponentModel.DataAnnotations;

namespace LawPortal.Web.Areas.Shared.ViewModels
{
    public class CaseTypeCopyViewModel
    {
        public string OriginalCaseType { get; set; }

        [Display(Name = "New Case Type")]
        [StringLength(3)]
        [Required]
        public string CaseType { get; set; }
    }
}
