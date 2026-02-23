using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class DocEntityDTO : BaseEntity
    {
        [Key]
        public int DocId { get; set; }
        public int? FileId { get; set; }
        public int ParentId { get; set; }
        public string? DocName { get; set; }
        public string? DocFileName { get; set; }
        public string? ThumbFileName { get; set; }
        public bool? IsPrintOnReport { get; set; }
        public int? DocTypeId { get; set; }
        public string? DocUrl { get; set; }
        public DateTime ImageDate { get; set; }
        public string? Remarks { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsPrivate { get; set; }
        public string? UserFileName { get; set; }
        public string? ScreenCode { get; set; }
        public bool? IncludeInWorkflow { get; set; }
        public string? Tags { get; set; }
    }
}
