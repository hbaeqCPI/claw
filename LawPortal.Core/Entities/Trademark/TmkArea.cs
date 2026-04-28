using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Trademark
{
    public class TmkArea : ClawBaseEntity
    {
        [Required]
        [StringLength(10)]
        [Display(Name = "Area")]
        public string? Area { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        [NotMapped]
        public bool IsNewRecord { get; set; }

        [NotMapped]
        public string? OriginalSystems { get; set; }

        [NotMapped]
        public string? CopyFromSystems { get; set; }

        [NotMapped]
        public string? CopyFromArea { get; set; }

        public List<TmkAreaCountry>? TmkAreaCountries { get; set; }
    }
}

