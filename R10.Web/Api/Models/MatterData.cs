using System;

namespace R10.Web.Api.Models
{
    public class MatterData 
    {        
        public string CaseNumber { get; set; }
        public string SubCase { get; set; }
        public string OldCaseNumber { get; set; }        
        public string MatterType { get; set; }
        public string MatterTitle { get; set; }
        public string ReferenceNumber { get; set; }
        public string MatterStatus { get; set; }
        public DateTime? MatterStatusDate { get; set; }
        

        //ENTITIES
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ClientRef { get; set; }

        public string AgentCode { get; set; }
        public string AgentName { get; set; }

        
        public DateTime? EffectiveOpenDate { get; set; }
        public DateTime? TerminationEndDate { get; set; }
        public string ResultRoyalty { get; set; }  
        public string AgreementType { get; set; }                
        public string Extent { get; set; }
        public string Court { get; set; }
        public string CourtDocketNumber { get; set; }
        public string CourtJudgeMagistrate { get; set; }
        public string MatterNumber { get; set; }
       
        public string Remarks { get; set; }

        public string RespOffice { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}
