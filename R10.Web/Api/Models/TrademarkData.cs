using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Api.Models
{
    public class TrademarkData 
    {
        public int TmkId { get; set; }

        public string CaseNumber { get; set; }
        public string OldCaseNumber { get; set; }
        public string Country { get; set; }
        public string CountryName { get; set; }
        public string SubCase { get; set; }
        public string CaseType { get; set; }
        public string MarkType { get; set; }
        public string TrademarkStatus { get; set; }
        public DateTime? TrademarkStatusDate { get; set; }
        public string TrademarkName { get; set; }

        //ENTITIES
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ClientRef { get; set; }

        public string? Attorney1Code { get; set; }
        public string? Attorney1Name { get; set; }
        public string? Attorney1Email { get; set; }
        public string? Attorney2Code { get; set; }
        public string? Attorney2Name { get; set; }
        public string? Attorney2Email { get; set; }
        public string? Attorney3Code { get; set; }
        public string? Attorney3Name { get; set; }
        public string? Attorney3Email { get; set; }
        public string? Attorney4Code { get; set; }
        public string? Attorney4Name { get; set; }
        public string? Attorney4Email { get; set; }
        public string? Attorney5Code { get; set; }
        public string? Attorney5Name { get; set; }
        public string? Attorney5Email { get; set; }

        public string AgentCode { get; set; }
        public string AgentName { get; set; }
        public string AgentRef { get; set; }

        public List<OwnerData> Owners { get; set; }

        public bool IntentToUse { get; set; }

        
        public DateTime? AllowanceDate { get; set; }       
        public string PriCountry { get; set; }        
        public string PriNumber { get; set; }        
        public DateTime? PriDate { get; set; }
        public string AppNumber { get; set; }
        public DateTime? FilDate { get; set; }
        public string PubNumber { get; set; }
        public DateTime? PubDate { get; set; }
        public string RegNumber { get; set; }
        public DateTime? RegDate { get; set; }
        public DateTime? LastRenewalDate { get; set; }
        public DateTime? NextRenewalDate { get; set; }
        public string LastRenewalNumber { get; set; }
        public string ParentAppNumber { get; set; }
        public DateTime? ParentFilDate { get; set; }
        public string MatterNumber { get; set; }       
        public string Storage { get; set; }
        
        public string Remarks { get; set; }

        public List<TmkClassData>? ClassGoods { get; set; }

        public string RespOffice { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdate { get; set; }
    }

    public class TmkClassData
    {
        public string? Class { get; set; }
        public string? ClassType { get; set; }
        public string? Goods { get; set; }
    }
}
