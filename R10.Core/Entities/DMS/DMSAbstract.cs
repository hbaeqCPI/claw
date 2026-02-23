using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSAbstract : BaseEntity
    {
        [Key]
        public int AbstractId { get; set; }

        [Required]
        public int DMSId { get; set; }

        //[Required, Display(Name = "Language")]
        //public int LanguageId { get; set; }

        [Required, StringLength(10)]
        public string? Language { get; set; }

        public int OrderOfEntry { get; set; }

        public string? Abstract { get; set; }

        public bool IsDefault { get; set; }

        public Language? AbstractLanguage { get; set; }
    }
}
