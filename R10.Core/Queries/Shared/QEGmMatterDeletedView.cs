using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEGmMatterDeletedView
    {
        public int MatId { get; set; }
        public string? CaseNumber { get; set; }
        public string? CountryCodes { get; set; }
        public string? CountryNames { get; set; }
        public string? SubCase { get; set; }
        public string? MatterType { get; set; }
        public string? MatterTitle { get; set; }
        public string? MatterStatus { get; set; }
        public DateTime? MatterStatusDate { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? ClientRef { get; set; }
        public string? Agent { get; set; }
        public string? AgentName { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? AgreementType { get; set; }
        public DateTime? EffectiveOpenDate { get; set; }
        public DateTime? TerminationEndDate { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime DateDeleted { get; set; }
        public string? DateToday { get; set; }
        public DateTime? DateTimeToday { get; set; }
    }
}
