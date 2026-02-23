using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class GlobalUpdateLookupDTO
    {
        public int EntityId { get; set; }
        public string? EntityCode { get; set; }
        public string? EntityName { get; set; }
    }
}