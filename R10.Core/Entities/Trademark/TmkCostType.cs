using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Trademark
{
    public class TmkCostType : BaseEntity
    {
        public int CostTypeId { get; set; }

        [Key]
        [StringLength(30)]
        [Display(Name = "Cost Type")]
        public string? CostType { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Default Cost")]
        public decimal DefaultCost { get; set; }

        [Display(Name = "Use in Cost Estimator")]
        public bool UseInCE { get; set; }

        public bool CPICostType { get; set; }

        public List<TmkCostTrack>? TmkCostTrackings { get; set; }
        public List<TmkBudgetManagement>? TmkBudgetManagements { get; set; }
    }

}
