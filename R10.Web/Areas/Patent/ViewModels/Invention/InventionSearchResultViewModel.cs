using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;
using R10.Core.Helpers;
using R10.Web.Areas.Trademark.ViewModels;
using R10.Web.Helpers;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class InventionSearchResultViewModel
    {
        public int InvId { get; set; }

        public string? CaseNumber { get; set; }

        [Display(Name = "Family Number")]
        public string? FamilyNumber { get; set; }

        [Display(Name = "Disclosure Status")]
        public string? DisclosureStatus { get; set; }

        [TradeSecret]
        [Display(Name = "Title")]
        public string? InvTitle { get; set; }

        [Display(Name = "LabelClient")]
        public string? ClientCode { get; set; }

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

        public string? SharePointRecKey { get; set; }
        public string? ThumbnailUrl { get; set; }

        public bool? IsTradeSecret { get; set; } = false;

        public InventionTradeSecret? TradeSecret { get; set; }
    }    

    #region Export
    public class InventionSearchResultExportViewModel
    {
        public int InvId { get; set; }
        public string? CaseNumber { get; set; }
        public string? FamilyNumber { get; set; }
        public string? DisclosureStatus { get; set; }
        public string? InvTitle { get; set; }
        public string? ClientCode { get; set; }
        public string? ClientRef { get; set; }
        public string? ClientName { get; set; }
        public DateTime? DisclosureDate { get; set; }
        public List<string>? Owners { get; set; }
        public string? OwnerNames { get; set; }
        public List<string>? Inventors { get; set; }
        public string? InventorNames { get; set; }
        public PatPriority? Priority { get; set; }
        public string? PriorityInfo { get; set; }
        public string? Abstract { get; set; }
        public string? Attorney1Name { get; set; }
        public string? Attorney2Name { get; set; }
        public string? Attorney3Name { get; set; }
        public string? Attorney4Name { get; set; }
        public string? Attorney5Name { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdate { get; set; }
        
        public string? CustomField1 { get; set; }
        public string? CustomField2 { get; set; }
        public string? CustomField3 { get; set; }
        public string? CustomField4 { get; set; }
        public DateTime? CustomField5 { get; set; }
        public bool? CustomField6 { get; set; }
        public string? CustomField7 { get; set; }
        public string? CustomField8 { get; set; }
        public DateTime? CustomField9 { get; set; }
        public bool? CustomField10 { get; set; }
        
        public string? ImageFile { get; set; }

        public bool? IsTradeSecret { get; set; }

        public List<InventionTradeSecretRequest>? TradeSecretRequests { get; set; }
    }

    public class InventionSearchResultExportOutputViewModel
    {
        [Display(Name = "LabelCaseNumber")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Family Number")]
        public string? FamilyNumber { get; set; }

        [Display(Name = "Disclosure Status")]
        public string? DisclosureStatus { get; set; }

        [Display(Name = "Title")]
        public string? InvTitle { get; set; }

        [Display(Name = "LabelClient")]
        public string? ClientCode { get; set; }

        [Display(Name = "LabelClientRef")]
        public string? ClientRef { get; set; }

        [Display(Name = "LabelClientName")]
        public string? ClientName { get; set; }

        [Display(Name = "Disclosure Date")]
        public DateTime? DisclosureDate { get; set; }

        [Display(Name = "LabelOwnerName")]
        public string? OwnerNames { get; set; }

        [Display(Name = "Inventors")]
        public string? InventorNames { get; set; }

        [Display(Name = "Priority")]
        public string? PriorityInfo { get; set; }

        [Display(Name = "Abstract")]
        public string? Abstract { get; set; }

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
        public string? CustomField4 { get; set; }
        public DateTime? CustomField5 { get; set; }
        public bool? CustomField6 { get; set; }
        public string? CustomField7 { get; set; }
        public string? CustomField8 { get; set; }
        public DateTime? CustomField9 { get; set; }
        public bool? CustomField10 { get; set; }

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
