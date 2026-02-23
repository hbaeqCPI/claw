using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSPFSTitleUpdHistoryDTO
    {
        public int AppId { get; set; }

        [Display(Name = "Batch ID")]
        public string? BatchId { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "Updated By")]
        public string? UserId { get; set; }

        [Display(Name = "Reverted?")]
        public bool UndoFlag { get; set; }

        [Display(Name = "Last Update")]
        public DateTime? LastUpdate { get; set; }
    }
}
