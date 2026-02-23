using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchContinuityChildDTO
    {
        public int PLAppId { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

    }
}
