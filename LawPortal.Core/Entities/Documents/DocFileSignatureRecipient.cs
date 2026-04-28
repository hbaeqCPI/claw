using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Documents
{
    public class DocFileSignatureRecipient : BaseEntity
    {
        [Key]
        public int RepId { get; set; }
        public string? EnvelopeId { get; set; }        
        public int? RecipientId { get; set; }
        public string? RecipientName { get; set; }
        public string? Email { get; set; }
        public int? RoutingOrder { get; set; }
        public string? Status { get; set; }
        public string? RecipientType { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? SignedDate { get; set; }
    }
}
