using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchAssignmentDTO
    {
        public int PLAppId { get; set; }

        [Display(Name = "Assign From")]
        public string? Assignor { get; set; }

        [Display(Name = "Assign To")]
        public string? Assignee { get; set; }

        [Display(Name = "Date")]
        public DateTime? DateRecorded { get; set; }

        [Display(Name = "Reel/Frame")]
        public string? Reel { get; set; }

    }
}
