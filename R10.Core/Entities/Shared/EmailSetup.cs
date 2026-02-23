using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    /// <summary>
    /// Defines the Subject and Body of an EmailType for a particular Language
    /// </summary>
    public class EmailSetup : EmailSetupDetail
    {
        public EmailType? EmailType { get; set; }
        public Language? LanguageLookup { get; set; }

    }
    public class EmailSetupDetail : BaseEntity
    {
        [Key]
        public int EmailSetupId { get; set; }

        [Required]
        public int EmailTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string?  Language { get; set; }

        public bool  Default {get;set;}

        [StringLength(255)]
        public string?  Subject { get; set; }

        public string?  Body { get; set; }
    }
}
