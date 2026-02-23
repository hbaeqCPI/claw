using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class Help : BaseEntity
    {
        [Key]
        public int HelpId { get; set; }

        [StringLength(50)]
        [Required]
        public string ClientType { get; set; }

        [StringLength(100)]
        [Required]
        public string Page { get; set; }

        [StringLength(100)]
        public string? Path { get; set; }
    }
}
