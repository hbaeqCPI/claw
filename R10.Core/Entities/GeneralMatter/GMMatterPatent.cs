using R10.Core.Entities.Patent;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMMatterPatent : BaseEntity
    {
        [Key]
        public int GMPId { get; set; }

        public int MatId { get; set; }

        public int? InvId { get; set; }

        public int? AppId { get; set; }

        [StringLength(25)]
        public string? CaseNumber { get; set; }

        [StringLength(5)]
        public string? Country { get; set; }

        [StringLength(8)]
        public string? SubCase { get; set; }

        [StringLength(255)]
        public string? Patent { get; set; }

        [StringLength(20)]
        public string? AppNumber { get; set; }

        [StringLength(20)]
        public string? PatNumber { get; set; }

        [StringLength(20)]
        public string? PubNumber { get; set; }

        public GMMatter? GMMatter { get; set; }

        public CountryApplication? ApplicationData { get; set; }

        public Invention? InventionData { get; set; }
    }
}
