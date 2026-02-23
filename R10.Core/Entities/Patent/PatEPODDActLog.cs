using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatEPODDActLog : BaseEntity
    {
        [Key]
        public int ActLogId { get; set; }
        public int LogId { get; set; }
        public int EPODDId { get; set; }
        public int ActId { get; set; }
        public int WorkflowStatus { get; set; }
        public string? WorkflowError { get; set; }
        public string? EmailWorkflow { get; set; }
    }    
}
