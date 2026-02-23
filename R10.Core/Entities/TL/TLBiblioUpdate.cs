using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TLBiblioUpdate: TMSEntityFilter
    {
        //[Key]
        public int TLTmkId { get; set; }
        public string? TMSAppNo { get; set; }
        public string? TMSPubNo { get; set; }
        public string? TMSRegNo { get; set; }
        public DateTime? TMSFilDate { get; set; }
        public DateTime? TMSPubDate { get; set; }
        public DateTime? TMSRegDate { get; set; }
        public DateTime? TMSAllowanceDate { get; set; }
        public DateTime? TMSNextRenewalDate { get; set; }
        public string? TLAppNo { get; set; }
        public string? TLPubNo { get; set; }
        public string? TLRegNo { get; set; }
        public DateTime? TLFilDate { get; set; }
        public DateTime? TLPubDate { get; set; }
        public DateTime? TLRegDate { get; set; }
        public DateTime? TLAllowanceDate { get; set; }
        public DateTime? TLNextRenewalDate { get; set; }
        public bool? UpdateAppNo { get; set; }
        public bool? UpdatePubNo { get; set; }
        public bool? UpdateRegNo { get; set; }
        public bool? UpdateFilDate { get; set; }
        public bool? UpdatePubDate { get; set; }
        public bool? UpdateRegDate { get; set; }
        public bool? UpdateAllowanceDate { get; set; }
        public bool? UpdateGoods { get; set; }
        public bool? UpdateNextRenewalDate { get; set; }

        public string? MarkAppNo { get; set; }
        public string? MarkPubNo { get; set; }
        public string? MarkRegNo { get; set; }
        public string? MarkFilDate { get; set; }
        public string? MarkPubDate { get; set; }
        public string? MarkRegDate { get; set; }
        public string? MarkAllowanceDate { get; set; }
        public string? MarkNextRenewalDate { get; set; }
        public int UpdateGoodsCount { get; set; }

        public bool? Exclude { get; set; }
        public DateTime? LastWebUpdate { get; set; }

        public bool? ActiveSwitch { get; set; }

        public string? TrademarkName { get; set; }

        public byte[]? tStamp { get; set; }
        
    }

    public class TMSEntityFilter {
        public int TMSTmkId { get; set; }
        public string? TMSCaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? TMSCountry { get; set; }

        [Display(Name = "Sub Case")]
        public string? TMSSubCase { get; set; }

        public string? TMSCaseType { get; set; }

        public string? RespOffice { get; set; }
        public int? ClientId { get; set; }
        public int? AgentId { get; set; }
        public int? Attorney1Id { get; set; }
        public int? Attorney2Id { get; set; }
        public int? Attorney3Id { get; set; }
    }
}
