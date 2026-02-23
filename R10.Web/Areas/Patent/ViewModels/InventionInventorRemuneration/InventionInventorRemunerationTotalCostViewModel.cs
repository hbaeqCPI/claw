using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionInventorRemunerationTotalCostViewModel
    {
        public int InvId { get; set; }

        public string CaseNumber { get; set; }

        public double TotalCost { get; set; }

        public string? Module { get; set; } = "German";

        [Display(Name = "End of Compensation Date")]
        public DateTime? CompensationEndDate { get; set; }

        public List<InventionInventorRemunerationYearlyCostViewModel> YearlyCost { get; set; }
    }

    public class InventionInventorRemunerationYearlyCostViewModel
    {
        public int Year { get; set; }
        public double Cost { get; set; }
    }
}
