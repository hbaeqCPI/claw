using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSLicensee : BaseEntity
    {
        [Key]
        public int LicenseeId { get; set; }

        public int AnnID { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Licensee")]
        public string? Licensee { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Licensor")]
        public string? Licensor { get; set; }

        [StringLength(25)]
        [Display(Name = "License No.")]
        public string? LicenseNo { get; set; }

        [Display(Name = "License Start")]
        public DateTime? LicenseStart { get; set; }

        [Display(Name = "License Expiration")]
        public DateTime? LicenseExpire { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public string? LicenseType { get; set; }

        public AMSMain? AMSMain { get; set; }
    }
}
