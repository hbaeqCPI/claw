using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMWorkflowActionParameter : BaseEntity
    {
        [Key]
        public int ActParamId { get; set; }
        public int WrkId { get; set; }

        [Required]
        [StringLength(60)]
        [Display(Name ="Action Due")]
        public string ActionDue { get; set; }

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
        
        public GMWorkflow? Workflow { get; set; }
        
    }
}
