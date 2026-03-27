using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Trademark
{
    public class TmkDesCaseType
    {
        [StringLength(5)]
        public string? IntlCode { get; set; }

        [StringLength(3)]
        public string? CaseType { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        public string? DesCountry { get; set; }

        [StringLength(3)]
        [Display(Name = "Case Type")]
        public string? DesCaseType { get; set; }

        [Display(Name = "Default")]
        [Column("Default")]
        public bool Default { get; set; }
    }
}
