using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class EmailNotificationDTO
    {
        public int RecId { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }
        public DateTime? NoticeDate { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }

        public string? Attorney1Email {get; set; }
        public string? Attorney2Email { get; set; }
        public string? Attorney3Email { get; set; }
        public string? Attorney4Email { get; set; }
        public string? Attorney5Email { get; set; }

    }
}


