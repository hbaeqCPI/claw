using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LawPortal.Core.DTOs
{
    [Keyless]
    public class DocImageDetailDTO  
    {
        public int DocId { get; set; }

        public int ParentId { get; set; }

        [Display(Name = "Title")]
        public string? ImageTitle { get; set; }

        [Display(Name = "User File")]
        public string? UserFileName { get; set; }

        public bool IsImage { get; set; }
        public bool IsFile { get; set; }

        [Display(Name = "Print on Reports?")]
        public bool IsPrintOnReport { get; set; }

        [Display(Name = "Image Type")]
        public string? ImageTypeName { get; set; }

        [Display(Name = "Image Date")]
        public DateTime? ImageDate { get; set; }
        
        [Display(Name = "Default Image?")]
        public bool IsDefault { get; set; }
        [Display(Name = "Public?")]
        public bool IsPublic { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "URL/Link")]
        public string? DocUrl { get; set; }
        public string? DocFileName { get; set; }
        public string? ThumbnailFile { get; set; }
        public string? SystemType { get; set; }


        [NotMapped]
        public bool IsDocViewable { get; set; } = false;       // is file viewable by document viewer?

        [NotMapped]
        public bool IsDocLinkable { get; set; } = false;       // is document linkable?

        [NotMapped]
        public string? DocumentLink { get; set; }
    }
}
