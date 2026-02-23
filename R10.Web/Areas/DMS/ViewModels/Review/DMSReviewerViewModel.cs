using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.DMS.ViewModels
{
    /// <summary>
    /// Stub view model for DMS Reviewer (DMS module removed).
    /// Retained for GridReviewers shared component compatibility.
    /// </summary>
    public class DMSReviewerViewModel : BaseEntity
    {
        public int DMSReviewerId { get; set; }
        public int EntityId { get; set; }
        public bool IsDefaultReviewer { get; set; }

        [Display(Name = "Reviewer Type")]
        public DMSReviewerTypeViewModel? ReviewerType { get; set; }

        [Display(Name = "Reviewer")]
        public DMSReviewerLookupViewModel? Reviewer { get; set; }
    }
}
