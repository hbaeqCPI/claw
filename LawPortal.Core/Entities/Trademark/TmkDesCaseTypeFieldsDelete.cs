using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Trademark
{
    public class TmkDesCaseTypeFieldsDelete
    {
        [Required]
        [Display(Name = "Des Case Type")]
        [StringLength(3)]
        public string? DesCaseType { get; set; }

        [Required]
        [Display(Name = "From Field")]
        [StringLength(50)]
        public string? FromField { get; set; }

        [Required]
        [Display(Name = "To Field")]
        [StringLength(50)]
        public string? ToField { get; set; }

        [Display(Name = "Des Case Type New")]
        [StringLength(3)]
        public string? DesCaseTypeNew { get; set; }

        [Display(Name = "From Field New")]
        [StringLength(50)]
        public string? FromFieldNew { get; set; }

        [Display(Name = "To Field New")]
        [StringLength(50)]
        public string? ToFieldNew { get; set; }

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
        public string? CopyFromDesCaseType { get; set; }
    }
}