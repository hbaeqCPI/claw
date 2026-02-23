using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class RTSBiblioUpdate: RTSEntityFilter
    {
        public string? YourCaseType { get; set; }
        public string? PubCaseType { get; set; }
        public string? YourAppNo { get; set; }
        public string? PubAppNo { get; set; }

        public DateTime? YourFilDate { get; set; }
        public DateTime? PubFilDate { get; set; }
        public string? YourPubNo { get; set; }
        public string? PubPubNo { get; set; }
        public DateTime? YourPubDate { get; set; }
        public DateTime? PubPubDate { get; set; }

        public string? YourPatNo { get; set; }
        public string? PubPatNo { get; set; }

        public DateTime? YourIssDate { get; set; }
        public DateTime? PubIssDate { get; set; }

        public DateTime? YourParentPCTDate { get; set; }
        public DateTime? PubParentPCTDate { get; set; }

        public string? MarkCaseType { get; set; }
        public string? MarkAppNo { get; set; }
        public string? MarkPubNo { get; set; }
        public string? MarkPatNo { get; set; }
        
        public string? MarkFilDate { get; set; }
        public string? MarkPubDate { get; set; }
        public string? MarkIssDate { get; set; }
        public string? MarkParentPCTDate { get; set; }

        public bool UpdateCaseType { get; set; }
        public bool UpdateAppNo { get; set; }
        public bool UpdatePubNo { get; set; }
        public bool UpdatePatNo { get; set; }
        public bool UpdateFilDate { get; set; }
        public bool UpdatePubDate { get; set; }
        public bool UpdateIssDate { get; set; }
        public bool UpdateParentPCTDate { get; set; }
        public bool Exclude { get; set; }
        public bool ActiveSwitch { get; set; }
        public DateTime? SentDate { get; set; }

        public byte[]? tStamp { get; set; }
    }

    public class RTSEntityFilter
    {
        public int AppId { get; set; }
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        public string? RespOffice { get; set; }
        public int? ClientId { get; set; }
        public int? AgentId { get; set; }
        public int? Attorney1Id { get; set; }
        public int? Attorney2Id { get; set; }
        public int? Attorney3Id { get; set; }
        public int? Attorney4Id { get; set; }
        public int? Attorney5Id { get; set; }
    }
}
