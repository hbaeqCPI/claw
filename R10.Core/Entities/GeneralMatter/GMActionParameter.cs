using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMActionParameter : BaseEntity
    {
        [Key]
        public int ActParamId { get; set; }

        public int ActionTypeID { get; set; }

        [Required]
        [StringLength(60)]
        [Display(Name = "Action Due")]
        public string? ActionDue { get; set; }

        [Display(Name = "Yr")]
        public int Yr { get; set; }

        [Display(Name = "Mo")]
        public int Mo { get; set; }

        [Display(Name = "Dy")]
        public int Dy { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Indicator")]
        public string? Indicator { get; set; }

        //[Display(Name = "Indicator")]
        //public GMIndicator? GMIndicator { get; set; }
        public GMActionType? ActionType { get; set; }
    }
}
