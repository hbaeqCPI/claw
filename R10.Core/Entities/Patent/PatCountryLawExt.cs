using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
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

    }
}