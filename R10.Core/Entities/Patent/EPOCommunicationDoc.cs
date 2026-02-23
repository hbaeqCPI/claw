using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class EPOCommunicationDoc : EPOCommunicationDocDetail
    {
        public EPOCommunication? Communication { get; set; }
    }
    public class EPOCommunicationDocDetail : BaseEntity
    {
        [Key]
        public int KeyId { get; set; }
                
        [Required]       
        public string?  CommunicationId { get; set; }
        [Required]
        public int DocId { get; set; }      
        public int WorkflowStatus { get; set; }
        public string? WorkflowError { get; set; }
        public string? EmailWorkflow { get; set; }
    }    
}
