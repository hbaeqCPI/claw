using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class CountryApplicationDetailViewModel : CountryApplicationDetail
    {
        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }
        public string? AgentCode { get; set; }
        public string? AgentName { get; set; }
        public string? ClientCode { get; set; }
        public string? ClientName { get; set; }
        public string? OwnerCode { get; set; }
        public string? OwnerName { get; set; }
        //[Display(Name = "Attorney 1")]
        public string? Attorney1 { get; set; }
        public string? Attorney1Label { get; set; }
        //[Display(Name = "Attorney 2")]
        public string? Attorney2 { get; set; }
        public string? Attorney2Label { get; set; }
        //[Display(Name = "Attorney 3")]
        public string? Attorney3 { get; set; }
        public string? Attorney3Label { get; set; }
        //[Display(Name = "Attorney 4")]
        public string? Attorney4 { get; set; }
        public string? Attorney4Label { get; set; }
        //[Display(Name = "Attorney 5")]
        public string? Attorney5 { get; set; }
        public string? Attorney5Label { get; set; }

        public string? ParentCase { get; set; }
        public string? RelatedTerminalDisclaimer { get; set; }

        public string? LabelTaxSchedule { get; set; } = "Tax Schedule";
        public bool IsActive { get; set; }
        public bool ShowNationalField { get; set; }
       
        public bool ShowClaimField { get; set; }
        public bool ShowTaxScheduleField { get; set; }
        public bool ShowConfirmationField { get; set; }
        public bool ShowBillingNumberField { get; set; }
        public bool LockRecord { get; set; }

        [NotMapped]
        public string? CopyOptions { get; set; }

        #region IDS
        [StringLength(10)]
        [Display(Name = "Group Art Unit")]
        public string? GroupArtUnit { get; set; }
        [StringLength(50)]
        [Display(Name = "Examiner")]
        public string? Examiner { get; set; }
        [StringLength(25)]
        [Display(Name = "Attorney Docket No")]
        public string? AttorneyDocketNo { get; set; }
        [StringLength(50)]
        [Display(Name = "Customer No")]
        public string? CustomerNo { get; set; }
        public int? RelatedCasesId { get; set; }
        
        [Display(Name = "Include in Patent Center Download?")]
        public bool? IncludeInPatCenterDownload { get; set; }

        [Display(Name = "1st Tier Paid Date")]
        public DateTime? FirstTierPaidDate { get; set; }

        [Display(Name = "1st Tier Paid Amount")]
        public decimal? FirstTierPaidAmount { get; set; } = 0;

        [Display(Name = "2nd Tier Paid Date")]
        public DateTime? SecondTierPaidDate { get; set; }

        [Display(Name = "2nd Tier Paid Amount")]
        public decimal? SecondTierPaidAmount { get; set; } = 0;

        [Display(Name = "3rd Tier Paid Date")]
        public DateTime? ThirdTierPaidDate { get; set; }

        [Display(Name = "3rd Tier Paid Amount")]
        public decimal? ThirdTierPaidAmount { get; set; } = 0;

        #endregion

        public string? RequiredEntities { get; set; }

        public bool IsOwnerRequired { get; set; }
        public bool IsInventorRequired { get; set; }

        public CountryApplicationPriorityViewModel? Priority { get; set; }
        public DefaultImageViewModel? DefaultImage { get; set; }
        public List<SysCustomFieldSetting>? SysCustomFieldSettings { get; set; }
        public double PatentScore { get; set; }

        public int? TerminalDisclaimerAppId { get; set; }
        public bool ShowUnitaryEffectFields { get; set; }
        public bool ShowUPCStatusFields { get; set; }
        public int? DesignatedCount { get; set; }

        public bool IsFavorite { get; set; }
        public int FavoriteCount { get; set; }
        public string? SharePointRecKey { get; set; }

        public string? TaxAgentCode { get; set; }
        public string? TaxAgentName { get; set; }
        public string? LegalRepresentativeCode { get; set; }
        public string? LegalRepresentativeName { get; set; }

        public bool? IsTradeSecret { get; set; } = false;
        public int RequestDocketPendingCount { get; set; }
    }

    public enum UnitaryPatent { 
        ShowUnitaryEffect =1,
        ShowUPCStatus,
        GetDesignatedCount
    }
}
