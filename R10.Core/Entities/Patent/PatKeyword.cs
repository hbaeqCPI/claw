using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatKeyword : BaseEntity
    {
        [Key]
        public int KwdId { get; set; }

        [Required]
        public int InvId { get; set; }

        [Required, StringLength(50)]
        public string? Keyword { get; set; }

        public int OrderOfEntry { get; set; }

        public Invention? Invention { get; set; }

    }
}
