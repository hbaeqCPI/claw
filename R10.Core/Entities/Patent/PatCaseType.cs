using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
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

    }
}
