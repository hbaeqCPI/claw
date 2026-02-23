using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Api.Models
{
    public class CountryApplicationData //: CountryApplicationDetail
    {
        public int AppId { get; set; }
        //public int InvId { get; set; }
        public string CaseNumber { get; set; }
        public string OldCaseNumber { get; set; }
        public string Country { get; set; }
        public string CountryName { get; set; }
        public string SubCase { get; set; }
        public string CaseType { get; set; }
        public string ApplicationStatus { get; set; }
        public DateTime? ApplicationStatusDate { get; set; }
        public string AppMatterNumber { get; set; }
        public string AppTitle { get; set; }

        //ENTITIES
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public string AppClientRef { get; set; }
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
        //public int? AgentID { get; set; }
        public string AgentCode { get; set; }
        public string AgentName { get; set; }
        public string AgentRef { get; set; }
        public List<InventorData> Inventors { get; set; }
        public List<OwnerData> Owners { get; set; }

        public string AppNumber { get; set; }
        public DateTime? FilDate { get; set; }
        public string PubNumber { get; set; }
        public DateTime? PubDate { get; set; }
        public string PatNumber { get; set; }
        public DateTime? IssDate { get; set; }
        public string ConfirmationNumber { get; set; }
        public DateTime? ExpDate { get; set; }
        public string TaxSchedule { get; set; }
        public short PatentTermAdj { get; set; }
        public string ParentFilCountry { get; set; }
        public string ParentAppNumber { get; set; }
        public DateTime? ParentFilDate { get; set; }
        public string ParentPatNumber { get; set; }
        public DateTime? ParentIssDate { get; set; }
        public string PCTNumber { get; set; }
        public DateTime? PCTDate { get; set; }

        public int Claim { get; set; }
        public DateTime? TaxStartDate { get; set; }
        //public int? OwnerID { get; set; }
        //public int? ParentAppId { get; set; }
        public string ParentCase { get; set; }
        //public int? TerminalDisclaimerAppId { get; set; }
        public string BillingNumber { get; set; }
        public string Storage { get; set; }
        public bool? TerminalDisclaimer { get; set; }
        public string PatNationalNumber { get; set; }
        public string PatCertificateNumber { get; set; }
        //public string PMSID { get; set; }

        public string Remarks { get; set; }

        public string RespOffice { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdate { get; set; }
        //public byte[] tStamp { get; set; }
    }
}
