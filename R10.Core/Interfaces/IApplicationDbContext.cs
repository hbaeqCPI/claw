using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using R10.Core.DTOs;
using R10.Core.Entities;
// using R10.Core.Entities.AMS; // Removed during deep clean
// using R10.Core.Entities.Clearance; // Removed during deep clean
// using R10.Core.Entities.DMS; // Removed during deep clean
using R10.Core.Entities.Documents;
using R10.Core.Entities.FormExtract;
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Core.Entities.GlobalSearch;
// using R10.Core.Entities.PatClearance; // Removed during deep clean
using R10.Core.Entities.Patent;
using R10.Core.Entities.ReportScheduler;
// using R10.Core.Entities.RMS; // Removed during deep clean
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Identity;
using R10.Core.Queries.Shared;
// using R10.Core.Entities.ForeignFiling; // Removed during deep clean

namespace R10.Core.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<DeleteLog> DeleteLogs { get; set; }

        #region Shared Auxiliaries
        DbSet<Agent> Agents { get; set; }
        DbSet<Attorney> Attorneys { get; set; }
        DbSet<Client> Clients { get; set; }
        DbSet<Owner> Owners { get; set; }
        DbSet<ContactPerson> ContactPersons { get; set; }

        //Product Aux
        DbSet<Product> Products { get; set; }
        DbSet<ProductGroup> ProductGroups { get; set; }
        DbSet<ProductCategory> ProductCategorys { get; set; }
        DbSet<RelatedProduct> RelatedProducts { get; set; }
        DbSet<ProductSale> ProductSales { get; set; }
        DbSet<RelatedProductDTO> RelatedProductDTO { get; set; }
        DbSet<SharedCountryLookupDTO> SharedCountryLookupDTO { get; set; }
        DbSet<Brand> Brands { get; set; }

        //Product Import
        DbSet<ProductImportHistory> ProductImportHistory { get; set; }
        DbSet<ProductImportMapping> ProductImportMappings { get; set; }
        DbSet<ProductImportError> ProductImportErrors { get; set; }
        DbSet<ProductImportTypeColumn> ProductImportTypeColumns { get; set; }

        DbSet<DocuSignAnchor> DocuSignAnchors { get; set; }
        DbSet<DocuSignAnchorTab> DocuSignAnchorTabs { get; set; }

        #endregion

        #region Patent
        DbSet<Invention> Inventions { get; set; }
        DbSet<CountryApplication> CountryApplications { get; set; }
        DbSet<PatAssignmentHistory> PatAssignmentsHistory { get; set; }
        DbSet<PatLicensee> PatLicensees { get; set; }
        DbSet<PatInventorApp> PatInventorsApp { get; set; }
        DbSet<PatDesignatedCountry> PatDesignatedCountries { get; set; }
        DbSet<PatProduct> PatProducts { get; set; }
        DbSet<PatProductInv> PatProductInvs { get; set; }
        DbSet<PatSubjectMatter> PatSubjectMatters { get; set; }
        DbSet<PatRelatedTrademark> PatRelatedTrademarks { get; set; }

        DbSet<PatPriority> PatPriorities { get; set; }
        DbSet<PatAbstract> PatAbstracts { get; set; }
        DbSet<PatKeyword> PatKeywords { get; set; }
        DbSet<PatInventorInv> PatInventorsInv { get; set; }

        DbSet<InventionRelatedDisclosure> InventionRelatedDisclosures { get; set; }

        DbSet<PatDisclosureStatus> PatDisclosureStatuses { get; set; }
        DbSet<PatCountry> PatCountries { get; set; }
        DbSet<PatArea> PatAreas { get; set; }
        DbSet<PatAreaCountry> PatAreasCountries { get; set; }
        DbSet<PatAssignmentStatus> PatAssignmentStatuses { get; set; }
        DbSet<PatCountryLaw> PatCountryLaws { get; set; }
        DbSet<PatCountryDue> PatCountryDues { get; set; }
        DbSet<PatCountryExp> PatCountryExpirations { get; set; }
        DbSet<PatCaseType> PatCaseTypes { get; set; }
        DbSet<PatActionType> PatActionTypes { get; set; }
        DbSet<PatActionParameter> PatActionParameters { get; set; }
        DbSet<PatApplicationStatus> ApplicationStatuses { get; set; }
        DbSet<PatIndicator> PatIndicators { get; set; }
        DbSet<PatDesCaseType> PatDesCaseTypes { get; set; }
        DbSet<PatInventor> PatInventors { get; set; }
        //DbSet<PatImageInv> PatImageInvs { get; set; }
        DbSet<PatOwnerInv> PatOwnerInvs { get; set; }
        DbSet<PatOwnerApp> PatOwnerApps { get; set; }
        DbSet<PatActionDue> PatActionDues { get; set; }
        DbSet<PatDueDate> PatDueDates { get; set; }
        DbSet<PatActionDueInv> PatActionDueInvs { get; set; }
        DbSet<PatDueDateInv> PatDueDateInvs { get; set; }
        DbSet<PatCostTrack> PatCostTracks { get; set; }
        DbSet<PatCostTrackInv> PatCostTrackInvs { get; set; }
        DbSet<PatCostType> PatCostTypes { get; set; }
        //DbSet<PatImageApp> PatImageApps { get; set; }
        //DbSet<PatImageAct> PatImageActs { get; set; }
        DbSet<PatIDSRelatedCase> PatIDSRelatedCases { get; set; }
        DbSet<PatIDSRelatedCasesInfo> PatIDSRelatedCasesInfos { get; set; }
        DbSet<PatIDSNonPatLiterature> PatIDSNonPatLiteratures { get; set; }
        DbSet<PatRelatedCase> PatRelatedCases { get; set; }
        DbSet<PatRelatedCaseDTO> PatRelatedCaseDTO { get; set; }
        //DbSet<PatImageCost> PatImageCosts { get; set; }
        DbSet<PatTaxBase> PatTaxBases { get; set; }
        DbSet<PatTaxYear> PatTaxYears { get; set; }
        DbSet<PatIDSReferenceSource> PatIDSReferenceSources { get; set; }
        DbSet<PatInventorAwardCriteria> PatInventorAwardCriterias { get; set; }
        DbSet<PatInventorAppAward> PatInventorAppAwards { get; set; }
        DbSet<PatInventorAwardType> PatInventorAwardTypes { get; set; }

        DbSet<PatBudgetManagement> PatBudgetManagements { get; set; }

        DbSet<PatIDSManageDTO> PatIDSManageDTO { get; set; }
        DbSet<InventionRelatedInvention> InventionRelatedInventions { get; set; }

        //Pat Cost Tracking Import
        DbSet<PatCostTrackingImportHistory> PatCostTrackingImportsHistory { get; set; }
        DbSet<PatCostTrackingImportMapping> PatCostTrackingImportMappings { get; set; }
        DbSet<PatCostTrackingImportError> PatCostTrackingImportErrors { get; set; }
        DbSet<PatCostTrackingImportTypeColumn> PatCostTrackingImportTypeColumns { get; set; }

        DbSet<InventionCopySetting> InventionCopySettings { get; set; }
        DbSet<CountryApplicationCopySetting> CountryApplicationCopySettings { get; set; }
        //DbSet<CountryApplicationCopySettingChild> CountryApplicationCopySettingsChild { get; set; }

        DbSet<PatCountryLawUpdate> PatCountryLawUpdate { get; set; }

        DbSet<PatWorkflow> PatWorkflows { get; set; }
        DbSet<PatWorkflowAction> PatWorkflowActions { get; set; }
        DbSet<PatWorkflowActionParameter> PatWorkflowActionParameters { get; set; }
        DbSet<LookupDescDTO> PatActionTypeDTO { get; set; }
        DbSet<PatSearchField> PatSearchFields { get; set; }
        DbSet<PatSearchNotify> PatSearchNotifies { get; set; }
        DbSet<PatSearchNotifyLog> PatSearchNotifyLogs { get; set; }
        DbSet<PatSearchDTO> PatSearchDTO { get; set; }
        DbSet<PatSearchExportDTO> PatSearchExportDTO { get; set; }
        DbSet<PatSearchEmailDTO> PatSearchEmailDTO { get; set; }
        DbSet<PatScoreCategory> PatScoreCategories { get; set; }
        DbSet<PatScore> PatScores { get; set; }
        DbSet<PatScoreDTO> PatScoreDTO { get; set; }
        DbSet<PatAverageScoreDTO> PatAverageScoreDTO { get; set; }
        DbSet<PatParentCaseTDDTO> PatParentCaseTDDTO { get; set; }

        //Cost Estimator
        DbSet<PatCEAnnuitySetup> PatCEAnnuitySetups { get; set; }
        DbSet<PatCEAnnuityCost> PatCEAnnuityCosts { get; set; }
        DbSet<PatCECountrySetup> PatCECountrySetups { get; set; }
        DbSet<PatCECountryCost> PatCECountryCosts { get; set; }
        DbSet<PatCECountryCostChild> PatCECountryCostChilds { get; set; }
        DbSet<PatCECountryCostSub> PatCECountryCostSubs { get; set; }
        DbSet<PatCEGeneralSetup> PatCEGeneralSetups { get; set; }
        DbSet<PatCEGeneralCost> PatCEGeneralCosts { get; set; }
        DbSet<PatCEFee> PatCEFees { get; set; }
        DbSet<PatCEFeeDetail> PatCEFeeDetails { get; set; }
        DbSet<PatCEStage> PatCEStages { get; set; }
        DbSet<PatCostEstimatorBaseAppDTO> PatCostEstimatorBaseAppDTO { get; set; }
        DbSet<PatCostEstimator> PatCostEstimators { get; set; }
        DbSet<PatCostEstimatorCountry> PatCostEstimatorCountries { get; set; }
        DbSet<PatCostEstimatorCountryCost> PatCostEstimatorCountryCosts { get; set; }
        DbSet<PatCEQuestionGeneral> PatCEQuestionGenerals { get; set; }
        DbSet<PatCostEstimatorCost> PatCostEstimatorCosts { get; set; }
        DbSet<PatCostEstimatorCostChild> PatCostEstimatorCostChilds { get; set; }
        DbSet<PatCostEstimatorCostSub> PatCostEstimatorCostSubs { get; set; }

        DbSet<PatEGrantDownloaded> PatEGrantDownloaded { get; set; }
        DbSet<PatTerminalDisclaimerChecked> PatTerminalDisclaimerCheckeds { get; set; }

        //MyEPO API
        DbSet<EPOPortfolio> EPOPortfolios { get; set; }
        DbSet<EPOApplication> EPOApplications { get; set; }
        DbSet<EPODueDate> EPODueDates { get; set; }        
        DbSet<EPOCommunication> EPOCommunications { get; set; }
        DbSet<EPOCommunicationDoc> EPOCommunicationDocs { get; set; }

        DbSet<PatEPODocumentCombined> PatEPODocumentCombineds { get; set; }
        DbSet<PatEPOMailLog> PatEPOMailLogs { get; set; }

        DbSet<PatEPODocumentMerge> PatEPODocumentMerges { get; set; }
        DbSet<PatEPODocumentMergeGuide> PatEPODocumentMergeGuides { get; set; }
        DbSet<PatEPODocumentMergeGuideSub> PatEPODocumentMergeGuideSubs { get; set; }

        DbSet<PatEPODocumentMap> PatEPODocumentMaps { get; set; }
        DbSet<PatEPODocumentMapAct> PatEPODocumentMapActs { get; set; }
        DbSet<PatEPODocumentMapTag> PatEPODocumentMapTags { get; set; }

        DbSet<EPODueDateTerm> EPODueDateTerms { get; set; }
        DbSet<PatEPOActionMapAct> PatEPOActionMapActs { get; set; }
        DbSet<PatEPOAppLog> PatEPOAppLogs { get; set; }

        DbSet<PatEPOCommActLog> PatEPOCommActLogs { get; set; }
        DbSet<PatEPODDActLog> PatEPODDActLogs { get; set; }

        DbSet<LookupDTO> EPODocuments { get; set; }

        //EPO OPS API
        DbSet<PatOPSLog> PatOPSLogs { get; set; }
        #endregion

        #region Patent Clearance Search

