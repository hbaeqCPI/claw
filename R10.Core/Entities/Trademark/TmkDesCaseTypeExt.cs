using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkDesCaseTypeExt : BaseEntity
    {
        [Display(Name = "Intl Code")]
        [StringLength(5)]
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

        [Display(Name = "Gen App")]
        public bool GenApp { get; set; }

    }
}