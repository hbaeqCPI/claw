using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatCountry : ClawBaseEntity
    {
        [StringLength(5)]
        [Required, Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(50)]
        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }

        [StringLength(5)]
        [Display(Name = "CPI Code")]
        public string? CPICode { get; set; }

        [Display(Name = "Country Paid Thru CPI")]
        public bool CountryPaidThruCPI { get; set; }

        public List<PatAreaCountry>? PatCountryAreas { get; set; }
    }
}
