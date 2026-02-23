using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities
{
    public class LetterLog
    {
        [Key]
        public int LetLogId { get; set; }
        [StringLength(1)]
        public string?  SystemType { get; set; }
        public int ScreenId { get; set; }
        public int LetId { get; set; }
        [StringLength(150)]
        public string?  LetFile { get; set; }
        public int LogLetId { get; set; }
        public int LogCatId { get; set; }
        [StringLength(12)]
        
        public string?  DataKey { get; set; }

        [StringLength(20)]
        public string?  GenBy { get; set; }
        public DateTime GenDate { get; set; }
        public DateTime? MetaUpdate { get; set; } = DateTime.Now;           // Azure blob storage metadata update date
        public string? ItemId { get; set; }
        public string? EnvelopeId { get; set; }
        public bool SentToDocuSign { get; set; }
        public bool SignatureCompleted { get; set; }
        public bool SignatureReviewed { get; set; } = false;
        public string? SignedFileName { get; set; }
        public string? SignatureReviewedBy { get; set; }
        public DateTime? SignatureReviewedDate { get; set; }
        public string? SignedDocDriveItemId { get; set; }
        public int? SignedLetLogId { get; set; }
        public string? EnvelopeStatus { get; set; }
        

        public List<LetterLogDetail>? LetterLogDetails { get; set; }
    }

    public class LetterLogDetail
    {
        [Key]
        public int LogDtlId { get; set; }

        [ForeignKey("LetLogId")]
        public int LetLogId { get; set; }
        public int DataKeyValue { get; set; }
        public string?  EntityType { get; set; }
        public int EntityId { get; set; }
        public int ContactId { get; set; }
        public int? LogEntityId { get; set; }
        public int? LogContactId { get; set; }
        public LetterLog? LetterLog { get; set; }
        
    }
}
