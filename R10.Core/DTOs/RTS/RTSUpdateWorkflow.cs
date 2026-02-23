using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSUpdateWorkflow
    {
        public int AppId { get; set; }
        public string? OldApplicationStatus { get; set; }
        public DateTime? TriggerDate { get; set; }
        public int ClientId { get; set; }
    }
}
