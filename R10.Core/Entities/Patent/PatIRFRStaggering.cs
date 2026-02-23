using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatIRFRStaggering : BaseEntity
    {

        [Key]
        public int StaggeringId { get; set; }

        [Range(1000, 3000, ErrorMessage = "Please enter an correct Year.")]
        [Display(Name = "Year")]
        [Required]
        public int? Year { get; set; }
        public virtual ICollection<PatIRFRStaggeringDetail>? PatIRStaggeringDetails { get; set; }
    }
}
