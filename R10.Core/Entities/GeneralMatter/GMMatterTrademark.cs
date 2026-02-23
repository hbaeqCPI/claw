using R10.Core.Entities.Trademark;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMMatterTrademark : BaseEntity
    {
        [Key]
        public int GMTId { get; set; }

        public int MatId { get; set; }

        public int? TmkId { get; set; }

        [StringLength(25)]
        public string? CaseNumber { get; set; }

        [StringLength(5)]
        public string? Country { get; set; }

        [StringLength(8)]
        public string? SubCase { get; set; }

        [StringLength(255)]
        public string? Trademark { get; set; }

        [StringLength(20)]
        public string? AppNumber { get; set; }

        [StringLength(20)]
        public string? RegNumber { get; set; }

        [StringLength(20)]
        public string? PubNumber { get; set; }

        [StringLength(3)]
        public TmkTrademark? TrademarkData { get; set; }

        public GMMatter? GMMatter { get; set; }
    }
}
