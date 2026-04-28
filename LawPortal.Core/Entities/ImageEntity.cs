using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LawPortal.Core.Entities
{
    public class ImageEntity : BaseEntity
    {
        [Key]
        public int DocId { get; set; }
        public int FileId { get; set; }
        public int ParentId { get; set; }
        public string? DocName { get; set; }
        public string? DocFileName { get; set; }
        public string? ThumbFileName { get; set; }
        public bool? IsPrintOnReport { get; set; } = true;
        public int? DocTypeId { get; set; }
        public string? DocUrl { get; set; }
        public DateTime? ImageDate { get; set; }
        public string? Remarks { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsPrivate { get; set; }
        public string? UserFileName { get; set; }
        public string? ScreenCode { get; set; }
    }
}
