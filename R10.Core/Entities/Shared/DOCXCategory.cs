using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    public partial class DOCXCategory: BaseEntity
    {
        [Key]
        public int DOCXCatId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "DOCX Category")]
        public string? DOCXCatDesc { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public int? EfsDocId { get; set; }

        //public List<DOCXEntitySetting>? DOCXEntitySettings { get; set; }
        public List<DOCXMain>? DOCXMains { get; set; }
    }
}
