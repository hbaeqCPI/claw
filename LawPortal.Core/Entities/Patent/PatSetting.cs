using LawPortal.Core.Entities.Shared;
using System.ComponentModel.DataAnnotations;

namespace LawPortal.Core.Entities.Patent
{
    public class PatSetting : DefaultSetting
    {
        public bool IsMultipleOwnerOn { get; set; }
        public bool IsFamilyNumberOn { get; set; }
        public bool IsTaxStartCalcOn { get; set; }
        public bool IsPatCtryAppTitleOn { get; set; }
        public string? ClientMatterDivider { get; set; }

        [Display(Description = "Billing", GroupName = "Modules")]
        public bool IsBillingNoOn { get; set; }

        [Display(Description = "Terminal Disclaimer", GroupName = "Modules")]
        public bool IsTerminalDisclaimerOn { get; set; }

        public string? InpadocURL { get; set; }
        public string? StatisticsURL { get; set; }
        public string? PatStatURL { get; set; }        

        public string? PrioCaseTypeAutoPopulate { get; set; }

        [Display(Description = "Real-Time Patent System", GroupName = "Modules")]
        public bool IsRTSOn { get; set; }

        public string? CountriesWithNationalField { get; set; }
        public string? CountriesWithTaxSchedAndClaimField { get; set; }
        public string? CountriesWithConfirmationField { get; set; }
        public string? IDSCopyActionToGenerate { get; set; }
        public bool IsSubjectMattersOn { get; set; }

        [Display(Description = "Inventor Awards", GroupName = "Modules")]
        public bool IsInventorAwardOn { get; set; }

        public string? DefaultUSTaxSchedule { get; set; }
        public bool IsPatCtryAppInventorON { get; set; }
        public bool IsPatCtryAppOwnerOn { get; set; }
        public bool IsPatCtryAppClientRefOn { get; set; }
        public string? CountryLawUpdateURL { get; set; }

        public string? HomeCountry { get; set; }
        public bool ReportSplitMasterListApplicationFields { get; set; }

        [Display(Description = "IDS Import", GroupName = "Modules")]
        public bool IsIDSImportOn { get; set; }

        public string? CACustomFieldsTabLabel { get; set; }
        public string? InvCustomFieldsTabLabel { get; set; }

        // Patent Search (Azure)
        public string? PatentSearchUrl { get; set; }
        public string? PatentSearchIndexName { get; set; }
        public string? PatentSearchLinkUrl { get; set; }
        public int PatentSearchResultSize { get; set; } = 5000;

        [Display(Description = "Patent Search Monitoring", GroupName = "Modules")]
        public bool IsPatSearchMonitoringOn { get; set; }

        [Display(Description = "Patent Score", GroupName = "Modules")]
        public bool IsPatentScoreOn { get; set; }

        [Display(Description = "IDS INPADOC Import", GroupName = "Modules")]
        public bool IsIDSImportINPADOCOn { get; set; }

        public bool IsPatentWatchNotificationEmailOn { get; set; }

        [Display(Description = "German Inventor Remuneration", GroupName = "Modules")]
        public bool IsInventorRemunerationOn { get; set; }

        public string InventorRemunerationCalculateDate { get; set; }
        public string InventorRemunerationDefaultFormula { get; set; }
        public string InventorRemunerationPayOption { get; set; }
        public int InventorRemunerationNoInventors { get; set; }
        public bool IsInventorRemunerationBuyingRightsOn { get; set; }
        public bool IsInventorRemunerationInitialPaymentOn { get; set; }
        public bool IsInventorRemunerationMultiClientsOn { get; set; }
        public bool IsInventorRemunerationUsingProductSalesOn { get; set; }
        public int ImportInventorRemunerationProductSalesCutOffYear { get; set; }

        [Display(Description = "French Inventor Remuneration", GroupName = "Modules")]
        public bool IsInventorFRRemunerationOn { get; set; }
        public int InventorFRRemunerationNoInventors { get; set; }

        public string? CountryCopyRanges { get; set; }

        public bool IsTerminalDisclaimerWithinFamily { get; set; }
        public bool IsRTSUpdateWorkflowOn { get; set; }
        public bool IsRTSUpdateWorkflowEmailOn { get; set; }
        public int RTSUpdateWorkflowCutOff { get; set; }
        public bool IsRTSUpdateWorkflowDateCheckOn { get; set; }

        [Display(Description = "Cost Estimator", GroupName = "Modules")]
        public bool IsCostEstimatorOn { get; set; }

        public string? CostEstimatorCurrencyFormat { get; set; }

        public string? CEAnnuityCostType { get; set; }
        public int DefaultBillingAttorney { get; set; }
        public bool IsUnitaryPatentMarkerOnDesignatedCountryOn { get; set; }

        public string? CountryLawDocTemplate { get; set; }

        public bool IsInventionProductOn { get; set; }
        public bool IsRelatedCasesMassCopyOn { get; set; }
        public bool IsIDSMassCopyOn { get; set; }
        public bool IsInventionCostTrackingOn { get; set; }
        public bool IsInventionActionOn { get; set; }
        public bool IsINPADOCImportIDSFamilyOn { get; set; }

        [Display(Description = "Trade Secret", GroupName = "Modules")]
        public bool IsTradeSecretOn { get; set; }

        public string? USLinkDirectMainCode { get; set; }
        public string? USLinkSearchMainCode { get; set; }
        public string? EPLinkDirectMainCode { get; set; }
        public string? EPLinkSearchMainCode { get; set; }

        public int IDSNoFeeMaxCount { get; set; }
        public int IDSFirstTierMaxCount { get; set; }
        public int IDSSecondTierMaxCount { get; set; }
        
        public string? CPiOPSUrl { get; set; }
        public string? MyEPOURL { get; set; }        
        public string? MyEPODownloadDateFrom { get; set; }

        public bool IsIDSAutoStandardOn { get; set; }
        public string? EPOOPSAuthServer { get; set; }
        public string? EPOIDSBiblioUrl { get; set; }
        public string? EPOIDSDocUrl { get; set; }
        public string? EPOIDSMainUrl { get; set; }
        public string? EPOIDSNonEnglishCountries { get; set; }
        


    }
}
