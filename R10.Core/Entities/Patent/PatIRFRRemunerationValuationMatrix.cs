using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatIRFRRemunerationValuationMatrix : BaseEntity
    {
        [Key]
        public int MatrixId { get; set; }

        [StringLength(20)]
        [Display(Name = "Name")]
        [Required]
        public string? Name { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "In Use?")]
        public bool ActiveSwitch { get; set; } = true;
        [Display(Name = "Allow to override value?")]
        public bool AllowManualEntry { get; set; } = true;

        [StringLength(20)]
        [Display(Name = "Matrix Type")]
        [Required]
        public string? MatrixType { get; set; }

        [Display(Name = "Max Value")]
        public double? MaxValue { get; set; }

        [Display(Name = "Min Value")]
        public double? MinValue { get; set; }

        [StringLength(3)]
        [Display(Name = "Variable")]
        [Required]
        public string? Variable { get; set; }

        [Display(Name = "Default Value")]
        [Required]
        public double? DefaultValue { get; set; }

        public List<PatIRFRRemunerationValuationMatrixCriteria>? Criterias { get; set; }
        public PatIRFRRemunerationValuationMatrixType? IRFRMatrixType { get; set; }
    }
}
