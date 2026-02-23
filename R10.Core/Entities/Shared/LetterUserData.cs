using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class LetterUserData : LetterUserDataDetail
    {
        public LetterMain? LetterMain { get; set; }
    }
    public class LetterUserDataDetail : BaseEntity
    {
        [Key]
        public int LetDataId { get; set; }

        public int LetId { get; set; }

        [Required]
        [Display(Name = "Data Name")]
        [StringLength(50)]
        public string?  DataName { get; set; }

        [Display(Name = "Default Value")]
        [StringLength(50)]
        public string?  DefaultValue { get; set; }
    }
}
