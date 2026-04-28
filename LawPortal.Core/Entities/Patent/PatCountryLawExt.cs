using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Patent
{
    public class PatCountryLawExt
    {
        [Display(Name = "Country")]
        [StringLength(5)]
        public string? Country { get; set; }

        [Display(Name = "Case Type")]
        [StringLength(3)]
        public string? CaseType { get; set; }

        [Display(Name = "Label Tax Sched")]
        [StringLength(20)]
        public string? LabelTaxSched { get; set; }

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        [NotMapped]
        public bool IsNewRecord { get; set; }

        [NotMapped]
        public string? OriginalSystems { get; set; }
    }
}
