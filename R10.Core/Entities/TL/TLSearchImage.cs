using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TLSearchImage
    {
        [Key]
        public int TLImageId { get; set; }
        public int TLTmkId { get; set; }
        public int OrderOfEntry { get; set; }
        public string? OrigFileName { get; set; }
        public int FileId { get; set; }
        public bool? Transferred { get; set; }
        public TLSearch? TLSearch { get; set; }
    }

}
