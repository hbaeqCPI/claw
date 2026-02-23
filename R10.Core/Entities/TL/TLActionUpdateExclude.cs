using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TLActionUpdateExclude
    {
        [Key]
        public int ExcludeActionId { get; set; }
        public int TLTmkID { get; set; }
        public string? ActionType { get; set; }
        public string? ActionDue { get; set; }
        public DateTime? BaseDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
    }

}
