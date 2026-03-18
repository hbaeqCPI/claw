using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkDesCaseTypeFields : BaseEntity
    {
        [Key]
        public int KeyID { get; set; }

        [StringLength(3)]
        [Required]
        public string DesCaseType { get; set; }

        [StringLength(50)]
        [Required]
        public string FromField { get; set; }

        [StringLength(50)]
        [Required]
        public string ToField { get; set; }

        public bool? InUse { get; set; }
    }
}
