using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMMatterOtherPartyTrademark : BaseEntity
    {
        [Key]
        public int GMOPTId { get; set; }

        public int MatId { get; set; }

        [Required]
        [StringLength(100)]
        public string? OtherPartyTrademark { get; set; }

        [Required]
        [StringLength(5)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? AppNumber { get; set; }

        [StringLength(20)]
        public string? RegNumber { get; set; }

        [StringLength(20)]
        public string? PubNumber { get; set; }

        public DateTime? FilDate { get; set; }

        public DateTime? RegDate { get; set; }

        public DateTime? PubDate { get; set; }

        public GMMatter? GMMatter { get; set; }

        public int? FileId { get; set; }
        public string? DriveItemId { get; set; }

        [StringLength(255)]
        [Display(Name = "New Document")]
        public string? DocFilePath { get; set; }
    }
}
