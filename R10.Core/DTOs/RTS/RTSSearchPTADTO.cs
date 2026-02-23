using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchPTADTO
    {
        public int PLAppId { get; set; }

        [Display(Name = "No.")]
        public int OrderOfEntry { get; set; }

        [Display(Name="Description")]
        public string? Description { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? Date { get; set; }

        [Display(Name = "Days")]
        public string? Days { get; set; }
    }
}
