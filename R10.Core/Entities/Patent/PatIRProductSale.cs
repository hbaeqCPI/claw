using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatIRProductSale : BaseEntity
    {
        [Key]
        public int ProductSaleId { get; set; }
        [Required]
        public int RemunerationId { get; set; }

        [Display(Name = "Product")]
        public string? Product { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }
        [Display(Name = "Year")]
        [Required]
        public int? Year { get; set; }

        [Display(Name = "License Factor")]
        public double? LicenseFactor { get; set; }
        [Display(Name = "Invention Value")]
        public double? InventionValue { get; set; }
        [Display(Name = "Unit Price")]
        public double? UnitPrice { get; set; }
        [Display(Name = "Quantity Sold")]
        public int? Quantity { get; set; }

        public PatIRRemuneration? Remuneration { get; set; }
        public PatIRTurnOver? PatIRTurnOver { get; set; }
        [Display(Name = "Revenue")]
        public double? Revenue { get; set; }
        [Display(Name = "Override Revenue")]
        public bool UseOverrideRevenue { get; set; }
        [Display(Name = "Currency Type")]
        public string? CurrencyType { get; set; }
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
    }
}
