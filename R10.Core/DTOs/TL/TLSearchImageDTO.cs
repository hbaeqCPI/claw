using Microsoft.EntityFrameworkCore;

namespace R10.Core.Entities.Trademark
{
    [Keyless]
    public class TLSearchImageDTO
    {
        public int TLTmkId { get; set; }
        public Int16 OrderOfEntry { get; set; }
        public string? OrigFileName { get; set; }
        public int FileId { get; set; }
        //public DateTime? LastWebUpdate { get; set; }
        
    }

}
