using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Documents
{
    public class DocDocumentTag : BaseEntity
    {
        [Key]
        public int DocTagId { get; set; }
        
        public int DocId { get; set; }
        public string? Tag { get; set; }

        public DocDocument? DocDocument { get; set; }
    }
}
