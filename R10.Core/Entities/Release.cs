using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class Release : BaseEntity
    {
        [Key]
        public int ReleaseId { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Year")]
        public int Year { get; set; }

        [Required]
        [StringLength(2)]
        [Display(Name = "Quarter")]
        public string Quarter { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "System Type")]
        public string SystemType { get; set; }

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        [Display(Name = "Generate Patent")]
        public bool GeneratePatent { get; set; } = true;

        [Display(Name = "Generate Trademark")]
        public bool GenerateTrademark { get; set; } = true;

        /// <summary>
        /// Free-form notes rendered at the top of the patent release-report PDF.
        /// Used for authoring-time announcements that aren't driven by MDB diffs
        /// (e.g. "all Opposition actions renamed to Opposition Period Ends").
        /// </summary>
        [Display(Name = "Patent Report Notes")]
        public string? ReportNotesPatent { get; set; }

        /// <summary>
        /// Free-form notes rendered at the top of the trademark release-report PDF.
        /// </summary>
        [Display(Name = "Trademark Report Notes")]
        public string? ReportNotesTrademark { get; set; }
    }
}
