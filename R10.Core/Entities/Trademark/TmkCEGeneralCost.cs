using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Trademark
{
    public class TmkCEGeneralCost : BaseEntity
    {
        [Key]
        public int CostId { get; set; }
        public int CEGeneralId { get; set; }

        [Required]        
        public string Description { get; set; }

        //VALID OPTIONS: (using C# value type names)
        //string | DateTime | DateTime Range | bool | numeric
        [Required]
        public TmkCECostDataType DataType { get; set; }

        [Display(Name = "Value")]
        public string? DefaultValue { get; set; }

        [Display(Name = "Operator")]
        public string? Opts { get; set; }

        [Display(Name = "Default Cost")]        
        public decimal Cost { get; set; }

        [Display(Name = "Alternate Cost")]
        public decimal AltCost { get; set; }

        [Display(Name = "Multiplier Cost")]
        public decimal MultCost { get; set; }

        public int OrderOfEntry { get; set; }

        [Display(Name = "Calculate?")]
        public bool ActiveSwitch { get; set; }

        [Display(Name = "CPI Cost")]
        public bool CPICost { get; set; }

        [Required]
        [Display(Name = "Cost Type")]
        public string? CostType { get; set; }

        [Required]
        [Display(Name = "Stage")]
        public string? Stage { get; set; }

        
        public TmkCEGeneralSetup? TmkCEGeneralSetup { get; set; }        
        public List<TmkCEQuestionGeneral>? TmkCEQuestionGenerals { get; set; }
        public TmkCEStage? TmkCEStage { get; set; }


        [NotMapped]
        public string? DataTypeDisplay { get; set; }
    }
}