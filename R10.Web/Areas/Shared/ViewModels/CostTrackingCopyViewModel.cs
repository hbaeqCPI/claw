using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class CostTrackingCopyViewModel
    {
        public int CostTrackId { get; set; }
        
        [Display(Name = "Remarks")]
        public bool CopyRemarks { get; set; }

        [Display(Name = "Documents")]
        public bool CopyDocuments { get; set; }

    }
}
