using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Patent
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

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        [NotMapped]
        public bool IsNewRecord { get; set; }

        [NotMapped]
        public string? OriginalSystems { get; set; }

        public List<PatAreaCountry>? PatCountryAreas { get; set; }
    }
}
