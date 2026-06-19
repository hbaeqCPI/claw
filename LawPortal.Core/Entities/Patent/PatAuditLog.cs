using System.ComponentModel.DataAnnotations;

namespace LawPortal.Core.Entities.Patent
{
    public class PatAuditLog
    {
        [Key]
        public long AuditLogId { get; set; }

        public DateTime ChangedAt { get; set; }

        [StringLength(100)]
        public string? ChangedBy { get; set; }

        [StringLength(1)]
        public string? Action { get; set; }

        [StringLength(100)]
        public string? TableName { get; set; }

        [StringLength(500)]
        public string? RecordId { get; set; }

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }
    }
}
