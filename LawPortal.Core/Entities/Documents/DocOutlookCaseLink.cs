using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Documents
{
    public class DocOutlookCaseLink 
    {
        [Key]
        public int LinkId { get; set; }
        public int EmailId { get; set; }

        public string? SystemType { get; set; }
        public string? DataKey { get; set; }
        public int DataKeyValue { get; set; }
        public int DocId { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? DateCreated { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[] tStamp { get; set; }

    }
}
