using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class PatentSelectionViewModel : BaseEntity
    {
        public int? AppId { get; set; }

        public int? InvId { get; set; }

        [StringLength(25)]
        public string CaseNumber { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        public string? Country { get; set; }

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
        [Display(Name = "Title")]
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
    }
}
