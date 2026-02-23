using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ProductLatestTopSaleViewModel : ProductViewModel
    {
        [Display(Name = "Currency Type")]
        public string? CurrencyType { get; set; }

        [Display(Name="Year")]
        public int? Yr { get; set; }

        [Display(Name = "Total Amount")]
        public decimal? Total { get; set; }
    }
}
