using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Patent
{
    public class PatCaseType : ClawBaseEntity
    {
        [Required]
        [StringLength(3)]
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public bool? LockPatRecord { get; set; }

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        [NotMapped]
        public bool IsNewRecord { get; set; }

        [NotMapped]
        public string? OriginalSystems { get; set; }
    }
}