//         DbSet<PacClearance> PacClearances { get; set; } // Removed during deep clean
//         DbSet<PacClearanceStatus> PacClearanceStatuses { get; set; } // Removed during deep clean
//         DbSet<PacQuestion> PacQuestions { get; set; } // Removed during deep clean
        //DbSet<PacImage> PacImages { get; set; }
//         DbSet<PacClearanceStatusHistory> PacClearanceStatusesHistory { get; set; } // Removed during deep clean
//         DbSet<PacInventor> PacInventors { get; set; } // Removed during deep clean
//         DbSet<PacKeyword> PacKeywords { get; set; } // Removed during deep clean

//         DbSet<PacQuestionGroup> PacQuestionGroups { get; set; } // Removed during deep clean
//         DbSet<PacQuestionGuide> PacQuestionGuides { get; set; } // Removed during deep clean
//         DbSet<PacQuestionGuideChild> PacQuestionGuideChildren { get; set; } // Removed during deep clean

//         DbSet<PacWorkflow> PacWorkflows { get; set; } // Removed during deep clean
//         DbSet<PacWorkflowAction> PacWorkflowActions { get; set; } // Removed during deep clean

//         DbSet<PacClearanceCopySetting> PacClearanceCopySettings { get; set; } // Removed during deep clean
//         DbSet<PacClearanceCopyDisclosureSetting> PacClearanceCopyDisclosureSettings { get; set; } // Removed during deep clean

        #endregion

        #region DMS
        // dms main
