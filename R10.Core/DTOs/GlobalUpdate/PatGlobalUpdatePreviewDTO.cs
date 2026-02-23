using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.DTOs
{
    [Keyless]
    public class PatGlobalUpdatePreviewDTO
    {
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }

        public string? FamilyNumber { get; set; }
        public string? Title { get; set; }
        public string? ApplicationStatus { get; set; }
        public bool ActiveSwitch { get; set; }
        public string? Client { get; set; }
        public string? Owner { get; set; }
        public string? AppOwner { get; set; }
        public string? Agent { get; set; }
        public string? Attorney1 { get; set; }
        public string? Attorney2 { get; set; }
        public string? Attorney3 { get; set; }
        public string? Attorney4 { get; set; }
        public string? Attorney5 { get; set; }

        public DateTime? FilDate { get; set; }
        public string? AppNumber { get; set; }
        public DateTime? PubDate { get; set; }
        public string? PubNumber { get; set; }
        public DateTime? IssDate { get; set; }
        public string? PatNumber { get; set; }
        public DateTime? ExpDate { get; set; }
        public string? RespOffice { get; set; }

        public string? DataKey { get; set; }
        public int? KeyId { get; set; }
        public string? ActionType { get; set; }
        public string? Responsible { get; set; }
        public string? DueDateAttorney { get; set; }
        public string? ActionDue { get; set; }
        public DateTime? BaseDate { get; set; }
        public string? Inventor { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? PayDate { get; set; }

        public string? ErrorConflict { get; set; }
        [NotMapped]
        public bool Selected { get; set; } = true;

    }
}
