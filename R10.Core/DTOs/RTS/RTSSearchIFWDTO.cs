using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchIFWDTO
    {
        public int PLAppId { get; set; }

        [Display(Name="No.")]
        public int OrderOfEntry { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Mail Room Date")]
        public DateTime? MailRoomDate { get; set; }

        [Display(Name = "Filename")]
        public string? Filename { get; set; }

        [Display(Name = "Page Count")]
        public int NoPages { get; set; }
        public int PageStart { get; set; }

        public string? DocName { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }
        public string? DriveItemId { get; set; }
    }
}
