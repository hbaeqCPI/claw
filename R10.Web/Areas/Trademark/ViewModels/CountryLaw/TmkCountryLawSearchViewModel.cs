using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Trademark.ViewModels.CountryLaw
{
    public class TmkCountryLawSearchViewModel
    {
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Systems")]
        public string? Systems { get; set; }

        [Display(Name = "User ID")]
        public string? UserID { get; set; }

        [Display(Name = "Created On")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Updated On")]
        public DateTime? LastUpdate { get; set; }
    }
}
