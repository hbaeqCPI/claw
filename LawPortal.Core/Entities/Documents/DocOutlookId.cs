using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Core.Entities.Documents
{
    public class DocOutlookId
    {
        [Key]
        public int CPiEmailId { get; set; }                 // this is the Id assigned by CPi, after logging
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
