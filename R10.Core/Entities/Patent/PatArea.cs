using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{

    public class PatArea: ClawBaseEntity
    {
        [StringLength(10)]
        [Required, Display(Name = "Area")]
        public string? Area { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public List <PatAreaCountry>? PatAreaCountries { get; set; }
    }
}
