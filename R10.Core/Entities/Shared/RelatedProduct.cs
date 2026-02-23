using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class RelatedProduct : BaseEntity
    {
        [Key]
        public int RelatedId { get; set; }

        public int ProductId { get; set; }

        public int RelProductId { get; set; }

        public Product? Product { get; set; }

    }
}
