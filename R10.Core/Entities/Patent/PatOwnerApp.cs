using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatOwnerApp : PatOwnerAppDetail
    {

        public CountryApplication? CountryApplication { get; set; }

        public Owner? Owner { get; set; }
    }

    public class PatOwnerAppDetail : BaseEntity
    {
        [Key]
        public int OwnerAppID { get; set; }

        [Required]
        public int AppId { get; set; }

        [Required]
        public int OwnerID { get; set; }

        public int? OrderOfEntry { get; set; }

        public string? Remarks { get; set; }

        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        public double? Percentage { get; set; }

        [Display(Name = "Applicant?")]
        public bool? IsApplicant { get; set; }
    }
}
