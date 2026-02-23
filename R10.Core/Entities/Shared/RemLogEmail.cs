using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities
{
    public class RemLogEmail<TDue, TRemLogDue> : RemLogEmailDetail where TRemLogDue:RemLogDue
    {
        public RemLog<TDue, TRemLogDue> RemLog { get; set; }
    }

    public class RemLogEmailDetail
    {
        [Key]
        public int LogEmailId { get; set; }

        public int RemId { get; set; }

        public DateTime SentDate { get; set; }

        [StringLength(320)]
        public string? Sender { get; set; }

        [StringLength(255)]
        public string? SendOption { get; set; }

        [StringLength(10)]
        public string? Client { get; set; }

        [StringLength(60)]
        public string? ClientName { get; set; }

        [StringLength(10)]
        public string? Contact { get; set; }

        [StringLength(60)]
        public string? ContactName { get; set; }

        [StringLength(320)]
        public string? Email { get; set; }

        [StringLength(1000)]
        public string? Subject { get; set; }

        public string? Body { get; set; }

        [StringLength(255)]
        public string? Attachment { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[] tStamp { get; set; }
    }
}
