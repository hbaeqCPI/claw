using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMMatterOtherPartyPatent : BaseEntity
    {
        [Key]
        public int GMOPPId { get; set; }

        public int MatId { get; set; }

        [Required]
        [StringLength(25)]
        public string? OtherPartyPatent { get; set; }

        [Required]
        [StringLength(5)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? AppNumber { get; set; }

        [StringLength(20)]
        public string? PatNumber { get; set; }

        [StringLength(20)]
        public string? PubNumber { get; set; }

        public DateTime? FilDate { get; set; }

        public DateTime? IssDate { get; set; }

        public DateTime? PubDate { get; set; }

        public GMMatter? GMMatter { get; set; }
    }
}
