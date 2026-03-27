using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatAreaDelete : ClawBaseEntity
    {
        [Required]
        [Display(Name = "Area")]
        [StringLength(10)]
        public string? Area { get; set; }

        [Display(Name = "Description")]
        [StringLength(255)]
        public string? Description { get; set; }

        [Display(Name = "Area New")]
        [StringLength(10)]
        public string? AreaNew { get; set; }

    }
}