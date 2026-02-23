using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkMarkType : BaseEntity
    {
        public int MarkTypeId { get; set; }

        [Key]
        [StringLength(25)]
        [Display(Name = "Mark Type")]
        public string? MarkType { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

    }
}
