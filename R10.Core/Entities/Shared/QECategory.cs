using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    public partial class QECategory : BaseEntity
    {
        [Key]
        public int QECatId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Quick Email Category")]
        public string? QECat { get; set; }
        [StringLength(50)]
        [Display(Name = "Description")]
        public string? QECatDesc { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public List<QEMain>? QEMains { get; set; }
    }
}
