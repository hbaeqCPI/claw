using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatIRRemunerationFormula : PatIRRemunerationFormulaDetail
    {
        public ICollection<Client>? Clients { get; set; }
    }
    public class PatIRRemunerationFormulaDetail : BaseEntity
    {
        [Key]
        public int FormulaId { get; set; }

        [StringLength(100)]
        [Display(Name = "Name")]
        [Required]
        public string? Name { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Eff Start Date")]
        public DateTime? EffStartDate { get; set; }

        [Display(Name = "Eff End Date")]
        public DateTime? EffEndDate { get; set; }

        [Display(Name = "Max Amount")]
        public double? MaxValue { get; set; }

        [Display(Name = "Min Amount")]
        public double? MinValue { get; set; }

        [StringLength(20)]
        [Display(Name = "Remuneration Type")]
        [Required]
        public string? RemunerationType { get; set; }

        [Display(Name = "Formula")]
        public string? Formula { get; set; }
    }
}
