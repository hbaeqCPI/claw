using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Queries.Shared
{
    public class QETmkCostTrackingView
    {
        public int TmkId { get; set; }
        public int CostTrackId { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? CountryName { get; set; }
        public string? SubCase { get; set; }
        public string? CaseType { get; set; }
        public string? MarkType { get; set; }
        public string? TrademarkStatus { get; set; }
        public DateTime? TrademarkStatusDate { get; set; }
        public string? TrademarkName { get; set; }
        public string? Attorney1 { get; set; }
        public string? Attorney2 { get; set; }
        public string? Attorney3 { get; set; }
        public string? Attorney4 { get; set; }
        public string? Attorney5 { get; set; }
        public string? Owner { get; set; }
        public string? OwnerName { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? ClientRef { get; set; }
        public string? Agent { get; set; }
        public string? AgentName { get; set; }
        public string? AgentRef { get; set; }
        //public DateTime? FirstUseDate { get; set; }
        //public DateTime? FirstUseInCommerce { get; set; }
        public bool? IntentToUse { get; set; }
        public DateTime? AllowanceDate { get; set; }
        public string? PriNumber { get; set; }
        public DateTime? PriDate { get; set; }
        public string? AppNumber { get; set; }
        public DateTime? FilDate { get; set; }
        public string? PubNumber { get; set; }
        public DateTime? PubDate { get; set; }
        public string? RegNumber { get; set; }
        public DateTime? RegDate { get; set; }
        public DateTime? LastRenewalDate { get; set; }
        public DateTime? NextRenewalDate { get; set; }
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
        //public bool ActiveSwitch { get; set; }
        //public string? RespOffice { get; set; }
        public string? DateToday { get; set; }
        public DateTime? DateTimeToday { get; set; }

    }
}
