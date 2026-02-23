using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public partial class ProductCategory : BaseEntity
    {
        public int ProductCategoryId { get; set; }

        [Key]
        [Required]
        [StringLength(25)]
        [Display(Name = "Product Category")]
        public string?  ProductCategoryName { get; set; }

        [Display(Name = "Description")]
        public string?  Description { get; set; }

    }
}
