using System;
using System.Collections.Generic;
using System.Text;
using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.DTOs
{
    public class RelatedProductDTO : BaseEntity
    {
        [Key]
        public int RelatedId { get; set; }

        public int ProductId { get; set; }

        public int RelProductId { get; set; }

        //public Product Product { get; set; }

        public string? RelatedProductCode { get; set; }
        public string? RelatedProductName { get; set; }
    }
}
