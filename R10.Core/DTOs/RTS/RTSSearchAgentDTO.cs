using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSSearchAgentDTO
    {
        public int PLAppId { get; set; }

        [Display(Name = "Reg #")]
        public string? RegNo { get; set; }

        [Display(Name = "Phone #")]
        public string? PhoneNo { get; set; }

        [Display(Name = "Agent")]
        public string? Agent { get; set; }
        
        [Display(Name = "Address")]
        public string? AgentAddress { get; set; }

        [Display(Name = "Country")]
        public string? AgentCountry { get; set; }
    }
}
