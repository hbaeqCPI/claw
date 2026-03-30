using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        [NotMapped]
        public bool IsNewRecord { get; set; }

        [NotMapped]
        public string? OriginalSystems { get; set; }

        public List<TmkAreaCountry>? TmkCountryAreas { get; set; }
    }
}
