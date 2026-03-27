using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkDesCaseTypeFieldsDeleteExt
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

        [Display(Name = "Des Case Type New")]
        [StringLength(3)]
        public string? DesCaseTypeNew { get; set; }

        [Display(Name = "From Field New")]
        [StringLength(50)]
        public string? FromFieldNew { get; set; }

        [Display(Name = "To Field New")]
        [StringLength(50)]
        public string? ToFieldNew { get; set; }

    }
}