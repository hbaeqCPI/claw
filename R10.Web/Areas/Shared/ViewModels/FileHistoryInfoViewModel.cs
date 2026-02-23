using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class FileHistoryInfoViewModel
    {
        public int RecId { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Date")]
        public DateTime? MailRoomDate { get; set; }

        public string? Filename { get; set; }
        public int NoPages { get; set; }
        public int PageStart { get; set; }
        public string? DocName { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }
        public string? ViewUrl { get; set; }
        public string? DriveItemId { get; set; }
    }
}
