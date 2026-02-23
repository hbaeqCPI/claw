using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class LicenseeViewModel : BaseEntity
    {
        public int LicenseeId { get; set; }
        public int ParentId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Licensee")]
        public string? Licensee { get; set; }
        
        [Required, StringLength(100)]
        [Display(Name = "Licensor")]
        public string? Licensor { get; set; }

        [StringLength(25)]
        [Display(Name = "License No")]
        public string? LicenseNo { get; set; }

        [Display(Name = "License Start")]
        public DateTime? LicenseStart { get; set; }
        [Display(Name = "License Expire")]
        public DateTime? LicenseExpire { get; set; }
        
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "License Type")]
        public string? LicenseType { get; set; }

        public string? DocFilePath { get; set; }
        public int? FileId { get; set; }

        [NotMapped]
        [Display(Name = "Document")]
        public string? CurrentDocFile { get; set; }
    }
}
