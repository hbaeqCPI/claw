using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Models;

namespace R10.Core.Entities.Shared
{
    public class QuickEmailParameterViewModel
    {
        public string? SystemType { get; set; } 
        public string? ScreenCode { get; set; } 
        public ScreenName ParentScreenName { get; set; } 
        public string? ParentKey { get; set; } 
        public int ParentId { get; set; }
        public int? LogParentId { get; set; }
        public string? LogParentKey { get; set; }
        public string? ParentTable { get; set; }
        public string? RoleLink { get; set; }
        public bool IncludeImages { get; set; }
        public int? QESetupId { get; set; }
        public string? StrId { get; set; }
        public int ImageParentId { get; set; }
        public string? AttachmentFilter { get; set; }

        // SharePoint
        public string? SharePointDocLibrary { get; set; }
        public string? SharePointDocLibraryFolder { get; set; }
        public string? SharePointRecKey { get; set; }
    }

    public class QuickEmailScreenParameterViewModel
    {
        public string? SystemType { get; set; }
        public string? ScreenCode { get; set; }
        public ScreenName ParentScreenName { get; set; }
        public string? ParentKey { get; set; }
        public int ParentId { get; set; }
        public string? ParentTable { get; set; }
        public bool IncludeImages { get; set; }
        public bool SendImmediately { get; set; }
        public string? LogParentKey { get; set; }
        public int LogParentId { get; set; }
        public string? RoleLink { get; set; }
        public int? QESetupId { get; set; }
        public bool AutoAttachImages { get; set; }
        public string? FileNames { get; set; }
        public int ImageParent { get; set; }
        public string? EmailTo { get; set; }
        public QuickEmailOptOutSetting QuickEmailOptOutSetting { get; set; }= QuickEmailOptOutSetting.QuickEmail;
        public string? StrId { get; set; }
        public string? AttachmentFilter { get; set; }

        // SharePoint
        public string? SharePointDocLibrary { get; set; }
        public string? SharePointDocLibraryFolder { get; set; }
        public string? SharePointRecKey { get; set; }

        //DocVerification
        public bool IsPopup { get; set; } = false;
    }

    public class QuickEmailAttachmentFilterViewModel {
        public string[]? FileType { get; set; }
        public string? DocumentName { get; set; }
        public string[]? Tags { get; set; }
    }


}
