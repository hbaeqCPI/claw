using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkCountry : ClawBaseEntity
    {
        [Key]
        [StringLength(5)]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [StringLength(50)]
        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }

        [StringLength(5)]
        [Display(Name = "CPI Code")]
        public string? CPICode { get; set; }

        public List<TmkAreaCountry>? TmkCountryAreas { get; set; }
    }
}
