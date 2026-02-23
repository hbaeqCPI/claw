using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchDocCitedDTO
    {
        public int PLAppId { get; set; }

        [Display(Name= "References Cited")]
        public string? DocCited { get; set; }
        
        public string? DocUrl { get; set; }
      
    }
}
