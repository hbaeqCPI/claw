using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Trademark
{
    public class TmkDesCaseTypeFields
    {
        [StringLength(3)]
        [Required]
        public string? DesCaseType { get; set; }

        [StringLength(50)]
        [Required]
        public string? FromField { get; set; }

        [StringLength(50)]
        [Required]
        public string? ToField { get; set; }

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        [NotMapped]
        public bool IsNewRecord { get; set; }

        [NotMapped]
        public string? OriginalSystems { get; set; }

        [NotMapped]
        public string? CopyFromSystems { get; set; }
    }
}
