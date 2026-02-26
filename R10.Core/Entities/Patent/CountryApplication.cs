using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using R10.Core.DTOs;
// using R10.Core.Entities.AMS; // Removed during deep clean
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Core.Helpers;

namespace R10.Core.Entities.Patent
{
    public class CountryApplication: CountryApplicationDetail
    {
        public Invention? Invention { get; set; }
        public PatCountry? PatCountry { get; set; }
        public PatCaseType? PatCaseType { get; set; }
        public PatCountryLaw? PatCountryLaw { get; set; }
        public PatApplicationStatus? PatApplicationStatus { get; set; }
        public Agent? Agent { get; set; }
        //public Owner Owner { get; set; }
        public PatIDSRelatedCasesInfo?  IDSRelatedCasesInfo { get; set; }
        public List<PatAssignmentHistory>? AssignmentsHistory { get; set; }
        public List<PatInventorApp>? Inventors { get; set; }
        public List<PatOwnerApp>? Owners { get; set; }
        public List<PatLicensee>? Licensees { get; set; }
        public List<PatActionDue>? ActionDues { get; set; }
        public List<PatCostTrack>? CostTrackings { get; set; }
        public List<PatDesignatedCountry>? DesignatedCountries { get; set; }
        public List<PatIDSRelatedCase>? IDSRelatedCases { get; set; }
        public List<PatIDSRelatedCaseDTO>? IDSRelatedCasesDTO { get; set; }
        public List<PatIDSNonPatLiterature>? NonPatLiteratures { get; set; }
        public List<PatRelatedCase>? RelatedCases { get; set; }
        public List<PatRelatedCaseDTO>? RelatedCasesDTO { get; set; }
        //public List<PatImageApp>? Images { get; set; }
        public List<PatTerminalDisclaimer>? PatTerminalDisclaimers { get; set; }
        public List<PatTerminalDisclaimer>? PatChildTerminalDisclaimers { get; set; }
        public List<PatInventorAppAward>? Awards { get; set; }
        public List<PatProduct>? Products { get; set; }
        public List<PatScore>? PatScores { get; set; }
        public CountryApplication? ParentCase { get; set; }
        public List<CountryApplication>? ChildCases { get; set; }
        //public CountryApplication? RelatedTerminalDisclaimer { get; set; }
        //public List<CountryApplication>? ChildTerminalDisclaimers { get; set; }
//         public AMSMain? AMSMain { get; set; } // Removed during deep clean
//         public RTSSearch? RTSSearch { get; set; } // Removed during deep clean
        public PatAverageScoreDTO? PatentScore { get; set; }
        public List<PatRelatedTrademark>? RelatedTrademarks { get; set; }
//         public List<GMMatterPatent>? GMMatterPatents { get; set; } // Removed during deep clean

        public List<PatCostEstimator>? CostEstimators { get; set; }
        public List<TimeTracker>? TimeTrackers { get; set; }
        public List<PatEGrantDownloaded>? EGrantDownloadeds { get; set; }
        public List<PatTerminalDisclaimerChecked>? TerminalDisclaimerCheckeds { get; set; }

        public Agent? TaxAgent { get; set; }
        public Agent? LegalRepresentative { get; set; }

        public List<PatAppImage>? Images { get; set; }
        public PatAppImageDefault? ImageDefault { get; set; }
        public List<PatDocketRequest>? PatDocketRequests { get; set; }
        
    }

    public class CountryApplicationDetail : BaseEntityWithRespOffice
    {
        [Key]
        public int AppId { get; set; }

        public int InvId { get; set; }

        [Required]
        [StringLength(25)]
        public string CaseNumber { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name ="Country")]
        public string Country { get; set; }

