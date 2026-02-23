using R10.Web.Helpers;
using R10.Core.Entities.Patent;
using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Helpers;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class CountryApplicationSearchResultViewModel
    {

        public int AppId { get; set; }

        [Display(Name = "LabelCaseNumber")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Status")]
        public string? ApplicationStatus { get; set; }

        [Display(Name = "Status Date")]
        public DateTime? ApplicationStatusDate { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Patent No.")]
        public string? PatNumber { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNumber { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

        [TradeSecret]
        [Display(Name = "Application Title")]
        public string? AppTitle { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Date Created")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Last Update")]
        public DateTime? LastUpdate { get; set; }

        [Display(Name = "Image")]
        public string? ImageFile { get; set; }

        public string? ThumbnailFile { get; set; }
        public string? ImageScreenCode { get; set; }
        public int ImageParentId { get; set; }

        public int InvId { get; set; }

        public bool? IsTradeSecret {  get; set; }

        public CountryApplicationTradeSecret? TradeSecret { get; set; }
    }

    public class CountryApplicationSearchResultSharePointViewModel : CountryApplicationSearchResultViewModel
    {
        public string? SharePointRecKey { get; set; }
        public string? ThumbnailUrl { get; set; }
    }

    #region Export
    public class CountryApplicationSearchResultExportViewModel
    {
        public int AppId { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }
        public string? CaseType { get; set; }
        public string? ApplicationStatus { get; set; }
        public DateTime? ApplicationStatusDate { get; set; }
        public string? OldCaseNumber { get; set; }
        public string? AppNumber { get; set; }
        public DateTime? FilDate { get; set; }
        public string? PatNumber { get; set; }
        public DateTime? IssDate { get; set; }
        public string? PubNumber { get; set; }
        public DateTime? PubDate { get; set; }
        public string? AppTitle { get; set; }
        public string? ParentAppNumber { get; set; }
        public DateTime? ParentFilDate { get; set; }
        public string? ParentPatNumber { get; set; }
        public DateTime? ParentIssDate { get; set; }
        public string? PCTNumber { get; set; }
        public DateTime? PCTDate { get; set; }
        public string? InvClientRef { get; set; }
        public string? AppClientRef { get; set; }
        public string? TaxSchedule { get; set; }
        public string? OtherReferenceNumber { get; set; }
        public DateTime? ExpDate { get; set; }
        public PatPriority? Priority { get; set; }
        public string? PriorityInfo { get; set; }
        public string? ClientName { get; set; }
        public string? AgentName { get; set; }
        public string? AgentRef { get; set; }
        public List<string>? Owners { get; set; }
        public string? OwnerNames { get; set; }
        public string? Attorney1Name { get; set; }
        public string? Attorney2Name { get; set; }
        public string? Attorney3Name { get; set; }
        public string? Attorney4Name { get; set; }
        public string? Attorney5Name { get; set; }
        public List<string>? Inventors { get; set; }
        public string? InventorNames { get; set; }
        public List<ActionDueExportViewModel>? ActionDues { get; set; }
        public string? NextActionDues { get; set; }
        public DateTime? UnitaryEffectRegDate { get; set; }
        public DateTime? UnitaryEffectReqDate { get; set; }
        public string? UPCStatus { get; set; }
        public DateTime? UPCStatusDate { get; set; }
        public string? TaxAgentName { get; set; }
        public string? LegalRepresentativeName { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdate { get; set; }

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

        public string? ImageFile { get; set; }

        public bool? IsTradeSecret { get; set; }
    }

    public class CountryApplicationSearchResultExportOutputViewModel
    {

        [Display(Name = "LabelCaseNumber")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Status")]
        public string? ApplicationStatus { get; set; }

        [Display(Name = "Status Date")]
        public DateTime? ApplicationStatusDate { get; set; }
        
        [Display(Name = "LabelOldCaseNumber")]
        public string? OldCaseNumber { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Patent No.")]
        public string? PatNumber { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? IssDate { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNumber { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

        [Display(Name = "Application Title")]
        public string? AppTitle { get; set; }

        [Display(Name = "Parent Application No.")]
        public string? ParentAppNumber { get; set; }

        [Display(Name = "Parent Filing Date")]
        public DateTime? ParentFilDate { get; set; }

        [Display(Name = "Parent Patent No.")]
        public string? ParentPatNumber { get; set; }

        [Display(Name = "Parent Issue Date")]
        public DateTime? ParentIssDate { get; set; }

        [Display(Name = "PCT No.")]
        public string? PCTNumber { get; set; }

        [Display(Name = "PCT Date")]
        public DateTime? PCTDate { get; set; }

        [Display(Name = "LabelClientRefInv")]
        public string? InvClientRef { get; set; }

        [Display(Name = "LabelClientRefCA")]
        public string? AppClientRef { get; set; }
        
        [Display(Name = "Tax Schedule")]
        public string? TaxSchedule { get; set; }

        [Display(Name = "Other Reference No.")]
        public string? OtherReferenceNumber { get; set; }

        [Display(Name = "Expiration Date")]
        public DateTime? ExpDate { get; set; }

        [Display(Name = "Priority")]
        public string? PriorityInfo { get; set; }

        [Display(Name = "LabelClientName")]
        public string? ClientName { get; set; }

        [Display(Name = "LabelAgentName")]
        public string? AgentName { get; set; }

        [Display(Name = "Agent Reference")]
        public string? AgentRef { get; set; }

        [Display(Name = "LabelOwnerName")]
        public string? OwnerNames { get; set; }

        [Display(Name = "LabelAttorney1")]
        public string? Attorney1Name { get; set; }

        [Display(Name = "LabelAttorney2")]
        public string? Attorney2Name { get; set; }

        [Display(Name = "LabelAttorney3")]
        public string? Attorney3Name { get; set; }

        [Display(Name = "LabelAttorney4")]
        public string? Attorney4Name { get; set; }

        [Display(Name = "LabelAttorney5")]
        public string? Attorney5Name { get; set; }
        
        [Display(Name = "Inventors")]
        public string? InventorNames { get; set; }

        [Display(Name = "Next Action Due")]
        public string? NextActionDues { get; set; }

        [Display(Name = "Unitary Effect Reg. Date")]
        public DateTime? UnitaryEffectRegDate { get; set; }

        [Display(Name = "Unitary Effect Request Date")]
        public DateTime? UnitaryEffectReqDate { get; set; }

        [Display(Name = "UPC Status")]
        public string? UPCStatus { get; set; }

        [Display(Name = "UPC Status Date")]
        public DateTime? UPCStatusDate { get; set; }

        [Display(Name = "LabelTaxAgentName")]
        public string? TaxAgentName { get; set; }

        [Display(Name = "LabelLegalRepresentativeName")]
        public string? LegalRepresentativeName { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Date Created")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Last Update")]
        public DateTime? LastUpdate { get; set; }

        [Display(Name = "Email download link")]
        public string? ExportInBackground { get; set; }

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

        [Display(Name = "Image File")]
        public string? ImageFile { get; set; }

        [NoExport]
        public string? SharePointDriveId { get; set; }

        [Display(Name = "Image")]
        public string? SharePointItemDriveId { get; set; }

        public bool? IsTradeSecret { get; set; }
    }
    #endregion
}
