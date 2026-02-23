using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.DTOs
{
    public class GMGlobalUpdateCriteriaDTO
    {
        public string? UpdateField { get; set; }
        public int? DataFrom { get; set; }
        public int? DataTo { get; set; }
        public DateTime? DateDataFrom { get; set; }
        public DateTime? DateDataTo { get; set; }

        public string? CaseNumber { get; set; }
        //public string? Attorney { get; set; }
        //public string? AttorneyName { get; set; }       
        public string? Client { get; set; }
        public string? ClientName { get; set; }
        public string? Agent { get; set; }
        public string? AgentName { get; set; }  
       
        public DateTime? EffectiveOpenDateFrom { get; set; }
        public DateTime? EffectiveOpenDateTo { get; set; }
        public DateTime? TerminationEndDateFrom { get; set; }
        public DateTime? TerminationEndDateTo { get; set; }
        
        public string? MatterStatuses { get; set; }
        public int? ActiveSwitch { get; set; }
        public string? UserName { get; set; }
        public string? Remarks { get; set; }
        public string? RespOffice { get; set; }

        public bool? DeDocketActions { get; set; }
        public int? DeDocketTakenDateFrom { get; set; }
        public DateTime? DeDocketTakenDate { get; set; }
        public bool? UpdateStatusDate { get; set; }
        public DateTime? StatusDate { get; set; }

        public string? Keywords { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime? InvoiceDateFrom { get; set; }
        public DateTime? InvoiceDateTo { get; set; }

        public int[]? KeyIds { get; set; }        
    }
}
