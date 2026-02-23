using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchInventorDTO
    {
        public int PLAppId { get; set; }

        [Display(Name="Inventor Name")]
        public string? Name { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

    }
}
