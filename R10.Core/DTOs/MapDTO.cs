using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class MapDTO
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Country { get; set; }
        public string? CountryName { get; set; }
        public string? Status { get; set; }
        public DateTime? FilDate { get; set; }
        public DateTime? IssRegDate { get; set; }
        public string? AppNumber { get; set; }
        public string? PatRegNumber { get; set; }
        public string? Title { get; set; }
        public string? SystemType { get; set; }
        public bool ActiveSwitch { get; set; }
        public string? DisplayType { get; set; }
    }
}
