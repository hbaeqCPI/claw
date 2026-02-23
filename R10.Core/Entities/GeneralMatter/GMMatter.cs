using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMMatter : GMMatterDetail
    {
        public Client? Client { get; set; }
        public Agent? Agent { get; set; }
        public GMMatterType? GMMatterType { get; set; }
        public GMMatterStatus? GMMatterStatus { get; set; }
        public List<GMMatterAttorney>? Attorneys { get; set; }
        public List<GMMatterCountry>? Countries { get; set; }
        public List<GMMatterPatent>? Patents { get; set; }
        public List<GMMatterTrademark>? Trademarks { get; set; }
        public List<GMMatterOtherParty>? OtherParties { get; set; }
        public List<GMCostTrack>? CostTrackings { get; set; }
        //public List<GMMatterImage>? Images { get; set; }
        public List<GMMatterKeyword>? Keywords { get; set; }
        public List<GMActionDue>? ActionsDue { get; set; }
        public List<GMMatterOtherPartyPatent>? OtherPartyPatents { get; set; }
        public List<GMMatterOtherPartyTrademark>? OtherPartyTrademarks { get; set; }
        public List<GMMatterRelatedMatter>? RelatedMatters { get; set; }
        public List<GMMatterRelatedMatter>? MatterRelateds { get; set; }
        public List<GMProduct>? GMProducts { get; set; }
        public List<TimeTracker>? TimeTrackers { get; set; }

        public GMMatter? ParentCase { get; set; }
        public List<GMMatter>? ChildCases { get; set; }
        public List<GMDocketRequest>? GMDocketRequests { get; set; }
    }

    public class GMMatterDetail : BaseEntityWithRespOffice
    {
        [Key]
        public int MatId { get; set; }

        [Required]
        [StringLength(25)]
        [Display(Name = "Case Number")]
        public string CaseNumber { get; set; }

        [StringLength(8)]
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [StringLength(25)]
        public string? OldCaseNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Matter Type")]
        public string? MatterType { get; set; }

        [StringLength(255)]
        [Display(Name = "Title")]
        public string? MatterTitle { get; set; }

        [StringLength(20)]
        [Display(Name = "Reference Number")]
        public string? ReferenceNumber { get; set; }

        public int? ClientID { get; set; }

        [StringLength(20)]
        public string? ClientRef { get; set; }

        public int? AgentID { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Matter Status")]
        public string? MatterStatus { get; set; }

        [Display(Name = "Matter Status Date")]
        public DateTime? MatterStatusDate { get; set; }

        [Display(Name = "Effective Open Date")]
        public DateTime? EffectiveOpenDate { get; set; }

        [Display(Name = "Termination/End Date")]
        public DateTime? TerminationEndDate { get; set; }

        [Display(Name = "Result/Royalty Description")]
        public string? ResultRoyalty { get; set; }

        [StringLength(20)]
        [Display(Name = "Agreement")]
        public string? AgreementType { get; set; }

        [StringLength(20)]
        [Display(Name = "Extent")]
        public string? Extent { get; set; }

        [StringLength(50)]
        [Display(Name = "Court")]
        public string? Court { get; set; }

        [StringLength(50)]
        [Display(Name = "Court Docket No.")]
        public string? CourtDocketNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Judge/Magistrate")]
        public string? CourtJudgeMagistrate { get; set; }

        [StringLength(19)]
        [Display(Name = "Matter Number")]
        public string? MatterNumber { get; set; }

        public string? Remarks { get; set; }

        public string? CustomField1 { get; set; }
        public string? CustomField2 { get; set; }
        public string? CustomField3 { get; set; }
        public DateTime? CustomField4 { get; set; }
        public DateTime? CustomField5 { get; set; }
        public bool? CustomField6 { get; set; }
        public string? CustomField7 { get; set; }
        public string? CustomField8 { get; set; }
        public string? CustomField9 { get; set; }
        public string? CustomField10 { get; set; }
        public string? CustomField11 { get; set; }
        public DateTime? CustomField12 { get; set; }
        public DateTime? CustomField13 { get; set; }
        public bool? CustomField14 { get; set; }
        public bool? CustomField15 { get; set; }

        [StringLength(50)]
        [Display(Name = "Project Name")]
        public string? ProjectName { get; set; }

        [StringLength(50)]        
        [Display(Name = "Program")]
        public string? Program { get; set; }

        [Display(Name = "Parent Matter")]
        public int? ParentMatId { get; set; }

        [Display(Name = "NDA")]
        public bool IsNDA { get; set; }

        [Display(Name = "Amendment")]
        public bool IsAmendment { get; set; }

        [Display(Name = "Continuing Obligation After the Termination Date")]
        public bool IsObligationContinue { get; set; }

        [Display(Name = "Length of Continuing Obligation")]
        public int? ObligationLength { get; set; }

        [Display(Name = "Notice Required for Termination")]
        public int? TerminationNotice { get; set; }

        [StringLength(25)]
        [Display(Name = "Other Reference No.")]
        public string? OtherReferenceNumber { get; set; }
    }
}
