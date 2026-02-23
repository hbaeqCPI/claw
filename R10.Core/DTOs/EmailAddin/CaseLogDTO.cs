using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class CaseLogDTO
    {
        public string? SystemName { get; set; }
        public string? Screen { get; set; }
        public string? CaseInfo { get; set; }
        public string? DateCreated { get; set; }
    }
}
