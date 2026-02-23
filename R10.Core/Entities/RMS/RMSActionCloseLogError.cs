using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.RMS
{
    public class RMSActionCloseLogError
    {
        [Key]
        public int LogErrorId { get; set; }

        public int LogId { get; set; }

        public string Message { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[] tStamp { get; set; }

        public RMSActionCloseLog RMSActionCloseLog { get; set; }
    }
}
