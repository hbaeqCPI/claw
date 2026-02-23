using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Identity
{
    public class CPiGroup : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [StringLength(60)]
        [Required]
        public string Name { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        [Display(Name="Enabled")]
        public bool IsEnabled { get; set; }

        public List<CPiUserGroup>? CPiUserGroups { get; set; }
    }
}
