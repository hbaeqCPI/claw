using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QETmkTrademarkDocRespDocketingView
    {
        public int TmkId { get; set; }
        public int DocId { get; set; }
        public string? DriveItemId { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? CountryName { get; set; }
        public string? SubCase { get; set; }
        public string? CaseType { get; set; }
        public string? TrademarkStatus { get; set; }
        public DateTime? TrademarkStatusDate { get; set; }
        public string? TrademarkName { get; set; }
        public string? Attorney1 { get; set; }
        public string? Attorney2 { get; set; }
        public string? Attorney3 { get; set; }
        public string? Attorney4 { get; set; }
        public string? Attorney5 { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? Owner { get; set; }
        public string? OwnerName { get; set; }
        public string? Agent { get; set; }
        public string? AgentName { get; set; }
        public string? AgentRef { get; set; }
        public string? AppNumber { get; set; }
        public DateTime? FilDate { get; set; }

        public string? PubNumber { get; set; }
        public DateTime? PubDate { get; set; }
        public string? RegNumber { get; set; }
        public DateTime? RegDate { get; set; }

        public string? DocName { get; set; }
        public DateTime? ImageDate { get; set; }
        public string? DateToday { get; set; }
        public DateTime? DateTimeToday { get; set; }


        public string? AssignedBy { get; set; }
        public DateTime? AssignedOn { get; set; }
        public string? AssignedTo { get; set; }

    }
}
