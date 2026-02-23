using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMCostType : BaseEntity
    {
        public int CostTypeID { get; set; }

        [Key]
        [Required]
        [StringLength(30)]
        [Display(Name = "Cost Type")]
        public string? CostType { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Default Cost")]
        public decimal DefaultCost { get; set; }

        public List<GMCostTrack>? GMCostTrackings { get; set; }

        public List<GMBudgetManagement>? GMBudgetManagements { get; set; }
    }
}
