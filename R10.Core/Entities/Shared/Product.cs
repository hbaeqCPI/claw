using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using R10.Core.DTOs;
using System.Text;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
// using R10.Core.Entities.AMS; // Removed during deep clean

namespace R10.Core.Entities
{
    public class Product : ProductDetail
    {
        public List<RelatedProduct>? RelatedProducts { get; set; }
        public List<PatProduct>? CountryApplicationProducts { get; set; }
        public List<PatProductInv>? InventionProducts { get; set; }
        public List<TmkProduct>? TrademarkProducts { get; set; }
//         public List<GMProduct>? GeneralMatterProducts { get; set; } // Removed during deep clean
//         public List<AMSProduct>? AMSProducts { get; set; } // Removed during deep clean
        public List<ProductSale>? ProductSales { get; set; }

        public ProductLatestTopSaleDTO? ProductLatestTopSale { get; set; }
    }

    public class ProductDetail : BaseEntity
    {
        [Key]
        public int ProductId { get; set; }

        [StringLength(10)]
        [Required]
        [Display(Name = "Product Code")]
        public string?  ProductCode { get; set; }

        [StringLength(50)]
        [Required]
        [Display(Name = "Product Name")]
        public string?  ProductName { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string?  Description { get; set; }

        [StringLength(25)]
        [Display(Name = "Product Group")]
        public string?  ProductGroup { get; set; }

        [StringLength(25)]
        [Display(Name = "Product Category")]
        public string?  ProductCategory { get; set; }

        [StringLength(25)]
        [Display(Name = "Brand")]
        public string?  Brand { get; set; }

        [StringLength(100)]
        [Display(Name = "Supplier")]
        public string?  Supplier { get; set; }

        [StringLength(100)]
        [Display(Name = "Manufacturer")]
        public string?  Manufacturer { get; set; }

        [Display(Name = "Country of Origin")]
        public string?  Country { get; set; }

        [Display(Name = "Country Name")]

        public string?  CountryName { get; set; }

        [Display(Name = "Active?")]
        public bool Active { get; set; }

        [StringLength(255)]
        [Display(Name = "Marketing Claims")]
        public string?  MarketingClaims { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
    }
   
}
