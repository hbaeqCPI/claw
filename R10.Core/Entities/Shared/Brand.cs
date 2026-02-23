using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class Brand : BaseEntity
    {
        public int BrandId { get; set; }

        [Key]
        [Required]
        [StringLength(25)]
        [Display(Name = "Brand")]
        public string?  BrandName { get; set; }

        [Display(Name = "Brand Type")]
        public string?  BrandType { get; set; }

        [Display(Name = "Remarks")]
        public string?  Remarks { get; set; }
    }

}
