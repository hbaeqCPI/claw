using R10.Core.Entities.Shared;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.DMS
{
    public class DMSSetting : DefaultSetting
    {
        public bool IsRatingOn { get; set; }

        public bool IsActionOn { get; set; }

        public bool IsReviewerTabOn { get; set; }

        public bool IsInventorInitialOn { get; set; }

        //VALID OPTIONS:
        //1 - Client (DEFAULT)
        //3 - Owner
        //10 - Area
        public DMSReviewerType ReviewerEntityType { get; set; }

        public bool IsDefaultInventorInitialOn { get; set; }
        public bool IsDefaultReviewerOn { get; set; }

        [Display(Description = "Preview", GroupName = "Modules")]
        public bool IsPreviewOn { get; set; }

        public string? InitialReviewStatus { get; set; }

        public string? FinalReviewStatus { get; set; }

        public string? SignatureStatus { get; set; }

        public string? UnderReviewStatus { get; set; }

        public string? SubmitStatus { get; set; }

        public bool IsPreviewTabOn { get; set; }

        public bool CanEditDMSReminderSettings { get; set; }

        [Display(Description = "Trade Secret", GroupName = "Modules")]
        public bool IsTradeSecretOn { get; set; }

        public bool IsStatusDaysLimitOn { get; set; }

        public bool IsStatusHistoryTabOn { get; set; }
    }

    public enum DMSReviewerType
    {
        None,
        Client = 1,
        Area
    }
}