//         DbSet<Disclosure> Disclosures { get; set; } // Removed during deep clean
//         DbSet<DisclosureCopySetting> DisclosureCopySettings { get; set; } // Removed during deep clean
//         DbSet<DisclosureCopyClearanceSetting> DisclosureCopyClearanceSettings { get; set; } // Removed during deep clean

//         DbSet<DMSActionDue> DMSActionDues { get; set; } // Removed during deep clean
//         DbSet<DMSDueDate> DMSDueDates { get; set; } // Removed during deep clean
//         DbSet<DMSActionReminderLog> DMSActionReminderLogs { get; set; } // Removed during deep clean
        // DbSet<DMSActionReminderEmailDTO> DMSActionReminderEmailDTOs { get; set; } // Removed during deep clean

//         DbSet<DMSInventor> DMSInventors { get; set; } // Removed during deep clean
//         DbSet<DMSInventorHistory> DMSInventorHistory { get; set; } // Removed during deep clean

//         DbSet<DMSAbstract> DMSAbstracts { get; set; } // Removed during deep clean
//         DbSet<DMSKeyword> DMSKeywords { get; set; } // Removed during deep clean
        //DbSet<DMSImage> DMSImages { get; set; }
        //DbSet<DMSImageAct> DMSImageActs { get; set; }                       // should be removed               

//         DbSet<DMSDisclosureStatusHistory> DMSDisclosureStatusesHistory { get; set; } // Removed during deep clean
//         DbSet<DMSRecommendationHistory> DMSRecommendationsHistory { get; set; } // Removed during deep clean
//         DbSet<DisclosureRelatedDisclosure> DisclosureRelatedDisclosures { get; set; } // Removed during deep clean
//         DbSet<DMSQuestion> DMSQuestions { get; set; } // Removed during deep clean
//         DbSet<DMSCombined> DMSCombineds { get; set; } // Removed during deep clean

        // workflow
//         DbSet<DMSWorkflow> DMSWorkflows { get; set; } // Removed during deep clean
//         DbSet<DMSWorkflowAction> DMSWorkflowActions { get; set; } // Removed during deep clean

        // dms aux
//         DbSet<DMSActionType> DMSActionTypes { get; set; } // Removed during deep clean
//         DbSet<DMSRating> DMSRatings { get; set; } // Removed during deep clean
//         DbSet<DMSDisclosureStatus> DMSDisclosureStatuses { get; set; } // Removed during deep clean
//         DbSet<DMSIndicator> DMSIndicators { get; set; } // Removed during deep clean
//         DbSet<DMSRecommendation> DMSRecommendations { get; set; } // Removed during deep clean
//         DbSet<DMSQuestionGroup> DMSQuestionGroups { get; set; } // Removed during deep clean
//         DbSet<DMSQuestionGuide> DMSQuestionGuides { get; set; } // Removed during deep clean
//         DbSet<DMSQuestionGuideChild> DMSQuestionGuideChildren { get; set; } // Removed during deep clean
//         DbSet<DMSQuestionGuideSub> DMSQuestionGuideSubs { get; set; } // Removed during deep clean
//         DbSet<DMSQuestionGuideSubDtl> DMSQuestionGuideSubDtls { get; set; } // Removed during deep clean

        // valuation matrix
//         DbSet<DMSValuationMatrix> DMSValuationMatrices { get; set; } // Removed during deep clean
//         DbSet<DMSValuationMatrixRate> DMSValuationMatrixRates { get; set; } // Removed during deep clean

        // DbSet<DMSAverageRatingDTO> DMSAverageRatingDTO { get; set; } // Removed during deep clean

        // agenda meeting
//         DbSet<DMSAgenda> DMSAgendas { get; set; } // Removed during deep clean
//         DbSet<DMSAgendaReviewer> DMSAgendaReviewers { get; set; } // Removed during deep clean
//         DbSet<DMSAgendaRelatedDisclosure> DMSAgendaRelatedDisclosures { get; set; } // Removed during deep clean

