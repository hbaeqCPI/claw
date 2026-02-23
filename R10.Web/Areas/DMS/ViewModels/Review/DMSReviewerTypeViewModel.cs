using Microsoft.Extensions.Localization;
using R10.Web.Models;
using System.Collections.Generic;

namespace R10.Web.Areas.DMS.ViewModels
{
    /// <summary>
    /// Stub view model for DMS Reviewer Type (DMS module removed).
    /// Retained for GridReviewers shared component compatibility.
    /// </summary>
    public class DMSReviewerTypeViewModel
    {
        public int EntityType { get; set; }
        public string? EntityName { get; set; }

        public static List<DMSReviewerTypeViewModel> BuildList(IStringLocalizer<SharedResource> localizer)
        {
            return new List<DMSReviewerTypeViewModel>();
        }

        public static DMSReviewerTypeViewModel GetDefault(IStringLocalizer<SharedResource> localizer)
        {
            return new DMSReviewerTypeViewModel { EntityType = 0, EntityName = "" };
        }
    }
}
