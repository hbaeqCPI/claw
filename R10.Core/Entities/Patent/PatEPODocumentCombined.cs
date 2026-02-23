using R10.Core.Entities.Documents;
using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatEPODocumentCombined : PatEPODocumentCombinedDetail
    {
        public DocDocument? DocDocument { get; set; }
    }
    public class PatEPODocumentCombinedDetail : BaseEntity
    {
        [Key]
        public int KeyId { get; set; }

        [Required]  
        public int CombinedDocId { get; set; }
        [Required]       
        public string?  CommunicationId { get; set; }

        public int OrderOfEntry { get; set; }
        public int GuideId { get; set; }
    }    
}
