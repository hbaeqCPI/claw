using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    /// <summary>
    /// Email templates that an EmailType can use.
    /// </summary>
    public class EmailTemplate : BaseEntity
    {
        [Key]
        public int EmailTemplateId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Name")]
        public string?  Name { get; set; }

        [StringLength(125)]
        [Display(Name = "Description")]
        public string?  Description { get; set; }

        public string?  Template { get; set; }

        public List<EmailType>? EmailTypes { get; set; }
    }
}
