using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities
{
    public class RemLog<TDue, TRemLogDue> where TRemLogDue:RemLogDue
    {
        [Key]
        public int RemId { get; set; }

        public DateTime RemDate { get; set; }

        public string? Filter { get; set; }

        [StringLength(20)]
        public string? UserId { get; set; }

        public ReminderStatus? Status { get; set; } = 0;

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[] tStamp { get; set; }

        public List<TRemLogDue> RemLogDues { get; set; }
        public List<RemLogEmail<TDue, TRemLogDue>> RemLogEmails { get; set; }
        public List<RemLogError<TDue, TRemLogDue>> RemLogErrors { get; set; }
    }

    public enum ReminderStatus
    {
        Pending,
        Complete,
        Error
    }
}
