using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class RTSPFSCountryAppUpdHistoryDTO
    {
        public int AppId { get; set; }

        public string? BatchId { get; set; }
        public string? UserId { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string? PMSApplNumber { get; set; }
        public string? ApplNumber { get; set; }
        public bool UndoApplNumber { get; set; }
        public string? PMSPubNumber { get; set; }
        public string? PubNumber { get; set; }
        public bool UndoPubNumber { get; set; }
        public string? PMSPatNumber { get; set; }
        public string? PatNumber { get; set; }
        public bool UndoPatNumber { get; set; }
        public string? PMSParentPCTNumber { get; set; }
        public string? PriorityNumber { get; set; }
        public bool UndoPriorityNumber { get; set; }
        public DateTime? PMSFilDate { get; set; }
        public DateTime? FilDate { get; set; }
        public bool UndoFilDate { get; set; }
        public DateTime? PMSPubDate { get; set; }
        public DateTime? PubDate { get; set; }
        public bool UndoPubDate { get; set; }
        public DateTime? PMSIssDate { get; set; }
        public DateTime? IssDate { get; set; }
        public bool UndoIssDate { get; set; }
        public DateTime? PMSParentPCTDate { get; set; }
        public DateTime? PriorityDate { get; set; }
        public bool UndoPriorityDate { get; set; }
        public bool UndoFlag { get; set; }
    }
}
