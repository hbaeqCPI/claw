using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class PatAreaCountryDelete
    {
        [Display(Name = "Area")]
        [Required, StringLength(10)]
        public string? Area { get; set; }

        [Display(Name = "Country")]
        [Required, StringLength(5)]
        public string? Country { get; set; }

        [Display(Name = "Area New")]
        [StringLength(10)]
        public string? AreaNew { get; set; }

        [Display(Name = "Country New")]
        [StringLength(5)]
        public string? CountryNew { get; set; }

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        [NotMapped]
        public bool IsNewRecord { get; set; }

        [NotMapped]
        public string? OriginalSystems { get; set; }
    }
}