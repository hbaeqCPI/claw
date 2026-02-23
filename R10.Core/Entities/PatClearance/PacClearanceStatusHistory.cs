using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.PatClearance
{
    public class PacClearanceStatusHistory
    {
        [Key]
        public int LogID { get; set; }

        [Required]
        public int PacId { get; set; }


        [Display(Name = "From")]
        public string OldStatus { get; set; }

        [Display(Name = "To")]
        public string NewStatus { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [StringLength(20)]
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Date Changed")]
        public DateTime? DateChanged { get; set; }

        public PacClearance? Clearance { get; set; }
    }
}
