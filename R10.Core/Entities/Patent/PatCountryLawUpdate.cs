using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatCountryLawUpdate
    {
        [StringLength(4)]
        public string? Year { get; set; }

        [StringLength(1)]
        public string? Quarter { get; set; }

        [StringLength(20)]
        [Display(Name = "User ID")]
        public string? UserID { get; set; }

        [Display(Name = "Run Date")]
        public DateTime? RunDate { get; set; }

        [StringLength(50)]
        [Display(Name = "Run Method")]
        public string? RunMethod { get; set; }
    }
}
