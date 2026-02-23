using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class SharedCountryLookupDTO 
    {
        public string? Country { get; set; }
        public string? CountryName { get; set; }
    }
}
