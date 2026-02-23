using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Trademark
{
    public class TLSearch:BaseEntity
    {
        [Key]
        public int TLTmkId { get; set; }
        public int TMSTmkId { get; set; }
        public string? TMSCaseNumber { get; set; }
        public string? TMSCountry { get; set; }
        public string? TMSSubCase { get; set; }
        public string? TMSCaseType { get; set; }
        public string? TMSAppNo { get; set; }
        public string? TMSPubNo { get; set; }
        public string? TMSRegNo { get; set; }
        public DateTime? TMSFilDate { get; set; }
        public DateTime? TMSPubDate { get; set; }
        public DateTime? TMSRegDate { get; set; }
        public DateTime? TMSAllowanceDate { get; set; }
        public string? TMSStdAppNo { get; set; }
        public string? TMSStdPubNo { get; set; }
        public string? TMSStdRegNo { get; set; }
        //public bool? TMSUpdCaseType { get; set; }
        //public bool? TMSUpdAppNo { get; set; }
        //public bool? TMSUpdPubNo { get; set; }
        //public bool? TMSUpdRegNo { get; set; }
        //public bool? TMSUpdFilDate { get; set; }
        //public bool? TMSUpdPubDate { get; set; }
        //public bool? TMSUpdRegDate { get; set; }
        //public bool? TMSUpdAllowanceDate { get; set; }
        public string? TMSTempAppNo { get; set; }
        public string? TMSTempPubNo { get; set; }
        public string? TMSTempRegNo { get; set; }
        public string? TMSYearAppNo { get; set; }
        public string? TMSYearPubNo { get; set; }
        public string? TMSYearRegNo { get; set; }
        public string? TMSChkDgtAppNo { get; set; }
        public string? TMSChkDgtPubNo { get; set; }
        public string? TMSChkDgtRegNo { get; set; }
        public string? TMSCityAppNo { get; set; }
        public string? TMSCityPubNo { get; set; }
        public string? TMSCityRegNo { get; set; }
        public string? TLAppNo { get; set; }
        public string? TLPubNo { get; set; }
        public string? TLRegNo { get; set; }
        public DateTime? TLFilDate { get; set; }
        public DateTime? TLPubDate { get; set; }
        public DateTime? TLRegDate { get; set; }
        public DateTime? TLAllowanceDate { get; set; }
        public string? TLStdAppNo { get; set; }
        public string? TLStdPubNo { get; set; }
        public string? TLStdRegNo { get; set; }
        public string? TLTempAppNo { get; set; }
        public string? TLTempPubNo { get; set; }
        public string? TLTempRegNo { get; set; }
        public string? TLYearAppNo { get; set; }
        public string? TLYearPubNo { get; set; }
        public string? TLYearRegNo { get; set; }
        public string? TLCityAppNo { get; set; }
        public string? TLCityPubNo { get; set; }
        public string? TLCityRegNo { get; set; }
        public string? MarkAppNo { get; set; }
        public string? MarkPubNo { get; set; }
        public string? MarkRegNo { get; set; }
        public string? MarkFilDate { get; set; }
        public string? MarkPubDate { get; set; }
        public string? MarkRegDate { get; set; }
        public string? MarkAllowanceDate { get; set; }
        public DateTime? LastWebCheckStart { get; set; }
        public DateTime? LastWebCheckDate { get; set; }
        public DateTime? LastWebUpdate { get; set; }
        public DateTime? LastNumFmtDate { get; set; }
        public bool? IsBiblioMatchOK { get; set; }
        public bool? UpdateAppNo { get; set; }
        public bool? UpdatePubNo { get; set; }
        public bool? UpdateRegNo { get; set; }
        public bool? UpdateFilDate { get; set; }
        public bool? UpdatePubDate { get; set; }
        public bool? UpdateRegDate { get; set; }
        public bool? UpdateAllowanceDate { get; set; }
        public bool? UpdateGoods { get; set; }
        public bool? UpdateTrademarkName { get; set; }
        public bool? Exclude { get; set; }
        public bool? ExcludeTrademarkName { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? TMSNextRenewalDate { get; set; }
        public DateTime? TLNextRenewalDate { get; set; }
        public bool? UpdateNextRenewalDate { get; set; }
        public string? MarkNextRenewalDate { get; set; }

        [ForeignKey("TMSTmkId")]
        public TmkTrademark? Trademark { get; set; }
        public List<TLSearchAction>? TLSearchActions { get; set; }
        public List<TLSearchImage>? TLSearchImages { get; set; }
        public List<TLSearchDocument>? TLSearchDocuments { get; set; }
    }

}
