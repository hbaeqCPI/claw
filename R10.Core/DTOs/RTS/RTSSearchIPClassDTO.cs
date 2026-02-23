using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchIPClassDTO
    {
        public int PLAppId { get; set; }
        [Display(Name = "Class")]
        public string? IPClass { get; set; }
        public int OrderofEntry { get; set; }
        [Display(Name = "Class Type")]
        public string? ClassType { get; set; }
    }
}
