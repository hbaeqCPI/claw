using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSInstrxCPiLog
    {
        [Key]
        public int SendId { get; set; }

        public DateTime SendDate { get; set; }

        public DateTime CPIConfirmDate { get; set; }

        [StringLength(1)]
        public string SendMethod { get; set; }

        [StringLength(20)]
        public string CreatedBy { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[] tStamp { get; set; }

        public List<AMSInstrxCPiLogDetail> AMSInstrxCPiLogDetails { get; set; }
        public List<AMSInstrxCPiLogEmail> AMSInstrxCPiLogEmails { get; set; }
        public List<AMSInstrxCPiLogError> AMSInstrxCPiLogErrors { get; set; }
    }
}
