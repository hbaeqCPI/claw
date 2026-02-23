using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSActionType : BaseEntity
    {
        [Key]
        public int ActionTypeId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Action")]
        public string Name { get; set; }

        [StringLength(800)]
        [Display(Name = "Description")]
        public string Description { get; set; }


    }
}
