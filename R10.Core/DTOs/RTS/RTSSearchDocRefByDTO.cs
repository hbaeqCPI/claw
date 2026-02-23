using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchDocRefByDTO
    {
        public int PLAppId { get; set; }

        [Display(Name= "Referenced By")]
        public string? DocRefBy { get; set; }

        [Display(Name = "Title")]
        public string? DocTitle { get; set; }


        public string? DocUrl { get; set; }
      
    }
}
