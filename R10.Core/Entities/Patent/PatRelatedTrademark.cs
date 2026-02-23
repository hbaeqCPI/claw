using R10.Core.Entities.Trademark;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatRelatedTrademark : BaseEntity
    {
        [Key]
        public int RelatedTmkId { get; set; }
        public int AppId { get; set; }
        public int TmkId { get; set; }

        public TmkTrademark? Trademark { get; set; }
        public CountryApplication? Application { get; set; }
    }
}
