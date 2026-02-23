using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchLSDDTO
    {
        public int AppId { get; set; }

        [Display(Name = "Event Date")]
        public DateTime? EventDate { get; set; }

        [Display(Name="Event Code")]
        public string? EventCode { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Effectivity Date")]
        public DateTime? EffectivityDate { get; set; }
    }
}
