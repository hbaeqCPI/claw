using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QEPatCostTrackingInvView
    {
        public int InvId { get; set; }
        public int CostTrackInvId { get; set; }
        public string? CaseNumber { get; set; }
        public string? DisclosureStatus { get; set; }
        public DateTime? DisclosureDate { get; set; }
        public string? InvTitle { get; set; }
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
        public DateTime? PayDate { get; set; }
        public string? CostType { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
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
