using System.ComponentModel.DataAnnotations;

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

    }
}