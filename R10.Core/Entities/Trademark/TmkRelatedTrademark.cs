using R10.Core.Entities.Trademark;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkRelatedTrademark : BaseEntity
    {
        [Key]
        public int RelatedId { get; set; }
        public int TmkId { get; set; }
        public int RelatedTmkId { get; set; }
        
        public TmkTrademark? Trademark { get; set; }
        public TmkTrademark? RelatedTrademark { get; set; }
    }
}
