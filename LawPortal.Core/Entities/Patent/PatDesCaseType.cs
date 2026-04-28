using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Patent
{
    public class PatDesCaseType
    {
        [StringLength(10)]
        public string? IntlCode { get; set; }

        [StringLength(3)]
        [Display(Name = "Parent Case Type")]
        public string? CaseType { get; set; }

        [StringLength(5)]
        [Display(Name = "Des Country")]
        [Required]
        public string? DesCountry { get; set; }

        [StringLength(3)]
        [Display(Name = "Des Case Type")]
        [Required]
        public string? DesCaseType { get; set; }

        [Display(Name = "Default?")]
        [Column("Default")]
        public bool Default { get; set; }

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        [NotMapped]
        public bool IsNewRecord { get; set; }

        [NotMapped]
        public string? OriginalSystems { get; set; }

        [NotMapped]
        public byte[]? ParentTStamp { get; set; }
    }
}
