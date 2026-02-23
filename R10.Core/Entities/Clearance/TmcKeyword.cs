using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Clearance
{
    public class TmcKeyword : BaseEntity
    {
        [Key]
        public int KwdId { get; set; }

        [Required]
        public int TmcId { get; set; }

        [Required, StringLength(255)]
        public string Keyword { get; set; }

        public TmcClearance? Clearance { get; set; }

    }
}