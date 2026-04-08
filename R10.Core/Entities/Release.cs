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
    }
}
