using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatCostType : BaseEntity
    {
        public int CostTypeID { get; set; }

        [Key]
        [Required]
        [StringLength(30)]
        [Display(Name = "Cost Type")]
        public string CostType { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Default Cost")]
        public decimal DefaultCost { get; set; }

        [Display(Name = "Use in Cost Estimator")]
        public bool UseInCE { get; set; }

        public bool CPICostType { get; set; }

        [Display(Name = "Use AMS for Real Cost")]
        public bool UseAMSRealCost { get; set; }

        public List<PatCostTrack>? PatCostTrackings { get; set; }

        public List<PatBudgetManagement>? PatBudgetManagements { get; set; }

        public List<PatCostTrackInv>? PatCostTrackingInvs { get; set; }
    }

}
