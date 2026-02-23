using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatIRFRValorizationRule : BaseEntity
    {
        [Key]
        public int ValorizationRuleId { get; set; }

        [Display(Name = "Point")]
        [Required]
        public int? Point { get; set; }
        [Display(Name = "Ratio (%)")]
        [Required]
        public int? Ratio { get; set; }
    }
}
