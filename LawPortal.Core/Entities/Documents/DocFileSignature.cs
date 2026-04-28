using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Documents
{
    public class DocFileSignature : BaseEntity
    {
        [Key]
        public int SignatureFileId { get; set; }
        public int FileId { get; set; }
        public string? EnvelopeId { get; set; }
        public bool? SignatureCompleted { get; set; }
        public int SignedDocFileId { get; set; }

        public string? SystemType { get; set; }
        public string? ScreenCode { get; set; }
        public string? DataKey { get; set; }
        public int DataKeyValue { get; set; }
        public int QESetupId { get; set; }
        public string? RoleLink { get; set; }
        
        public bool SignatureReviewed { get; set; } = false;
        public string? SignatureReviewedBy { get; set; }
        public DateTime? SignatureReviewedDate { get; set; }
        
        public string? EnvelopeStatus { get; set; }

        public DocFile? DocFile { get; set; }
    }
}
