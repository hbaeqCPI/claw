using System.ComponentModel.DataAnnotations;

namespace LawPortal.Web.Areas.Shared.ViewModels
{
    public class AreaCopyViewModel
    {
        public string OriginalArea { get; set; }

        [Display(Name = "New Area")]
        [StringLength(10)]
        [Required]
        public string Area { get; set; }

        [Display(Name = "Countries")]
        public bool CopyCountries { get; set; }
    }
}
