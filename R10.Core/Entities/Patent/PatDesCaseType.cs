using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class PatDesCaseType
    {
        [StringLength(10)]
        public string? IntlCode { get; set; }

        [StringLength(3)]
        public string? CaseType { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        [Required]
        public string? DesCountry { get; set; }

        [StringLength(3)]
        [Display(Name = "Case Type")]
        [Required]
        public string? DesCaseType { get; set; }

        [Display(Name = "Default?")]
        [Column("Default")]
        public bool Default { get; set; }

        [NotMapped]
        public byte[]? ParentTStamp { get; set; }
    }
}
