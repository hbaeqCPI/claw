using Microsoft.EntityFrameworkCore;

namespace LawPortal.Core.DTOs
{
    [Keyless]
    public class ChartDTO
    {
        public string? Group { get; set; }
        public string? Category { get; set; }
        public decimal Value { get; set; }
        public string? Color { get; set; }
        public bool Explode { get; set; }

        //Widgets using stored procs against ChartDTO will break
        //if new properties are added without modifying the stored
        //procs. Use inheritance for convenience. 
        //
        //Only use DTOs if using stored procs.
        //Use view models if not using stored procs.
    }
}
