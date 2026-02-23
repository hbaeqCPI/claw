using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchUSCorrespondenceDTO
    {
        public int PLAppId { get; set; }

        [Display(Name= "Correspondence")]
        public string? Correspondence { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [NotMapped]
        public List<RTSSearchAgentDTO>? Agents { get; set; }
        
    }

    
}
