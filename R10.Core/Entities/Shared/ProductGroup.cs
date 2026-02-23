using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public partial class ProductGroup : BaseEntity
    {
        public int ProductGroupId { get; set; }

        [Key]
        [Required]
        [StringLength(25)]
        [Display(Name = "Product Group")]
        public string?  ProductGroupName { get; set; }

        [Display(Name = "Description")]
        public string?  Description { get; set; }

    }
}
