using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatIRStaggering : BaseEntity
    {

        [Key]
        public int StaggeringId { get; set; }

        [Display(Name = "Name")]
        [Required]
        public string? Name { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<PatIRStaggeringDetail>? PatIRStaggeringDetails { get; set; }
    }
}
