using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DesCaseTypeCopyViewModel
    {
        public int DesCaseTypeID { get; set; }

        [Display(Name = "Intl Code")]
        [StringLength(5)]
        [Required]
        public string IntlCode { get; set; }

        [Display(Name = "Case Type")]
        [StringLength(3)]
        [Required]
        public string CaseType { get; set; }
    }
}