//         DbSet<DMSFaqDoc> DMSFaqDocs { get; set; } // Removed during deep clean

        #endregion

        #region RTS
//         DbSet<RTSMapActionDue> RTSMapActionDues { get; set; } // Removed during deep clean
//         DbSet<RTSMapActionDueSource> RTSMapActionDueSources { get; set; } // Removed during deep clean
//         DbSet<RTSMapActionClose> RTSMapActionsClose { get; set; } // Removed during deep clean
//         DbSet<RTSSearch> RTSSearchRecords { get; set; } // Removed during deep clean
//         DbSet<RTSSearchAction> RTSSearchActions { get; set; } // Removed during deep clean
//         DbSet<RTSSearchUSIFW> RTSSearchUSIFWs { get; set; } // Removed during deep clean
        //DbSet<RTSPatentWatch> PatentWatchList { get; set; }
//         DbSet<PDTSentLog> PDTSentLogs { get; set; } // Removed during deep clean
//         DbSet<RTSPFSWorkflowBatch> RTSPFSWorkflowBatches { get; set; } // Removed during deep clean
//         DbSet<RTSPFSWorkflowApp> RTSPFSWorkflowApps { get; set; } // Removed during deep clean
//         DbSet<RTSBiblioUpdate> RTSBiblioUpdates { get; set; } // Removed during deep clean
//         DbSet<PubNumberConverted> PubNumberConverteds { get; set; } // Removed during deep clean
//         DbSet<RTSBiblioUpdateHistory> RTSBiblioUpdatesHistory { get; set; } // Removed during deep clean

//         DbSet<RTSMapActionDocument> RTSMapActionDocuments { get; set; } // Removed during deep clean
//         DbSet<RTSMapActionDocumentClient> RTSMapActionDocumentClients { get; set; } // Removed during deep clean

//         DbSet<RTSSearchIDSCount> RTSSearchIDSCounts { get; set; } // Removed during deep clean
        #endregion

        #region AMS
//         DbSet<AMSMain> AMSMain { get; set; } // Removed during deep clean
        #endregion

        #region RMS
//         DbSet<RMSReminderSetup> RMSReminderSetup { get; set; } // Removed during deep clean
//         DbSet<RMSInstrxTypeAction> RMSInstrxTypeAction { get; set; } // Removed during deep clean
        #endregion

        #region FF
//         DbSet<FFReminderSetup> FFReminderSetup { get; set; } // Removed during deep clean
//         DbSet<FFInstrxTypeAction> FFInstrxTypeAction { get; set; } // Removed during deep clean
        #endregion

        #region TL
