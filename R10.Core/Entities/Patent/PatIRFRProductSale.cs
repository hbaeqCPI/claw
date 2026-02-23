using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatIRFRProductSale : BaseEntity
    {
        [Key]
        public int ProductSaleId { get; set; }
        [Required]
        public int FRRemunerationId { get; set; }
        public string? Product { get; set; }
        public string? Country { get; set; }
        [Display(Name = "Year")]
        [Required]
        public int? Year { get; set; }

        [Display(Name = "License Factor")]
        [Required]
        public double? LicenseFactor { get; set; }
        [Display(Name = "Invention Value")]
        [Required]
        public double? InventionValue { get; set; }
        [Display(Name = "Unit Price")]
        public double? UnitPrice { get; set; }
        [Display(Name = "Quantity Sold")]
        public int? Quantity { get; set; }

        public PatIRFRRemuneration? FRRemuneration { get; set; }
        public PatIRFRTurnOver? PatIRFRTurnOver { get; set; }
        [Display(Name = "Revenue")]
        public double? Revenue { get; set; }
        [Display(Name = "Override Revenue")]
        public bool UseOverrideRevenue { get; set; }
        [Display(Name = "Currency Type")]
        public string? CurrencyType { get; set; }
    }
}
