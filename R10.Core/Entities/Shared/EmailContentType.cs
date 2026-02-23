using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class EmailContentType : BaseEntity
    {
        public int Id { get; set; }

        [Key]
        [Required]
        [StringLength(450)]
        public string?  Name { get; set; }

        [StringLength(255)]
        public string?  Description { get; set; }

        [Required]
        [StringLength(450)]
        public string?  Policy { get; set; }

        public List<EmailType>? EmailTypes { get; set; }
    }
}
