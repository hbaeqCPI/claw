using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchTitleDTO
    {
        public int PLAppId { get; set; }

        [Display(Name = "Language")]
        public string? Language { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }
      
    }
}
