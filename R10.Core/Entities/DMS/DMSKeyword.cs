using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSKeyword : BaseEntity
    {
        [Key]
        public int KwdId { get; set; }

        [Required]
        public int DMSId { get; set; }

        public int? OrderOfEntry { get; set; }

        [Required, StringLength(50)]
        public string? Keyword { get; set; }

        public Disclosure? Disclosure { get; set; }

    }
}
