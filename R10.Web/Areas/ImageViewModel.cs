using Microsoft.AspNetCore.Http;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web
{

    //public class ImageViewModel : ImageEntity
    //{
    //    public string? Url { get; set; }

    //    [Display(Name = "Image Type")]
    //    public string? ImageTypeName { get; set; }

    //    [Display(Name= "User File Name")]
    //    public string? UserFileName { get; set; }

    //    public IEnumerable<IFormFile>? Files { get; set; }
    //    public IFormFile? File { get; set; }

    //    public string? UploadAction { get; set; }

    //    public string? ImageSelected { get; set; }

    //    public string? Area { get; set; }
    //    public string? Controller { get; set; }
    //    public bool IsRestrictImageAccess { get; set; }
    //    public bool HasDefaultImage { get; set; } = true;
    //    public string? ScreenCode { get; set; }
    //    public string? ScreenImageId { get; set; }
    //    public byte[]? FileBytes { get; set; }

    //    [Display(Name = "Release Lock?")]
    //    public bool ReleaseFileLock { get; set; }
    //    public int FolderId { get; set; }

    //    public bool FolderIsPublic { get; set; }
    //    public string? FolderCreatedBy { get; set; }

    //    public string? DocumentLink { get; set; }
    //    public string? Folder { get; set; }

    //}

    public class ImageViewModel {
       public int ImageId { get; set; }
       public int ParentId { get; set; }
       public string? ImageFile { get; set; }
       public string? ImageTitle { get; set; }
       public string? ImageSource { get; set; }
    }

    public class ImageSearchViewModel{

        public string? Controller { get; set; }
        public int ParentId { get; set; }
        public bool HasScreenSource { get; set; }
        public string? SharePointDocLibrary { get; set; }
        public string? SharePointDocLibraryFolder { get; set; }
        public string? SharePointRecKey { get; set; }
    }

    public class ImageSearchResultViewModel
    {
        public string? ImageTitle { get; set; }
        public string? ImageTypeName { get; set; }
        public string? UserFileName { get; set; }
    }

    
}

