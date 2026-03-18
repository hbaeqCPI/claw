using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DesCaseTypeFieldsCopyViewModel
    {
        public int KeyID { get; set; }

        [Display(Name = "Des Case Type")]
        [StringLength(3)]
        [Required]
        public string DesCaseType { get; set; }
    }
}
