using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatAreaCountryDelete
    {
        [Display(Name = "Area")]
        [Required, StringLength(10)]
        public string? Area { get; set; }

        [Display(Name = "Country")]
        [Required, StringLength(5)]
        public string? Country { get; set; }

        [Display(Name = "Area New")]
        [Required, StringLength(10)]
        public string? AreaNew { get; set; }

        [Display(Name = "Country New")]
        [Required, StringLength(5)]
        public string? CountryNew { get; set; }
    }
}