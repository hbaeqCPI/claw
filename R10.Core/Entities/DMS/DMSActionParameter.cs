using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSActionParameter : BaseEntity
    {
        [Key]
        public int ActParamId { get; set; }

        public int ActionTypeID { get; set; }

        [Required]
        [StringLength(60)]
        [Display(Name = "Action Due")]
        public string? ActionDue { get; set; }

        public int Yr { get; set; }
        public int Mo { get; set; }
        public int Dy { get; set; }

        [Required]
        [StringLength(20)]
        public string? Indicator { get; set; }

        [Display(Name = "Indicator")]
        public DMSIndicator? DMSIndicator { get; set; }
        public DMSActionType? ActionType { get; set; }
    }
}
