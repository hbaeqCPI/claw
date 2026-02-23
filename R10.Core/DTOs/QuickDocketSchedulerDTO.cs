using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class QuickDocketSchedulerDTO
    {
        public int TaskID { get; set; }
        public int OwnerID { get; set; }
        public string? Title { get; set; } 
        public string? ActionDueSubject { get; set; } 
        public DateTime Start { get; set; } // DueDateDisplay
        public DateTime End { get; set; } // DueDateDisplay
        public string? Description { get; set; } // CaseDescription
        public string? StartTimezone { get; set; }
        public string? EndTimezone { get; set; }
        public string? RecurrenceRule { get; set; }
        public string? RecurrenceException { get; set; }
        public bool IsAllDay { get; set; }
        public bool IsPastDue { get; set; }

        public int ActId { get; set; }
        public string? System { get; set; }
        public string? ActionDue { get; set; }
        public string? CaseNumber { get; set; }
        public string? SubCase { get; set; }
        public string? Country { get; set; }        
        public string? AppNumber { get; set; }
        public DateTime? FilDate { get; set; }
        public string? RecordTitle { get; set; }
        public DateTime DueDate { get; set; }

        public string? Indicator { get; set; }
    }
}
