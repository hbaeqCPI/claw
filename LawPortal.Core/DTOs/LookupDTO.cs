using Microsoft.EntityFrameworkCore;

namespace LawPortal.Core.DTOs
{
    [Keyless]
    public class LookupDTO
    {
        public string? Value { get; set; }
        public string? Text { get; set; }
    }

    [Keyless]
    public class LookupDescDTO
    {
        public string? Value { get; set; }
        public string? Text { get; set; }
        public string? Description { get; set; }
    }

    [Keyless]
    public class LookupIntDTO
    {
        public int Value { get; set; }
    }
}