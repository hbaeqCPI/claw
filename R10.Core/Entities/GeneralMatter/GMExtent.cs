using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMExtent : BaseEntity
    {
        public int ExtentID { get; set; }

        [Key]
        [Required]
        [StringLength(20)]
        [Display(Name = "Extent")]
        public string Extent { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
