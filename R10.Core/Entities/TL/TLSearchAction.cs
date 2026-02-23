using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TLSearchAction
    {
        [Key]
        public int TLActId { get; set; }
        public int TLTmkId { get; set; }
        public int OrderOfEntry { get; set; }
        public string? SearchAction { get; set; }
        public DateTime? BaseDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? DateCompleted { get; set; }
        public DateTime? LastWebUpdate { get; set; }

        public TLSearch? TLSearch { get; set; }
    }

}
