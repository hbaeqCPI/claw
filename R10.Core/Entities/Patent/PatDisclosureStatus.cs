using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatDisclosureStatus : BaseEntity
    {        
        
        public int DisclosureStatusID { get; set; }

        [Key]
        [StringLength(20)]
        [Required(ErrorMessage = "Disclosure Status is required.")]
        [Display(Name = "Disclosure Status")]
        public string DisclosureStatus { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

    }
}
