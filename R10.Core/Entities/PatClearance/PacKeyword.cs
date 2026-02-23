using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.PatClearance
{
    public class PacKeyword : BaseEntity
    {
        [Key]
        public int KwdId { get; set; }

        [Required]
        public int PacId { get; set; }

        [Required, StringLength(50)]
        public string Keyword { get; set; }

        public int OrderOfEntry { get; set; }

        public PacClearance Clearance { get; set; }
    }
}
