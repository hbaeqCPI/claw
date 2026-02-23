using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    public partial class DataQueryCategory : BaseEntity
    {
        [Key]
        public int DQCatId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Custom Query Category")]
        public string? DQCat { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Description")]
        public string? DQCatDesc { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public List<DataQueryMain>? DataQueryMains { get; set; }
    }
}
