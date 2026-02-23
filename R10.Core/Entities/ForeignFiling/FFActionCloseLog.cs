using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.ForeignFiling
{
    public class FFActionCloseLog
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

        public List<FFActionCloseLogDue> FFActionCloseLogDues { get; set; }
        public List<FFActionCloseLogEmail> FFActionCloseLogEmails { get; set; }
        public List<FFActionCloseLogError> FFActionCloseLogErrors { get; set; }
    }
}
