using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.DTOs
{
    public class TmkGlobalUpdateCriteriaDTO
    {
        public string? UpdateField { get; set; }
        public int? DataFrom { get; set; }
        public int? DataTo { get; set; }
        public DateTime? DateDataFrom { get; set; }
        public DateTime? DateDataTo { get; set; }

        public string? CaseNumber { get; set; }
        public string? TrademarkName { get; set; }
        public string? Attorney1 { get; set; }
        public string? Attorney1Name { get; set; }
        public string? Attorney2 { get; set; }
        public string? Attorney2Name { get; set; }
        public string? Attorney3 { get; set; }
        public string? Attorney3Name { get; set; }
        public string? Attorney4 { get; set; }
        public string? Attorney4Name { get; set; }
        public string? Attorney5 { get; set; }
        public string? Attorney5Name { get; set; }
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? Agent { get; set; }
        public string? AgentName { get; set; }        
        public string? Owner { get; set; }
        public string? OwnerName { get; set; }

        public string? Country { get; set; }
        public string? CountryName { get; set; }
        public DateTime? FilDateFrom { get; set; }
        public DateTime? FilDateTo { get; set; }
        public DateTime? PubDateFrom { get; set; }
        public DateTime? PubDateTo { get; set; }
        public DateTime? RegDateFrom { get; set; }
        public DateTime? RegDateTo { get; set; }
        public DateTime? NextRenewalDateFrom { get; set; }
        public DateTime? NextRenewalDateTo { get; set; }
        public string? TrademarkStatuses { get; set; }
        public int? ActiveSwitch { get; set; }
        public string? UserName { get; set; }
        public string? Remarks { get; set; }
        public string? RespOffice { get; set; }
        //public int[]? KeyIds { get; set; }
        public List<TmkGlobalUpdateCriteriaKeyId>? KeyIds { get; set; }
        public string[]? DataKeyIds { get; set; }

        public bool? DeDocketActions { get; set; }
        public int? DeDocketTakenDateFrom { get; set; }
        public DateTime? DeDocketTakenDate { get; set; }
        public bool? UpdateStatusDate { get; set; }
        public DateTime? StatusDate { get; set; }

        public string? Keywords { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime? InvoiceDateFrom { get; set; }
        public DateTime? InvoiceDateTo { get; set; }

        public int AttorneyPosition { get; set; }
        public int AttorneyFilter1 { get; set; }
        public int AttorneyFilter2 { get; set; }
        public int AttorneyFilter3 { get; set; }
        public int AttorneyFilter4 { get; set; }
        public int AttorneyFilter5 { get; set; }
        public int AttorneyFilterR { get; set; }
        public int AttorneyFilterD { get; set; }

        public string? CaseTypes { get; set; }
        public int IncludeAttyInClient { get; set; }
    }

    public class TmkGlobalUpdateCriteriaKeyId
    {
        public int Id { get; set; }
        public string? DataKey { get; set; }
    }
}
