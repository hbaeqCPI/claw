using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchAbstractDTO
    {
        public int PLAppId { get; set; }

        [Display(Name="Patent Office Abstract")]
        public string? Abstract { get; set; }

        [Display(Name = "Patent Office Claims")]
        public string? Claims { get; set; }
      
    }
}
