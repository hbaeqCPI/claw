using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Patent
{
    public class PatDesCaseTypeDelete
    {
        [Required]
        [Display(Name = "Intl Code")]
        [StringLength(10)]
        public string? IntlCode { get; set; }

        [Required]
        [Display(Name = "Case Type")]
        [StringLength(3)]
        public string? CaseType { get; set; }

        [Required]
        [Display(Name = "Des Country")]
        [StringLength(5)]
        public string? DesCountry { get; set; }

        [Required]
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

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        [NotMapped]
        public bool IsNewRecord { get; set; }

        [NotMapped]
        public string? OriginalSystems { get; set; }
    }
}