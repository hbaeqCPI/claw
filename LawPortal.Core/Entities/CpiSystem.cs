using System.ComponentModel.DataAnnotations;

namespace LawPortal.Core.Entities
{
    public class AppSystem : BaseEntity
    {
        public int SystemId { get; set; }

        [Key]
        [Required]
        [StringLength(50)]
        [Display(Name = "System Name")]
        public string? SystemName { get; set; }

        [StringLength(50)]
        [Display(Name = "System Type")]
        public string? SystemType { get; set; }
    }
}
