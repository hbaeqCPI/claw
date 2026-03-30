using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        [NotMapped]
        public bool IsNewRecord { get; set; }

        [NotMapped]
        public string? OriginalSystems { get; set; }
    }
}
