using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchIPCDTO
    {
        public int AppId { get; set; }
      
        [Display(Name = "IP Class")]
        public string? IPClass { get; set; }

    }
}
