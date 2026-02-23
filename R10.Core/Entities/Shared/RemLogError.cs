using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities
{
    public class RemLogError<TDue, TRemLogDue> where TRemLogDue:RemLogDue
    {
        [Key]
        public int LogErrorId { get; set; }

        public int RemId { get; set; }

        public string? Message { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[] tStamp { get; set; }

        public RemLog<TDue, TRemLogDue> RemLog { get; set; }
    }
}
