using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    /// <summary>
    /// Types of email that the system sends out.
    /// </summary>
    public class EmailType : EmailTypeDetail
    {
        public EmailTemplate? EmailTemplate { get; set; }

        public EmailContentType? EmailContentType { get; set; }

        public List<EmailSetup>? EmailSetups { get; set; }
    }

    public class EmailTypeDetail : BaseEntity
    {
        [Key]
        public int EmailTypeId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Name")]
        public string?  Name { get; set; }

        [StringLength(125)]
        [Display(Name = "Description")]
        public string?  Description { get; set; }

        [Display(Name = "Enabled")]
        public bool IsEnabled { get; set; }

        [Display(Name = "Template")]
        public int? EmailTemplateId { get; set; }
        
        [Required]
        [Display(Name = "Notification Type")]
        [StringLength(450)]
        public string?  ContentType { get; set; }
    }
}
