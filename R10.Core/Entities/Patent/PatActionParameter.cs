using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatActionParameter : BaseEntity
    {
        [Key]
        public int ActParamId { get; set; }

        public int ActionTypeID { get; set; }

        [Required]
        [StringLength(60)]
        [Display(Name = "Action Due")]
        public string? ActionDue { get; set; }

        [Display(Name = "Yr")]
        public int Yr { get; set; }

        [Display(Name = "Mo")]
        public int Mo { get; set; }

        [Display(Name = "Dy")]
        public int Dy { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Indicator")]
        public string? Indicator { get; set; }

        public PatActionType? ActionType { get; set; }
    }
}