        [StringLength(8)]
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Required]
        [StringLength(3)]
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        public int? AgentID { get; set; }

        [StringLength(20)]
        [Display(Name = "Agent Reference")]
        public string? AgentRef { get; set; }

        [Required]
        [StringLength(15)]
        [Display(Name = "Status")]
        public string? ApplicationStatus { get; set; } = "Unfiled"; //Default, but is updated during save based on entered date.

        [Display(Name = "Status Date")]
        public DateTime? ApplicationStatusDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Parent Application No.")]
        public string? ParentAppNumber { get; set; }

        [Display(Name = "Parent Filing Date")]
        public DateTime? ParentFilDate { get; set; }

        [Display(Name = "Parent Filing Country")]
        public string? ParentFilCountry { get; set; }

        [StringLength(20)]
        [Display(Name = "Parent Patent No.")]
        public string? ParentPatNumber { get; set; }

        [Display(Name = "Parent Issue Date")]
        public DateTime? ParentIssDate { get; set; }

        [StringLength(20)]
        [Display(Name = "PCT No.")]
        public string? PCTNumber { get; set; }

        [Display(Name = "PCT Date")]
        public DateTime? PCTDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [StringLength(6)]
        [Display(Name = "Confirmation No.")]
        public string? ConfirmationNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Publication No.")]
        public string? PubNumber { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Patent No.")]
        public string? PatNumber { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

        [Display(Name = "Expiration Date")]
        public DateTime? ExpDate { get; set; }

        [Display(Name = "Patent Term Adj. (days)")]
        [Range(0, short.MaxValue, ErrorMessage = "Patent Term Adj. is not valid")]
        public short PatentTermAdj { get; set; }

        [StringLength(3)]
        public string? TaxSchedule { get; set; }

        [Display(Name = "Claims")]
        [Range(0, 999, ErrorMessage = "Claims must be between 0 and 999")]
        public int? Claim { get; set; }

        public DateTime? TaxStartDate { get; set; }

        [StringLength(19)]
        public string? AppMatterNumber { get; set; }

        [TradeSecret]
        [StringLength(255)]
        [Display(Name = "Application Title")]
        public string? AppTitle { get; set; }

        [NotMapped]
        public int? OwnerID { get; set; }

        [StringLength(20)]
        public string? AppClientRef { get; set; }

        [Display(Name = "Family Reference")]
        public int? ParentAppId { get; set; }

        //[Display(Name = "Terminal Disclaimer")]
        //public int? TerminalDisclaimerAppId { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [StringLength(25)]
        public string? OldCaseNumber { get; set; }

        [StringLength(25)]
        [Display(Name = "Billing Number")]
        public string? BillingNumber { get; set; }

        [StringLength(25)]
        [Display(Name = "Storage")]
        public string? Storage { get; set; }

        [Display(Name = "Has Terminal Disclaimer?")]
        public bool? TerminalDisclaimer { get; set; }

        [StringLength(20)]
        [Display(Name = "National Number")]
        public string? PatNationalNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Certificate Number")]
        public string? PatCertificateNumber { get; set; }

        [StringLength(20)]
        public string? PMSID { get; set; }

        
        [Display(Name = "Export Control?")]
        public bool? ExportControl { get; set; }

        public string? CustomField1 { get; set; }
        public string? CustomField2 { get; set; }
        public string? CustomField3 { get; set; }
        public DateTime? CustomField4 { get; set; }
        public DateTime? CustomField5 { get; set; }
        public bool? CustomField6 { get; set; }
        public string? CustomField7 { get; set; }
        public string? CustomField8 { get; set; }
        public string? CustomField9 { get; set; }
        public DateTime? CustomField10 { get; set; }
        public DateTime? CustomField11 { get; set; }
        public bool? CustomField12 { get; set; }

        [Display(Name = "Track One?")]
        public bool TrackOne { get; set; }

        [StringLength(25)]
        [Display(Name = "Other Reference No.")]
        public string? OtherReferenceNumber { get; set; }

        [Display(Name = "Unitary Effect Registration Date")]
        public DateTime? UnitaryEffectRegDate { get; set; }
        [Display(Name = "Unitary Effect Request Date")]
        public DateTime? UnitaryEffectReqDate { get; set; }
        [Display(Name = "UPC Status")]
        public string? UPCStatus { get; set; }
        [Display(Name = "UPC Status Date")]
        public DateTime? UPCStatusDate { get; set; }

        public int? TaxAgentID { get; set; }
        public int? LegalRepresentativeID { get; set; }

        public CountryApplicationTradeSecret? TradeSecret { get; set; }

        public string? AppNumberSearch { get; set; }
        public string? PubNumberSearch { get; set; }
        public string? PatNumberSearch { get; set; }
        public string? ParentAppNumberSearch { get; set; }
        public string? ParentPatNumberSearch { get; set; }
        public string? PCTNumberSearch { get; set; }
    }

    public class CountryApplicationTradeSecret
    {
        [Encrypted]
        public string? AppTitle { get; set; }
    }

}
