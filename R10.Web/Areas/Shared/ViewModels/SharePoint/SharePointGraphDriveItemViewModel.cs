using Microsoft.Graph;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Areas.Shared.ViewModels.SharePoint
{
    public class SharePointGraphDriveItemViewModel
    {
        public string? Path { get; set; }
        public string? DocLibraryFolder { get; set; }
        public string? RecKey { get; set; }
        public DriveItem DriveItem { get; set; }
      
    }

    public class SharePointGraphDriveItemParamViewModel
    {
        public string? DocLibrary { get; set; }
        public string? Name { get; set; }
        public string? DriveItemId { get; set; }
  
    }

    public class SharePointGraphDriveItemVersionViewModel
    {
        [Display(Name = "Version")]
        public string? Id { get; set; }
        public string? Name { get; set; }

        [Display(Name = "Modified On")]
        public DateTime? DateModified { get; set; }
        //public DateTimeOffset? DateModified_Offset { get; set; }

        [Display(Name = "Modified By")]
        public string? ModifiedBy { get; set; }

        [Display(Name = "Size")]
        public long? Size { get; set; }
        public string? DownloadUrl { get; set; }
        public string? RestoreUrl { get; set; }
    }

    public class SharePointGraphDriveItemKeyViewModel
    {
        public string? DriveId { get; set; }
        public string? DriveItemId { get; set; }

    }

    public class SharePointGraphTreeViewModel
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public bool IsFolder { get; set; }
    }

    public class SharePointGraphDefaultImageViewModel
    {
        public string? Id { get; set; }
        public string? DriveId { get; set; }
        public string? RecKey { get; set; }
        public string? ImageFile { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? DisplayUrl { get; set; }
    }
    public class SharePointGraphDefaultImageParamViewModel
    {
        public string? Id { get; set; }
        public string? RecKey { get; set; }
    }

    public class SharePointGraphThumbnailViewModel
    {
        public string? SmallThumbnailUrl { get; set; }
        public string? MediumThumbnailUrl { get; set; }
        public string? BigThumbnailUrl { get; set; }
    }

    public class SharePointGraphDocPicklistViewModel
    {
        public string? Id { get; set; }
        public string? Folder { get; set; }
        public string? DocName { get; set; }
        public int ParentId { get; set; }
        public string? RecKey { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? DateModified { get; set; }
        public bool IsPrivate { get; set; }
    }

    public class SharePointGraphDocumentSetViewModel
    {
        public string? DocLibrary { get; set; }
        public string? DocumentSet { get; set; }

    }
}