//         DbSet<TLSearch> TLSearchRecords { get; set; } // Removed during deep clean
//         DbSet<TLSearchAction> TLSearchActions { get; set; } // Removed during deep clean
//         DbSet<TLSearchImage> TLSearchImages { get; set; } // Removed during deep clean
//         DbSet<TLSearchDocument> TLSearchDocuments { get; set; } // Removed during deep clean
//         DbSet<TLMapActionDue> TLMapActionDues { get; set; } // Removed during deep clean
//         DbSet<TLMapActionDueSource> TLMapActionDueSources { get; set; } // Removed during deep clean
//         DbSet<TLMapActionClose> TLMapActionsClose { get; set; } // Removed during deep clean
//         DbSet<TLBiblioUpdate> TLBiblioUpdates { get; set; } // Removed during deep clean
//         DbSet<TLTrademarkNameUpdate> TLTrademarkNameUpdates { get; set; } // Removed during deep clean
//         DbSet<TLActionComparePTO> TLActionComparePTO { get; set; } // Removed during deep clean
//         DbSet<TLActionUpdateHistory> TLActionUpdatesHistory { get; set; } // Removed during deep clean
//         DbSet<TLBiblioUpdateHistory> TLBiblioUpdatesHistory { get; set; } // Removed during deep clean
//         DbSet<TLTmkNameUpdateHistory> TLTmkNameUpdatesHistory { get; set; } // Removed during deep clean
//         DbSet<TLGoodsUpdateHistory> TLGoodsUpdatesHistory { get; set; } // Removed during deep clean
//         DbSet<TLSearchImageDTO> TLSearchImageDTO { get; set; } // Removed during deep clean
//         DbSet<TLMapActionDocument> TLMapActionDocuments { get; set; } // Removed during deep clean
//         DbSet<TLMapActionDocumentClient> TLMapActionDocumentClients { get; set; } // Removed during deep clean
//         DbSet<TLActionUpdateExclude> TLActionUpdateExcludes { get; set; } // Removed during deep clean
        #endregion

        #region Trademark
        DbSet<TmkCountry> TmkCountries { get; set; }
        DbSet<TmkArea> TmkAreas { get; set; }
        DbSet<TmkAreaCountry> TmkAreasCountries { get; set; }

        DbSet<TmkAssignmentStatus> TmkAssignmentStatuses { get; set; }
        DbSet<TmkCaseType> TmkCaseTypes { get; set; }
        DbSet<TmkDesCaseType> TmkDesCaseTypes { get; set; }
        DbSet<TmkConflictStatus> TmkConflictStatuses { get; set; }
        DbSet<TmkCostType> TmkCostTypes { get; set; }
        DbSet<TmkCountryLaw> TmkCountryLaws { get; set; }
        DbSet<TmkCountryDue> TmkCountryDues { get; set; }
        DbSet<TmkActionType> TmkActionTypes { get; set; }
        DbSet<TmkActionParameter> TmkActionParameters { get; set; }
        DbSet<TmkIndicator> TmkIndicators { get; set; }
        DbSet<TmkMarkType> TmkMarkTypes { get; set; }
        DbSet<TmkStandardGood> TmkStandardGoods { get; set; }
        DbSet<TmkTrademarkStatus> TmkTrademarkStatuses { get; set; }
        DbSet<TmkTrademark> TmkTrademarks { get; set; }
        DbSet<TmkTrademarkClass> TmkTrademarkClasses { get; set; }

        DbSet<TmkActionDue> TmkActionDues { get; set; }
        DbSet<TmkDueDate> TmkDueDates { get; set; }
        DbSet<TmkAssignmentHistory> TmkAssignmentsHistory { get; set; }
        DbSet<TmkCostTrack> TmkCostTracks { get; set; }
        DbSet<TmkConflict> TmkConflicts { get; set; }
        DbSet<TmkLicensee> TmkLicensees { get; set; }
        DbSet<TmkKeyword> TmkKeywords { get; set; }
        //DbSet<TmkImage> TmkImages { get; set; }
        //DbSet<TmkImageAct> TmkImageActs { get; set; }
        //DbSet<TmkImageCost> TmkImageCosts { get; set; }
        DbSet<TmkDesignatedCountry> TmkDesignatedCountries { get; set; }
        DbSet<TmkOwner> TmkOwners { get; set; }
        DbSet<TmkBudgetManagement> TmkBudgetManagements { get; set; }
        DbSet<TmkRelatedTrademark> TmkRelatedTrademarks { get; set; }

        //Tmk Cost Tracking Import
        DbSet<TmkCostTrackingImportHistory> TmkCostTrackingImportHistory { get; set; }
        DbSet<TmkCostTrackingImportMapping> TmkCostTrackingImportMappings { get; set; }
        DbSet<TmkCostTrackingImportError> TmkCostTrackingImportErrors { get; set; }
        DbSet<TmkCostTrackingImportTypeColumn> TmkCostTrackingImportTypeColumns { get; set; }

        DbSet<TmkTrademarkCopySetting> TrademarkCopySettings { get; set; }

        DbSet<TmkCountryLawUpdate> TmkCountryLawUpdate { get; set; }
        DbSet<TmkWorkflow> TmkWorkflows { get; set; }
        DbSet<TmkWorkflowAction> TmkWorkflowActions { get; set; }
        DbSet<TmkWorkflowActionParameter> TmkWorkflowActionParameters { get; set; }
        DbSet<LookupDescDTO> TmkActionTypeDTO { get; set; }

        //Cost Estimator        
        DbSet<TmkCECountrySetup> TmkCECountrySetups { get; set; }
        DbSet<TmkCECountryCost> TmkCECountryCosts { get; set; }
        DbSet<TmkCECountryCostChild> TmkCECountryCostChilds { get; set; }
        DbSet<TmkCECountryCostSub> TmkCECountryCostSubs { get; set; }
        DbSet<TmkCEGeneralSetup> TmkCEGeneralSetups { get; set; }
        DbSet<TmkCEGeneralCost> TmkCEGeneralCosts { get; set; }
        DbSet<TmkCEFee> TmkCEFees { get; set; }
        DbSet<TmkCEFeeDetail> TmkCEFeeDetails { get; set; }
        DbSet<TmkCEStage> TmkCEStages { get; set; }
        DbSet<TmkCostEstimator> TmkCostEstimators { get; set; }
        DbSet<TmkCostEstimatorCountry> TmkCostEstimatorCountries { get; set; }
        DbSet<TmkCostEstimatorCountryCost> TmkCostEstimatorCountryCosts { get; set; }
        DbSet<TmkCEQuestionGeneral> TmkCEQuestionGenerals { get; set; }
        DbSet<TmkCostEstimatorCost> TmkCostEstimatorCosts { get; set; }
        DbSet<TmkCostEstimatorCostChild> TmkCostEstimatorCostChilds { get; set; }
        DbSet<TmkCostEstimatorCostSub> TmkCostEstimatorCostSubs { get; set; }

        #endregion

        #region Trademark Clearance

//         DbSet<TmcClearance> TmcClearances { get; set; } // Removed during deep clean
//         DbSet<TmcClearanceStatus> TmcClearanceStatuses { get; set; } // Removed during deep clean
//         DbSet<TmcQuestion> TmcQuestions { get; set; } // Removed during deep clean
        //DbSet<TmcImage> TmcImages { get; set; }
//         DbSet<TmcClearanceStatusHistory> TmcClearanceStatusesHistory { get; set; } // Removed during deep clean

//         DbSet<TmcQuestionGroup> TmcQuestionGroups { get; set; } // Removed during deep clean
//         DbSet<TmcQuestionGuide> TmcQuestionGuides { get; set; } // Removed during deep clean
//         DbSet<TmcQuestionGuideChild> TmcQuestionGuideChildren { get; set; } // Removed during deep clean

//         DbSet<TmcWorkflow> TmcWorkflows { get; set; } // Removed during deep clean
//         DbSet<TmcWorkflowAction> TmcWorkflowActions { get; set; } // Removed during deep clean

//         DbSet<TmcKeyword> TmcKeywords { get; set; } // Removed during deep clean
//         DbSet<TmcList> TmcLists { get; set; } // Removed during deep clean
//         DbSet<TmcRelatedTrademark> TmcRelatedTrademarks { get; set; } // Removed during deep clean
//         DbSet<TmcMark> TmcMarks { get; set; } // Removed during deep clean

//         DbSet<TmcClearanceCopySetting> ClearanceCopySettings { get; set; } // Removed during deep clean

        #endregion

        #region General Matters
//         DbSet<GMMatter> GMMatters { get; set; } // Removed during deep clean
//         DbSet<GMMatterPatent> GMMatterPatents { get; set; } // Removed during deep clean
//         DbSet<GMMatterAttorney> GMMatterAttorneys { get; set; } // Removed during deep clean
//         DbSet<GMDueDate> GMDueDates { get; set; } // Removed during deep clean
//         DbSet<GMMatterCopySetting> GMMatterCopySettings { get; set; } // Removed during deep clean
//         DbSet<GMCostTrack> GMCostTracks { get; set; } // Removed during deep clean
//         DbSet<GMCostType> GMCostTypes { get; set; } // Removed during deep clean
//         DbSet<GMActionDue> GMActionsDue { get; set; } // Removed during deep clean
//         DbSet<GMWorkflow> GMWorkflows { get; set; } // Removed during deep clean
//         DbSet<GMWorkflowAction> GMWorkflowActions { get; set; } // Removed during deep clean
//         DbSet<GMWorkflowActionParameter> GMWorkflowActionParameters { get; set; } // Removed during deep clean
        // DbSet<LookupDTO> GMActionTypeDTO { get; set; } // Removed during deep clean
