using System.ComponentModel.DataAnnotations;

namespace LawPortal.Core.DTOs
{
    public class DocInfoDTO
    {
        [Key]
        public string? DriveItemId { get; set; }
        public bool IsPrivate { get; set; }
        public string? Remarks { get; set; }
        public bool IsDefault { get; set; }
        public bool IsPrintOnReport { get; set; }
        public string? Tags { get; set; }
        public bool IsVerified { get; set; }
        public bool IncludeInWorkflow { get; set; }
        public bool IsActRequired { get; set; }
        public string? Source { get; set; }
        public bool CheckAct { get; set; }
        public bool SendToClient { get; set; }
    }
}
