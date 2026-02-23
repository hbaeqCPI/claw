using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class CostViewModel
    {
        public int CostTrackId { get; set; }

        [Display(Name = "Cost Type")]
        public string? CostType { get; set; }

        [Display(Name = "Invoice Number")]
        public string? InvoiceNumber { get; set; }

        [Display(Name = "Invoice Date")]
        public DateTime? InvoiceDate { get; set; }

        [Display(Name = "Pay Date")]
        public DateTime? PayDate { get; set; }

        [Display(Name = "Net Cost")]
        public double NetCost { get; set; }

    }
}
