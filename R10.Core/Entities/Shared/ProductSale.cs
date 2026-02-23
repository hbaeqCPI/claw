using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities
{
    public class ProductSale : ProductSaleDetail
    {
        [NotMapped]
        public ProductSale? ProductSaleBeforeUpdate { get; set; }
        //public Product Product { get; set; }

    }
    public class ProductSaleDetail : BaseEntity
    {
        [Key]
        public int SaleId { get; set; }

        public int ProductId { get; set; }

        [Display(Name = "Currency Type")]
        public string?  CurrencyType { get; set; }

        [Display(Name = "Year")]
        public int Yr { get; set; }

        [Display(Name = "Q1")]
        public decimal Q1 { get; set; }

        [Display(Name = "Q2")]
        public decimal Q2 { get; set; }

        [Display(Name = "Q3")]
        public decimal Q3 { get; set; }

        [Display(Name = "Q4")]
        public decimal Q4 { get; set; }

        [Display(Name = "Total Amount")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal Net { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        public string?  Country { get; set; }

        public Product? Product { get; set; }
        public bool? RecentAddedByImport { get; set; }
        public bool? RecentUpdatedByImport { get; set; }
        public string? CurrencyTypeBeforeImport { get; set; }
    }
}
