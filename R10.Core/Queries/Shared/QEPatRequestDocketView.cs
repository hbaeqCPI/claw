using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEPatRequestDocketView
    {
        public int AppId { get; set; }
        public int ReqId { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? CountryName { get; set; }
        public string? SubCase { get; set; }
        public string? CaseType { get; set; }
        public string? ApplicationStatus { get; set; }
        public DateTime? ApplicationStatusDate { get; set; }
        public string? InvTitle { get; set; }
        public string? AppTitle { get; set; }
        public string? Attorney1 { get; set; }
        public string? Attorney2 { get; set; }
        public string? Attorney3 { get; set; }
        public string? Attorney4 { get; set; }
        public string? Attorney5 { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? ClientRef { get; set; }
        public string? Owner { get; set; }
        public string? OwnerName { get; set; }
        public string? Agent { get; set; }
        public string? AgentName { get; set; }
        public string? AgentRef{ get; set; }
        public string? AppNumber { get; set; }
        public DateTime? FilDate { get; set; }
        public string? ConfirmationNumber { get; set; }
        public string? PubNumber { get; set; }
        public DateTime? PubDate { get; set; }
        public string? PatNumber { get; set; }
        public string? ParentAppNumber { get; set; }
        public DateTime? ParentFilDate { get; set; }
        public string? ParentPatNumber { get; set; }
        public DateTime? ParentIssDate { get; set; }
        public string? PCTNumber { get; set; }
        public DateTime? PCTDate { get; set; }
        public DateTime? IssDate { get; set; }
        public DateTime? ExpDate { get; set; }

        public string? RequestType { get; set; }
        public string? DueDate { get; set; }
        public string? RequestDocketRemarks { get; set; }

        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string? DateToday { get; set; }
        public DateTime? DateTimeToday { get; set; }
    }
}
