using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEPatCountryApplicationView
    {
        public int AppId { get; set; }
        public int InvId { get; set; }
        public string? CaseNumber { get; set; }
        public string? OldCaseNumber { get; set; }
        public string? FamilyNumber { get; set; }
        public string? Country { get; set; }
        public string? CountryName { get; set; }
        public string? SubCase { get; set; }
        public string? CountrySubCase { get; set; }
        public string? CaseType { get; set; }
        public string? CaseTypeDescription { get; set; }
        public string? ApplicationStatus { get; set; }
        public DateTime? ApplicationStatusDate { get; set; }
        public string? InvTitle { get; set; }
        public string? AppTitle { get; set; }
        public string? Attorney1 { get; set; }
        public string? AttorneyName1 { get; set; }
        public string? Attorney2 { get; set; }
        public string? AttorneyName2 { get; set; }
        public string? Attorney3 { get; set; }
        public string? AttorneyName3 { get; set; }
        public string? Attorney4 { get; set; }
        public string? AttorneyName4 { get; set; }
        public string? Attorney5 { get; set; }
        public string? AttorneyName5 { get; set; }
        public string? InvOwner { get; set; }
        public string? InvOwnerName { get; set; }
        public string? AppOwner { get; set; }
        public string? AppOwnerName { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? InvClientRef { get; set; }
        public string? AppClientRef { get; set; }
        public string? Agent { get; set; }
        public string? AgentName { get; set; }
        public string? AgentRef{ get; set; }
        public string? DisclosureStatus { get; set; }
        public DateTime? DisclosureDate { get; set; }
        public string? ParentAppNumber { get; set; }
        public DateTime? ParentFilDate { get; set; }
        public string? ParentPatNumber { get; set; }
        public DateTime? ParentIssDate { get; set; }
        public string? PCTNumber { get; set; }
        public DateTime? PCTDate { get; set; }
        public string? AppNumber { get; set; }
        public DateTime? FilDate { get; set; }
        public string? ConfirmationNumber { get; set; }
        public string? PubNumber { get; set; }
        public DateTime? PubDate { get; set; }
        public string? PatNumber { get; set; }
        public DateTime? IssDate { get; set; }
        public DateTime? ExpDate { get; set; }
        public string? GroupArtUnit { get; set; }
        public string? Examiner { get; set; }
        public string? AttorneyDocketNo { get; set; }
        public string? CustomerNo { get; set; }
        public short PatentTermAdj { get; set; }
        public string? TaxSchedule { get; set; }
        public DateTime? TaxStartDate { get; set; }
        public string? BillingNumber { get; set; }
        public string? Storage { get; set; }
        public bool? TerminalDisclaimer { get; set; }
        public string? InvMatterNumber { get; set; }
        public string? AppMatterNumber { get; set; }
        public string? InvAbstract { get; set; }
        public string? InvRemarks { get; set; }
        public string? AppRemarks { get; set; }
        public string? InvInventors { get; set; }
        public string? AppInventors { get; set; }
        public string? AppOwners { get; set; }
        public string? InvOwners { get; set; }
        public string? DesignatedStates { get; set; }
        public string? Products { get; set; }
        public string? PriorityInfo { get; set; }
        public string? Assignments { get; set; }
        public string? Actions { get; set; }
        public string? Images { get; set; }
        public string? CostInfo { get; set; }
        public int? ParentAppId { get; set; }
        public string? InvCreatedBy { get; set; }
        public string? InvUpdatedBy { get; set; }
        public DateTime? InvDateCreated { get; set; }
        public DateTime InvLastUpdate { get; set; }
        public string? InvRespOffice { get; set; }
        public string? AppCreatedBy { get; set; }
        public string? AppUpdatedBy { get; set; }
        public DateTime? AppDateCreated { get; set; }
        public DateTime AppLastUpdate { get; set; }
        public string? AppRespOffice { get; set; }
        public string? DateToday { get; set; }
        public DateTime? DateTimeToday { get; set; }

        public string? OutstandingActions { get; set; }
        public string? NextActions { get; set; }
        public string? OldApplicationStatus { get; set; }
    }
}
