using System.ComponentModel.DataAnnotations;


namespace R10.Core.Entities.Trademark
{
    public class TLSearchTTABParty
    {
        [Key]
        public int PartyInfoId { get; set; }
        public int TTABId { get; set; }
        public string? Identifier { get; set; }
        public string? RoleCode { get; set; }
        public string? Name { get; set; }
        public int OrderOfEntry { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}
