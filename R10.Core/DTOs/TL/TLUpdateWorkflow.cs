using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class TLUpdateWorkflow
    {
        public int TmkId { get; set; }
        public string? OldTrademarkStatus { get; set; }
        public DateTime? TriggerDate { get; set; }
        public int ClientId { get; set; }
    }
}
