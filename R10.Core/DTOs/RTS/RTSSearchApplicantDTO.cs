using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchApplicantDTO
    {
        public int PLAppId { get; set; }
        [Display(Name = "Assignee")]
        public string? Name { get; set; }

        [Display(Name = "Assignee Address")]
        public string? Address { get; set; }
        public string? Country { get; set; }
        
      
    }
}
