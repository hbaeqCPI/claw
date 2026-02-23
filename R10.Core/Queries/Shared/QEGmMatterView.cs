using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEGmMatterView
    {
        public int MatId { get; set; }
        public string? CaseNumber { get; set; }
        public string? CountryCodes { get; set; }
        public string? CountryNames { get; set; }
        public string? SubCase { get; set; }
        public string? OldCaseNumber { get; set; }
        public string? MatterType { get; set; }
        public string? MatterTitle { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Attorneys { get; set; }
        public string? AttorneyNames { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? ClientRef { get; set; }
        public string? Agent { get; set; }
        public string? AgentName { get; set; }
        public string? MatterStatus { get; set; }
        public DateTime? MatterStatusDate { get; set; }
        public bool ActiveSwitch { get; set; }
        public DateTime? EffectiveOpenDate { get; set; }
        public DateTime? TerminationEndDate { get; set; }
        public string? ResultRoyalty { get; set; }
        public string? AgreementType { get; set; }
        public string? Extent { get; set; }
        public string? Court { get; set; }
        public string? CourtDocketNumber { get; set; }
        public string? CourtJudgeMagistrate { get; set; }
        public string? MatterNumber { get; set; }
        public string? Remarks { get; set; }
        public string? OtherParties { get; set; }
        public string? OurPatents { get; set; }
        public string? OtherPartyPatents { get; set; }
        public string? OurTrademarks { get; set; }
        public string? OtherPartyTrademarks { get; set; }
        public string? Actions { get; set; }
        public string? Images { get; set; }
        public string? CostInfo { get; set; }
        public string? Keywords { get; set; }
        public string? RespOffice { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime LastUpdate { get; set; }
        public string? DateToday { get; set; }
        public DateTime? DateTimeToday { get; set; }
        public string? OutstandingActions { get; set; }
        public string? NextActions { get; set; }
        public string? OldMatterStatus { get; set; }
    }
}
