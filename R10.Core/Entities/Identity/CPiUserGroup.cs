using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Identity
{
    public class CPiUserGroup : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [StringLength(450)]
        [Required]
        public string UserId { get; set; }

        [Required]
        public int GroupId { get; set; }

        public CPiUser? CPiUser { get; set; }

        public CPiGroup? CPiGroup { get; set; }
    }
}
