using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace R10.Core.Entities.Patent
{
    public class PatTaxYear: BaseEntity
    {
        [Key]
        public int TaxYID { get; set; }

        public int TaxBID { get; set; }

        [Display(Name = "Tax Year")]
        [StringLength(10)]
        [Required]
        public string? TaxYear { get; set; }

        [Display(Name = "Fee Code")]
        [StringLength(5)]
        public string? Annuity { get; set; }

        [Display(Name = "Tax Amount")]
        public decimal TaxAmount { get; set; }

        [Display(Name = "Service Fee")]
        public double? ServiceFee { get; set; }

        public bool TaxYearZero { get; set; }

        public PatTaxBase? PatTaxBase { get; set; }
    }
}
