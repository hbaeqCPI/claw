using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class PatCountryLaw : ClawBaseEntity
    {
        [NotMapped]
        public string? CopyOptions { get; set; }

        [NotMapped]
        public string? OriginalSystems { get; set; }

        [Key]
        [StringLength(5)]
        [Display(Name = "Country")]
        [Required]
        public string Country { get; set; } = "";

        [Key]
        [StringLength(3)]
        [Display(Name = "Case Type")]
        [Required]
        public string CaseType { get; set; } = "";

        [StringLength(5)]
        [Display(Name = "Agent")]
        public string DefaultAgent { get; set; } = "";

        public string Remarks { get; set; } = "";

        [Display(Name = "Auto Gen Des Ctry")]
        public bool AutoGenDesCtry { get; set; }

        [Display(Name = "Auto Updt Des Pat Recs")]
        public bool AutoUpdtDesPatRecs { get; set; }

        [Display(Name = "Calc Exp Before Issue")]
        public bool CalcExpirBeforeIssue { get; set; }

        public string UserRemarks { get; set; } = "";

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        public List<PatCountryDue>? PatCountryDues { get; set; }
    }
}
