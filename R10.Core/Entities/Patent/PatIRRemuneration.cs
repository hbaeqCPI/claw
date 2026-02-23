using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatIRRemuneration : BaseEntity
    {
        [Key]
        public int RemunerationId { get; set; }
        [Required]
        public int InvId { get; set; }
        public string? ProductSalesUpdatedYears { get; set; }
        [Display(Name = "End of Compensation Date")]
        public DateTime? CompensationEndDate { get; set; }
        public List<PatIRProductSale>? PatIRProductSales { get; set; }
        public List<PatInventorInv>? Inventors { get; set; }
        public List<PatIRRemunerationValuationMatrixData>? ValuationMatrixData { get; set; }
    }
}
