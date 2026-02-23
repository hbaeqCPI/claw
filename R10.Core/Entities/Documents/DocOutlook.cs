using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Documents
{
    public class DocOutlook 
    {
        [Key]
        public int EmailId { get; set; }
        public string? OLItemId { get; set; }
        public string? OLSender { get; set; }
        public string? OLFrom { get; set; }
        public string? OLTo { get; set; }
        public string? OLCc { get; set; }
        public string? OLBcc { get; set; }
        public string? OLReplyTo { get; set; }
        public string? OLSubject { get; set; }
        public string? OLBodyPreview { get; set; }
        public string? OLImportance { get; set; }

        public DateTime OLSent { get; set; }
        public DateTime? OLReceived { get; set; }
        public DateTime? OLModified { get; set; }
        
        public string? OLSavedAttachments { get; set; }

        public int FileId { get; set; }
        public int CPiEmailId { get; set; }                 // this is the Id assigned by CPi, after logging

        public string? CreatedBy { get; set; }
        public DateTime? DateCreated { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[] tStamp { get; set; }
    }
}
