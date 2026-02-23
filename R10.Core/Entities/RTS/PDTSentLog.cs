using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    
    public class PDTSentLog
    {
        [Key]
        public int LogId { get; set; }
        public string BatchID { get; set; }
        public DateTime? SentDate { get; set; }
        public string ReportType { get; set; }
        public bool? GenerateWorkflow { get; set; }
        public bool? WorkflowGenerated { get; set; }
    }
}

