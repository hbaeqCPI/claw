using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace LawPortal.Core.Entities.Documents
{
    public class DocGmailCaseLink
    {
        [Key]
        public int LinkId { get; set; }
        public string? EmailId { get; set; }

        public string? SystemType { get; set; }
        public string? DataKey { get; set; }
        public int DataKeyValue { get; set; }
        public int DocId { get; set; }

        [StringLength(20)]
        public string? CreatedBy { get; set; }

        public DateTime? DateCreated { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[] tStamp { get; set; }

    }
}
