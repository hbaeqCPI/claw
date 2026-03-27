using System.ComponentModel.DataAnnotations;

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
    }
}
