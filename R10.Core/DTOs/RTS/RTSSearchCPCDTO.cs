using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchCPCDTO
    {
        public int AppId { get; set; }
      
        [Display(Name = "CPC")]
        public string? PCSymbol { get; set; }

    }
}
