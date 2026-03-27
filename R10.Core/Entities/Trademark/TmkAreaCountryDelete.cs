using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkAreaCountryDelete
    {
        [Display(Name = "Area")]
        [StringLength(10)]
        public string? Area { get; set; }

        [Display(Name = "Country")]
        [StringLength(5)]
        public string? Country { get; set; }

        [Display(Name = "Area New")]
        [StringLength(10)]
        public string? AreaNew { get; set; }

        [Display(Name = "Country New")]
        [StringLength(5)]
        public string? CountryNew { get; set; }

    }
}