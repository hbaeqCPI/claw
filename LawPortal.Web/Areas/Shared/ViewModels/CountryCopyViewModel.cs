using System.ComponentModel.DataAnnotations;

namespace LawPortal.Web.Areas.Shared.ViewModels
{
    public class CountryCopyViewModel
    {
        public string OriginalCountry { get; set; }

        [Display(Name = "New Country")]
        [StringLength(5)]
        [Required]
        public string Country { get; set; }
    }
}
