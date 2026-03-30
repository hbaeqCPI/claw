using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatAreaCountry
    {
        [StringLength(10)]
        [Required, Display(Name = "Area")]
        public string? Area { get; set; }

        [StringLength(5)]
        [Required, Display(Name = "Country")]
        public string? Country { get; set; }
    }
}
