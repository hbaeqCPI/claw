using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class PatLicensee : BaseEntity
    {
        [Key]
        public int LicenseeId { get; set; }
        
        public int AppId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Licensee")]
        public string? Licensee { get; set; }

        [Display(Name = "Licensor")]
        public string? Licensor { get; set; }

        [Display(Name = "License No.")]
        public string? LicenseNo { get; set; }
        [Display(Name = "License Start")]
        public DateTime? LicenseStart { get; set; }

        [Display(Name = "License Expiration")]
        public DateTime? LicenseExpire { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public string? LicenseType { get; set; }
        
        public string? DocFilePath { get; set; }
        public int? FileId { get; set; }

        [NotMapped]
        public string? CurrentDocFile { get; set; }
        public CountryApplication? CountryApplication { get; set; }
    }
}