//         DbSet<GMProduct> GMProducts { get; set; } // Removed during deep clean
//         DbSet<GMBudgetManagement> GMBudgetManagements { get; set; } // Removed during deep clean
//         DbSet<GMActionType> GMActionTypes { get; set; } // Removed during deep clean
//         DbSet<GMActionParameter> GMActionParameters { get; set; } // Removed during deep clean
//         DbSet<GMIndicator> GMIndicators { get; set; } // Removed during deep clean

        //Cost Tracking Import
//         DbSet<GMCostTrackingImportHistory> GMCostTrackingImportHistory { get; set; } // Removed during deep clean
//         DbSet<GMCostTrackingImportMapping> GMCostTrackingImportMappings { get; set; } // Removed during deep clean
//         DbSet<GMCostTrackingImportError> GMCostTrackingImportErrors { get; set; } // Removed during deep clean
//         DbSet<GMCostTrackingImportTypeColumn> GMCostTrackingImportTypeColumns { get; set; } // Removed during deep clean
        #endregion

        #region Images
        DbSet<ImageType> ImageTypes { get; set; }
        //DbSet<FileHandler> FileHandler { get; set; }
        #endregion

        #region Shared
        DbSet<CPiLanguage> CPiLanguages { get; set; }
        DbSet<Language> Languages { get; set; }
        DbSet<SearchCriteria> SearchCriteria { get; set; }
        DbSet<SearchCriteriaDetail> SearchCriteriaDetails { get; set; }
        DbSet<DeDocketInstruction> DeDocketInstructions { get; set; }
        #endregion

        #region Quick Email
        DbSet<QEMain> QEMains { get; set; }
        DbSet<QELayout> QELayouts { get; set; }
        DbSet<QERecipient> QERecipients { get; set; }
        DbSet<QEDataSource> QEDataSources { get; set; }
        DbSet<QEDataSourceScreen> QEDataSourceScreens { get; set; }
        DbSet<QERoleSource> QERoleSources { get; set; }
        DbSet<QELog> QELogs { get; set; }
        DbSet<QEColumnDTO> QEColumnDTO { get; set; }
        DbSet<QEPatActionDueDeletedView> QEPatActionDueDeletedView { get; set; }
        DbSet<QEPatActionDueInvDeletedView> QEPatActionDueInvDeletedView { get; set; }
        DbSet<QETmkActionDueDeletedView> QETmkActionDueDeletedView { get; set; }
        DbSet<QEGmActionDueDeletedView> QEGmActionDueDeletedView { get; set; }
        DbSet<QEPatCountryAppDeletedView> QEPatCountryAppDeletedView { get; set; }
        DbSet<QETmkTrademarkDeletedView> QETmkTrademarkDeletedView { get; set; }
        DbSet<QEGmMatterDeletedView> QEGmMatterDeletedView { get; set; }
        DbSet<QECustomField> QECustomFields { get; set; }
        DbSet<QEFieldListDTO> QEFieldListDTO { get; set; }
        DbSet<QECategory> QECategories { get; set; }
        DbSet<QETag> QETags { get; set; }

        #endregion

        #region Report Scheduler
        DbSet<RSActionType> RSActionTypes { get; set; }
        DbSet<RSCriteriaControl> RSCriteriaControls { get; set; }
        DbSet<RSFrequencyType> RSFrequencyTypes { get; set; }
        DbSet<RSOrderByControl> RSOrderByControls { get; set; }
        DbSet<RSDateTypeControl> RSDateTypeControls { get; set; }
        DbSet<RSPrintOptionControl> RSPrintOptionControls { get; set; }
        DbSet<RSReportType> RSReportTypes { get; set; }
        DbSet<RSMain> RSMains { get; set; }
        DbSet<RSHistory> RSHistorys { get; set; }
        DbSet<RSPrintOption> RSPrintOptions { get; set; }
        DbSet<RSPrintOptionHistory> RSPrintOptionHistorys { get; set; }
        DbSet<RSCriteria> RSCriterias { get; set; }
        DbSet<RSCriteriaHistory> RSCriteriaHistorys { get; set; }
        DbSet<RSAction> RSActions { get; set; }
        DbSet<RSActionHistory> RSActionHistorys { get; set; }

        #endregion Report Scheduler

        #region Data Query
        DbSet<DataQueryMain> DataQueriesMain { get; set; }
        DbSet<DataQueryAllowedFunction> DataQueryAllowedFunctions { get; set; }
        DbSet<DQMetadataDTO> DQMetadataDTO { get; set; }
        DbSet<DQMetaRelationsDTO> DQMetaRelationsDTO { get; set; }
        DbSet<DataQueryCategory> DataQueryCategories { get; set; }
        DbSet<DataQueryTag> DataQueryTags { get; set; }
        #endregion

        #region System Tables
        DbSet<CPiMenuItem> CPiMenuItems { get; set; }
        DbSet<CPiMenuPage> CPiMenuPages { get; set; }

        DbSet<CPiDefaultPage> CPiDefaultPages { get; set; }
        DbSet<CPiSetting> CPiSettings { get; set; }
        DbSet<CPiUserSetting> CPiUserSettings { get; set; }
        DbSet<CPiSystemSetting> CPiSystemSettings { get; set; }

        DbSet<CPiWidget> CPiWidgets { get; set; }
        DbSet<CPiUserWidget> CPiUserWidgets { get; set; }

        DbSet<Option> Options { get; set; }
        DbSet<ModuleMain> ModulesMain { get; set; }
        DbSet<SystemScreen> SystemScreens { get; set; }

        DbSet<ChartDTO> ChartDTO { get; set; }
        DbSet<CaseListDTO> CaseListDTO { get; set; }

        DbSet<LocalizationRecords> LocalizationRecords { get; set; }
        DbSet<LocalizationRecordsGrouping> LocalizationRecordsGrouping { get; set; }
        DbSet<Notification> Notifications { get; set; }
        DbSet<NotificationConnection> NotificationConnections { get; set; }
        DbSet<SysCustomFieldSetting> SysCustomFieldSettings { get; set; }

        #endregion

        #region Security & System Logs

        DbSet<CPiUserEntityFilter> CPiUserEntityFilters { get; set; }
        DbSet<CPiUserSystemRole> CPiUserSystemRoles { get; set; }
        DbSet<CPiRespOffice> CPiRespOffices { get; set; }
        DbSet<CPiUser> CPiUser { get; set; }
        DbSet<CPiGroup> CPiGroups { get; set; }
        DbSet<Log> Logs { get; set; }
        #endregion

        #region Data Import
        DbSet<DataImportHistory> DataImportsHistory { get; set; }
        DbSet<DataImportType> DataImportTypes { get; set; }
        DbSet<DataImportMapping> DataImportMappings { get; set; }
        DbSet<DataImportError> DataImportErrors { get; set; }
        DbSet<DataImportTypeColumn> DataImportTypeColumns { get; set; }
        #endregion

        #region Letters
        DbSet<LetterMain> LettersMain { get; set; }
        DbSet<LetterCategory> LetterCategories { get; set; }
        DbSet<LetterDataSource> LetterDataSources { get; set; }
        DbSet<LetterRecordSource> LetterRecordSources { get; set; }
        DbSet<LetterRecordSourceFilter> LetterRecordSourceFilters { get; set; }
        DbSet<LetterRecordSourceFilterUser> LetterRecordSourceFiltersUser { get; set; }
        DbSet<LetterUserData> LetterUserData { get; set; }
        DbSet<LetterSubCategory> LetterSubCategories { get; set; }
        DbSet<LetterTag> LetterTags { get; set; }

        DbSet<LetterEntitySetting> LetterEntitySettings { get; set; }
        DbSet<LookupDTO> LetterFilterLookUpDTO { get; set; }
        DbSet<LetterFieldListDTO> LetterFieldListDTO { get; set; }
        DbSet<LetterContactDTO> LetterContactDTO { get; set; }
        DbSet<LetterLog> LetterLogs { get; set; }
        DbSet<LetterLogDetail> LetterLogDetails { get; set; }
        DbSet<LetterCustomField> LetterCustomFields { get; set; }

        #endregion

        #region DOCX
        DbSet<DOCXMain> DOCXesMain { get; set; }
        DbSet<DOCXCategory> DOCXCategories { get; set; }
        DbSet<DOCXDataSource> DOCXDataSources { get; set; }
        DbSet<DOCXRecordSource> DOCXRecordSources { get; set; }
        DbSet<DOCXRecordSourceFilter> DOCXRecordSourceFilters { get; set; }
        DbSet<DOCXRecordSourceFilterUser> DOCXRecordSourceFiltersUser { get; set; }
        DbSet<DOCXUserData> DOCXUserData { get; set; }

        //DbSet<DOCXEntitySetting> DOCXEntitySettings { get; set; }
        DbSet<LookupDTO> DOCXFilterLookUpDTO { get; set; }
        DbSet<DOCXFieldListDTO> DOCXFieldListDTO { get; set; }
        //DbSet<DOCXContactDTO> DOCXContactDTO { get; set; }
        DbSet<DOCXLog> DOCXLogs { get; set; }
        DbSet<DOCXUSPTOHeader> DOCXUSPTOHeaders { get; set; }
        DbSet<DOCXUSPTOHeaderKeyword> DOCXUSPTOHeaderKeywords { get; set; }
        DbSet<DOCXUSPTOHeaderKeywordDTO> DOCXUSPTOHeaderKeywordDTO { get; set; }
        DbSet<DOCXUSPTOHeaderKeywordExcelDTO> DOCXUSPTOHeaderKeywordExcelDTO { get; set; }


        #endregion

        #region QuickDocket
        DbSet<PatDueDateDeDocket> PatDueDateDeDockets { get; set; }
        DbSet<PatDueDateInvDeDocket> PatDueDateInvDeDockets { get; set; }
        DbSet<TmkDueDateDeDocket> TmkDueDateDeDockets { get; set; }
