using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark

{
    public class TLSearchTTAB
    {
        [Key]
        public int TTABId { get; set; }
        public string? Status { get; set; }
        public int OrderOfEntry { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdate { get; set; }
        public int TLTmkId { get; set; }
    }
}
