using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
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

        public List<TmkAreaCountry>? TmkAreaCountries { get; set; }
    }
}