//         DbSet<GMDueDateDeDocket> GMDueDateDeDockets { get; set; } // Removed during deep clean
        DbSet<PatDueDateExtension> PatDueDateExtensions { get; set; }
        DbSet<PatDueDateInvExtension> PatDueDateInvExtensions { get; set; }
        DbSet<TmkDueDateExtension> TmkDueDateExtensions { get; set; }
//         DbSet<GMDueDateExtension> GMDueDateExtensions { get; set; } // Removed during deep clean
//         DbSet<DMSDueDateExtension> DMSDueDateExtensions { get; set; } // Removed during deep clean
        DbSet<DueDateExtensionLog> DueDateExtensionsLog { get; set; }

        DbSet<PatDueDateDeDocketResp> PatDueDateDeDocketResps { get; set; }
        DbSet<TmkDueDateDeDocketResp> TmkDueDateDeDocketResps { get; set; }
        DbSet<GMDueDateDeDocketResp> GMDueDateDeDocketResps { get; set; }

        DbSet<PatDocketRequest> PatDocketRequests { get; set; }
        DbSet<PatDocketInvRequest> PatDocketInvRequests { get; set; }
        DbSet<TmkDocketRequest> TmkDocketRequests { get; set; }
        DbSet<GMDocketRequest> GMDocketRequests { get; set; }

        DbSet<PatDocketRequestResp> PatDocketRequestResps { get; set; }
        DbSet<TmkDocketRequestResp> TmkDocketRequestResps { get; set; }
        DbSet<GMDocketRequestResp> GMDocketRequestResps { get; set; }


        #endregion

        #region Family Trees
        DbSet<FamilyTreeDTO> FamilyTreeDTO { get; set; }

        #endregion

        #region Documents
        DbSet<DocSystem> DocSystems { get; set; }
        DbSet<DocMatterTree> DocMatterTrees { get; set; }
        DbSet<DocTreeDTO> DocTreeDTO { get; set; }
        DbSet<DocTreeEmailApiDTO> DocTreeEmailApiDTO { get; set; }

        DbSet<DocImageDetailDTO> DocImageDetailDTO { get; set; }
        DbSet<DocLetterLogDetailDTO> DocLetterLogDetailDTO { get; set; }
        DbSet<DocQELogDetailDTO> DocQELogDetailDTO { get; set; }

        DbSet<DocEFSLogDetailDTO> DocEFSLogDetailDTO { get; set; }
        DbSet<DocIDSRelCasesDTO> DocIDSRelCasesDTO { get; set; }
        DbSet<DocIDSNonPatLitDTO> DocIDSNonPatLitDTO { get; set; }
        DbSet<DocViewDTO> DocViewDTO { get; set; }
        DbSet<DocInfoDTO> DocInfoDTO { get; set; }

        DbSet<DocFolder> DocFolders { get; set; }
        DbSet<DocDocument> DocDocuments { get; set; }
        DbSet<DocDocumentTag> DocDocumentTags { get; set; }
        DbSet<DocFile> DocFiles { get; set; }
        DbSet<DocFileSignature> DocFileSignatures { get; set; }
        DbSet<SharePointFileSignature> SharePointFileSignatures { get; set; }
        DbSet<DocFileSignatureRecipient> DocFileSignatureRecipients { get; set; }

        DbSet<DocIcon> DocIcons { get; set; }
        DbSet<DocType> DocTypes { get; set; }
        DbSet<DocFixedFolder> DocFixedFolders { get; set; }

        DbSet<DocGmailCaseLink> DocGmailCaseLinks { get; set; }

        DbSet<CaseLogDTO> CaseLogDTO { get; set; }

        DbSet<DocOutlook> DocOutlook { get; set; }
        DbSet<DocOutlookCaseLink> DocOutlookCaseLinks { get; set; }
        DbSet<DocOutlookId> DocOutlookIds { get; set; }
        DbSet<DocReviewDTO> DocReviewDTO { get; set; }

        DbSet<DocVerification> DocVerifications { get; set; }
        DbSet<DocVerificationSearchField> DocVerificationSearchFields { get; set; }

        DbSet<DocResponsibleLog> DocResponsibleLogs { get; set; }
        DbSet<DocResponsibleDocketing> DocRespDocketings { get; set; }
        DbSet<DocResponsibleReporting> DocRespReportings { get; set; }

        DbSet<DocQuickEmailLog> DocQuickEmailLogs { get; set; }
        #endregion

        #region Global Search
        DbSet<GSSystem> GSSystems { get; set; }
        DbSet<GSScreen> GSScreens { get; set; }
        DbSet<GSTable> GSTables { get; set; }
        DbSet<GSField> GSFields { get; set; }
        #endregion

        #region Form Extract
        DbSet<FormSystem> FormSystems { get; set; }
        DbSet<FormSource> FormSources { get; set; }
        DbSet<FormIFWFormType> FormIFWFormTypes { get; set; }
        DbSet<FormIFWDocType> FormIFWDocTypes { get; set; }
        DbSet<FormIFWDataExtract> FormIFWDataExtracts { get; set; }
        DbSet<FormIFWFieldUsage> FormIFWFieldUsages { get; set; }
        DbSet<FormIFWActionMap> FormIFWActionMaps { get; set; }
        DbSet<FormIFWActMap> FormIFWActMaps { get; set; }
        DbSet<FormIFWActMapPat> FormIFWActMapsPat { get; set; }
        DbSet<FormIFWActMapTmk> FormIFWActMapsTmk { get; set; }

        DbSet<FormIFWActionDueDTO> FormIFWActionDueDTO { get; set; }
        DbSet<FormIFWActionUpdateDTO> FormIFWActionUpdateDTO { get; set; }

        DbSet<FormIFWActionRemarksDTO> FormIFWActionRemarksDTO { get; set; }

        DbSet<FormPLMapDTO> FormPLMapDTO { get; set; }

        #endregion

        #region Others
        DbSet<PatParentCaseDTO> PatParentCaseDTO { get; set; }
        DbSet<EFSLog> EFSLogs { get; set; }
        DbSet<LookupIntDTO> LookupIntDTO { get; set; }        
        DbSet<DelegationEmailDTO> DelegationEmailDTO { get; set; }
        DbSet<DelegationDetailDTO> DelegationDetailDTO { get; set; }
        DbSet<MyFavorite> MyFavorites { get; set; }

        DbSet<CEEstimatedCostDTO> CEEstimatedCostDTOs { get; set; }        
        DbSet<CECascadeCostDTO> CECascadeCostDTOs { get; set; }
        DbSet<PatActionMultipleBasedOnDTO> PatActionMultipleBasedOnDTO { get; set; }
        #endregion

        #region API
        DbSet<CountryApplicationWebSvc> CountryApplicationWebSvcs { get; set; }
        DbSet<TmkTrademarkWebSvc> TmkTrademarkWebSvcs { get; set; }
        DbSet<PatIDSDownloadWebSvc> PatIDSDownloadWebSvcs { get; set; }
        DbSet<WebServiceLog> WebServiceLogs { get; set; }
        #endregion

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        //DbSet<TQuery> Query<TQuery>() where TQuery : class;

        EntityEntry Entry(object entity);

        DatabaseFacade Database { get; }
        void DetachAllEntities();
        List<EntityEntry> GetAllTrackedEntities();
    }
}
