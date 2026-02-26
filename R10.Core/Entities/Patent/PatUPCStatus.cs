using R10.Core.Entities;
// using R10.Core.Entities.AMS; // Removed during deep clean
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{
    public class PatUPCStatus : BaseEntity
    {
        [Key]
        public int StatusId { get; set; }

        [Required]
        [StringLength(15)]
        [Display(Name = "UPC Status")]
        public string UPCStatus { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }


    }

}
