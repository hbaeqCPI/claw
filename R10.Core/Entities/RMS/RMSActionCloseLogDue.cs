using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.RMS
{
    public class RMSActionCloseLogDue
    {
        [Key]
        public int LogDueId { get; set; }

        public int LogId { get; set; }

        public int DueId { get; set; }

        public int ClientInstructionLogId { get; set; }

        [StringLength(5)]
        public string SentInstructionType { get; set; }

        public DateTime SentInstructionDate { get; set; }
        public DateTime? NextRenewalDate { get; set; } //next renewal date used when generating new actions during action closing
        public bool? CloseAction { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[] tStamp { get; set; }

        public RMSActionCloseLog RMSActionCloseLog { get; set; }
        public TmkDueDate TmkDueDate { get; set; }
        public RMSInstrxType SentInstrxType { get; set; }
        public RMSInstrxChangeLog RMSInstrxChangeLog { get; set; }
    }
}
