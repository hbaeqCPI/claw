using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Trademark
{
    public class TLMapActionDocument
    {
        [Key]
        public int MapId { get; set; }
        public string? SearchAction { get; set; }
        public string? DocumentDescription { get; set; }
        public string? Country { get; set; }
        public string? DocCode { get; set; }
        public bool IsActRequired { get; set; }
        public bool CheckAct { get; set; }
        public bool SendToClient { get; set; }
        public bool UseAI { get; set; }
        public bool UseWatch { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string? WatchModelId { get; set; }

        [NotMapped]
        public string? SystemType { get; set; }
        [NotMapped]
        public bool IsGenAction { get; set; }
        [NotMapped]
        public int MapHdrId { get; set; }
    }

    public class TLMapActionDocumentClient : BaseEntity
    {
        [Key]
        public int MapClientId { get; set; }
        public int MapId { get; set; }
        public int ClientID { get; set; }

        [NotMapped]
        [Display(Name = "Client")]
        public string? ClientCode { get; set; }

        [NotMapped]
        [Display(Name = "Client Name")]
        public string? ClientName { get; set; }

        public Client? Client { get; set; }
    }

}
