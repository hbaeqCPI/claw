using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    public class QETag : BaseEntity
    {
        [Key]
        public int QETagId { get; set; }

        public int QESetupId { get; set; }

        [Display(Name = "Tag")]
        public string? Tag { get; set; }

        public QEMain? QE { get; set; }
    }
}
