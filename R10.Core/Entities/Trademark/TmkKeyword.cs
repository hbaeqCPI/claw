using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkKeyword : BaseEntity
    {
        [Key]
        public int KwdId { get; set; }

        [Required]
        public int TmkId { get; set; }

        [Required, StringLength(255)]
        public string? Keyword { get; set; }

        public TmkTrademark? TmkTrademark { get; set; }

    }
}
