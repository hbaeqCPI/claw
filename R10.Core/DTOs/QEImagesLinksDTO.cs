using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.DTOs
{
    public class QEImagesLinksDTO
    {
        [Display(Name ="Send?")]
        public bool? Send { get; set; }
        public int? ParentId { get; set; }
        public string? FilePath { get; set; }
        public string? ThumbnailFile { get; set; }

        [Display(Name = "Image")]
        public string? ThumbnailPath { get; set; }

        [Display(Name = "Image Title")]
        public string? ImageTitle { get; set; }
        public string? Remarks { get; set; }
        public int? FileId { get; set; }
        public bool? IsFileExists { get; set; }
        public string? ScreenCode { get; set; }
        public bool? IsPrivate { get; set; }
        public string? CreatedBy { get; set; }
        public bool? IncludeInWorkflow { get; set; }
        public string? Tags { get; set; }
    }
    
}
