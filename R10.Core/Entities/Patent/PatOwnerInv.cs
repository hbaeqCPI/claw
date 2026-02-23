using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatOwnerInv: PatOwnerInvDetail
    {

        public Invention? Invention { get; set; }

        public Owner? Owner { get; set; }
    }

    public class PatOwnerInvDetail : BaseEntity
    {
        [Key]
        public int OwnerInvID { get; set; }

        [Required]
        public int InvId { get; set; }

        [Required]
        public int OwnerID { get; set; }

        public int? OrderOfEntry { get; set; }

        public string? Remarks { get; set; }

        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        public double? Percentage { get; set; }

        [Display(Name ="Applicant?")]
        public bool? IsApplicant { get; set; }
    }
}
