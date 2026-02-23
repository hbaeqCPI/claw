using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkConflictStatus : BaseEntity
    {
        public int ConflictStatusId { get; set; }

        [Key]
        [StringLength(20)]
        [Display(Name = "Conflict Status")]
        public string ConflictStatus { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Active?")]
        public bool ActiveSwitch { get; set; }

        public List<TmkConflict>? TmkConflicts { get; set; }

    }
}
