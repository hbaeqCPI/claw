using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEPatActionDueDateDelegationView
    {
        public int AppId { get; set; }
        public int ActId { get; set; }
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
        public string? GroupArtUnit { get; set; }
        public string? Examiner { get; set; }
        public string? AttorneyDocketNo { get; set; }
        public string? CustomerNo { get; set; }
        public string? ActionType { get; set; }
        public DateTime BaseDate { get; set; }
        public DateTime? ResponseDate { get; set; }
        public DateTime? VerifyDate { get; set; }
        public bool ComputerGenerated { get; set; }
        public bool? IsElectronic { get; set; }
        public string? Responsible { get; set; }
        public string? ActionRemarks { get; set; }
        public DateTime? FinalDate { get; set; }

        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime LastUpdate { get; set; }
        public string? DateToday { get; set; }
        public DateTime? DateTimeToday { get; set; }

        public int DDId { get; set; }
        public string? ActionDue { get; set; }
        public DateTime DueDate { get; set; }
        public string? Indicator { get; set; }
        public DateTime? DateTaken { get; set; }
        public string? DueDateRemarks { get; set; }

        public int DelegationId { get; set; }
        public int? GroupID { get; set; }
        public string? UserId { get; set; }
        public string? AssignedBy { get; set; }
        public string? AssignedTo { get; set; }

    }
}
