using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    public class LetterTag : BaseEntity
    {
        [Key]
        public int LetTagId { get; set; }

        public int LetId { get; set; }
        
        [Display(Name ="Tag")]
        public string? Tag { get; set; }

        public LetterMain? Letter { get; set; }
    }
}
