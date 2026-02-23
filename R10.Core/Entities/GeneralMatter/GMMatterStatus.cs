using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMMatterStatus : BaseEntity
    {
        public int MatterStatusID { get; set; }

        [Key]
        [Required]
        [StringLength(20)]
        [Display(Name = "Matter Status")]
        public string? MatterStatus { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Active?")]
        public bool ActiveSwitch { get; set; } = true;

        public List<GMMatter>? GMMatters { get; set; }
    }
}
