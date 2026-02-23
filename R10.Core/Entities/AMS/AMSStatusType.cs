using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSStatusType : BaseEntity
    {
        [Required]
        public int StatusTypeID { get; set; }

        [Key]
        [StringLength(5)]
        public string CPIStatus { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        [Required]
        [StringLength(15)]
        public string ClientApplicationStatus { get; set; }

        [Required]
        public bool Active  { get; set; }

        public PatApplicationStatus PatApplicationStatus { get; set; }
    }
}
