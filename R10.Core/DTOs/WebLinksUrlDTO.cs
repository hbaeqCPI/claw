using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class WebLinksUrlDTO
    {
        public int UrlId {get; set; }
        public string? UrlExpr {get; set; }
        [NotMapped]
        public string? ActiveUrl {get; set; }
        public DateTime? EffFromDate {get; set; }
        public DateTime? EffToDate {get; set; }
        public string? EffBasedOn {get; set; }
        public bool? NeedsAppNo {get; set; }
        public bool? NeedsPubNo {get; set; }
        public bool? NeedsPatRegNo {get; set; }
        public bool? NeedsFilDate {get; set; }
        public bool? NeedsPubDate {get; set; }
        public bool? NeedsIssRegDate {get; set; }
        public bool? StopWhenFail {get; set; }
        public string? StopErrorCode {get; set; }
        public string? Country {get; set; }
        public string? CaseType {get; set; }
        public string? AppNumber {get; set; }
        public string? PubNumber {get; set; }
        public string? PatRegNumber {get; set; }
        public DateTime? FilDate {get; set; }
        public DateTime? PubDate {get; set; }
        public DateTime? IssRegDate {get; set; }

    }
}


