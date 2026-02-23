using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.AMS
{
    public class AMSMain : AMSMainDetail
    {
        public List<AMSDue> AMSDue { get; set; }

        public List<AMSProjection> AMSProjection { get; set; }

        public AMSAbstract AMSAbstract { get; set; }

        public PatApplicationStatus PatApplicationStatus { get; set; }

        public CountryApplication CountryApplication { get; set; }

        public Client Client { get; set; }

        public Agent Agent { get; set; }

        public Attorney Attorney { get; set; }

        public PatCountry PatCountry { get; set; }

        public List<AMSStatusChangeLog> AMSStatusChangeLog { get; set; }
        public List<AMSProduct> AMSProducts { get; set; }
        public List<AMSLicensee> AMSLicensees { get; set; }
    }

    public class AMSMainDetail : BaseEntity
    {
        [Key]
        public int AnnID { get; set; }

        [Required]
        public int AS4AppID { get; set; }

        [StringLength(20)]
        public string? PMSID { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name = "CPI Code")]
        public string CPIClientCode { get; set; }

        [Required]
        [StringLength(25)]
        [Display(Name = "Case Number")]
        public string CaseNumber { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [StringLength(8)]
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [StringLength(45)]
        public string? PrevCaseNumber { get; set; }

        [StringLength(3)]
        [Display(Name = "CPI Case Type")]
        public string? CPICaseType { get; set; }

        [StringLength(5)]
        [Display(Name = "CPI Tax Schedule")]
        public string? CPITaxSchedule { get; set; }

        [Display(Name = "Tax Start Date")]
        public DateTime? CPITaxStartDate { get; set; }

        [StringLength(255)]
        [Display(Name = "Title")]
        public string? CPITitle { get; set; }

        [StringLength(11)]
        [Display(Name = "CPI Status")]
        public string? CPIStatus { get; set; }

        [StringLength(10)]
        public string? CPIClient { get; set; }

        [StringLength(60)]
        public string? CPIOwner { get; set; }

        [StringLength(10)]
        public string? CPIAgent { get; set; }

        [StringLength(20)]
        public string? CPIAgentRef { get; set; }

        [StringLength(10)]
        [Display(Name = "Attorney")]
        public string? CPIAttorney { get; set; }

        [StringLength(20)]
        [Display(Name = "Application Number")]
        public string? CPIAppNo { get; set; }

        [StringLength(20)]
        [Display(Name = "Publication Number")]
        public string? CPIPubNo { get; set; }

        [StringLength(20)]
        [Display(Name = "Patent Number")]
        public string? CPIPatNo { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? CPIFilDate { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? CPIPubDate { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? CPIIssDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Priority Number")]
        public string? CPIPrioNo { get; set; }

        [Display(Name = "Priority Date")]
        public DateTime? CPIPrioDate { get; set; }

        [StringLength(5)]
        [Display(Name = "Priority Country")]
        public string? CPIPrioCountry { get; set; }

        [StringLength(20)]
        [Display(Name = "PCT Number")]
        public string? CPIPCTNo { get; set; }

        [Display(Name = "PCT Date")]
        public DateTime? CPIPCTDate { get; set; }

        [StringLength(1000)]
        [Display(Name = "Inventors")]
        public string? CPIInventors { get; set; }

        [StringLength(20)]
        public string? CPIClientRef1 { get; set; }

        [StringLength(20)]
        public string? CPIClientRef2 { get; set; }

        [Display(Name = "Expiration Date")]
        public DateTime? CPIExpireDate { get; set; }

        [StringLength(60)]
        public string? CPIInvAssignee { get; set; }

        [StringLength(20)]
        public string? CPIInvClientRef { get; set; }

        [StringLength(120)]
        [Display(Name = "Designated States")]
        public string? CPILocTitle { get; set; }

        [StringLength(10)]
        public string? CPIPatAttorney { get; set; }

        [StringLength(30)]
        public string? CPIRemarks { get; set; }

        public bool? EndClientHandles { get; set; }

        public bool? SendReminder { get; set; }

        public bool? ProcessFlag { get; set; }

        public string? Remarks { get; set; }

        public DateTime? ImportDate { get; set; }

        public int CPIChangeDate { get; set; }

        [StringLength(5)]
        public string? LastInstructionType { get; set; }

        public DateTime? LastInstructionDate { get; set; }

        public DateTime? LastInstructedDueDate { get; set; }

        [StringLength(1)]
        public string? LastAgentLetterType { get; set; }

        public DateTime? LastAgentLetterDate { get; set; }

        public DateTime? LastStatusUpdate { get; set; }

        public DateTime? CPIStopDate { get; set; }

        public bool? CPIDeleteFlag { get; set; }

        [StringLength(1)]
        public string? CPISrchAppCh { get; set; }

        [StringLength(20)]
        public string? CPISrchAppNo { get; set; }

        [StringLength(4)]
        public string? CPISrchAppYear { get; set; }

        [StringLength(1)]
        public string? CPISrchPatCh { get; set; }

        [StringLength(20)]
        public string? CPISrchPatNo { get; set; }

        [StringLength(4)]
        public string? CPISrchPatYear { get; set; }

        [StringLength(1)]
        public string? CPISrchPubCh { get; set; }

        [StringLength(20)]
        public string? CPISrchPubNo { get; set; }

        [StringLength(4)]
        public string? CPISrchPubYear { get; set; }

        [StringLength(2)]
        public string? CPISrchWIPO { get; set; }

        [StringLength(25)]
        public string? CPIOldCaseNumber { get; set; }

        [StringLength(8)]
        public string? CPIOldSubCase { get; set; }

        public DateTime? CPISentDate { get; set; }

        [StringLength(20)]
        public string? CPIEurNatNo { get; set; }

        [StringLength(20)]
        public string? CPIREIAppNo { get; set; }

        public DateTime? CPIREIFilDate { get; set; }

        [StringLength(20)]
        public string? CPIREIIssNo { get; set; }

        public DateTime? CPIREIIssDate { get; set; }

        [StringLength(20)]
        public string? CPISPCAppNo { get; set; }

        public DateTime? CPISPCFilDate { get; set; }

        [StringLength(20)]
        public string? CPISPCIssNo { get; set; }

        public DateTime? CPISPCIssDate { get; set; }

        public bool? IsAgentComm { get; set; }

        public bool? IsAgentCommSent { get; set; }

        public bool? AutoPay { get; set; }

        public DateTime? SQL_LastUpdate { get; set; }

        public DateTime? SQL_DateCreated { get; set; }

        [StringLength(20)]
        [Display(Name = "Certificate Number")]
        public string? CPIEPOCerNo { get; set; }

        [StringLength(20)]
        [Display(Name = "National Number")]
        public string? CPIEPONatNo { get; set; }

        public string? CPIAppNoSearch { get; set; }
        public string? CPIPubNoSearch { get; set; }
        public string? CPIPatNoSearch { get; set; }

        //public int? ClientID { get; set; }

        //[Display(Name = "Attorney")]
        //public int? AttorneyID { get; set; }

        //public int? AgentID { get; set; }
    }
}
