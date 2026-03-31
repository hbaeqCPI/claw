using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Trademark
{
    public class TmkDesCaseTypeFieldsExt : BaseEntity
    {
        [Display(Name = "Des Case Type")]
        [StringLength(3)]
        public string? DesCaseType { get; set; }

        [Display(Name = "From Field")]
        [StringLength(50)]
        public string? FromField { get; set; }

        [Display(Name = "To Field")]
        [StringLength(50)]
        public string? ToField { get; set; }

        [Display(Name = "In Use")]
        public bool InUse { get; set; }

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string Systems { get; set; } = "";

        [NotMapped]
        public bool IsNewRecord { get; set; }

        [NotMapped]
        public string? OriginalSystems { get; set; }
    }
}
