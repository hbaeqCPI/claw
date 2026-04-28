using Microsoft.EntityFrameworkCore;

namespace LawPortal.Core.DTOs
{
    [Keyless]
    public class WebLinksNumberTemplateDTO
    {
        public string? EffBasedOn { get; set; }
        public DateTime? EffFromDate { get; set; }
        public DateTime? EffToDate { get; set; }
        public string? Template { get; set; }
        public Int16 MinLength { get; set; }
        public Int16 MaxLength { get; set; }
    }
}
