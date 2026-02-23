using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchPriorityDTO
    {
        public int PLAppId { get; set; }

        [Display(Name = "Prio Country")]
        public string? PrioCountry { get; set; }

        [Display(Name = "Prio No")]
        public string? PrioNo { get; set; }

        [Display(Name = "Prio Date")]
        public DateTime? PrioDate { get; set; }

    }
}
