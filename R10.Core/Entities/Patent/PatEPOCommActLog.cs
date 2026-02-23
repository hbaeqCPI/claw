using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatEPOCommActLog : BaseEntity
    {
        [Key]
        public int ActLogId { get; set; }
        public int LogId { get; set; }
        public string? CommunicationId { get; set; }
        public int ActId { get; set; }
        public int WorkflowStatus { get; set; }
        public string? WorkflowError { get; set; }
        public string? EmailWorkflow { get; set; }
    }    
}
