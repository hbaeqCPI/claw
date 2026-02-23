using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class ImageEntityDTO : BaseEntity
    {
        [Key]
        public int ImageId { get; set; }

        [Required]
        public int ParentId { get; set; }

        [Required, StringLength(255)]
        public string? ImageTitle { get; set; }

        [Required, StringLength(255)]
        public string? ImagePathName { get; set; }

        public string? ImageFile { get; set; }
        public string? ThumbnailFile { get; set; }

        public bool IsPrintOnReport { get; set; }

        public int? ImageTypeId { get; set; }

        public DateTime ImageDate { get; set; }

        public string? ImageSource { get; set; }

        public int OrderOfEntry { get; set; }

        public string? Remarks { get; set; }

        public string? ThumbnailPath { get; set; }

        public int? FileID { get; set; }

        public long? FileSize { get; set; }

        public bool IsDefault { get; set; }

        public bool IsPublic { get; set; }

        public ImageType ImageType { get; set; }

        public string? ScreenCode { get; set; }
        public int LinkId { get; set; }
    }
}
