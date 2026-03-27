using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class CountryLawCopyViewModel
    {
        // Source record identifiers (hidden, not editable)
        public string? SourceCountry { get; set; }
        public string? SourceCaseType { get; set; }
        public string? SourceSystems { get; set; }

        // New record values (editable by user)
        [Display(Name = "Country")]
        [StringLength(5)]
        [Required]
        public string Country { get; set; }

        [Display(Name = "Case Type")]
        [StringLength(3)]
        [Required]
        public string CaseType { get; set; }

        public string? Systems { get; set; }

        [Display(Name = "Law Highlights")]
        public bool CopyRemarks { get; set; }

        [Display(Name = "Law Actions")]
        public bool CopyLawActions { get; set; }

        [Display(Name = "Expiration Terms")]
        public bool CopyExpirationTerms { get; set; }

        [Display(Name = "Designated Countries")]
        public bool CopyDesignatedCountries { get; set; }
    }
}
