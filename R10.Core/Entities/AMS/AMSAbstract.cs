using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSAbstract : BaseEntity
    {
        [Key]
        public int AnnID { get; set; }

        [Required]
        [StringLength(3)]
        public string SourceData { get; set; }

        [Required]
        public string Abstract { get; set; }

        bool Active { get; set; }

        public AMSMain AMSMain { get; set; }
    }
}
