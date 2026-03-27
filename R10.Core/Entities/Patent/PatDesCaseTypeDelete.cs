using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatDesCaseTypeDelete
    {
        [Display(Name = "Intl Code")]
        [StringLength(10)]
        public string? IntlCode { get; set; }

        [Display(Name = "Case Type")]
        [StringLength(3)]
        public string? CaseType { get; set; }

        [Display(Name = "Des Country")]
        [StringLength(5)]
        public string? DesCountry { get; set; }

        [Display(Name = "Des Case Type")]
        [StringLength(3)]
        public string? DesCaseType { get; set; }

        [Display(Name = "Default")]
        public bool Default { get; set; }

        [Display(Name = "Intl Code New")]
        [StringLength(10)]
        public string? IntlCodeNew { get; set; }

        [Display(Name = "Case Type New")]
        [StringLength(3)]
        public string? CaseTypeNew { get; set; }

        [Display(Name = "Des Country New")]
        [StringLength(5)]
        public string? DesCountryNew { get; set; }

        [Display(Name = "Des Case Type New")]
        [StringLength(3)]
        public string? DesCaseTypeNew { get; set; }

    }
}