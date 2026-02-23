using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class CPiLanguage
    {
        [Key]
        public int LanguageId { get; set; }

        [StringLength(50)]
        public string Language { get; set; }

        public bool IsDefault { get; set; } = false;

        [StringLength(10)]
        public string LanguageCulture { get; set; }

    }
}
