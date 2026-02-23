using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatIDSReferenceSource : BaseEntity
    {
        [Key]
        public int IDSRefSrcID { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name ="Reference Src")]
        public string ReferenceSrc { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

    }
}
