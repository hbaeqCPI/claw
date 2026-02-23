using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Documents
{
    public class SharePointFileSignature : BaseEntity
    {
        [Key]
        public int SignatureFileId { get; set; }
        public string? DriveItemId { get; set; }
        public string? EnvelopeId { get; set; }
        public bool? SignatureCompleted { get; set; } = false;

        public string? SignedDocDriveItemId { get; set; }
        public string? DocLibrary { get; set; }
        public string? DocLibraryFolder { get; set; }
        //public string? RecKey { get; set; }
        public string? ScreenCode { get; set; }
        public int QESetupId { get; set; }
        public string? FileName { get; set; }
        public DateTime? FileDate { get; set; }
        public int ParentId { get; set; }
        public string? SystemTypeCode { get; set; }
        public string? RoleLink { get; set; }
        public bool? SignatureReviewed { get; set; } = false;
        public string? SignatureReviewedBy { get; set; }
        public DateTime? SignatureReviewedDate { get; set; }
        public string? SignedFileName { get; set; }
        public string? EnvelopeStatus { get; set; }
    }
}
