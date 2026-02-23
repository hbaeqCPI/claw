using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class EFSLog
    {
        [Key]
        public int EfsLogId { get; set; }
        public string?  SystemType { get; set; }
        public int? ModuleId { get; set; }
        public int? LogEfsDocId { get; set; }
        public string?  EfsFile { get; set; }
        public string?  DataKey { get; set; }
        public int? DataKeyValue { get; set; }
        public short? PageNo { get; set; }
        public short? PageCount { get; set; }
        public DateTime? GenDate { get; set; }
        public string?  GenBy { get; set; }
        public DateTime? MetaUpdate { get; set; } = DateTime.Now;            // Azure blob storage metadata update date

        public string? ItemId { get; set; }
        public string? Signatory { get; set; }
        public string? EnvelopeId { get; set; }
        public bool SentToDocuSign { get; set; }
        public bool SignatureCompleted { get; set; }
        public bool SignatureReviewed { get; set; } = false;
        public string? SignatureReviewedBy { get; set; }
        public DateTime? SignatureReviewedDate { get; set; }
        public string? SignedFileName { get; set; }
        public string? SignedDocDriveItemId { get; set; }
        public int? SignedEfsLogId { get; set; }
        public string? EnvelopeStatus { get; set; }
    }
}
