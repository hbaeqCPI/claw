using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMMatterKeyword : BaseEntity
    {
        [Key]
        public int KwdId { get; set; }

        [Required]
        public int MatId { get; set; }

        [Required, StringLength(50)]
        public string? Keyword { get; set; }

        public GMMatter? GMMatter { get; set; }
    }
}
