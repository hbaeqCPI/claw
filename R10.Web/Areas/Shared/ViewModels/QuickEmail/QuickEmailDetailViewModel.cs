using R10.Core.Entities;
using System.ComponentModel.DataAnnotations;
using R10.Web.Models;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class QuickEmailDetailViewModel: QEMain 
    {

        public string? ScreenCode { get; set; }
        public ScreenName ParentScreenName { get; set; }
        public int ParentId { get; set; }
        public string? ParentKey { get; set; }
        public int? LogParentId { get; set; }
        public string? LogParentKey { get; set; }
        public string? ScreenName { get; set; }
        public string? SystemType { get; set; }
        public string? ParentTable { get; set; }
        public string? RoleLink { get; set; }
        public bool Log { get; set; } = true;
        public int ImageParentId { get; set; }

        [Required]
        [Display(Name ="To")]
        public string? To { get; set; }

        [Display(Name = "Cc")]
        public string? CopyTo { get; set; }
        public string? Bcc { get; set; }
        public string? Body { get; set; }

        public IEnumerable<IFormFile>? Files { get; set; }

        /// <summary>
        /// Key=AttachmentFilePath
        /// Value=FileId|ItemId
        /// </summary>
        public Dictionary<string, string>? AttachedImages { get; set; }

        public string? Images { get; set; }
        public string? LanguageCulture { get; set; }

        public bool IncludeImages { get; set; }
        public bool DisplayImages { get; set; }
        public bool AutoAttachImages { get; set; }
        public string? AutoAttachFileNames { get; set; }
        public bool ShowAttachments { get; set; }
        public string? Attachments { get; set; }
        public int LogId { get; set; }
        public string? BodyMailTo { get; set; }
        public bool HasNonAscii { get; set; }

        public QuickEmailOptOutSetting OptOutSetting { get; set; } = QuickEmailOptOutSetting.QuickEmail;

        // SharePoint
        public string? SharePointDocLibrary { get; set; }
        public string? SharePointDocLibraryFolder { get; set; }
        public string? SharePointRecKey { get; set; }
    }

    public class QuickEmailMultipleCriteriaViewModel
    {
        public string? SystemType { get; set; }
        public string? ParentKey { get; set; }
        public List<QuickEmailMultipleViewModel> SearchIdsToSend { get; set; }
        public string? ParentTable { get; set; }
        public string? ScreenName { get; set; }
    }

    public class QuickEmailMultipleViewModel
    {

        public int Id { get; set; }
        public int QESetupId { get; set; }
        public string? To { get; set; }
        public string? Cc { get; set; }
        public QuickEmailAdditionalDataViewModel AdditionalData { get; set; }
    }

    public class QuickEmailAdditionalDataViewModel
    {
        public string? NewEntries { get; set; }
    }

    public class QuickEmailForPatentWatchViewModel
    {
        public string? EmailTo { get; set; }
        public string? Updates { get; set; }
    }

    public enum QuickEmailOptOutSetting
    {
        QuickEmail,
        Workflow,
        PatentWatch,
        DMSNewDisclosure,
        DMSNewDiscussion,
        DMSDiscussionReply,
        DMSInventorChange,
        ActionDelegated,
        ActionCompleted,
        ActionDeleted,
        ActionReassigned,
        ActionDueDateChanged,
        DMSActionReminder,
        None
    }

}
