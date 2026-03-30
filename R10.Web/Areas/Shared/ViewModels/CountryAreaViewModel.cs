using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class CountryAreaViewModel
    {
        [Required]
        [UIHint("Country")]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Required(ErrorMessage = "Area is required.")]
        [Display(Name = "Area")]
        public string? Area { get; set; }

        [Display(Name = "Description")]
        public string? AreaDescription { get; set; }

        public CountryLookupViewModel? CountryLookup { get; set; }
    }
}
