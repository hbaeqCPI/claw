using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class ActionCopyViewModel
    {
        public int ActId { get; set; }

        [Display(Name="Due Dates")]
        public bool CopyDueDates { get; set; }

        [Display(Name = "Remarks")]
        public bool CopyRemarks { get; set; }

        [Display(Name = "Documents")]
        public bool CopyDocuments { get; set; }

    }
}
