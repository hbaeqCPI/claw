using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSInstrxCPiLogDetail
    {
        [Key]
        public int SendDetailId { get; set; }

        public int SendId { get; set; }

        public int DueId { get; set; }

        public int ClientInstructionLogId { get; set; }

        [StringLength(5)]
        public string SentInstructionType { get; set; }

        public DateTime SentInstructionDate { get; set; }

        [StringLength(5)]
        public string CPITaxSchedule { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[] tStamp { get; set; }

        public AMSInstrxCPiLog AMSInstrxCPiLog { get; set; }
        public AMSDue AMSDue { get; set; }
        public AMSInstrxType SentInstrxType { get; set; }
        public AMSInstrxChangeLog AMSInstrxChangeLog { get; set; }
    }
}
