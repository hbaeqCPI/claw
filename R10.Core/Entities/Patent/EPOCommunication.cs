using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class EPOCommunication :EPOCommunicationDetail
    {
        public List<EPOCommunicationDoc>? CommunicationDocs { get; set; }

        public List<PatEPOMailLog>? PatEPOMailLogs { get; set; }
    }

    public class EPOCommunicationDetail : BaseEntity
    {
        [Key]
        public int KeyId { get; set; }
        public int LogId { get; set; }

        [Required]       
        public string?  CommunicationId { get; set; }   
        
        public string?  ApplicantName { get; set; }        
        public string?  Title { get; set; }
        public DateTime? DispatchDate { get; set; }
        public string? ApplicationNumber { get; set; }
        public string? RecipientName { get; set; }
        public string? UserReference { get; set; }
        public string? Document { get; set; }
        public string? DigitalFile { get; set; }
        public string? DocumentCode { get; set; }
        public bool Read { get; set; }
        public string? FolderId { get; set; }
        public bool Handled { get; set; } = false;
    }
}
