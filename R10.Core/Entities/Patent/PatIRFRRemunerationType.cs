using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatIRFRRemunerationType : BaseEntity
    {
        [Key]
        public int RemunerationTypeId { get; set; }

        [StringLength(20)]
        [Display(Name = "Remuneration Type")]
        public string? RemunerationType { get; set; }
    }
}
