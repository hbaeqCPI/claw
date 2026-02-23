using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMMatterType : BaseEntity
    {
        public int MatterTypeID { get; set; }

        [Key]
        [Required]
        [StringLength(20)]
        [Display(Name = "Matter Type")]
        public string? MatterType { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "CPI Matter Type")]
        public bool CPIMatterType { get; set; }

        public List<GMMatter>? GMMatters { get; set; }
    }
}
