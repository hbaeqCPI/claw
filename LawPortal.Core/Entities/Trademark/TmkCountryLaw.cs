using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Trademark
{
    public class TmkCountryLaw : ClawBaseEntity
    {
        [NotMapped]
        public string? CopyOptions { get; set; }

        [NotMapped]
        public string? OriginalSystems { get; set; }

        [Key]
        [StringLength(5)]
        [Display(Name = "Country")]
        public string Country { get; set; } = "";

        [Key]
        [StringLength(3)]
        [Display(Name = "Case Type")]
        public string CaseType { get; set; } = "";

        [StringLength(5)]
        [Display(Name = "Agent")]
        public string DefaultAgent { get; set; } = "";

        public string Remarks { get; set; } = "";

        public string UserRemarks { get; set; } = "";

        /// <summary>
        /// Free-form internal notes for the editor's own use. Never exported to
        /// MDB, never rendered in release PDFs — purely informational.
        /// </summary>
        [Display(Name = "Internal Remarks")]
        public string? InternalRemarks { get; set; }

        [Display(Name = "Auto Gen Des Ctry")]
        public bool AutoGenDesCtry { get; set; }

        [Display(Name = "Auto Update Des Tmk Recs")]
        public bool AutoUpdtDesTmkRecs { get; set; }

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        public List<TmkCountryDue>? TmkCountryDues { get; set; }
    }
}
