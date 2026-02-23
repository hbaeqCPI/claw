using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEGmCostTrackingView
    {
        public int MatId { get; set; }
        public int CostTrackID { get; set; }
        public string? CaseNumber { get; set; }
        public string? CountryCodes { get; set; }
        public string? CountryNames { get; set; }
        public string? SubCase { get; set; }
        public string? MatterType { get; set; }
        public string? MatterTitle { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? MatterStatus { get; set; }
        public DateTime? MatterStatusDate { get; set; }
        public DateTime? EffectiveOpenDate { get; set; }
        //public DateTime? TerminationOpenDate { get; set; }
        public DateTime? TerminationEndDate { get; set; }
        public string? Attorneys { get; set; }
        public string? AttorneyNames { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? ClientRef { get; set; }
        public string? Agent { get; set; }
        public string? AgentName { get; set; }
        public string? CostType { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? PayDate { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string? CurrencyType { get; set; }
        public double? ExchangeRate { get; set; }
        public double NetCost { get; set; }
        public string? Remarks { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime LastUpdate { get; set; }
        public string? DateToday { get; set; }
        public DateTime? DateTimeToday { get; set; }
    }
}
