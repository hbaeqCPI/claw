using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatIRFRRemunerationFormulaFactor : BaseEntity
    {
        [Key]
        public int FactorId { get; set; }

        [StringLength(20)]
        [Display(Name = "Name")]
        [Required]
        public string? Name { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(3)]
        [Display(Name = "Variable")]
        [Required]
        public string? Variable { get; set; }

        [Display(Name = "Max Value")]
        public double? MaxValue { get; set; }

        [Display(Name = "Min Value")]
        public double? MinValue { get; set; }

        [Display(Name = "Default Value")]
        [Required]
        public double? DefaultValue { get; set; }

        [Display(Name = "Allow to override value?")]
        public bool AllowManualEntry { get; set; }

        [Display(Name = "Formula")]
        public string? Formula { get; set; }
    }
}
