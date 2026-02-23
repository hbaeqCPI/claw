using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.ForeignFiling
{
    public class FFActionCloseLogDue
    {
        [Key]
        public int LogDueId { get; set; }

        public int LogId { get; set; }

        public int DueId { get; set; }

        public int ClientInstructionLogId { get; set; }

        [StringLength(5)]
        public string SentInstructionType { get; set; }

        public DateTime SentInstructionDate { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[] tStamp { get; set; }

        public FFActionCloseLog FFActionCloseLog { get; set; }
        public PatDueDate PatDueDate { get; set; }
        public FFInstrxType SentInstrxType { get; set; }
        public FFInstrxChangeLog FFInstrxChangeLog { get; set; }
    }
}
