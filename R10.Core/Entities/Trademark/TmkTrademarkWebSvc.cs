using System;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkTrademarkWebSvc : TmkTrademarkWebSvcDetail
    {
        [Key]
        public int EntityId { get; set; }

        public int LogId { get; set; }
    }    

    public class TmkTrademarkWebSvcDetail
    {
        [Required]
        [StringLength(25)]
        public string? CaseNumber { get; set; }

        [Required]
        [StringLength(5)]
        public string? Country { get; set; }

        [StringLength(8)]
        public string? SubCase { get; set; }

        [StringLength(3)]
        public string? CaseType { get; set; }

        [StringLength(20)]
        public string? TrademarkStatus { get; set; } = "Unfiled";

        [StringLength(100)]
        public string? TrademarkName { get; set; }

        [StringLength(10)]
        public string? Agent { get; set; }

        [StringLength(20)]
        public string? PubNumber { get; set; }

        public DateTime? PubDate { get; set; }

        [StringLength(10)]
        public string? RespOffice { get; set; }
    }
}
