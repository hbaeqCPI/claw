using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatIRTurnOver : BaseEntity
    {
        [Key]
        public int TurnOverId { get; set; }

        [Range(1000, 3000, ErrorMessage = "Please enter a correct Year.")]
        [Display(Name = "Year")]
        [Required]
        public int? Year { get; set; }

        [Display(Name = "Turn Over")]
        [Required]
        public double? TurnOver { get; set; }
        public List<PatIRProductSale>? PatIRProductSales { get; set; }
    }
}
