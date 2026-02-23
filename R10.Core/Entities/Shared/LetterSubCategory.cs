using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    public partial class LetterSubCategory: BaseEntity
    {
        [Key]
        public int LetSubCatId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Letter Sub Category")]
        public string? LetSubCat { get; set; }
        [StringLength(50)]
        [Display(Name = "Description")]
        public string? LetSubCatDesc { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public List<LetterMain>? LetterMains { get; set; }
    }
}
