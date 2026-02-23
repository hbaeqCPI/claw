using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    public partial class LetterCategory: BaseEntity
    {
        [Key]
        public int LetCatId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Letter Category")]
        public string? LetCatDesc { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public string? Systems { get; set; }

        public List<LetterEntitySetting>? LetterEntitySettings { get; set; }
        public List<LetterMain>? LetterMains { get; set; }
    }
}
