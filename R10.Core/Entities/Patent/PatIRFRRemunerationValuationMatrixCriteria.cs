using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatIRFRRemunerationValuationMatrixCriteria : BaseEntity
    {
        [Key]
        public int CriteriaId { get; set; }

        [StringLength(255)]
        [Display(Name = "Category")]
        [Required]
        public string? Category { get; set; }
        public string? Remarks { get; set; }

        [Display(Name = "In Use?")]
        [DefaultValue(true)]
        public bool ActiveSwitch { get; set; }

        [Required]
        public int MatrixId { get; set; }

        [Display(Name = "Max Value")]
        public double? MaxValue { get; set; }

        [Display(Name = "Min Value")]
        public double? MinValue { get; set; }

        [Display(Name = "Value")]
        public double? Value { get; set; }

        public PatIRFRRemunerationValuationMatrix? ValuationMatrix { get; set; }
    }
}
