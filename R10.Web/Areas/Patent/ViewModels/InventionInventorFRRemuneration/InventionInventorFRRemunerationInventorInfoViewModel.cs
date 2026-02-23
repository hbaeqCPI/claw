using System.ComponentModel.DataAnnotations;
using R10.Core.Entities.Patent;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionInventorFRRemunerationInventorInfoViewModel : PatInventorInv
    {
        [Display(Name = "Inventor")]
        public string? Inventor { get; set; }
        [Display(Name = "Employee Title")]
        public string? Position { get; set; }
        //public decimal? Percentage { get; set; }
        public int? SumABC { get; set; }
        [Display(Name = "% of Ownership")]
        public int? InventorPosition { get; set; }
    }
}
