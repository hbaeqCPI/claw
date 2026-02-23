using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class DefaultImageViewModel
    {
        public int ImageId { get; set; }
        public string? ImageFile { get; set; }
        public string? ImageTitle { get; set; }
        public string? ImageTypeName { get; set; }
        public string? ThumbnailFile { get; set; }
        public bool IsPublic { get; set; }
        public string? System { get; set; }
        public string? ScreenCode { get; set; }
        public int Key { get; set; }
        public int FolderId { get; set; }

        public string? DriveItemId { get; set; }
        public string? RootContainerId { get; set; }
        public string? DefaultFolderId { get; set; }

        public string? SharePointImageUrl { get; set; }
        public string? SharePointDriveItemId { get; set; }
        public string? SharePointDocLibrary { get; set; }
        public string? SharePointDriveId { get; set; }
    }
}
