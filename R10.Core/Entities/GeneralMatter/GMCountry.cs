using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMCountry : BaseEntity
    {
        public int CountryID { get; set; }

        [Key]
        [Required]
        [StringLength(5)]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(50)]
        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }

        [StringLength(5)]
        [Display(Name = "CPI Code")]
        public string? CPICode { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public List<GMMatterCountry>? GMMatterCountries { get; set; }
        public List<GMAreaCountry>? GMAreaCountries { get; set; }

        public List<GMOtherParty>? CountryOtherParties { get; set; }
        public List<GMOtherParty>? POCountryOtherParties { get; set; }

        public List<GMActionType>? GMActionTypes { get; set; }
        public List<GMBudgetManagement>? GMBudgetManagements { get; set; }
    }
}
