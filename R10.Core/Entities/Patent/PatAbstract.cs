using R10.Core.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{

    public class PatAbstract : BaseEntity
    {
        [Key]
        public int AbstractId { get; set; }

        [Required]
        public int InvId { get; set; }

        [Required, StringLength(10)]
        [Display(Name = "Language")]
        public string LanguageName { get; set; }

        public int OrderOfEntry { get; set; }

        [TradeSecret]
        [UIHint("TextArea")]
        [Display(Name = "Abstract")]
        public string? Abstract { get; set; }

        public bool IsDefault { get; set; }

        public AbstractTradeSecret? TradeSecret { get; set; }

        public Invention? Invention { get; set; }
        public Language? AbstractLanguage { get; set; }
    }

    public class AbstractTradeSecret
    {
        [Encrypted]
        public string? Abstract { get; set; }
    }
}
