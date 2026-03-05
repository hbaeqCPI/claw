using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{

    public class PatCountry:BaseEntity
    {
        public int CountryID { get; set; }

        [Key]
        [Required]
        [StringLength(5)]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [StringLength(50)]
        [Display(Name ="Country Name")]
        public string? CountryName { get; set; }

        [StringLength(5)]
        [Display(Name = "CPI Code")]
        public string? CPICode { get; set; }

        public bool CountryPaidThruCPi { get; set; }

        [StringLength(20)]
        [Display(Name = "Tax Schedule Label")]
        public string? LabelTaxSched { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [Display(Name = "Currency Type")]
        public string? CurrencyType { get; set; }

        public List<PatAreaCountry>? PatCountryAreas { get; set; }
        public List<PatActionType>? PatActionTypes { get; set; }

        public List<PatCountryLaw>? PatCountryLaws { get; set; }

        public List<PatDesCaseType>? ParentPatDesCaseTypes { get; set; }
        public List<PatDesCaseType>? ChildPatDesCaseTypes { get; set; }

        public List<PatDesignatedCountry>? PatDesignatedCountries { get; set; }
    }
}
