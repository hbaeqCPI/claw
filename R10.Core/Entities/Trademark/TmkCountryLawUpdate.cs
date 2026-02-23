using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkCountryLawUpdate
    {
        [Key]
        public int keyID { get; set; }
        
        [StringLength(4)]   
        public string? Year { get; set; }

        [StringLength(1)]
        public string? Quarter { get; set; }

        [StringLength(20)]
        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Run Date")]
        public DateTime? RunDate { get; set; }
    }
}
