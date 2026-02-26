using R10.Core.Entities.Patent;
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
// using R10.Core.Entities.Clearance; // Removed during deep clean
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Trademark
{
    public class TmkTrademark : TmkTrademarkDetail
    {
        public TmkCountry? TmkCountry { get; set; }
        public TmkCountry? TmkPrioCountry { get; set; }
        public TmkCaseType? TmkCaseType { get; set; }
        public TmkTrademarkStatus? TmkTrademarkStatus { get; set; }

        //public TmkOwner? TmkOwner { get; set; }
        public Client? Client { get; set; }
        public Agent? Agent { get; set; }
        public Attorney? Attorney1 { get; set; }
        public Attorney? Attorney2 { get; set; }
        public Attorney? Attorney3 { get; set; }
        public Attorney? Attorney4 { get; set; }
        public Attorney? Attorney5 { get; set; }

        public List<TmkTrademarkClass>? TrademarkClasses { get; set; }
        public List<TmkAssignmentHistory>? AssignmentsHistory { get; set; }
        public List<TmkKeyword>? Keywords { get; set; }
        public List<TmkOwner>? Owners { get; set; }
        public List<TmkActionDue>? ActionDues { get; set; }
        public List<TmkCostTrack>? CostTrackings { get; set; }
        public List<TmkLicensee>? Licensees { get; set; }
        public List<TmkImage>? Images { get; set; }
        //public List<TmkTrademarkDocFolder>? DocFolders { get; set; }
        public List<TmkDesignatedCountry>? DesignatedCountries { get; set; }
        public List<TmkConflict>? TmkConflicts { get; set; }
        public List<TmkProduct>? TmkProducts { get; set; }
        public List<TmkRelatedTrademark>? TmkRelatedTrademarks { get; set; }
        public List<TmkRelatedTrademark>? TmkTrademarkRelateds { get; set; }
        public List<PatRelatedTrademark>? PatRelatedTrademarks { get; set; }
//         public List<GMMatterTrademark>? GMMatterTrademarks { get; set; } // Removed during deep clean

//         public List<TmcRelatedTrademark>? TmcRelatedTrademarks { get; set; } // Removed during deep clean

//         public TLSearch? TLSearch { get; set; } // Removed during deep clean
        public List<TimeTracker>? TimeTrackers { get; set; }

        public List<TmkCostEstimator>? CostEstimators { get; set; }
        public List<TmkDocketRequest>? TmkDocketRequests { get; set; }

        // related matter
        // docs out
        // web links
    }

    public class TmkTrademarkDetail : BaseEntityWithRespOffice
    {
        [Key]
        public int TmkId { get; set; }

        [Required]
        [StringLength(25)]
        public string CaseNumber { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [StringLength(8)]
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Required]
        [StringLength(3)]
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [StringLength(25)]
        [Display(Name = "Mark Type")]
        public string? MarkType { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Trademark Name")]
        public string? TrademarkName { get; set; }


        [Display(Name = "Attorney 1")]
        public int? Attorney1ID { get; set; }
        [Display(Name = "Attorney 2")]
        public int? Attorney2ID { get; set; }
        [Display(Name = "Attorney 3")]
        public int? Attorney3ID { get; set; }
        [Display(Name = "Attorney 4")]
        public int? Attorney4ID { get; set; }
        [Display(Name = "Attorney 5")]
        public int? Attorney5ID { get; set; }


        [NotMapped]
        [Display(Name = "Owner")]
        public int? OwnerID { get; set; }

        [Display(Name = "Client")]
        public int? ClientID { get; set; }

        [StringLength(20)]
        [Display(Name = "Client Reference")]
        public string? ClientRef { get; set; }


        [Display(Name = "Agent")]
        public int? AgentID { get; set; }

        [StringLength(20)]
        [Display(Name = "Agent Reference")]
        public string? AgentRef { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Status")]
        public string? TrademarkStatus { get; set; } = "Unfiled"; //Default, but is updated during save based on entered date.

        [Display(Name = "Status Date")]
        public DateTime? TrademarkStatusDate { get; set; }

        [Display(Name = "Intent to Use")]
        public bool IntentToUse { get; set; }

        [Display(Name = "Allowance Date")]
        public DateTime? AllowanceDate { get; set; }

        [StringLength(5)]
        [Display(Name = "Priority Country")]
        public string? PriCountry { get; set; }

        [StringLength(20)]
        [Display(Name = "Priority No.")]
        public string? PriNumber { get; set; }

        [Display(Name = "Priority Date")]
        public DateTime? PriDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Publication No.")]
        public string? PubNumber { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Registration No.")]
        public string? RegNumber { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime? RegDate { get; set; }

        [Display(Name = "Last Renewal Date")]
        public DateTime? LastRenewalDate { get; set; }

        [Display(Name = "Next Renewal Date")]
        public DateTime? NextRenewalDate { get; set; }

        [Display(Name = "Last Renewal No.")]
        [StringLength(20)]
        public string? LastRenewalNumber { get; set; }

        [Display(Name = "Parent Application Number")]
        [StringLength(20)]
        public string? ParentAppNumber { get; set; }

        [Display(Name = "Parent Filing Date")]
        public DateTime? ParentFilDate { get; set; }

        [StringLength(14)]
        [Display(Name = "Matter Number")]
        public string? MatterNumber { get; set; }


        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [StringLength(25)]
        public string? OldCaseNumber { get; set; }

        [StringLength(25)]
        [Display(Name = "Storage")]
        public string? Storage { get; set; }

        [Display(Name = "Family Reference")]
        public int? ParentTmkId { get; set; }

        public int? TmcId { get; set; }

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

        public string? AppNumberSearch { get; set; }
        public string? PubNumberSearch { get; set; }
        public string? RegNumberSearch { get; set; }
        public string? PriNumberSearch { get; set; }
    }
}
