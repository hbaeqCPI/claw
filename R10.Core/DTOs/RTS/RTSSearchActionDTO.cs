using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchActionDTO
    {
        public int PLAppId { get; set; }
        public List<RTSSearchActionAsDownloadedDTO>? ActionsAsDownloaded { get; set; }
    }

    [Keyless]
    public class RTSSearchActionAsDownloadedDTO
    {
        public int PLAppId { get; set; }

        [Display(Name = "Action")]
        public string? SearchAction { get; set; }

        [Display(Name = "Base Date")]
        public DateTime BaseDate { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Completed Date")]
        public DateTime? DateCompleted { get; set; }

        public string? PMSAction { get; set; }
    }
    
}
