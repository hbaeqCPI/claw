using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkCaseType : ClawBaseEntity
    {
        [StringLength(3)]
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public bool? LockTmkRecord { get; set; }
    }
}
