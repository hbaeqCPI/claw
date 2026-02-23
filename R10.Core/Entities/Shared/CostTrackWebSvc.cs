using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Shared
{
    public class CostTrackWebSvc
    {
        [Required]
        [StringLength(25)]
        public string? CaseNumber { get; set; }

        [Required]
        [StringLength(5)]
        public string? Country { get; set; }

        [StringLength(8)]
        public string? SubCase { get; set; }

        [Required]
        [StringLength(30)]
        public string? CostType { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }

        [Required]
        [StringLength(50)]
        public string? InvoiceNumber { get; set; }

        public decimal? InvoiceAmount { get; set; }

        public DateTime? PayDate { get; set; }

        [StringLength(10)]
        public string? Agent { get; set; }
    }
}
