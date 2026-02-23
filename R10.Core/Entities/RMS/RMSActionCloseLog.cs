using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.RMS
{
    public class RMSActionCloseLog
    {
        [Key]
        public int LogId { get; set; }

        public DateTime CloseDate { get; set; }

        public string Filter { get; set; }

        [StringLength(20)]
        public string CreatedBy { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[] tStamp { get; set; }

        public List<RMSActionCloseLogDue> RMSActionCloseLogDues { get; set; }
        public List<RMSActionCloseLogEmail> RMSActionCloseLogEmails { get; set; }
        public List<RMSActionCloseLogError> RMSActionCloseLogErrors { get; set; }
    }
}
