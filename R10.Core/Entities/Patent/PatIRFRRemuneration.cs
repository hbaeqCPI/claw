using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatIRFRRemuneration : BaseEntity
    {
        [Key]
        public int FRRemunerationId { get; set; }
        [Required]
        public int InvId { get; set; }
        public string? ProductSalesUpdatedYears { get; set; }
        public List<PatIRFRProductSale>? PatIRFRProductSales { get; set; }
        public List<PatInventorInv>? Inventors { get; set; }
        public List<PatIRFRRemunerationValuationMatrixData>? ValuationMatrixData { get; set; }
    }
}
