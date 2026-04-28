using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Trademark
{
    public class TmkDesCaseTypeExt : BaseEntity
    {
        [Display(Name = "Intl Code")]
        [StringLength(5)]
        public string? IntlCode { get; set; }

        [Display(Name = "Parent Case Type")]
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

        [Display(Name = "Gen App")]
        public bool GenApp { get; set; }

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        [NotMapped]
        public bool IsNewRecord { get; set; }

        [NotMapped]
        public string? OriginalSystems { get; set; }
    }
}
