using R10.Web.Helpers;
using R10.Web.Areas.Shared.ViewModels;
using R10.Core.Entities.Trademark;
using System;
using System.ComponentModel.DataAnnotations;


namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkTrademarkSearchResultViewModel
    {
        public int TmkId { get; set; }

        [Display(Name = "LabelCaseNumber")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Trademark Name")]
        public string? TrademarkName { get; set; }

        [Display(Name = "Status")]
        public string? TrademarkStatus { get; set; }
        
        [Display(Name = "Status Date")]
        public DateTime? TrademarkStatusDate { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Registration No.")]
        public string? RegNumber { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime? RegDate { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNumber { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

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

        [Display(Name = "Image")]
        public string? ThumbnailFile { get; set; }

    }
    public class TmkTrademarkSearchResultSharePointViewModel : TmkTrademarkSearchResultViewModel {
        public string? SharePointRecKey { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
    #region Export
    public class TmkTrademarkSearchResultExportViewModel
    {
        public int TmkId { get; set; }
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        public string? SubCase { get; set; }
        public string? CaseType { get; set; }
        public string? TrademarkName { get; set; }
        public string? TrademarkStatus { get; set; }
        public DateTime? TrademarkStatusDate { get; set; }
        public string? OldCaseNumber { get; set; }
        public string? AppNumber { get; set; }
        public DateTime? FilDate { get; set; }
        public string? RegNumber { get; set; }
        public DateTime? RegDate { get; set; }
        public string? PubNumber { get; set; }
        public DateTime? PubDate { get; set; }

        public string? ClientRef { get; set; }
        public List<GoodsExportViewModel>? TrademarkClasses { get; set; }
        public string? Goods { get; set; }
        public string? MarkType { get; set; }
        public string? ClientName { get; set; }
        public List<string>? Owners { get; set; }
        public string? OwnerNames { get; set; }
        public string? AgentName { get; set; }
        public string? AgentRef { get; set; }
        public string? OtherReferenceNumber { get; set; }
        public DateTime? AllowanceDate { get; set; }
        public DateTime? NextRenewalDate { get; set; }
        public string? Attorney1Name { get; set; }
        public string? Attorney2Name { get; set; }
        public string? Attorney3Name { get; set; }
        public string? Attorney4Name { get; set; }
        public string? Attorney5Name { get; set; }
        public List<ActionDueExportViewModel>? ActionDues { get; set; }
        public string? NextActionDues { get; set; }

        public string? PriCountry { get; set; }
        public string? PriNumber { get; set; }
        public DateTime? PriDate { get; set; }

        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string? ImageFile { get; set; }
        public string? ThumbnailFile { get; set; }

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


    }
    public class TmkTrademarkSearchResultExportOutputViewModel
    {
        [Display(Name = "LabelCaseNumber")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Trademark Name")]
        public string? TrademarkName { get; set; }

        [Display(Name = "Status")]
        public string? TrademarkStatus { get; set; }

        [NoExport]
        [Display(Name = "Status Date")]
        public DateTime? TrademarkStatusDate { get; set; }

        [Display(Name = "LabelOldCaseNumber")]
        public string? OldCaseNumber { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Registration No.")]
        public string? RegNumber { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime? RegDate { get; set; }

        [Display(Name = "Publication No.")]
        public string? PubNumber { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? PubDate { get; set; }

        [Display(Name = "Client Reference")]
        public string? ClientRef { get; set; }

        [Display(Name = "Class/Goods")]
        public string? Goods { get; set; }

        [Display(Name = "Mark Type")]
        public string? MarkType { get; set; }

        [Display(Name = "LabelClientName")]
        public string? ClientName { get; set; }

        [Display(Name = "LabelOwnerName")]
        public string? OwnerNames { get; set; }

        [Display(Name = "LabelAgentName")]
        public string? AgentName { get; set; }

        [Display(Name = "Agent Reference")]
        public string? AgentRef { get; set; }

        [Display(Name = "Other Reference No.")]
        public string? OtherReferenceNumber { get; set; }

        [Display(Name = "Allowance Date")]
        public DateTime? AllowanceDate { get; set; }

        [Display(Name = "Next Renewal Date")]
        public DateTime? NextRenewalDate { get; set; }

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

        [Display(Name = "Next Action Due")]
        public string? NextActionDues { get; set; }

        [Display(Name = "Image")]
        public string? ImageFile { get; set; }

        [Display(Name = "Priority Country")]
        public string? PriCountry { get; set; }

        [Display(Name = "Priority Number")]
        public string? PriNumber { get; set; }

        [Display(Name = "Priority Date")]
        public DateTime? PriDate { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Date Created")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Last Update")]
        public DateTime? LastUpdate { get; set; }

        [NoExport]
        public string? SharePointDriveId { get; set; }

        [Display(Name = "Image")]
        public string? SharePointItemDriveId { get; set; }

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

    }

    public class GoodsExportViewModel
    {
        public string? Class { get; set; }
        public string? ClassType { get; set; }
        public string? Goods { get; set; }
    }
    #endregion
}
