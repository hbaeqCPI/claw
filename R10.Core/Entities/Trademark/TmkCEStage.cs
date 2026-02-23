using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Trademark
{
    public class TmkCEStage : BaseEntity
    {        
        
        public int StageID { get; set; }

        [Key]
        [StringLength(20)]
        [Required(ErrorMessage = "Stage is required.")]
        [Display(Name = "Stage")]
        public string Stage { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Stage Order")]
        public int? StageOrder { get; set; }

        public List<TmkCECountryCost>? TmkCECountryCosts { get; set; }
        public List<TmkCEGeneralCost>? TmkCEGeneralCosts { get; set; }

    }
}
