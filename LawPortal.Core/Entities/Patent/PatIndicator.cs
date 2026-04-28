using System.ComponentModel.DataAnnotations;

namespace LawPortal.Core.Entities.Patent
{
    public class PatIndicator : BaseEntity
    {
        public int IndicatorId { get; set; }

        [Key]
        [Required]
        [StringLength(20)]
        [Display(Name = "Indicator")]
        public string? Indicator { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "CPI Indicator")]
        public bool CPIIndicator { get; set; } = false;

        [Display(Name = "Foreign Filing Indicator")]
        public bool FFIndicator { get; set; }
    }
}
