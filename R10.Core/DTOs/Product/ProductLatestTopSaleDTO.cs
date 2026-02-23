using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.DTOs
{
    public class ProductLatestTopSaleDTO
    {
        [Key]
        public int ProductId { get; set; }

        public string? CurrencyType { get; set; }

        public int? Yr { get; set; }

        public decimal? Total { get; set; }

        public Product Product { get; set; }
    }
}
