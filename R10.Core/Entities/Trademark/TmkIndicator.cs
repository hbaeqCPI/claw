using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkIndicator : BaseEntity
    {
        public int IndicatorId { get; set; }

        [Key]
        [StringLength(20)]
        [Display(Name = "Indicator")]
        public string? Indicator { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "CPI Indicator")]
        public bool CPIIndicator { get; set; } = false;

        [Display(Name = "RMS Indicator")]
        public bool RMSIndicator { get; set; }
    }
}
