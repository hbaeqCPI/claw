using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Clearance;
using R10.Core.Entities.DMS;
using R10.Core.Entities.Documents;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.GlobalSearch;
using R10.Core.Entities.Patent;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Queries.Shared;
using R10.Infrastructure.Data.AMS.mappings;
using R10.Infrastructure.Data.Clearance.mappings;
using R10.Infrastructure.Data.DMS.mappings;
using R10.Infrastructure.Data.Documents.mappings;
using R10.Infrastructure.Data.GeneralMatter.mappings;
using R10.Infrastructure.Data.GlobalSearch.mappings;
using R10.Infrastructure.Data.Patent.mappings;
using R10.Infrastructure.Data.ReportScheduler.mappings;
using R10.Infrastructure.Data.RTS.mappings;
using R10.Infrastructure.Data.Shared.mappings;
using R10.Infrastructure.Data.TL.mappings;
using R10.Infrastructure.Data.Trademark.mappings;
using R10.Infrastructure.Identity.Mappings;
using R10.Infrastructure.Data.RMS.mappings;
using R10.Core.Entities.PatClearance;
using R10.Infrastructure.Data.PatClearance.mappings;
using R10.Core.Entities.FormExtract;
using R10.Infrastructure.Data.FormExtract.mappings;
using R10.Infrastructure.Data.ForeignFiling.mappings;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using R10.Infrastructure.Data.MailDownload.mappings;
using R10.Core.Entities.AMS;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using R10.Core.Helpers;
using R10.Core.Entities.RMS;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using R10.Core.Entities.ForeignFiling;

namespace R10.Infrastructure.Data
{

    public class ApplicationDbContext : DbContext, IApplicationDbContext, IDataProtectionKeyContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        #region Entities Declaration
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
        public DbSet<DeleteLog> DeleteLogs { get; set; }

        #region Shared Auxiliaries
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Attorney> Attorneys { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientDesignatedCountry> ClientDesignatedCountries { get; set; }

        public DbSet<ContactPerson> ContactPersons { get; set; }        
        public DbSet<ClientContact> ClientContacts { get; set; }
        public DbSet<OwnerContact> OwnerContacts { get; set; }
        public DbSet<AgentContact> AgentContacts { get; set; }
        public DbSet<AgentCEFee> AgentCEFees { get; set; }

        public DbSet<Language> Languages { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<CurrencyType> CurrencyTypes { get; set; }
        public DbSet<CPiLanguage> CPiLanguages { get; set; }
        public DbSet<DeDocketInstruction> DeDocketInstructions { get; set; }
        public DbSet<SearchCriteria> SearchCriteria { get; set; }
        public DbSet<SearchCriteriaDetail> SearchCriteriaDetails { get; set; }
        public DbSet<CustomReport> CustomReports { get; set; }
        public DbSet<TimeTracker> TimeTrackers { get; set; }
        public DbSet<TimeTrack> TimeTracks { get; set; }

        //Product Aux
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductGroup> ProductGroups { get; set; }
        public DbSet<ProductCategory> ProductCategorys { get; set; }
        public DbSet<RelatedProduct> RelatedProducts { get; set; }
        public DbSet<ProductSale> ProductSales { get; set; }
        public DbSet<RelatedProductDTO> RelatedProductDTO { get; set; }
        public DbSet<SharedCountryLookupDTO> SharedCountryLookupDTO { get; set; }

        //Product Import
        public DbSet<ProductImportHistory> ProductImportHistory { get; set; }
        public DbSet<ProductImportMapping> ProductImportMappings { get; set; }
        public DbSet<ProductImportError> ProductImportErrors { get; set; }
        public DbSet<ProductImportTypeColumn> ProductImportTypeColumns { get; set; }

        public DbSet<DocuSignAnchor> DocuSignAnchors { get; set; }
        public DbSet<DocuSignAnchorTab> DocuSignAnchorTabs { get; set; }  
        public DbSet<Brand> Brands { get; set; }     //YX 20210806 placehoder for brand table delete later

        //YX 20210806

        #endregion

        #region Patent

        public DbSet<Invention> Inventions { get; set; }
        public DbSet<CountryApplication> CountryApplications { get; set; }
        public DbSet<PatAssignmentHistory> PatAssignmentsHistory { get; set; }
        public DbSet<PatLicensee> PatLicensees { get; set; }
        public DbSet<PatInventorApp> PatInventorsApp { get; set; }
        public DbSet<PatDesignatedCountry> PatDesignatedCountries { get; set; }
        public DbSet<PatProduct> PatProducts { get; set; }
        public DbSet<PatProductInv> PatProductInvs { get; set; }
        public DbSet<PatSubjectMatter> PatSubjectMatters { get; set; }
        public DbSet<PatRelatedTrademark> PatRelatedTrademarks { get; set; }

        public DbSet<PatPriority> PatPriorities { get; set; }
        public DbSet<PatAbstract> PatAbstracts { get; set; }
        public DbSet<PatKeyword> PatKeywords { get; set; }
        public DbSet<PatInventorInv> PatInventorsInv { get; set; }

        public DbSet<InventionRelatedDisclosure> InventionRelatedDisclosures { get; set; }

        public DbSet<PatDisclosureStatus> PatDisclosureStatuses { get; set; }
        public DbSet<PatCountry> PatCountries { get; set; }
        public DbSet<PatArea> PatAreas { get; set; }
        public DbSet<PatAreaCountry> PatAreasCountries { get; set; }
        public DbSet<PatAssignmentStatus> PatAssignmentStatuses { get; set; }
        public DbSet<PatCountryLaw> PatCountryLaws { get; set; }
        public DbSet<PatCountryDue> PatCountryDues { get; set; }
        public DbSet<PatCountryExp> PatCountryExpirations { get; set; }
        public DbSet<PatCaseType> PatCaseTypes { get; set; }
        public DbSet<PatActionType> PatActionTypes { get; set; }
        public DbSet<PatActionParameter> PatActionParameters { get; set; }
        public DbSet<PatApplicationStatus> ApplicationStatuses { get; set; }
        public DbSet<PatUPCStatus> PatUPCStatuses { get; set; }
        public DbSet<PatDesignationDTO> PatDesignationDTO { get; set; }
        public DbSet<PatIndicator> PatIndicators { get; set; }
        public DbSet<PatDesCaseType> PatDesCaseTypes { get; set; }
        public DbSet<PatInventor> PatInventors { get; set; }
        public DbSet<PatIREmployeePosition> PatIREmployeePositions { get; set; }
        public DbSet<PatIRTurnOver> PatIRTurnOvers { get; set; }
        public DbSet<PatIRStaggering> PatIRStaggerings { get; set; }
        public DbSet<PatIRStaggeringDetail> PatIRStaggeringDetails { get; set; }
        public DbSet<PatIREuroExchangeRate> PatIREuroExchangeRates { get; set; }
        public DbSet<PatIREuroExchangeRateYearly> PatIREuroExchangeRateYearlys { get; set; }
        public DbSet<PatIRProductSale> PatIRProductSales { get; set; }
        public DbSet<PatIRDistribution> PatIRDistributions { get; set; }
        public DbSet<PatIRValorizationRule> PatIRValorizationRules { get; set; }
        public DbSet<PatIRRemuneration> PatIRRemunerations { get; set; }
        public DbSet<PatIRRemunerationType> PatIRRemunerationTypes { get; set; }
        public DbSet<PatIRRemunerationFormula> PatIRRemunerationFormulas { get; set; }
        public DbSet<PatIRRemunerationFormulaFactor> PatIRRemunerationFormulaFactors { get; set; }
        public DbSet<PatIRRemunerationValuationMatrixType> PatIRRemunerationValuationMatrixTypes { get; set; }
        public DbSet<PatIRRemunerationValuationMatrix> PatIRRemunerationValuationMatrixes { get; set; }
        public DbSet<PatIRRemunerationValuationMatrixCriteria> PatIRRemunerationValuationMatrixCriterias { get; set; }
        public DbSet<PatIRRemunerationValuationMatrixData> PatIRRemunerationValuationData { get; set; }
        public DbSet<PatIRFREmployeePosition> PatIRFREmployeePositions { get; set; }
        public DbSet<PatIRFRTurnOver> PatIRFRTurnOvers { get; set; }
        public DbSet<PatIRFRStaggering> PatIRFRStaggerings { get; set; }
        public DbSet<PatIRFRStaggeringDetail> PatIRFRStaggeringDetails { get; set; }
        public DbSet<PatIRFRProductSale> PatIRFRProductSales { get; set; }
        public DbSet<PatIRFRDistribution> PatIRFRDistributions { get; set; }
        public DbSet<PatIRFRValorizationRule> PatIRFRValorizationRules { get; set; }
        public DbSet<PatIRFRRemuneration> PatIRFRRemunerations { get; set; }
        public DbSet<PatIRFRRemunerationType> PatIRFRRemunerationTypes { get; set; }
        public DbSet<PatIRFRRemunerationFormula> PatIRFRRemunerationFormulas { get; set; }
        public DbSet<PatIRFRRemunerationFormulaFactor> PatIRFRRemunerationFormulaFactors { get; set; }
        public DbSet<PatIRFRRemunerationValuationMatrixType> PatIRFRRemunerationValuationMatrixTypes { get; set; }
        public DbSet<PatIRFRRemunerationValuationMatrix> PatIRFRRemunerationValuationMatrixes { get; set; }
        public DbSet<PatIRFRRemunerationValuationMatrixCriteria> PatIRFRRemunerationValuationMatrixCriterias { get; set; }
        public DbSet<PatIRFRRemunerationValuationMatrixData> PatIRFRRemunerationValuationData { get; set; }

        //public DbSet<PatImageInv> PatImageInvs { get; set; }
        public DbSet<PatOwnerInv> PatOwnerInvs { get; set; }
        public DbSet<PatOwnerApp> PatOwnerApps { get; set; }
        public DbSet<PatActionDue> PatActionDues { get; set; }
        public DbSet<PatDueDate> PatDueDates { get; set; }
        public DbSet<PatActionDueInv> PatActionDueInvs { get; set; }
        public DbSet<PatDueDateInv> PatDueDateInvs { get; set; }
        public DbSet<PatDueDateDelegation> PatDueDateDelegations { get; set; }
        public DbSet<PatDueDateInvDelegation> PatDueDateInvDelegations { get; set; }
        public DbSet<PatCostTrack> PatCostTracks { get; set; }
        public DbSet<PatCostTrackInv> PatCostTrackInvs { get; set; }
        public DbSet<PatCostType> PatCostTypes { get; set; }
        //public DbSet<PatImageApp> PatImageApps { get; set; }
        //public DbSet<PatImageAct> PatImageActs { get; set; }
        public DbSet<PatTerminalDisclaimer> PatTerminalDisclaimers { get; set; }
        public DbSet<PatIDSRelatedCase> PatIDSRelatedCases { get; set; }
        public DbSet<PatIDSRelatedCasesInfo> PatIDSRelatedCasesInfos { get; set; }
        public DbSet<PatIDSNonPatLiterature> PatIDSNonPatLiteratures { get; set; }
        public DbSet<PatRelatedCase> PatRelatedCases { get; set; }
        //public DbSet<PatImageCost> PatImageCosts { get; set; }
        public DbSet<PatTaxBase> PatTaxBases { get; set; }
        public DbSet<PatTaxYear> PatTaxYears { get; set; }
        public DbSet<PatIDSReferenceSource> PatIDSReferenceSources { get; set; }
        public DbSet<PatInventorAwardCriteria> PatInventorAwardCriterias { get; set; }
        public DbSet<PatInventorAppAward> PatInventorAppAwards { get; set; }
        public DbSet<PatInventorDMSAward> PatInventorDMSAwards { get; set; }
        public DbSet<PatInventorAwardType> PatInventorAwardTypes { get; set; }
        public DbSet<PatBudgetManagement> PatBudgetManagements { get; set; }
        public DbSet<PatIDSManageDTO> PatIDSManageDTO { get; set; }
        public DbSet<PatWorkflow> PatWorkflows { get; set; }
        public DbSet<PatWorkflowAction> PatWorkflowActions { get; set; }
        public DbSet<PatWorkflowActionParameter> PatWorkflowActionParameters { get; set; }
        public DbSet<LookupDescDTO> PatActionTypeDTO { get; set; }
        public DbSet<PatScoreCategory> PatScoreCategories { get; set; }
        public DbSet<PatScore> PatScores { get; set; }
        public DbSet<PatScoreDTO> PatScoreDTO { get; set; }
        public DbSet<PatAverageScoreDTO> PatAverageScoreDTO { get; set; }
        public DbSet<PatParentCaseTDDTO> PatParentCaseTDDTO { get; set; }
        public DbSet<InventionRelatedInvention> InventionRelatedInventions { get; set; }
        public DbSet<PatTerminalDisclaimerChildDTO> PatTerminalDisclaimerChildDTO { get; set; }

        //Pat Cost Tracking Import
        public DbSet<PatCostTrackingImportHistory> PatCostTrackingImportsHistory { get; set; }
        public DbSet<PatCostTrackingImportMapping> PatCostTrackingImportMappings { get; set; }
        public DbSet<PatCostTrackingImportError> PatCostTrackingImportErrors { get; set; }
        public DbSet<PatCostTrackingImportTypeColumn> PatCostTrackingImportTypeColumns { get; set; }

        public DbSet<InventionCopySetting> InventionCopySettings { get; set; }
        public DbSet<CountryApplicationCopySetting> CountryApplicationCopySettings { get; set; }
        //public DbSet<CountryApplicationCopySettingChild> CountryApplicationCopySettingsChild { get; set; }

        public DbSet<PatCountryLawUpdate> PatCountryLawUpdate { get; set; }
        public DbSet<PatSearchField> PatSearchFields { get; set; }
        public DbSet<PatSearchNotify> PatSearchNotifies { get; set; }
        public DbSet<PatSearchNotifyLog> PatSearchNotifyLogs { get; set; }
        public DbSet<PatSearchDTO> PatSearchDTO { get; set; }
        public DbSet<PatSearchExportDTO> PatSearchExportDTO { get; set; }
        public DbSet<PatSearchEmailDTO> PatSearchEmailDTO { get; set; }

        //Cost Estimator
        public DbSet<PatCEAnnuitySetup> PatCEAnnuitySetups { get; set; }
        public DbSet<PatCEAnnuityCost> PatCEAnnuityCosts { get; set; }
        public DbSet<PatCECountrySetup> PatCECountrySetups { get; set; }
        public DbSet<PatCECountryCost> PatCECountryCosts { get; set; }
        public DbSet<PatCECountryCostChild> PatCECountryCostChilds { get; set; }
        public DbSet<PatCECountryCostSub> PatCECountryCostSubs { get; set; }
        public DbSet<PatCEGeneralSetup> PatCEGeneralSetups { get; set; }
        public DbSet<PatCEGeneralCost> PatCEGeneralCosts { get; set; }
        public DbSet<PatCEFee> PatCEFees { get; set; }
        public DbSet<PatCEFeeDetail> PatCEFeeDetails { get; set; }
        public DbSet<PatCEStage> PatCEStages { get; set; }
        public DbSet<PatCostEstimator> PatCostEstimators { get; set; }
        public DbSet<PatCostEstimatorCountry> PatCostEstimatorCountries { get; set; }
        public DbSet<PatCostEstimatorCountryCost> PatCostEstimatorCountryCosts { get; set; }
        public DbSet<PatCEQuestionGeneral> PatCEQuestionGenerals { get; set; }
        public DbSet<PatCostEstimatorCost> PatCostEstimatorCosts { get; set; }
        public DbSet<PatCostEstimatorCostChild> PatCostEstimatorCostChilds { get; set; }
        public DbSet<PatCostEstimatorCostSub> PatCostEstimatorCostSubs { get; set; }

        public DbSet<PatEGrantDownloaded> PatEGrantDownloaded { get; set; }
        public DbSet<PatTerminalDisclaimerChecked> PatTerminalDisclaimerCheckeds { get; set; }

        //MyEPO API
        public DbSet<EPOPortfolio> EPOPortfolios { get; set; }
        public DbSet<EPOApplication> EPOApplications { get; set; }
        public DbSet<EPODueDate> EPODueDates { get; set; }
        
        public DbSet<EPOCommunication> EPOCommunications { get; set; }
        public DbSet<EPOCommunicationDoc> EPOCommunicationDocs { get; set; }

        public DbSet<PatEPODocumentCombined> PatEPODocumentCombineds { get; set; }
        public DbSet<PatEPOMailLog> PatEPOMailLogs { get; set; }

        public DbSet<PatEPODocumentMerge> PatEPODocumentMerges { get; set; }
        public DbSet<PatEPODocumentMergeGuide> PatEPODocumentMergeGuides { get; set; }
        public DbSet<PatEPODocumentMergeGuideSub> PatEPODocumentMergeGuideSubs { get; set; }

        public DbSet<PatEPODocumentMap> PatEPODocumentMaps { get; set; }
        public DbSet<PatEPODocumentMapAct> PatEPODocumentMapActs { get; set; }
        public DbSet<PatEPODocumentMapTag> PatEPODocumentMapTags { get; set; }

        public DbSet<EPODueDateTerm> EPODueDateTerms { get; set; }
        public DbSet<PatEPOActionMapAct> PatEPOActionMapActs { get; set; }

        public DbSet<PatEPOAppLog> PatEPOAppLogs { get; set; }
        public DbSet<PatEPOCommActLog> PatEPOCommActLogs { get; set; }
        public DbSet<PatEPODDActLog> PatEPODDActLogs { get; set; }

        public DbSet<LookupDTO> EPODocuments { get; set; }

        //EPO OPS API
        public DbSet<PatOPSLog> PatOPSLogs { get; set; }
        #endregion

        #region Patent Clearance Search

        public DbSet<PacClearance> PacClearances { get; set; }
        public DbSet<PacClearanceStatus> PacClearanceStatuses { get; set; }
        public DbSet<PacClearanceStatusHistory> PacClearanceStatusesHistory { get; set; }

        public DbSet<PacKeyword> PacKeywords { get; set; }

        public DbSet<PacQuestion> PacQuestions { get; set; }
        public DbSet<PacQuestionGroup> PacQuestionGroups { get; set; }
        public DbSet<PacQuestionGuide> PacQuestionGuides { get; set; }
        public DbSet<PacQuestionGuideChild> PacQuestionGuideChildren { get; set; }

        //public DbSet<PacImage> PacImages { get; set; }

        public DbSet<PacWorkflow> PacWorkflows { get; set; }
        public DbSet<PacWorkflowAction> PacWorkflowActions { get; set; }

        public DbSet<PacClearanceCopySetting> PacClearanceCopySettings { get; set; }
        public DbSet<PacClearanceCopyDisclosureSetting> PacClearanceCopyDisclosureSettings { get; set; }

        public DbSet<PacDiscussion> PacDiscussions { get; set; }
        public DbSet<PacDiscussionReply> PacDiscussionReplies { get; set; }

        public DbSet<PacInventor> PacInventors { get; set; }

        #endregion

        #region AMS
        public DbSet<AMSMain> AMSMain { get; set; }
        //public DbSet<AMSDue> AMSDue { get; set; }
        //public DbSet<AMSProjection> AMSProjection { get; set; }
        //public DbSet<AMSAbstract> AMSAbstracts { get; set; }
        //public DbSet<AMSInstrxType> AMSInstrxTypes { get; set; }
        //public DbSet<AMSStatusType> AMSStatusTypes { get; set; }
        #endregion AMS

        #region RMS
        public DbSet<RMSReminderSetup> RMSReminderSetup { get; set; }
        public DbSet<RMSInstrxTypeAction> RMSInstrxTypeAction { get; set; }
        #endregion

        #region RMS
        public DbSet<FFReminderSetup> FFReminderSetup { get; set; }
        public DbSet<FFInstrxTypeAction> FFInstrxTypeAction { get; set; }
        #endregion

        #region DMS
        // main
        public DbSet<Disclosure> Disclosures { get; set; }
        public DbSet<DisclosureCopySetting> DisclosureCopySettings { get; set; }
        public DbSet<DisclosureCopyClearanceSetting> DisclosureCopyClearanceSettings { get; set; }

        public DbSet<DMSActionDue> DMSActionDues { get; set; }
        public DbSet<DMSDueDate> DMSDueDates { get; set; }
        public DbSet<DMSDueDateDelegation> DMSDueDateDelegations { get; set; }
        public DbSet<DMSDueDateDateTakenLog> DMSDueDateDateTakenLogs { get; set; }
        public DbSet<DMSActionReminderLog> DMSActionReminderLogs { get; set; }
        public DbSet<DMSActionReminderEmailDTO> DMSActionReminderEmailDTOs { get; set; }

        public DbSet<DMSInventor> DMSInventors { get; set; }
        public DbSet<DMSInventorHistory> DMSInventorHistory { get; set; }


        public DbSet<DMSAbstract> DMSAbstracts { get; set; }
        public DbSet<DMSKeyword> DMSKeywords { get; set; }
        //public DbSet<DMSImage> DMSImages { get; set; }
        //public DbSet<DMSImageAct> DMSImageActs { get; set; }
        public DbSet<DMSDiscussion> DMSDiscussions { get; set; }
        public DbSet<DMSDiscussionReply> DMSDiscussionReplies { get; set; }

        public DbSet<DMSReview> DMSReview { get; set; }
        public DbSet<DMSPreview> DMSPreview { get; set; }
        public DbSet<DMSEntityReviewer> DMSEntityReviewer { get; set; }

        public DbSet<DMSValuation> DMSValuation { get; set; }

        public DbSet<DMSDisclosureStatusHistory> DMSDisclosureStatusesHistory { get; set; }
        public DbSet<DMSRecommendationHistory> DMSRecommendationsHistory { get; set; }
        public DbSet<DisclosureRelatedDisclosure> DisclosureRelatedDisclosures { get; set; }
        public DbSet<DMSQuestion> DMSQuestions { get; set; }
        public DbSet<DMSCombined> DMSCombineds { get; set; }

        // workflow
        public DbSet<DMSWorkflow> DMSWorkflows { get; set; }
        public DbSet<DMSWorkflowAction> DMSWorkflowActions { get; set; }

        // valuation matrix
        public DbSet<DMSValuationMatrix> DMSValuationMatrices { get; set; }
        public DbSet<DMSValuationMatrixRate> DMSValuationMatrixRates { get; set; }

        // dms aux
        public DbSet<DMSActionType> DMSActionTypes { get; set; }
        public DbSet<DMSRating> DMSRatings { get; set; }
        public DbSet<DMSDisclosureStatus> DMSDisclosureStatuses { get; set; }
        public DbSet<DMSIndicator> DMSIndicators { get; set; }
        public DbSet<DMSRecommendation> DMSRecommendations { get; set; }
        public DbSet<DMSQuestionGroup> DMSQuestionGroups { get; set; }
        public DbSet<DMSQuestionGuide> DMSQuestionGuides { get; set; }
        public DbSet<DMSQuestionGuideChild> DMSQuestionGuideChildren { get; set; }
        public DbSet<DMSQuestionGuideSub> DMSQuestionGuideSubs { get; set; }
        public DbSet<DMSQuestionGuideSubDtl> DMSQuestionGuideSubDtls { get; set; }

        public DbSet<DMSAverageRatingDTO> DMSAverageRatingDTO { get; set; }

        // agenda meeting
        public DbSet<DMSAgenda> DMSAgendas { get; set; }
        public DbSet<DMSAgendaReviewer> DMSAgendaReviewers { get; set; }
        public DbSet<DMSAgendaRelatedDisclosure> DMSAgendaRelatedDisclosures { get; set; }

        public DbSet<DMSFaqDoc> DMSFaqDocs { get; set; }
        #endregion DMS

        #region RTS
        public DbSet<RTSInfoSettingsMenu> RTSInfoSettingsMenu { get; set; }
        public DbSet<RTSInfoSettingsMenuCountry> RTSInfoCountrySettings { get; set; }
        public DbSet<RTSSearchBiblioDTO> RTSSearchBiblioDTO { get; set; }
        public DbSet<RTSSearchInventorDTO> RTSSearchInventorDTO { get; set; }
        public DbSet<RTSSearchApplicantDTO> RTSSearchApplicantDTO { get; set; }
        public DbSet<RTSSearchIPClassDTO> RTSSearchIPClassDTO { get; set; }
        public DbSet<RTSSearchBiblioUSDTO> RTSSearchBiblioUSDTO { get; set; }
        public DbSet<RTSSearchAssignmentDTO> RTSSearchAssignmentDTO { get; set; }
        public DbSet<RTSSearchPriorityDTO> RTSSearchPriorityDTO { get; set; }
        public DbSet<RTSSearchAbstractDTO> RTSSearchAbstractDTO { get; set; }
        public DbSet<RTSSearchDocCitedDTO> RTSSearchDocCitedDTO { get; set; }
        public DbSet<RTSSearchDocRefByDTO> RTSSearchDocRefByDTO { get; set; }
        public DbSet<RTSSearchPTADTO> RTSSearchPTADTO { get; set; }
        public DbSet<RTSSearchContinuityParentDTO> RTSSearchContinuityParentDTO { get; set; }
        public DbSet<RTSSearchContinuityChildDTO> RTSSearchContinuityChildDTO { get; set; }
        public DbSet<RTSSearchIFWDTO> RTSSearchIFWDTO { get; set; }
        public DbSet<RTSSearchUSCorrespondenceDTO> RTSSearchUSCorrespondenceDTO { get; set; }
        public DbSet<RTSSearchAgentDTO> RTSSearchAgentDTO { get; set; }
        public DbSet<RTSSearchPFSDocDTO> RTSSearchPFSDocDTO { get; set; }
        public DbSet<RTSSearchLSDDTO> RTSSearchLSDDTO { get; set; }
        public DbSet<RTSSearchIPCDTO> RTSSearchIPCDTO { get; set; }
        public DbSet<RTSSearchCPCDTO> RTSSearchCPCDTO { get; set; }
        public DbSet<RTSPFSTitleUpdHistoryDTO> RTSPFSTitleUpdHistoryDTO { get; set; }
        public DbSet<RTSPFSAbstractUpdHistoryDTO> RTSPFSAbstractUpdHistoryDTO { get; set; }
        public DbSet<RTSPFSCountryAppUpdHistoryDTO> RTSPFSCountryAppUpdHistoryDTO { get; set; }
        public DbSet<RTSSearchActionUpdHistoryDTO> RTSSearchActionUpdHistoryDTO { get; set; }
        public DbSet<UpdateHistoryBatchDTO> RTSSearchActionUpdHistoryBatchDTO { get; set; }
        public DbSet<RTSSearchActionAsDownloadedDTO> RTSSearchActionAsDownloadedDTO { get; set; }
        public DbSet<RTSSearchActionClosedUpdHistoryDTO> RTSSearchActionClosedUpdHistoryDTO { get; set; }
        public DbSet<RTSSearchDesCountryDTO> RTSSearchDesCountryDTO { get; set; }
        public DbSet<RTSSearchTitleDTO> RTSSearchTitleDTO { get; set; }
        public DbSet<RTSPFSStatisticsSearchOutput> RTSPFSStatisticsSearchOutput { get; set; }
        public DbSet<RTSSearch> RTSSearchRecords { get; set; }
        public DbSet<RTSSearchAction> RTSSearchActions { get; set; }
        public DbSet<RTSSearchUSIFW> RTSSearchUSIFWs { get; set; }
        public DbSet<RTSMapActionDue> RTSMapActionDues { get; set; }
        public DbSet<RTSMapActionDueSource> RTSMapActionDueSources { get; set; }
        public DbSet<RTSMapActionClose> RTSMapActionsClose { get; set; }
        public DbSet<LSDText> lSDTexts { get; set; }
        //public DbSet<RTSPatentWatch> PatentWatchList { get; set; }
        public DbSet<PDTSentLog> PDTSentLogs { get; set; }
        public DbSet<RTSPFSWorkflowBatch> RTSPFSWorkflowBatches { get; set; }
        public DbSet<RTSPFSWorkflowApp> RTSPFSWorkflowApps { get; set; }
        public DbSet<RTSBiblioUpdate> RTSBiblioUpdates { get; set; }
        public DbSet<PubNumberConverted> PubNumberConverteds { get; set; }
        public DbSet<RTSBiblioUpdateHistory> RTSBiblioUpdatesHistory { get; set; }

        public DbSet<RTSMapActionDocument> RTSMapActionDocuments { get; set; }
        public DbSet<RTSMapActionDocumentClient> RTSMapActionDocumentClients { get; set; }

        public DbSet<RTSSearchIDSCount> RTSSearchIDSCounts { get; set; }
        #endregion RTS

        #region TL
        public DbSet<TLInfoSettingsMenu> TLInfoSettingsMenu { get; set; }
        public DbSet<TLInfoSettingsMenuCountry> TLInfoCountrySettings { get; set; }
        public DbSet<TLSearch> TLSearchRecords { get; set; }
        public DbSet<TLSearchAction> TLSearchActions { get; set; }
        public DbSet<TLSearchImage> TLSearchImages { get; set; }
        public DbSet<TLSearchDocument> TLSearchDocuments { get; set; }
        public DbSet<TLSearchTTABParty> TLSearchTTABParties { get; set; }
        public DbSet<TLSearchTTAB> TLSearchTTABs { get; set; }
        public DbSet<TLMapActionDue> TLMapActionDues { get; set; }
        public DbSet<TLMapActionDueSource> TLMapActionDueSources { get; set; }
        public DbSet<TLMapActionClose> TLMapActionsClose { get; set; }
        public DbSet<TLBiblioUpdate> TLBiblioUpdates { get; set; }
        public DbSet<TLTrademarkNameUpdate> TLTrademarkNameUpdates { get; set; }
        public DbSet<TLCompareGoodsDTO> TLCompareGoodsDTO { get; set; }
        public DbSet<TLNumberFormatDTO> TLNumberFormatDTO { get; set; }
        public DbSet<TLActionComparePTO> TLActionComparePTO { get; set; }
        public DbSet<TLActionUpdateHistory> TLActionUpdatesHistory { get; set; }
        public DbSet<TLBiblioUpdateHistory> TLBiblioUpdatesHistory { get; set; }
        public DbSet<TLTmkNameUpdateHistory> TLTmkNameUpdatesHistory { get; set; }
        public DbSet<TLGoodsUpdateHistory> TLGoodsUpdatesHistory { get; set; }
        public DbSet<TLSearchBiblioDTO> TLSearchBiblioDTO { get; set; }
        public DbSet<TLSearchAssignmentDTO> TLSearchAssignmentDTO { get; set; }
        public DbSet<TLSearchGoodsDTO> TLSearchGoodsDTO { get; set; }
        public DbSet<TLSearchActionAsDownloadedDTO> TLSearchActionAsDownloadedDTO { get; set; }
        public DbSet<TLSearchDocDTO> TLSearchDocDTO { get; set; }
        public DbSet<TLSearchTTABDTO> TLSearchTTABDTO { get; set; }
        public DbSet<TLSearchImageDTO> TLSearchImageDTO { get; set; }
        public DbSet<TLMapActionDocument> TLMapActionDocuments { get; set; }
        public DbSet<TLMapActionDocumentClient> TLMapActionDocumentClients { get; set; }
        public DbSet<TLActionUpdateExclude> TLActionUpdateExcludes { get; set; }

        #endregion

        #region EFS
        public DbSet<EFS> EFS { get; set; }
        public DbSet<EFSFormDTO> EFSFormDTO { get; set; }
        public DbSet<EFSLog> EFSLogs { get; set; }
        #endregion

        #region Trademark
        //trademark
        public DbSet<TmkCountry> TmkCountries { get; set; }
        public DbSet<TmkArea> TmkAreas { get; set; }
        public DbSet<TmkAreaCountry> TmkAreasCountries { get; set; }

        public DbSet<TmkAssignmentStatus> TmkAssignmentStatuses { get; set; }
        public DbSet<TmkCaseType> TmkCaseTypes { get; set; }
        public DbSet<TmkDesCaseType> TmkDesCaseTypes { get; set; }
        public DbSet<TmkConflictStatus> TmkConflictStatuses { get; set; }
        public DbSet<TmkCostType> TmkCostTypes { get; set; }
        public DbSet<TmkCountryLaw> TmkCountryLaws { get; set; }
        public DbSet<TmkCountryDue> TmkCountryDues { get; set; }
        public DbSet<TmkActionType> TmkActionTypes { get; set; }
        public DbSet<TmkActionParameter> TmkActionParameters { get; set; }
        public DbSet<TmkIndicator> TmkIndicators { get; set; }
        public DbSet<TmkMarkType> TmkMarkTypes { get; set; }
        public DbSet<TmkStandardGood> TmkStandardGoods { get; set; }
        public DbSet<TmkTrademarkStatus> TmkTrademarkStatuses { get; set; }
        public DbSet<TmkTrademark> TmkTrademarks { get; set; }
        public DbSet<TmkTrademarkClass> TmkTrademarkClasses { get; set; }
        public DbSet<TmkOwner> TmkOwners { get; set; }

        public DbSet<TmkActionDue> TmkActionDues { get; set; }
        public DbSet<TmkDueDate> TmkDueDates { get; set; }
        public DbSet<TmkDueDateDelegation> TmkDueDateDelegations { get; set; }
        public DbSet<TmkAssignmentHistory> TmkAssignmentsHistory { get; set; }
        public DbSet<TmkCostTrack> TmkCostTracks { get; set; }
        public DbSet<TmkConflict> TmkConflicts { get; set; }
        public DbSet<TmkLicensee> TmkLicensees { get; set; }
        public DbSet<TmkKeyword> TmkKeywords { get; set; }
        //public DbSet<TmkImage> TmkImages { get; set; }
        //public DbSet<TmkImageAct> TmkImageActs { get; set; }
        //public DbSet<TmkImageCost> TmkImageCosts { get; set; }
        public DbSet<TmkDesignatedCountry> TmkDesignatedCountries { get; set; }
        public DbSet<TmkProduct> TmkProducts { get; set; }
        public DbSet<TmkBudgetManagement> TmkBudgetManagements { get; set; }
        public DbSet<TmkRelatedTrademark> TmkRelatedTrademarks { get; set; }

        //public DbSet<TmkTrademarkDTO> TmkTrademarkDTO { get; set; }             // obsolete

        //Tmk Cost Tracking Import
        public DbSet<TmkCostTrackingImportHistory> TmkCostTrackingImportHistory { get; set; }
        public DbSet<TmkCostTrackingImportMapping> TmkCostTrackingImportMappings { get; set; }
        public DbSet<TmkCostTrackingImportError> TmkCostTrackingImportErrors { get; set; }
        public DbSet<TmkCostTrackingImportTypeColumn> TmkCostTrackingImportTypeColumns { get; set; }

        public DbSet<TmkTrademarkCopySetting> TrademarkCopySettings { get; set; }

        public DbSet<TmkCountryLawUpdate> TmkCountryLawUpdate { get; set; }
        public DbSet<TmkWorkflow> TmkWorkflows { get; set; }
        public DbSet<TmkWorkflowAction> TmkWorkflowActions { get; set; }
        public DbSet<TmkWorkflowActionParameter> TmkWorkflowActionParameters { get; set; }
        public DbSet<LookupDescDTO> TmkActionTypeDTO { get; set; }

        //Cost Estimator
        public DbSet<TmkCECountrySetup> TmkCECountrySetups { get; set; }
        public DbSet<TmkCECountryCost> TmkCECountryCosts { get; set; }
        public DbSet<TmkCECountryCostChild> TmkCECountryCostChilds { get; set; }
        public DbSet<TmkCECountryCostSub> TmkCECountryCostSubs { get; set; }
        public DbSet<TmkCEGeneralSetup> TmkCEGeneralSetups { get; set; }
        public DbSet<TmkCEGeneralCost> TmkCEGeneralCosts { get; set; }
        public DbSet<TmkCEFee> TmkCEFees { get; set; }
        public DbSet<TmkCEFeeDetail> TmkCEFeeDetails { get; set; }
        public DbSet<TmkCEStage> TmkCEStages { get; set; }
        public DbSet<TmkCostEstimator> TmkCostEstimators { get; set; }
        public DbSet<TmkCostEstimatorCountry> TmkCostEstimatorCountries { get; set; }
        public DbSet<TmkCostEstimatorCountryCost> TmkCostEstimatorCountryCosts { get; set; }
        public DbSet<TmkCEQuestionGeneral> TmkCEQuestionGenerals { get; set; }
        public DbSet<TmkCostEstimatorCost> TmkCostEstimatorCosts { get; set; }
        public DbSet<TmkCostEstimatorCostChild> TmkCostEstimatorCostChilds { get; set; }
        public DbSet<TmkCostEstimatorCostSub> TmkCostEstimatorCostSubs { get; set; }
        #endregion

        #region Trademark Clearance

        public DbSet<TmcClearance> TmcClearances { get; set; }
        public DbSet<TmcClearanceStatus> TmcClearanceStatuses { get; set; }
        public DbSet<TmcClearanceStatusHistory> TmcClearanceStatusesHistory { get; set; }

        public DbSet<TmcQuestion> TmcQuestions { get; set; }
        public DbSet<TmcQuestionGroup> TmcQuestionGroups { get; set; }
        public DbSet<TmcQuestionGuide> TmcQuestionGuides { get; set; }
        public DbSet<TmcQuestionGuideChild> TmcQuestionGuideChildren { get; set; }

        //public DbSet<TmcImage> TmcImages { get; set; }

        public DbSet<TmcWorkflow> TmcWorkflows { get; set; }
        public DbSet<TmcWorkflowAction> TmcWorkflowActions { get; set; }

        public DbSet<TmcKeyword> TmcKeywords { get; set; }
        public DbSet<TmcList> TmcLists { get; set; }
        public DbSet<TmcRelatedTrademark> TmcRelatedTrademarks { get; set; }
        public DbSet<TmcMark> TmcMarks { get; set; }

        public DbSet<TmcClearanceCopySetting> ClearanceCopySettings { get; set; }

        public DbSet<TmcDiscussion> TmcDiscussions { get; set; }
        public DbSet<TmcDiscussionReply> TmcDiscussionReplies { get; set; }

        #endregion

        #region General Matters
        //general matter
        public DbSet<GMMatter> GMMatters { get; set; }
        public DbSet<GMMatterAttorney> GMMatterAttorneys { get; set; }
        public DbSet<GMMatterCountry> GMMatterCountries { get; set; }
        public DbSet<GMMatterOtherParty> GMMatterOtherParties { get; set; }
        public DbSet<GMCountry> GMCountries { get; set; }
        public DbSet<GMArea> GMAreas { get; set; }
        public DbSet<GMAreaCountry> GMAreaCountries { get; set; }
        public DbSet<GMCostTrack> GMCostTracks { get; set; }
        public DbSet<GMActionDue> GMActionsDue { get; set; }
        public DbSet<GMDueDate> GMDueDates { get; set; }
        public DbSet<GMDueDateDelegation> GMDueDateDelegations { get; set; }

        //public DbSet<GMMatterImage> GMMatterImages { get; set; }
        //public DbSet<GMMatterImageAct> GMMatterActImages { get; set; }
        //public DbSet<GMMatterImageCost> GMMatterImageCosts { get; set; }
        public DbSet<GMMatterPatent> GMMatterPatents { get; set; }
        public DbSet<GMMatterTrademark> GMMatterTrademarks { get; set; }
        public DbSet<GMMatterKeyword> GMMatterKeywords { get; set; }

        public DbSet<GMMatterType> GMMatterTypes { get; set; }
        public DbSet<GMMatterStatus> GMMatterStatuses { get; set; }
        public DbSet<GMAgreementType> GMAgreementTypes { get; set; }
        public DbSet<GMExtent> GMExtents { get; set; }
        public DbSet<GMOtherPartyType> GMOtherPartyTypes { get; set; }
        public DbSet<GMOtherParty> GMOtherParties { get; set; }
        public DbSet<GMIndicator> GMIndicators { get; set; }
        public DbSet<GMCostType> GMCostTypes { get; set; }
        public DbSet<GMActionType> GMActionTypes { get; set; }
        public DbSet<GMActionParameter> GMActionParameters { get; set; }

        public DbSet<GMMatterOtherPartyPatent> GMMatterOtherPartyPatent { get; set; }
        public DbSet<GMMatterOtherPartyTrademark> GMMatterOtherPartyTrademark { get; set; }
        public DbSet<GMMatterRelatedMatter> GMMatterRelatedMatter { get; set; }

        public DbSet<GMMatterCopySetting> GMMatterCopySettings { get; set; }
        public DbSet<GMWorkflow> GMWorkflows { get; set; }
        public DbSet<GMWorkflowAction> GMWorkflowActions { get; set; }
        public DbSet<GMWorkflowActionParameter> GMWorkflowActionParameters { get; set; }
        public DbSet<LookupDTO> GMActionTypeDTO { get; set; }

        public DbSet<GMProduct> GMProducts { get; set; }

        public DbSet<GMBudgetManagement> GMBudgetManagements { get; set; }

        //Cost Tracking Import
        public DbSet<GMCostTrackingImportHistory> GMCostTrackingImportHistory { get; set; }
        public DbSet<GMCostTrackingImportMapping> GMCostTrackingImportMappings { get; set; }
        public DbSet<GMCostTrackingImportError> GMCostTrackingImportErrors { get; set; }
        public DbSet<GMCostTrackingImportTypeColumn> GMCostTrackingImportTypeColumns { get; set; }
        #endregion

        #region Images
        public DbSet<ImageType> ImageTypes { get; set; }
        //public DbSet<FileHandler> FileHandler { get; set; }
        #endregion

        #region Quick Docket
        public DbSet<QuickDocketDTO> QuickDocketDTO { get; set; }
        public DbSet<QuickDocketSchedulerDTO> QuickDocketSchedulerDTO { get; set; }
        public DbSet<QuickDocketLookupDTO> QuickDocketLookupDTO { get; set; }
        public DbSet<QDActionTypeLookupDTO> QDActionTypeLookupDTO { get; set; }
        public DbSet<QDActionDueLookupDTO> QDActionDueLookupDTO { get; set; }
        public DbSet<QDCaseNumberLookupDTO> QDCaseNumberLookupDTO { get; set; }
        public DbSet<QDCaseTypeLookupDTO> QDCaseTypeLookupDTO { get; set; }
        public DbSet<QDRespOfficeLookupDTO> QDRespOfficeLookupDTO { get; set; }
        public DbSet<QDClientRefLookupDTO> QDClientRefLookupDTO { get; set; }
        public DbSet<QDDeDocketInstructionLookupDTO> QDDeDocketInstructionLookupDTO { get; set; }
        public DbSet<QDDeDocketInstructedByLookupDTO> QDDeDocketInstructedByLookupDTO { get; set; }
        public DbSet<QDStatusLookupDTO> QDStatusLookupDTO { get; set; }
        public DbSet<QDTitleLookupDTO> QDTitleLookupDTO { get; set; }
        public DbSet<QDIndicatorLookupDTO> QDIndicatorLookupDTO { get; set; }
        public DbSet<QDCountryLookupDTO> QDCountryLookupDTO { get; set; }

        public DbSet<QDClientLookupDTO> QDClientLookupDTO { get; set; }
        public DbSet<QDAgentLookupDTO> QDAgentLookupDTO { get; set; }
        public DbSet<QDOwnerLookupDTO> QDOwnerLookupDTO { get; set; }
        public DbSet<QDAttorneyLookupDTO> QDAttorneyLookupDTO { get; set; }

        public DbSet<PatDueDateDeDocket> PatDueDateDeDockets { get; set; }
        public DbSet<PatDueDateInvDeDocket> PatDueDateInvDeDockets { get; set; }
        public DbSet<TmkDueDateDeDocket> TmkDueDateDeDockets { get; set; }
        public DbSet<GMDueDateDeDocket> GMDueDateDeDockets { get; set; }

        public DbSet<PatDueDateExtension> PatDueDateExtensions { get; set; }
        public DbSet<PatDueDateInvExtension> PatDueDateInvExtensions { get; set; }
        public DbSet<TmkDueDateExtension> TmkDueDateExtensions { get; set; }
        public DbSet<GMDueDateExtension> GMDueDateExtensions { get; set; }
        public DbSet<DMSDueDateExtension> DMSDueDateExtensions { get; set; }
        public DbSet<DueDateExtensionLog> DueDateExtensionsLog { get; set; }

        public DbSet<PatDueDateDeDocketResp> PatDueDateDeDocketResps { get; set; }        
        public DbSet<TmkDueDateDeDocketResp> TmkDueDateDeDocketResps { get; set; }
        public DbSet<GMDueDateDeDocketResp> GMDueDateDeDocketResps { get; set; }

        public DbSet<PatDocketRequest> PatDocketRequests { get; set; }
        public DbSet<TmkDocketRequest> TmkDocketRequests { get; set; }
        public DbSet<GMDocketRequest> GMDocketRequests { get; set; }
        public DbSet<PatDocketInvRequest> PatDocketInvRequests { get; set; }

        public DbSet<PatDocketRequestResp> PatDocketRequestResps { get; set; }
        public DbSet<TmkDocketRequestResp> TmkDocketRequestResps { get; set; }
        public DbSet<GMDocketRequestResp> GMDocketRequestResps { get; set; }


        public DbSet<PatDueDateDateTakenLog> PatDueDateDateTakenLogs { get; set; }
        public DbSet<PatDueDateInvDateTakenLog> PatDueDateDateInvTakenLogs { get; set; }
        public DbSet<TmkDueDateDateTakenLog> TmkDueDateDateTakenLogs { get; set; }
        public DbSet<GMDueDateDateTakenLog> GMDueDateDateTakenLogs { get; set; }

        #endregion

        #region Quick Email

        public DbSet<QEMain> QEMains { get; set; }
        public DbSet<QELayout> QELayouts { get; set; }
        public DbSet<QERecipient> QERecipients { get; set; }
        public DbSet<QEDataSource> QEDataSources { get; set; }
        public DbSet<QEDataSourceScreen> QEDataSourceScreens { get; set; }
        public DbSet<QERoleSource> QERoleSources { get; set; }
        public DbSet<QELog> QELogs { get; set; }
        public DbSet<QEDetailView> QEDetailView { get; set; }
        public DbSet<QERecipientView> QERecipientsView { get; set; }
        public DbSet<QEColumnDTO> QEColumnDTO { get; set; }
        public DbSet<QEPatInventionView> QEPatInventionView { get; set; }
        public DbSet<QEPatCountryApplicationView> QEPatCountryApplicationView { get; set; }
        public DbSet<QEPatInventorAppAwardView> QEPatInventorAppAwardView { get; set; }
        public DbSet<QEPatInventorDMSAwardView> QEPatInventorDMSAwardView { get; set; }
        public DbSet<QEPatIRLumpSumAwardView> QEPatIRLumpSumAwardView { get; set; }
        public DbSet<QEPatIRYearlyAwardView> QEPatIRYearlyAwardView { get; set; }
        public DbSet<QEPatIRDistributionAwardView> QEPatIRDistributionAwardView { get; set; }
        public DbSet<QEPatIRFRRemunerationAwardView> QEPatIRFRRemunerationAwardView { get; set; }
        public DbSet<QEPatCostTrackingView> QEPatCostTrackingView { get; set; }
        public DbSet<QEPatCostTrackingInvView> QEPatCostTrackingInvView { get; set; }
        public DbSet<QEPatActionDueView> QEPatActionDueView { get; set; }
        public DbSet<QEPatActionDueInvView> QEPatActionDueInvView { get; set; }
        public DbSet<QEPatActionDueDateView> QEPatActionDueDateView { get; set; }
        public DbSet<QEPatActionDueDateInvView> QEPatActionDueDateInvView { get; set; }
        public DbSet<QEPatActionDueDateDedocketView> QEPatActionDueDateDedocketView { get; set; }
        public DbSet<QEPatActionDueDateInvDedocketView> QEPatActionDueDateInvDedocketView { get; set; }
        public DbSet<QEPatActionDueDateDelegationView> QEPatActionDueDateDelegationView { get; set; }
        public DbSet<QEPatActionDueDateInvDelegationView> QEPatActionDueDateInvDelegationView { get; set; }
        public DbSet<QEPatSearchView> QEPatSearchView { get; set; }
        public DbSet<QEPatCountryApplicationImageView> QEPatCountryApplicationImageView { get; set; }
        public DbSet<QEPatActionImageView> QEPatActionImageView { get; set; }
        public DbSet<QEPatActionInvImageView> QEPatActionInvImageView { get; set; }
        public DbSet<QEPatActionDueDeletedView> QEPatActionDueDeletedView { get; set; }
        public DbSet<QEPatActionDueInvDeletedView> QEPatActionDueInvDeletedView { get; set; }
        public DbSet<QEPatCountryAppDeletedView> QEPatCountryAppDeletedView { get; set; }
        public DbSet<QEPatInventionAttyChangedView> QEPatInventionAttyChangedView { get; set; }
     
        public DbSet<QEPatRequestDocketView> QEPatRequestDocketView { get; set; }
        public DbSet<QETmkRequestDocketView> QETmkRequestDocketView { get; set; }
        public DbSet<QEGmRequestDocketView> QEGmRequestDocketView { get; set; }

        public DbSet<QEGmActionDueDateView> QEGmActionDueDateView { get; set; }
        public DbSet<QEGmActionDueDateDedocketView> QEGmActionDueDateDedocketView { get; set; }
        public DbSet<QEGmActionDueDateDelegationView> QEGmActionDueDateDelegationView { get; set; }
        public DbSet<QEDmsActionDueDateView> QEDmsActionDueDateView { get; set; }
        public DbSet<QETmkTrademarkView> QETmkTrademarkView { get; set; }
        public DbSet<QETmkCostTrackingView> QETmkCostTrackingView { get; set; }
        public DbSet<QETmkActionDueView> QETmkActionDueView { get; set; }
        public DbSet<QETmkConflictView> QETmkConflictView { get; set; }
        public DbSet<QETmkActionImageView> QETmkActionImageView { get; set; }
        public DbSet<QETmkImageView> QETmkImageView { get; set; }
        public DbSet<QETmkActionDueDeletedView> QETmkActionDueDeletedView { get; set; }
        public DbSet<QETmkTrademarkDeletedView> QETmkTrademarkDeletedView { get; set; }
        public DbSet<QETmkActionDueDateView> QETmkActionDueDateView { get; set; }
        public DbSet<QETmkActionDueDateDedocketView> QETmkActionDueDateDedocketView { get; set; }
        public DbSet<QETmkActionDueDateDelegationView> QETmkActionDueDateDelegationView { get; set; }
        public DbSet<QETmkTrademarkAttyChangedView> QETmkTrademarkAttyChangedView { get; set; }

        public DbSet<QEGmMatterView> QEGmMatterView { get; set; }
        public DbSet<QEGmCostTrackingView> QEGmCostTrackingView { get; set; }
        public DbSet<QEGmActionDueView> QEGMActionDueView { get; set; }
        public DbSet<QEGmActionImageView> QEGmActionImageView { get; set; }
        public DbSet<QEGmImageView> QEGmImageView { get; set; }
        public DbSet<QEGmActionDueDeletedView> QEGmActionDueDeletedView { get; set; }
        public DbSet<QEGmMatterDeletedView> QEGmMatterDeletedView { get; set; }

        public DbSet<QEDmsDisclosureView> QEDmsDisclosureView { get; set; }
        public DbSet<QEDmsDisclosureReviewView> QEDmsDisclosureReviewView { get; set; }
        public DbSet<QEDmsActionDueView> QEDmsActionDueView { get; set; }
        public DbSet<QEDmsActionDueDateDelegationView> QEDmsActionDueDateDelegationView { get; set; }
        public DbSet<QEDmsAgendaView> QEDmsAgendaView { get; set; }

        public DbSet<QETmcClearanceView> QETmcClearanceView { get; set; }
        public DbSet<QEPacClearanceView> QEPacClearanceView { get; set; }
        public DbSet<QERecipientRoleDTO> QERecipientRoleDTO { get; set; }

        //public DbSet<ImageEntityDTO> ImageEntityDTO { get; set; }
        public DbSet<DocEntityDTO> DocEntityDTO { get; set; }

        public DbSet<QEPatCountryApplicationDocRespDocketingView> QEPatCountryApplicationDocRespDocketingView { get; set; }        
        public DbSet<QETmkTrademarkDocRespDocketingView> QETmkTrademarkDocRespDocketingView { get; set; }        
        public DbSet<QEGmMatterDocRespDocketingView> QEGmMatterDocRespDocketingView { get; set; }   
        
        public DbSet<QEPatCountryApplicationDocRespReportingView> QEPatCountryApplicationDocRespReportingView { get; set; }        
        public DbSet<QETmkTrademarkDocRespReportingView> QETmkTrademarkDocRespReportingView { get; set; }        
        public DbSet<QEGmMatterDocRespReportingView> QEGmMatterDocRespReportingView { get; set; }

        public DbSet<QECustomField> QECustomFields { get; set; }
        public DbSet<QEFieldListDTO> QEFieldListDTO { get; set; }

        public DbSet<QEDocVerificationImageView> QEDocVerificationImageView { get; set; }
        public DbSet<QECategory> QECategories { get; set; }
        public DbSet<QETag> QETags { get; set; }

        #endregion

        #region Report
        public DbSet<ReportParameter> ReportParameters { get; set; }

        public DbSet<SharedReportActionTypeLookupDTO> SharedReportActionTypeView { get; set; }
        public DbSet<SharedReportActionDueLookupDTO> SharedReportActionDueView { get; set; }
        public DbSet<SharedReportIndicatorLookupDTO> SharedReportIndicatorView { get; set; }
        public DbSet<SharedReportCountryLookupDTO> SharedReportCountryView { get; set; }
        public DbSet<SharedReportAreaLookupDTO> SharedReportAreaView { get; set; }
        public DbSet<SharedReportClientLookupDTO> SharedReportClientView { get; set; }
        public DbSet<SharedReportOwnerLookupDTO> SharedReportOwnerView { get; set; }
        public DbSet<SharedReportAttorneyLookupDTO> SharedReportAttorneyView { get; set; }
        public DbSet<SharedReportStatusLookupDTO> SharedReportStatusView { get; set; }
        public DbSet<SharedReportCaseTypeLookupDTO> SharedReportCaseTypeView { get; set; }
        public DbSet<SharedReportResponsibleOfficeLookupDTO> SharedReportResponsibleOfficeView { get; set; }
        public DbSet<SharedReportCaseNumberLookupDTO> SharedReportCaseNumberView { get; set; }
        public DbSet<SharedReportCostTypeLookupDTO> SharedReportCostTypeView { get; set; }
        public DbSet<SharedReportAgentLookupDTO> SharedReportAgentView { get; set; }
        #endregion Report

        #region Report Scheduler
        public DbSet<RSActionType> RSActionTypes { get; set; }
        public DbSet<RSCriteriaControl> RSCriteriaControls { get; set; }
        public DbSet<RSFrequencyType> RSFrequencyTypes { get; set; }
        public DbSet<RSOrderByControl> RSOrderByControls { get; set; }
        public DbSet<RSDateTypeControl> RSDateTypeControls { get; set; }
        public DbSet<RSPrintOptionControl> RSPrintOptionControls { get; set; }
        public DbSet<RSReportType> RSReportTypes { get; set; }
        public DbSet<RSMain> RSMains { get; set; }
        public DbSet<RSHistory> RSHistorys { get; set; }
        public DbSet<RSPrintOption> RSPrintOptions { get; set; }
        public DbSet<RSPrintOptionHistory> RSPrintOptionHistorys { get; set; }
        public DbSet<RSCriteria> RSCriterias { get; set; }
        public DbSet<RSCriteriaHistory> RSCriteriaHistorys { get; set; }
        public DbSet<RSAction> RSActions { get; set; }
        public DbSet<RSActionHistory> RSActionHistorys { get; set; }

        #endregion Report Scheduler

        #region Letters
        public DbSet<LetterMain> LettersMain { get; set; }
        public DbSet<LetterCategory> LetterCategories { get; set; }
        public DbSet<LetterDataSource> LetterDataSources { get; set; }
        public DbSet<LetterRecordSource> LetterRecordSources { get; set; }
        public DbSet<LetterRecordSourceFilter> LetterRecordSourceFilters { get; set; }
        public DbSet<LetterRecordSourceFilterUser> LetterRecordSourceFiltersUser { get; set; }
        public DbSet<LetterUserData> LetterUserData { get; set; }
        public DbSet<LetterSubCategory> LetterSubCategories { get; set; }
        public DbSet<LetterTag> LetterTags { get; set; }

        public DbSet<LetterEntitySetting> LetterEntitySettings { get; set; }
        public DbSet<LookupDTO> LetterFilterLookUpDTO { get; set; }
        public DbSet<LetterFieldListDTO> LetterFieldListDTO { get; set; }
        public DbSet<LetterContactDTO> LetterContactDTO { get; set; }
        public DbSet<LetterCustomField> LetterCustomFields { get; set; }


        #endregion Letters

        #region DOCX
        public DbSet<DOCXMain> DOCXesMain { get; set; }
        public DbSet<DOCXCategory> DOCXCategories { get; set; }
        public DbSet<DOCXDataSource> DOCXDataSources { get; set; }
        public DbSet<DOCXRecordSource> DOCXRecordSources { get; set; }
        public DbSet<DOCXRecordSourceFilter> DOCXRecordSourceFilters { get; set; }
        public DbSet<DOCXRecordSourceFilterUser> DOCXRecordSourceFiltersUser { get; set; }
        public DbSet<DOCXUserData> DOCXUserData { get; set; }
        //public DbSet<DOCXEntitySetting> DOCXEntitySettings { get; set; }
        public DbSet<LookupDTO> DOCXFilterLookUpDTO { get; set; }
        public DbSet<DOCXFieldListDTO> DOCXFieldListDTO { get; set; }
        //public DbSet<DOCXContactDTO> DOCXContactDTO { get; set; }
        public DbSet<DOCXLog> DOCXLogs { get; set; }
        public DbSet<DOCXUSPTOHeader> DOCXUSPTOHeaders { get; set; }
        public DbSet<DOCXUSPTOHeaderKeyword> DOCXUSPTOHeaderKeywords { get; set; }
        public DbSet<DOCXUSPTOHeaderKeywordDTO> DOCXUSPTOHeaderKeywordDTO { get; set; }
        public DbSet<DOCXUSPTOHeaderKeywordExcelDTO> DOCXUSPTOHeaderKeywordExcelDTO { get; set; }

        #endregion

        #region Data Query
        public DbSet<DataQueryMain> DataQueriesMain { get; set; }
        public DbSet<DataQueryAllowedFunction> DataQueryAllowedFunctions { get; set; }
        public DbSet<DQMetadataDTO> DQMetadataDTO { get; set; }
        public DbSet<DQMetaRelationsDTO> DQMetaRelationsDTO { get; set; }
        public DbSet<DataQueryCategory> DataQueryCategories { get; set; }
        public DbSet<DataQueryTag> DataQueryTags { get; set; }
        #endregion

        #region Audit Trail
        public DbSet<AuditHeaderDTO> AuditHeaderDTO { get; set; }
        public DbSet<AuditKeyDTO> AuditKeyDTO { get; set; }
        public DbSet<AuditDetailDTO> AuditDetailDTO { get; set; }
        public DbSet<AuditSearchDTO> AuditSearchDTO { get; set; }
        public DbSet<LookupDTO> AuditLookupDTO { get; set; }
        public DbSet<AuditReportDTO> AuditReportDTO { get; set; }

        #endregion AuditTrail  

        #region Web Links
        public DbSet<WebLinksDTO> WebLinksDTO { get; set; }
        public DbSet<WebLinksUrlDTO> WebLinksUrlDTO { get; set; }
        public DbSet<WebLinksNumberTemplateDTO> WebLinksNumberTemplateDTO { get; set; }
        #endregion

        #region Security & System Logs
        public DbSet<CPiUser> CPiUser { get; set; }
        public DbSet<CPiUserPasswordHistory> CPiUserPasswordHistory { get; set; }
        public DbSet<CPiUserEntityFilter> CPiUserEntityFilters { get; set; }
        public DbSet<CPiUserSystemRole> CPiUserSystemRoles { get; set; }
        public DbSet<CPiRespOffice> CPiRespOffices { get; set; }
        public DbSet<CPiGroup> CPiGroups { get; set; }

        public DbSet<ErrorMapping> ErrorMappings { get; set; }
        public DbSet<Log> Logs { get; set; }

        #endregion

        #region System Tables
        public DbSet<CPiMenuItem> CPiMenuItems { get; set; }
        public DbSet<CPiMenuPage> CPiMenuPages { get; set; }

        public DbSet<CPiDefaultPage> CPiDefaultPages { get; set; }
        public DbSet<CPiSetting> CPiSettings { get; set; }
        public DbSet<CPiUserSetting> CPiUserSettings { get; set; }
        public DbSet<CPiSystemSetting> CPiSystemSettings { get; set; }
        public DbSet<CPiUserSettingLog> CPiUserSettingLog { get; set; }

        public DbSet<CPiWidget> CPiWidgets { get; set; }
        public DbSet<CPiUserWidget> CPiUserWidgets { get; set; }

        public DbSet<LocalizationRecords> LocalizationRecords { get; set; }
        public DbSet<LocalizationRecordsGrouping> LocalizationRecordsGrouping { get; set; }

        public DbSet<Option> Options { get; set; }
        public DbSet<ModuleMain> ModulesMain { get; set; }
        public DbSet<SystemScreen> SystemScreens { get; set; }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationConnection> NotificationConnections { get; set; }

        //DTOs for dashboard widgets that use stored procs
        //Use view models if not using stored procs
        public DbSet<ChartDTO> ChartDTO { get; set; }
        public DbSet<ListDTO> ListDTO { get; set; }
        public DbSet<CaseListDTO> CaseListDTO { get; set; }
        public DbSet<PatLatestPTODocumentsDTO> PatLatestPTODocumentsDTO { get; set; }

        public DbSet<SysCustomFieldSetting> SysCustomFieldSettings { get; set; }

        #endregion

        #region Family Trees
        public DbSet<FamilyTreeDTO> FamilyTreeDTO { get; set; }
        public DbSet<FamilyTreeTmkDTO> FamilyTreeTmkDTO { get; set; }
        public DbSet<FamilyTreePatDTO> FamilyTreePatDTO { get; set; }
        public DbSet<FamilyTreeParentCaseDTO> FamilyTreeParentCaseDTO { get; set; }

        #endregion

        #region Docs Out
        public DbSet<DocsOutDTO> DocsOutDTO { get; set; }
        public DbSet<LetterLog> LetterLogs { get; set; }
        public DbSet<LetterLogDetail> LetterLogDetails { get; set; }
        #endregion

        #region Global Update
        public DbSet<GlobalUpdateFields> GlobalUpdateFields { get; set; }
        public DbSet<GlobalUpdateLookupDTO> GlobalUpdateLookupDTO { get; set; }
        public DbSet<PatGlobalUpdatePreviewDTO> PatGlobalUpdatePreviewDTO { get; set; }
        public DbSet<PatGlobalUpdateLog> PatGlobalUpdateLog { get; set; }
        public DbSet<TmkGlobalUpdatePreviewDTO> TmkGlobalUpdatePreviewDTO { get; set; }
        public DbSet<TmkGlobalUpdateLog> TmkGlobalUpdateLog { get; set; }
        public DbSet<GMGlobalUpdatePreviewDTO> GMGlobalUpdatePreviewDTO { get; set; }
        public DbSet<GMGlobalUpdateLog> GMGlobalUpdateLog { get; set; }
        public DbSet<DelegationUtilityPreviewDTO> DelegationUtilityPreviewDTO { get; set; }
        public DbSet<DelegationActionTypeDTO> DelegationActionTypeDTO { get; set; }
        public DbSet<DelegationActionDueDTO> DelegationActionDueDTO { get; set; }
        public DbSet<DelegationIndicatorDTO> DelegationIndicatorDTO { get; set; }

        #endregion

        #region Data Import
        public DbSet<DataImportHistory> DataImportsHistory { get; set; }
        public DbSet<DataImportType> DataImportTypes { get; set; }
        public DbSet<DataImportMapping> DataImportMappings { get; set; }
        public DbSet<DataImportError> DataImportErrors { get; set; }
        public DbSet<DataImportTypeColumn> DataImportTypeColumns { get; set; }

        #endregion

        #region Documents
        public DbSet<DocSystem> DocSystems { get; set; }
        public DbSet<DocMatterTree> DocMatterTrees { get; set; }
        public DbSet<DocTreeDTO> DocTreeDTO { get; set; }
        public DbSet<DocTreeEmailApiDTO> DocTreeEmailApiDTO { get; set; }
        public DbSet<DocImageDetailDTO> DocImageDetailDTO { get; set; }
        public DbSet<DocLetterLogDetailDTO> DocLetterLogDetailDTO { get; set; }
        public DbSet<DocQELogDetailDTO> DocQELogDetailDTO { get; set; }
        public DbSet<DocEFSLogDetailDTO> DocEFSLogDetailDTO { get; set; }
        public DbSet<DocIDSRelCasesDTO> DocIDSRelCasesDTO { get; set; }
        public DbSet<DocIDSNonPatLitDTO> DocIDSNonPatLitDTO { get; set; }
        public DbSet<DocViewDTO> DocViewDTO { get; set; }
        public DbSet<DocInfoDTO> DocInfoDTO { get; set; }

        public DbSet<DocFolder> DocFolders { get; set; }
        public DbSet<DocDocument> DocDocuments { get; set; }
        public DbSet<DocDocumentTag> DocDocumentTags { get; set; }
        public DbSet<DocFile> DocFiles { get; set; }
        public DbSet<DocFileSignature> DocFileSignatures { get; set; }
        public DbSet<SharePointFileSignature> SharePointFileSignatures { get; set; }
        public DbSet<DocFileSignatureRecipient> DocFileSignatureRecipients { get; set; }
        public DbSet<DocIcon> DocIcons { get; set; }
        public DbSet<DocType> DocTypes { get; set; }
        public DbSet<DocFixedFolder> DocFixedFolders { get; set; }

        public DbSet<DocGmailCaseLink> DocGmailCaseLinks { get; set; }
        public DbSet<CaseLogDTO> CaseLogDTO { get; set; }

        public DbSet<DocOutlook> DocOutlook { get; set; }
        public DbSet<DocOutlookCaseLink> DocOutlookCaseLinks { get; set; }
        public DbSet<DocOutlookId> DocOutlookIds { get; set; }
        public DbSet<DocReviewDTO> DocReviewDTO { get; set; }

        public DbSet<DocVerification> DocVerifications { get; set; }
        public DbSet<DocVerificationSearchField> DocVerificationSearchFields { get; set; }
        public DbSet<DocumentVerificationNewDTO> DocumentVerificationNewDTO { get; set; }
        public DbSet<DocumentVerificationDTO> DocumentVerificationDTO { get; set; }
        public DbSet<DocumentVerificationActionDTO> DocumentVerificationActionDTO { get; set; }
        public DbSet<DocumentVerificationCommunicationDTO> DocumentVerificationCommunicationDTO { get; set; }

        public DbSet<DocResponsibleLog> DocResponsibleLogs { get; set; }
        public DbSet<DocResponsibleDocketing> DocRespDocketings { get; set; }
        public DbSet<DocResponsibleReporting> DocRespReportings { get; set; }

        public DbSet<DocQuickEmailLog> DocQuickEmailLogs { get; set; }
        #endregion

        #region Form Extract
        public DbSet<FormSystem> FormSystems { get; set; }
        public DbSet<FormSource> FormSources { get; set; }
        public DbSet<FormIFWFormType> FormIFWFormTypes { get; set; }
        public DbSet<FormIFWDocType> FormIFWDocTypes { get; set; }
        public DbSet<FormIFWDataExtract> FormIFWDataExtracts { get; set; }
        public DbSet<FormIFWFieldUsage> FormIFWFieldUsages { get; set; }
        public DbSet<FormIFWActionMap> FormIFWActionMaps { get; set; }
        public DbSet<FormIFWActMap> FormIFWActMaps { get; set; }
        public DbSet<FormIFWActMapPat> FormIFWActMapsPat { get; set; }
        public DbSet<FormIFWActMapTmk> FormIFWActMapsTmk { get; set; }

        public DbSet<FormIFWActionDueDTO> FormIFWActionDueDTO { get; set; }
        public DbSet<FormIFWActionUpdateDTO> FormIFWActionUpdateDTO { get; set; }

        public DbSet<FormIFWActionRemarksDTO> FormIFWActionRemarksDTO { get; set; }

        public DbSet<FormPLMapDTO> FormPLMapDTO { get; set; }

        #endregion

        #region Others
        public DbSet<PatParentCaseDTO> PatParentCaseDTO { get; set; }
        public DbSet<CountryApplicationDTO> CountryApplicationDTO { get; set; }
        public DbSet<PatIDSRelatedCaseDTO> PatIDSRelatedCaseDTO { get; set; }
        public DbSet<PatIDSRelatedCaseCopyDTO> PatIDSRelatedCaseCopyDTO { get; set; }
        public DbSet<PatIDSCopyFamilyDTO> PatIDSCopyFamilyDTO { get; set; }
        public DbSet<PatRelatedCaseDTO> PatRelatedCaseDTO { get; set; }
        public DbSet<ActionTabDTO> ActionTabDTO { get; set; }
        public DbSet<LookupDTO> LookupDTO { get; set; }
        public DbSet<LookupDescDTO> LookupDescDTO { get; set; }
        public DbSet<MapDTO> MapDTO { get; set; }
        public DbSet<LookupIntDTO> LookupIntDTO { get; set; }
        public DbSet<DelegationEmailDTO> DelegationEmailDTO { get; set; }
        public DbSet<DelegationDetailDTO> DelegationDetailDTO { get; set; }
        public DbSet<PatCostEstimatorBaseAppDTO> PatCostEstimatorBaseAppDTO { get; set; }
        public DbSet<MyFavorite> MyFavorites { get; set; }
        public DbSet<CEEstimatedCostDTO> CEEstimatedCostDTOs { get; set; }
        public DbSet<CECascadeCostDTO> CECascadeCostDTOs { get; set; }
        public DbSet<PatIDSSearchInputDTO> PatIDSSearchInputDTO { get; set; }
        public DbSet<SharePointToAzureBlobSyncDTO> SharePointToAzureBlobSyncDTO { get; set; }
        public DbSet<EmailNotificationDTO> EmailNotificationDTO { get; set; }
        public DbSet<PatActionMultipleBasedOnDTO> PatActionMultipleBasedOnDTO { get; set; }
        #endregion

        #region Global Search
        public DbSet<GSSystem> GSSystems { get; set; }
        public DbSet<GSScreen> GSScreens { get; set; }
        public DbSet<GSTable> GSTables { get; set; }
        public DbSet<GSField> GSFields { get; set; }
        #endregion

        #region API
        public DbSet<CountryApplicationWebSvc> CountryApplicationWebSvcs { get; set; }
        public DbSet<TmkTrademarkWebSvc> TmkTrademarkWebSvcs { get; set; }
        public DbSet<PatIDSDownloadWebSvc> PatIDSDownloadWebSvcs { get; set; }
        public DbSet<WebServiceLog> WebServiceLogs { get; set; }
        #endregion

        #region Task Scheduler
        // Add ScheduledTask to include it in ModelBuilder
        // to enable EncryptionConverter for Password property
        public DbSet<ScheduledTask> ScheduledTasks { get; set; }
        #endregion

        #region Trade Secret
        // Add TradeSecretAuditLog to include it in ModelBuilder
        // to enable EncryptionConverter for encrypted properties
        public DbSet<TradeSecretAuditLog> TradeSecretAuditLogs { get; set; }
        #endregion
        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //Column encryption
            builder.UseEncryption();

            builder.ApplyConfiguration(new DeleteLogMap());

            #region Shared Auxiliaries
            //shared
            builder.ApplyConfiguration(new AgentMap());
            builder.ApplyConfiguration(new AgentContactMap());
            builder.ApplyConfiguration(new AgentCEFeeMap());
            builder.ApplyConfiguration(new AttorneyMap());
            builder.ApplyConfiguration(new ClientMap());
            builder.ApplyConfiguration(new ClientContactMap());
            builder.ApplyConfiguration(new ClientDesignatedCountryMap());
            builder.ApplyConfiguration(new ContactPersonMap());
            builder.ApplyConfiguration(new OwnerMap());
            builder.ApplyConfiguration(new OwnerContactMap());
            builder.ApplyConfiguration(new LanguageMap());
            builder.ApplyConfiguration(new CurrencyTypeMap());
            builder.ApplyConfiguration(new ImageTypeMap());
            builder.ApplyConfiguration(new DeDocketInstructionMap());
            builder.ApplyConfiguration(new SearchCriteriaMap());
            builder.ApplyConfiguration(new SearchCriteriaDetailMap());
            builder.ApplyConfiguration(new CustomReportMap());
            builder.ApplyConfiguration(new TimeTrackerMap());
            builder.ApplyConfiguration(new TimeTrackMap());

            //Product Aux
            builder.ApplyConfiguration(new ProductMap());
            builder.ApplyConfiguration(new ProductGroupMap());
            builder.ApplyConfiguration(new ProductCategoryMap());
            builder.ApplyConfiguration(new RelatedProductMap());
            builder.ApplyConfiguration(new ProductSaleMap());
            builder.ApplyConfiguration(new BrandMap());
            builder.ApplyConfiguration(new ProductLatestTopSaleDTOMap());

            //Product Import
            builder.ApplyConfiguration(new ProductImportHistoryMap());
            builder.ApplyConfiguration(new ProductImportMappingMap());
            builder.ApplyConfiguration(new ProductImportErrorMap());
            builder.ApplyConfiguration(new ProductImportTypeColumnMap());

            builder.Entity<CPiLanguage>().ToTable("tblCPiLanguage");

            builder.ApplyConfiguration(new HelpMap());
            builder.ApplyConfiguration(new FavoriteMap());
            builder.ApplyConfiguration(new DocuSignAnchorMap());
            builder.ApplyConfiguration(new DocuSignAnchorTabMap());
            #endregion

            #region Patent
            //patent
            builder.ApplyConfiguration(new PatDisclosureStatusMap());
            builder.ApplyConfiguration(new InventionMap());
            builder.ApplyConfiguration(new PatPriorityMap());
            builder.ApplyConfiguration(new PatAbstractMap());
            builder.ApplyConfiguration(new PatKeywordMap());
            builder.ApplyConfiguration(new InventionRelatedDisclosureMap());
            builder.ApplyConfiguration(new PatCountryLawMap());
            builder.ApplyConfiguration(new PatCountryDueMap());
            builder.ApplyConfiguration(new PatCountryExpMap());
            builder.ApplyConfiguration(new PatCountryMap());
            builder.ApplyConfiguration(new PatAreaMap());
            builder.ApplyConfiguration(new PatAreaCountryMap());
            builder.ApplyConfiguration(new PatCaseTypeMap());
            builder.ApplyConfiguration(new PatCostTypeMap());
            builder.ApplyConfiguration(new PatCostTrackInvMap());
            builder.ApplyConfiguration(new PatApplicationStatusMap());
            builder.ApplyConfiguration(new PatUPCStatusMap());
            builder.ApplyConfiguration(new PatIndicatorMap());
            builder.ApplyConfiguration(new PatDesCaseTypeMap());
            builder.ApplyConfiguration(new PatAssignmentStatusMap());
            builder.ApplyConfiguration(new PatTerminalDisclaimerMap());
            builder.ApplyConfiguration(new CountryApplicationMap());
            builder.ApplyConfiguration(new PatActionDueMap());
            builder.ApplyConfiguration(new PatDueDateMap());
            builder.ApplyConfiguration(new PatDueDateDelegationMap());
            builder.ApplyConfiguration(new PatActionDueInvMap());
            builder.ApplyConfiguration(new PatDueDateInvMap());
            builder.ApplyConfiguration(new PatDueDateInvDelegationMap());
            builder.ApplyConfiguration(new PatAssignmentHistoryMap());
            builder.ApplyConfiguration(new PatLicenseeMap());
            builder.ApplyConfiguration(new PatInventorAppMap());
            builder.ApplyConfiguration(new PatDesignatedCountryMap());
            builder.ApplyConfiguration(new PatRelatedCasesMap());
            builder.ApplyConfiguration(new PatRelatedCasesDTOMap());
            builder.ApplyConfiguration(new PatIDSRelatedCasesMap());
            builder.ApplyConfiguration(new PatIDSRelatedCasesInfoMap());
            builder.ApplyConfiguration(new PatIDSRelatedCasesDTOMap());
            builder.ApplyConfiguration(new PatIDSRelatedCasesCopyDTOMap());
            builder.ApplyConfiguration(new PatIDSManageDTOMap());
            builder.ApplyConfiguration(new PatNonPatLiteratureMap());
            builder.ApplyConfiguration(new PatParentCaseDTOMap());
            builder.ApplyConfiguration(new PatParentCaseTDDTOMap());
            builder.ApplyConfiguration(new PatProductMap());
            builder.ApplyConfiguration(new PatProductInvMap());
            builder.ApplyConfiguration(new PatSubjectMatterMap());
            builder.ApplyConfiguration(new PatRelatedTrademarkMap());

            builder.ApplyConfiguration(new PatInventorMap());
            builder.ApplyConfiguration(new PatIREmployeePositionMap());
            builder.ApplyConfiguration(new PatIRTurnOverMap());
            builder.ApplyConfiguration(new PatIRStaggeringMap());
            builder.ApplyConfiguration(new PatIRStaggeringDetailMap());
            builder.ApplyConfiguration(new PatIREuroExchangeRateMap());
            builder.ApplyConfiguration(new PatIREuroExchangeRateYearlyMap());
            builder.ApplyConfiguration(new PatIRProductSaleMap());
            builder.ApplyConfiguration(new PatIRDistributionMap());
            builder.ApplyConfiguration(new PatIRValorizationRuleMap());
            builder.ApplyConfiguration(new PatIRRemunerationMap());
            builder.ApplyConfiguration(new PatIRRemunerationTypeMap());
            builder.ApplyConfiguration(new PatIRRemunerationFormulaMap());
            builder.ApplyConfiguration(new PatIRRemunerationFormulaFactorMap());
            builder.ApplyConfiguration(new PatIRRemunerationValuationMatrixTypeMap());
            builder.ApplyConfiguration(new PatIRRemunerationValuationMatrixMap());
            builder.ApplyConfiguration(new PatIRRemunerationValuationMatrixCriteriaMap());
            builder.ApplyConfiguration(new PatIRRemunerationValuationMatrixDataMap());
            builder.ApplyConfiguration(new PatIRFREmployeePositionMap());
            builder.ApplyConfiguration(new PatIRFRTurnOverMap());
            builder.ApplyConfiguration(new PatIRFRStaggeringMap());
            builder.ApplyConfiguration(new PatIRFRStaggeringDetailMap());
            builder.ApplyConfiguration(new PatIRFRProductSaleMap());
            builder.ApplyConfiguration(new PatIRFRDistributionMap());
            builder.ApplyConfiguration(new PatIRFRValorizationRuleMap());
            builder.ApplyConfiguration(new PatIRFRRemunerationMap());
            builder.ApplyConfiguration(new PatIRFRRemunerationTypeMap());
            builder.ApplyConfiguration(new PatIRFRRemunerationFormulaMap());
            builder.ApplyConfiguration(new PatIRFRRemunerationFormulaFactorMap());
            builder.ApplyConfiguration(new PatIRFRRemunerationValuationMatrixTypeMap());
            builder.ApplyConfiguration(new PatIRFRRemunerationValuationMatrixMap());
            builder.ApplyConfiguration(new PatIRFRRemunerationValuationMatrixCriteriaMap());
            builder.ApplyConfiguration(new PatIRFRRemunerationValuationMatrixDataMap());
            builder.ApplyConfiguration(new PatActionTypeMap());
            builder.ApplyConfiguration(new PatActionParameterMap());
            builder.ApplyConfiguration(new PatInventorInvMap());
            builder.ApplyConfiguration(new InventionImageMap());
            
            //builder.ApplyConfiguration(new PatImageInvMap());
            builder.ApplyConfiguration(new PatOwnerInvMap());
            builder.ApplyConfiguration(new PatOwnerAppMap());
            builder.ApplyConfiguration(new PatCostTrackMap());
            //builder.ApplyConfiguration(new PatImageAppMap());
            //builder.ApplyConfiguration(new PatImageActMap());
            //builder.ApplyConfiguration(new PatImageCostMap());

            builder.ApplyConfiguration(new PatAppImageMap());
            builder.ApplyConfiguration(new PatAppImageDefaultMap());

            builder.ApplyConfiguration(new PatTaxBaseMap());
            builder.ApplyConfiguration(new PatTaxYearMap());
            builder.ApplyConfiguration(new PatIDSReferenceSourceMap());
            builder.ApplyConfiguration(new PatDueDateDeDocketOutstandingMap());
            builder.ApplyConfiguration(new PatDueDateInvDeDocketOutstandingMap());
            builder.ApplyConfiguration(new PatInventorAwardCriteriaMap());
            builder.ApplyConfiguration(new PatInventorAppAwardMap());
            builder.ApplyConfiguration(new PatInventorDMSAwardMap());
            builder.ApplyConfiguration(new PatInventorAwardTypeMap());
            builder.ApplyConfiguration(new PatBudgetManagementMap());
            builder.ApplyConfiguration(new InventionRelatedInventionMap());

            //Patent Cost Tracking Import
            builder.ApplyConfiguration(new PatCostTrackingImportHistoryMap());
            builder.ApplyConfiguration(new PatCostTrackingImportMappingMap());
            builder.ApplyConfiguration(new PatCostTrackingImportErrorMap());
            builder.ApplyConfiguration(new PatCostTrackingImportTypeColumnMap());

            builder.ApplyConfiguration(new InventionCopySettingMap());
            builder.ApplyConfiguration(new CountryAppCopySettingMap());
            //builder.ApplyConfiguration(new CountryAppCopySettingChildMap());

            builder.ApplyConfiguration(new PatCountryLawUpdateMap());

            builder.ApplyConfiguration(new PatWorkflowMap());
            builder.ApplyConfiguration(new PatWorkflowActionMap());
            builder.ApplyConfiguration(new PatWorkflowActionParameterMap());
            builder.ApplyConfiguration(new PatSearchFieldMap());
            builder.ApplyConfiguration(new PatSearchNotifyMap());
            builder.ApplyConfiguration(new PatSearchNotifyLogMap());
            builder.ApplyConfiguration(new PatSearchDTOMap());
            builder.ApplyConfiguration(new PatSearchExportDTOMap());
            builder.ApplyConfiguration(new PatSearchEmailDTOMap());
            builder.ApplyConfiguration(new PatScoreCategoryMap());
            builder.ApplyConfiguration(new PatScoreMap());
            builder.ApplyConfiguration(new PatScoreDTOMap());
            builder.ApplyConfiguration(new PatAverageScoreDTOMap());

            //Cost Estimator
            builder.ApplyConfiguration(new PatCEAnnuitySetupMap());
            builder.ApplyConfiguration(new PatCEAnnuityCostMap());
            builder.ApplyConfiguration(new PatCECountrySetupMap());
            builder.ApplyConfiguration(new PatCECountryCostMap());
            builder.ApplyConfiguration(new PatCECountryCostChildMap());
            builder.ApplyConfiguration(new PatCECountryCostSubMap());
            builder.ApplyConfiguration(new PatCEGeneralSetupMap());
            builder.ApplyConfiguration(new PatCEGeneralCostMap());
            builder.ApplyConfiguration(new PatCEFeeMap());
            builder.ApplyConfiguration(new PatCEFeeDetailMap());
            builder.ApplyConfiguration(new PatCEStageMap());
            builder.ApplyConfiguration(new PatCostEstimatorBaseAppDTOMap());
            builder.ApplyConfiguration(new PatCostEstimatorMap());
            builder.ApplyConfiguration(new PatCostEstimatorCountryMap());
            builder.ApplyConfiguration(new PatCostEstimatorCountryCostMap());
            builder.ApplyConfiguration(new PatCEQuestionGeneralMap());
            builder.ApplyConfiguration(new PatCostEstimatorCostMap());
            builder.ApplyConfiguration(new PatCostEstimatorCostChildMap());
            builder.ApplyConfiguration(new PatCostEstimatorCostSubMap());

            builder.ApplyConfiguration(new PatEGrantDownloadedMap());
            builder.ApplyConfiguration(new PatTerminalDisclaimerCheckedMap());

            //MyEPO API
            builder.ApplyConfiguration(new EPOPortfolioMap());
            builder.ApplyConfiguration(new EPOApplicationMap());
            builder.ApplyConfiguration(new EPODueDateMap());
            
            builder.ApplyConfiguration(new EPOCommunicationMap());
            builder.ApplyConfiguration(new EPOCommunicationDocMap());
            builder.ApplyConfiguration(new PatEPODocumentCombinedMap());
            builder.ApplyConfiguration(new PatEPOMailLogMap());

            builder.ApplyConfiguration(new PatEPODocumentMergeMap());
            builder.ApplyConfiguration(new PatEPODocumentMergeGuideMap());
            builder.ApplyConfiguration(new PatEPODocumentMergeGuideSubMap());

            builder.ApplyConfiguration(new PatEPODocumentMapMap());
            builder.ApplyConfiguration(new PatEPODocumentMapActMap());
            builder.ApplyConfiguration(new PatEPODocumentMapTagMap());

            builder.ApplyConfiguration(new EPODueDateTermMap());
            builder.ApplyConfiguration(new PatEPOActionMapActMap());
            builder.ApplyConfiguration(new PatEPOAppLogMap());
            builder.ApplyConfiguration(new PatEPOCommActLogMap());
            builder.ApplyConfiguration(new PatEPODDActLogMap());

            //EPO OPS API
            builder.ApplyConfiguration(new PatOPSLogMap());
            #endregion

            #region Patent Clearance Search

            builder.ApplyConfiguration(new PacClearanceMap());

            builder.ApplyConfiguration(new PacClearanceStatusMap());
            builder.ApplyConfiguration(new PacClearanceStatusHistoryMap());

            builder.ApplyConfiguration(new PacKeywordMap());

            builder.ApplyConfiguration(new PacQuestionGroupMap());
            builder.ApplyConfiguration(new PacQuestionGuideMap());
            builder.ApplyConfiguration(new PacQuestionGuideChildMap());
            builder.ApplyConfiguration(new PacQuestionMap());

            //builder.ApplyConfiguration(new PacImageMap());

            builder.ApplyConfiguration(new PacWorkflowMap());
            builder.ApplyConfiguration(new PacWorkflowActionMap());

            builder.ApplyConfiguration(new PacClearanceCopySettingMap());
            builder.ApplyConfiguration(new PacClearanceCopyDisclosureSettingMap());

            builder.ApplyConfiguration(new PacDiscussionMap());
            builder.ApplyConfiguration(new PacDiscussionReplyMap());

            builder.ApplyConfiguration(new PacInventorMap());

            #endregion

            #region AMS
            builder.ApplyConfiguration(new AMSMainMap());
            builder.ApplyConfiguration(new AMSDueMap());
            builder.ApplyConfiguration(new AMSProjectionMap());
            builder.ApplyConfiguration(new AMSAbstractMap());
            builder.ApplyConfiguration(new AMSInstrxTypeMap());
            builder.ApplyConfiguration(new AMSStatusTypeMap());
            builder.ApplyConfiguration(new AMSInstrxChangeLogMap());
            builder.ApplyConfiguration(new AMSTaxSchedHistoryMap());
            builder.ApplyConfiguration(new AMSRemLogMap());
            builder.ApplyConfiguration(new AMSRemLogDueMap());
            builder.ApplyConfiguration(new AMSRemLogEmailMap());
            builder.ApplyConfiguration(new AMSRemLogErrorMap());
            builder.ApplyConfiguration(new AMSFeeMap());
            builder.ApplyConfiguration(new AMSFeeDetailMap());
            builder.ApplyConfiguration(new AMSInstrxCPiLogMap());
            builder.ApplyConfiguration(new AMSInstrxCPiLogDetailMap());
            builder.ApplyConfiguration(new AMSInstrxCPiLogEmailMap());
            builder.ApplyConfiguration(new AMSInstrxCPiLogErrorMap());
            builder.ApplyConfiguration(new AMSStatusChangeLogMap());
            builder.ApplyConfiguration(new AMSInstrxDecisionMgtMap());
            builder.ApplyConfiguration(new AMSVATRateMap());
            builder.ApplyConfiguration(new AMSProductMap());
            builder.ApplyConfiguration(new AMSLicenseeMap());
            builder.ApplyConfiguration(new AMSCostExportLogMap());
            builder.ApplyConfiguration(new AMSInstrxCPiViewLogMap());
            #endregion AMS

            #region RMS
            builder.ApplyConfiguration(new RMSDueMap());
            builder.ApplyConfiguration(new RMSReminderSetupMap());
            builder.ApplyConfiguration(new RMSInstrxTypeActionMap());
            builder.ApplyConfiguration(new RMSInstrxTypeActionDetailMap());
            builder.ApplyConfiguration(new RMSInstrxTypeMap());
            builder.ApplyConfiguration(new RMSInstrxChangeLogMap());
            builder.ApplyConfiguration(new RMSRemLogMap());
            builder.ApplyConfiguration(new RMSRemLogDueMap());
            builder.ApplyConfiguration(new RMSRemLogEmailMap());
            builder.ApplyConfiguration(new RMSRemLogErrorMap());
            builder.ApplyConfiguration(new RMSActionCloseLogMap());
            builder.ApplyConfiguration(new RMSActionCloseLogDueMap());
            builder.ApplyConfiguration(new RMSActionCloseLogEmailMap());
            builder.ApplyConfiguration(new RMSActionCloseLogErrorMap());
            builder.ApplyConfiguration(new RMSDocMap());
            builder.ApplyConfiguration(new RMSDueDocMap());
            builder.ApplyConfiguration(new RMSDueDocUploadLogMap());
            builder.ApplyConfiguration(new RMSReminderSetupDocMap());
            builder.ApplyConfiguration(new RMSDueCountryMap());
            #endregion RMS

            #region Foreign Filing
            builder.ApplyConfiguration(new FFDueMap());
            builder.ApplyConfiguration(new FFDueCountryMap());
            builder.ApplyConfiguration(new FFInstrxTypeMap());
            builder.ApplyConfiguration(new FFInstrxTypeActionMap());
            builder.ApplyConfiguration(new FFInstrxTypeActionDetailMap());
            builder.ApplyConfiguration(new FFInstrxChangeLogMap());
            builder.ApplyConfiguration(new FFRemLogMap());
            builder.ApplyConfiguration(new FFRemLogDueMap());
            builder.ApplyConfiguration(new FFRemLogEmailMap());
            builder.ApplyConfiguration(new FFRemLogErrorMap());
            builder.ApplyConfiguration(new FFActionCloseLogMap());
            builder.ApplyConfiguration(new FFActionCloseLogDueMap());
            builder.ApplyConfiguration(new FFActionCloseLogEmailMap());
            builder.ApplyConfiguration(new FFActionCloseLogErrorMap());
            builder.ApplyConfiguration(new FFDocMap());
            builder.ApplyConfiguration(new FFDueDocMap());
            builder.ApplyConfiguration(new FFDueDocUploadLogMap());
            builder.ApplyConfiguration(new FFReminderSetupMap());
            builder.ApplyConfiguration(new FFReminderSetupDocMap());
            #endregion Foreign Filing

            #region DMS
            //DMS - invention disclosure
            builder.ApplyConfiguration(new DisclosureMap());
            builder.ApplyConfiguration(new DisclosureCopySettingMap());
            builder.ApplyConfiguration(new DisclosureCopyClearanceSettingMap());

            builder.ApplyConfiguration(new DMSAbstractMap());
            builder.ApplyConfiguration(new DMSKeywordMap());
            builder.ApplyConfiguration(new DMSInventorMap());
            builder.ApplyConfiguration(new DMSInventorHistoryMap());
            builder.ApplyConfiguration(new DMSDisclosureStatusMap());
            builder.ApplyConfiguration(new DMSActionTypeMap());
            builder.ApplyConfiguration(new DMSIndicatorMap());
            builder.ApplyConfiguration(new DMSRecommendationMap());
            builder.ApplyConfiguration(new DMSActionParameterMap());
            builder.ApplyConfiguration(new DMSActionDueMap());
            builder.ApplyConfiguration(new DMSDueDateMap());
            builder.ApplyConfiguration(new DMSDueDateDelegationMap());            
            builder.ApplyConfiguration(new DisclosureRelatedDisclosureMap());
            builder.ApplyConfiguration(new DMSActionReminderLogMap());
            builder.ApplyConfiguration(new DMSCombinedMap());

            // workflow
            builder.ApplyConfiguration(new DMSWorkflowMap());
            builder.ApplyConfiguration(new DMSWorkflowActionMap());

            // valuation matrix
            builder.ApplyConfiguration(new DMSValuationMatrixMap());
            builder.ApplyConfiguration(new DMSValuationMatrixRateMap());

            builder.ApplyConfiguration(new DMSReviewMap());
            builder.ApplyConfiguration(new DMSPreviewMap());
            builder.ApplyConfiguration(new DMSEntityReviewerMap());
            builder.ApplyConfiguration(new DMSValuationMap());
            builder.ApplyConfiguration(new DMSDisclosureStatusHistoryMap());
            builder.ApplyConfiguration(new DMSRecommendationHistoryMap());
            builder.ApplyConfiguration(new DMSRatingMap());
            builder.ApplyConfiguration(new DMSQuestionGroupMap());
            builder.ApplyConfiguration(new DMSQuestionGuideMap());
            builder.ApplyConfiguration(new DMSQuestionGuideChildMap());
            builder.ApplyConfiguration(new DMSQuestionGuideSubMap());
            builder.ApplyConfiguration(new DMSQuestionGuideSubDtlMap());
            builder.ApplyConfiguration(new DMSQuestionMap());

            builder.ApplyConfiguration(new DMSDiscussionMap());
            builder.ApplyConfiguration(new DMSDiscussionReplyMap());

            builder.ApplyConfiguration(new DMSAverageRatingDTOMap());

            //agenda meeting
            builder.ApplyConfiguration(new DMSAgendaMap());
            builder.ApplyConfiguration(new DMSAgendaReviewerMap());
            builder.ApplyConfiguration(new DMSAgendaRelatedDisclosureMap());

            builder.ApplyConfiguration(new DMSFaqDocMap());
            #endregion

            #region RTS
            builder.ApplyConfiguration(new RTSInfoSettingsMenuMap());
            builder.ApplyConfiguration(new RTSInfoSettingsMenuCountryMap());
            builder.ApplyConfiguration(new RTSSearchMap());
            builder.ApplyConfiguration(new RTSSearchActionMap());
            builder.ApplyConfiguration(new RTSSearchUSIFWMap());
            builder.ApplyConfiguration(new RTSMapActionDueMap());
            builder.ApplyConfiguration(new RTSMapActionDueSourceMap());
            builder.ApplyConfiguration(new RTSMapActionCloseMap());
            builder.ApplyConfiguration(new LSDTextMap());
            builder.ApplyConfiguration(new PDTSentLogMap());
            builder.ApplyConfiguration(new RTSBiblioUpdateMap());
            builder.ApplyConfiguration(new PubNumberConvertedMap());
            builder.ApplyConfiguration(new RTSBiblioUpdateHistoryMap());

            builder.ApplyConfiguration(new RTSMapActionDocumentMap());
            builder.ApplyConfiguration(new RTSMapActionDocumentClientMap());

            builder.ApplyConfiguration(new RTSSearchIDSCountMap());
            #endregion

            #region TL
            builder.ApplyConfiguration(new TLInfoSettingsMenuMap());
            builder.ApplyConfiguration(new TLInfoSettingsMenuCountryMap());
            builder.ApplyConfiguration(new TLSearchMap());
            builder.ApplyConfiguration(new TLSearchActionMap());
            builder.ApplyConfiguration(new TLSearchImageMap());
            builder.ApplyConfiguration(new TLSearchDocumentMap());
            builder.ApplyConfiguration(new TLSearchTTABPartyMap());
            builder.ApplyConfiguration(new TLSearchTTABMap());
            builder.ApplyConfiguration(new TLMapActionDueMap());
            builder.ApplyConfiguration(new TLMapActionDueSourceMap());
            builder.ApplyConfiguration(new TLMapActionCloseMap());
            builder.ApplyConfiguration(new TLBiblioUpdateMap());
            builder.ApplyConfiguration(new TLTrademarkNameUpdateMap());
            builder.ApplyConfiguration(new TLActionComparePTOMap());
            builder.ApplyConfiguration(new TLActionUpdateHistoryMap());
            builder.ApplyConfiguration(new TLBiblioUpdateHistoryMap());
            builder.ApplyConfiguration(new TLTmkNameUpdateHistoryMap());
            builder.ApplyConfiguration(new TLGoodsUpdateHistoryMap());
            builder.ApplyConfiguration(new TLMapActionDocumentMap());
            builder.ApplyConfiguration(new TLMapActionDocumentClientMap());
            builder.ApplyConfiguration(new TLActionUpdateExcludeMap());

            

            #endregion

            #region EFS
            builder.ApplyConfiguration(new EFSLogMap());
            builder.ApplyConfiguration(new EFSMap());
            #endregion

            #region Trademark
            //trademark
            builder.ApplyConfiguration(new TmkCountryMap());
            builder.ApplyConfiguration(new TmkAreaMap());
            builder.ApplyConfiguration(new TmkAreaCountryMap());
            builder.ApplyConfiguration(new TmkAssignmentStatusMap());
            builder.ApplyConfiguration(new TmkCaseTypeMap());
            builder.ApplyConfiguration(new TmkDesCaseTypeMap());
            builder.ApplyConfiguration(new TmkConflictStatusMap());
            builder.ApplyConfiguration(new TmkCostTypeMap());
            builder.ApplyConfiguration(new TmkCountryLawMap());
            builder.ApplyConfiguration(new TmkCountryDueMap());
            builder.ApplyConfiguration(new TmkDesCaseTypeMap());
            builder.ApplyConfiguration(new TmkIndicatorMap());
            builder.ApplyConfiguration(new TmkMarkTypeMap());
            builder.ApplyConfiguration(new TmkStandardGoodMap());
            builder.ApplyConfiguration(new TmkTrademarkStatusMap());
            builder.ApplyConfiguration(new TmkActionTypeMap());
            builder.ApplyConfiguration(new TmkActionParameterMap());
            builder.ApplyConfiguration(new TmkTrademarkMap());
            builder.ApplyConfiguration(new TmkTrademarkClassMap());
            builder.ApplyConfiguration(new TmkTrademarkClassMap());
            builder.ApplyConfiguration(new TmkActionDueMap());
            builder.ApplyConfiguration(new TmkDueDateMap());
            builder.ApplyConfiguration(new TmkDueDateDelegationMap());
            builder.ApplyConfiguration(new TmkAssignmentHistoryMap());
            builder.ApplyConfiguration(new TmkCostTrackMap());
            builder.ApplyConfiguration(new TmkConflictMap());
            builder.ApplyConfiguration(new TmkLicenseeMap());
            builder.ApplyConfiguration(new TmkKeywordMap());
            builder.ApplyConfiguration(new TmkImageMap());
            //builder.ApplyConfiguration(new TmkImageActMap());
            //builder.ApplyConfiguration(new TmkImageCostMap());
            builder.ApplyConfiguration(new TmkDesignatedCountryMap());
            builder.ApplyConfiguration(new TmkOwnerMap());
            builder.ApplyConfiguration(new TmkDueDateDeDocketOutstandingMap());
            builder.ApplyConfiguration(new TmkProductMap());
            builder.ApplyConfiguration(new TmkBudgetManagementMap());
            builder.ApplyConfiguration(new TmkRelatedTrademarkMap());

            //Cost Tracking Import
            builder.ApplyConfiguration(new TmkCostTrackingImportHistoryMap());
            builder.ApplyConfiguration(new TmkCostTrackingImportMappingMap());
            builder.ApplyConfiguration(new TmkCostTrackingImportErrorMap());
            builder.ApplyConfiguration(new TmkCostTrackingImportTypeColumnMap());

            builder.ApplyConfiguration(new TmkTrademarkCopySettingMap());

            builder.ApplyConfiguration(new TmkCountryLawUpdateMap());
            builder.ApplyConfiguration(new TmkWorkflowMap());
            builder.ApplyConfiguration(new TmkWorkflowActionMap());
            builder.ApplyConfiguration(new TmkWorkflowActionParameterMap());

            //Cost Estimator
            builder.ApplyConfiguration(new TmkCECountrySetupMap());
            builder.ApplyConfiguration(new TmkCECountryCostMap());
            builder.ApplyConfiguration(new TmkCECountryCostChildMap());
            builder.ApplyConfiguration(new TmkCECountryCostSubMap());
            builder.ApplyConfiguration(new TmkCEGeneralSetupMap());
            builder.ApplyConfiguration(new TmkCEGeneralCostMap());
            builder.ApplyConfiguration(new TmkCEFeeMap());
            builder.ApplyConfiguration(new TmkCEFeeDetailMap());
            builder.ApplyConfiguration(new TmkCEStageMap());
            builder.ApplyConfiguration(new TmkCostEstimatorMap());
            builder.ApplyConfiguration(new TmkCostEstimatorCountryMap());
            builder.ApplyConfiguration(new TmkCostEstimatorCountryCostMap());
            builder.ApplyConfiguration(new TmkCEQuestionGeneralMap());
            builder.ApplyConfiguration(new TmkCostEstimatorCostMap());
            builder.ApplyConfiguration(new TmkCostEstimatorCostChildMap());
            builder.ApplyConfiguration(new TmkCostEstimatorCostSubMap());

            #endregion

            #region Trademark Clearance

            builder.ApplyConfiguration(new TmcClearanceMap());

            builder.ApplyConfiguration(new TmcClearanceStatusMap());
            builder.ApplyConfiguration(new TmcClearanceStatusHistoryMap());

            builder.ApplyConfiguration(new TmcQuestionGroupMap());
            builder.ApplyConfiguration(new TmcQuestionGuideMap());
            builder.ApplyConfiguration(new TmcQuestionGuideChildMap());
            builder.ApplyConfiguration(new TmcQuestionMap());

            //builder.ApplyConfiguration(new TmcImageMap());            

            builder.ApplyConfiguration(new TmcWorkflowMap());
            builder.ApplyConfiguration(new TmcWorkflowActionMap());

            builder.ApplyConfiguration(new TmcKeywordMap());
            builder.ApplyConfiguration(new TmcListMap());
            builder.ApplyConfiguration(new TmcRelatedTrademarkMap());
            builder.ApplyConfiguration(new TmcMarkMap());

            builder.ApplyConfiguration(new TmcClearanceCopySettingMap());

            builder.ApplyConfiguration(new TmcDiscussionMap());
            builder.ApplyConfiguration(new TmcDiscussionReplyMap());

            #endregion

            #region General Matter
            //general matter
            builder.ApplyConfiguration(new GMMatterMap());
            builder.ApplyConfiguration(new GMMatterAttorneyMap());
            builder.ApplyConfiguration(new GMMatterCountryMap());
            builder.ApplyConfiguration(new GMMatterPatentMap());
            builder.ApplyConfiguration(new GMMatterTrademarkMap());
            builder.ApplyConfiguration(new GMMatterKeywordMap());
            builder.ApplyConfiguration(new GMMatterOtherPartyMap());
            builder.ApplyConfiguration(new GMCountryMap());
            builder.ApplyConfiguration(new GMAreaMap());
            builder.ApplyConfiguration(new GMAreaCountryMap());
            builder.ApplyConfiguration(new GMMatterTypeMap());
            builder.ApplyConfiguration(new GMMatterStatusMap());
            builder.ApplyConfiguration(new GMAgreementTypeMap());
            builder.ApplyConfiguration(new GMExtentMap());
            builder.ApplyConfiguration(new GMOtherPartyTypeMap());
            builder.ApplyConfiguration(new GMOtherPartyMap());
            //builder.ApplyConfiguration(new GMMatterImageMap());
            //builder.ApplyConfiguration(new GMMatterImageActMap());
            //builder.ApplyConfiguration(new GMMatterImageCostMap());
            builder.ApplyConfiguration(new GMIndicatorMap());
            builder.ApplyConfiguration(new GMActionTypeMap());
            builder.ApplyConfiguration(new GMActionParameterMap());
            builder.ApplyConfiguration(new GMCostTypeMap());
            builder.ApplyConfiguration(new GMCostTrackMap());
            builder.ApplyConfiguration(new GMActionDueMap());
            builder.ApplyConfiguration(new GMDueDateMap());
            builder.ApplyConfiguration(new GMDueDateDelegationMap());
            builder.ApplyConfiguration(new GMDueDateDeDocketOutstandingMap());
            builder.ApplyConfiguration(new GMMatterOtherPartyPatentMap());
            builder.ApplyConfiguration(new GMMatterOtherPartyTrademarkMap());
            builder.ApplyConfiguration(new GMMatterRelatedMatterMap());

            builder.ApplyConfiguration(new GMMatterCopySettingMap());
            builder.ApplyConfiguration(new GMWorkflowMap());
            builder.ApplyConfiguration(new GMWorkflowActionMap());
            builder.ApplyConfiguration(new GMWorkflowActionParameterMap());

            builder.ApplyConfiguration(new GMProductMap());
            builder.ApplyConfiguration(new GMBudgetManagementMap());

            //Cost Tracking Import
            builder.ApplyConfiguration(new GMCostTrackingImportHistoryMap());
            builder.ApplyConfiguration(new GMCostTrackingImportMappingMap());
            builder.ApplyConfiguration(new GMCostTrackingImportErrorMap());
            builder.ApplyConfiguration(new GMCostTrackingImportTypeColumnMap());
            #endregion

            #region Quick Email
            builder.ApplyConfiguration(new QEMainMap());
            builder.ApplyConfiguration(new QEMainMap());
            builder.ApplyConfiguration(new QELayoutMap());
            builder.ApplyConfiguration(new QERecipientMap());
            builder.Entity<QEDataSource>().ToTable("tblQEDataSource");
            builder.Entity<QEDataSourceScreen>().ToTable("tblQEDataSourceScreen");
            builder.Entity<QERoleSource>().ToTable("tblQERoleSource");
            builder.Entity<QELog>().ToTable("tblQELog");
            builder.ApplyConfiguration(new QECategoryMap());
            builder.ApplyConfiguration(new QETagMap());
            #endregion

            #region Dedocket
            builder.ApplyConfiguration(new PatDueDateDeDocketMap());
            builder.ApplyConfiguration(new PatDueDateInvDeDocketMap());
            builder.ApplyConfiguration(new TmkDueDateDeDocketMap());
            builder.ApplyConfiguration(new GMDueDateDeDocketMap());

            builder.ApplyConfiguration(new PatDueDateDeDocketRespMap());            
            builder.ApplyConfiguration(new TmkDueDateDeDocketRespMap());
            builder.ApplyConfiguration(new GMDueDateDeDocketRespMap());
            #endregion

            #region DuedateExtension
            builder.ApplyConfiguration(new PatDueDateExtensionMap());
            builder.ApplyConfiguration(new PatDueDateInvExtensionMap());
            builder.ApplyConfiguration(new TmkDueDateExtensionMap());
            builder.ApplyConfiguration(new GMDueDateExtensionMap());
            builder.ApplyConfiguration(new DMSDueDateExtensionMap());
            builder.ApplyConfiguration(new DueDateExtensionLogMap());
            #endregion

            #region DocketRequest
            builder.ApplyConfiguration(new PatDocketRequestMap());
            builder.ApplyConfiguration(new TmkDocketRequestMap());
            builder.ApplyConfiguration(new GMDocketRequestMap());
            builder.ApplyConfiguration(new PatDocketInvRequestMap());

            builder.ApplyConfiguration(new PatDocketRequestRespMap());
            builder.ApplyConfiguration(new TmkDocketRequestRespMap());
            builder.ApplyConfiguration(new GMDocketRequestRespMap());
            #endregion

            #region Duedatelog
            builder.ApplyConfiguration(new PatDueDateDateTakenLogMap());
            builder.ApplyConfiguration(new PatDueDateInvDateTakenLogMap());
            builder.ApplyConfiguration(new TmkDueDateDateTakenLogMap());
            builder.ApplyConfiguration(new GMDueDateDateTakenLogMap());
            builder.ApplyConfiguration(new DMSDueDateDateTakenLogMap());
            #endregion

            #region Quick Email
            builder.Entity<QEPatInventionView>().HasNoKey().ToView("vwQE_Pat_Invention");
            builder.Entity<QEPatCountryApplicationView>().HasNoKey().ToView("vwQE_Pat_CountryApplication");
            builder.Entity<QEPatInventorAppAwardView>().HasNoKey().ToView("vwQE_Pat_InventorAppAward");
            builder.Entity<QEPatInventorDMSAwardView>().HasNoKey().ToView("vwQE_Pat_InventorDMSAward");
            builder.Entity<QEPatIRLumpSumAwardView>().HasNoKey().ToView("vwQE_Pat_RemunerationLumpSum");
            builder.Entity<QEPatIRYearlyAwardView>().HasNoKey().ToView("vwQE_Pat_RemunerationYearly");
            builder.Entity<QEPatIRDistributionAwardView>().HasNoKey().ToView("vwQE_Pat_RemunerationDistribution");
            builder.Entity<QEPatIRFRRemunerationAwardView>().HasNoKey().ToView("vwQE_Pat_IRFRRemunerationAward");
            builder.Entity<QEPatCostTrackingView>().HasNoKey().ToView("vwQE_Pat_CostTracking");
            builder.Entity<QEPatCostTrackingInvView>().HasNoKey().ToView("vwQE_Pat_CostTrackingInv");
            builder.Entity<QEPatActionDueView>().HasNoKey().ToView("vwQE_Pat_ActionDue");
            builder.Entity<QEPatActionDueDateView>().HasNoKey().ToView("vwQE_Pat_ActionDueDate");
            builder.Entity<QEPatActionDueDateDedocketView>().HasNoKey().ToView("vwQE_Pat_ActionDueDateDedocket");
            builder.Entity<QEPatActionDueDateDelegationView>().HasNoKey().ToView("vwQE_Pat_ActionDueDateDelegation");
            builder.Entity<QEPatActionDueInvView>().HasNoKey().ToView("vwQE_Pat_ActionDueInv");
            builder.Entity<QEPatActionDueDateInvView>().HasNoKey().ToView("vwQE_Pat_ActionDueDateInv");
            builder.Entity<QEPatActionDueDateInvDedocketView>().HasNoKey().ToView("vwQE_Pat_ActionDueDateInvDedocket");
            builder.Entity<QEPatActionDueDateInvDelegationView>().HasNoKey().ToView("vwQE_Pat_ActionDueDateInvDelegation");
            builder.Entity<QEPatSearchView>().HasNoKey().ToView("vwQE_Pat_PatSearch");
            builder.Entity<QEPatCountryApplicationImageView>().HasNoKey().ToView("vwQE_Pat_CountryAppImages");
            builder.Entity<QEPatActionImageView>().HasNoKey().ToView("vwQE_Pat_ActionImages");
            builder.Entity<QEPatActionDueDeletedView>().HasNoKey().ToView("vwQE_Pat_ActionDueDeleted");
            builder.Entity<QEPatActionInvImageView>().HasNoKey().ToView("vwQE_Pat_ActionInvImages");
            builder.Entity<QEPatActionDueInvDeletedView>().HasNoKey().ToView("vwQE_Pat_ActionDueInvDeleted");
            builder.Entity<QEPatCountryAppDeletedView>().HasNoKey().ToView("vwQE_Pat_CountryAppDeleted");
            builder.Entity<QEPatInventionAttyChangedView>().HasNoKey().ToView("vwQE_Pat_InventionAttyModified");

            builder.Entity<QETmkTrademarkView>().HasNoKey().ToView("vwQE_Tmk_Trademark");
            builder.Entity<QETmkCostTrackingView>().HasNoKey().ToView("vwQE_Tmk_CostTracking");
            builder.Entity<QETmkActionDueView>().HasNoKey().ToView("vwQE_Tmk_ActionDue");
            builder.Entity<QETmkActionDueDateView>().HasNoKey().ToView("vwQE_Tmk_ActionDueDate");
            builder.Entity<QETmkActionDueDateDedocketView>().HasNoKey().ToView("vwQE_Tmk_ActionDueDateDedocket");
            builder.Entity<QETmkActionDueDateDelegationView>().HasNoKey().ToView("vwQE_Tmk_ActionDueDateDelegation");
            builder.Entity<QETmkConflictView>().HasNoKey().ToView("vwQE_Tmk_Conflict");
            builder.Entity<QETmkActionImageView>().HasNoKey().ToView("vwQE_Tmk_ActionImages");
            builder.Entity<QETmkImageView>().HasNoKey().ToView("vwQE_Tmk_Images");
            builder.Entity<QETmkActionDueDeletedView>().HasNoKey().ToView("vwQE_Tmk_ActionDueDeleted");
            builder.Entity<QETmkTrademarkDeletedView>().HasNoKey().ToView("vwQE_Tmk_TrademarkDeleted");
            builder.Entity<QETmkTrademarkAttyChangedView>().HasNoKey().ToView("vwQE_Tmk_TrademarkAttyModified");

            builder.Entity<QEGmMatterView>().HasNoKey().ToView("vwQE_GM_GMMatter");
            builder.Entity<QEGmCostTrackingView>().HasNoKey().ToView("vwQE_GM_CostTracking");
            builder.Entity<QEGmActionDueView>().HasNoKey().ToView("vwQE_GM_ActionDue");
            builder.Entity<QEGmActionDueDateView>().HasNoKey().ToView("vwQE_GM_ActionDueDate");
            builder.Entity<QEGmActionDueDateDedocketView>().HasNoKey().ToView("vwQE_GM_ActionDueDateDedocket");
            builder.Entity<QEGmActionDueDateDelegationView>().HasNoKey().ToView("vwQE_GM_ActionDueDateDelegation");

            builder.Entity<QEGmActionImageView>().HasNoKey().ToView("vwQE_GM_ActionImages");
            builder.Entity<QEGmImageView>().HasNoKey().ToView("vwQE_GM_Images");
            builder.Entity<QEGmActionDueDeletedView>().HasNoKey().ToView("vwQE_GM_ActionDueDeleted");
            builder.Entity<QEGmMatterDeletedView>().HasNoKey().ToView("vwQE_GM_MatterDeleted");

            builder.Entity<QEDmsDisclosureView>().HasNoKey().ToView("vwQE_Dms_Disclosure");
            builder.Entity<QEDmsDisclosureReviewView>().HasNoKey().ToView("vwQE_Dms_DisclosureReview");
            builder.Entity<QEDmsActionDueView>().HasNoKey().ToView("vwQE_DMS_ActionDue");
            builder.Entity<QEDmsActionDueDateView>().HasNoKey().ToView("vwQE_DMS_ActionDueDate");
            builder.Entity<QEDmsActionDueDateDelegationView>().HasNoKey().ToView("vwQE_DMS_ActionDueDateDelegation");
            builder.Entity<QEDmsAgendaView>().HasNoKey().ToView("vwQE_Dms_Agenda");

            builder.Entity<QETmcClearanceView>().HasNoKey().ToView("vwQE_Tmc_Clearance");
            builder.Entity<QEPacClearanceView>().HasNoKey().ToView("vwQE_Pac_Clearance");

            builder.Entity<QEPatCountryApplicationDocRespDocketingView>().HasNoKey().ToView("vwQE_Pat_CountryAppDocRespDocketing");
            builder.Entity<QETmkTrademarkDocRespDocketingView>().HasNoKey().ToView("vwQE_Tmk_TrademarkDocRespDocketing");
            builder.Entity<QEGmMatterDocRespDocketingView>().HasNoKey().ToView("vwQE_GM_GMMatterDocRespDocketing");

            builder.Entity<QEPatCountryApplicationDocRespReportingView>().HasNoKey().ToView("vwQE_Pat_CountryAppDocRespReporting");
            builder.Entity<QETmkTrademarkDocRespReportingView>().HasNoKey().ToView("vwQE_Tmk_TrademarkDocRespReporting");
            builder.Entity<QEGmMatterDocRespReportingView>().HasNoKey().ToView("vwQE_GM_GMMatterDocRespReporting");

            builder.ApplyConfiguration(new QECustomFieldMap());

            builder.Entity<QEDocVerificationImageView>().HasNoKey().ToView("vwQE_DocVer_Images");
            
            builder.Entity<QEPatRequestDocketView>().HasNoKey().ToView("vwQE_Pat_RequestDocket");
            builder.Entity<QETmkRequestDocketView>().HasNoKey().ToView("vwQE_Tmk_RequestDocket");
            builder.Entity<QEGmRequestDocketView>().HasNoKey().ToView("vwQE_GM_RequestDocket");


            #endregion

            #region Quick Email Setup
            builder.Entity<QEDetailView>().HasNoKey().ToView("vwSysQEDetail");
            builder.Entity<QERecipientView>().HasNoKey().ToView("vwSysQERecipient");
            #endregion

            #region Report
            // Report
            builder.ApplyConfiguration(new ReportParameterMap());

            builder.Entity<SharedReportActionTypeLookupDTO>().HasNoKey().ToView("vwWebSysSharedActionType");
            builder.Entity<SharedReportActionDueLookupDTO>().HasNoKey().ToView("vwWebSysSharedActionDue");
            builder.Entity<SharedReportIndicatorLookupDTO>().HasNoKey().ToView("vwWebSysSharedIndicator");
            builder.Entity<SharedReportCountryLookupDTO>().HasNoKey().ToView("vwWebSysSharedCountry");
            builder.Entity<SharedReportAreaLookupDTO>().HasNoKey().ToView("vwWebSysSharedArea");
            builder.Entity<SharedReportClientLookupDTO>().HasNoKey().ToView("vwWebSysSharedClient");
            builder.Entity<SharedReportOwnerLookupDTO>().HasNoKey().ToView("vwWebSysSharedOwner");
            builder.Entity<SharedReportAttorneyLookupDTO>().HasNoKey().ToView("vwWebSysSharedAttorney");
            builder.Entity<SharedReportStatusLookupDTO>().HasNoKey().ToView("vwWebSysSharedStatus");
            builder.Entity<SharedReportCaseTypeLookupDTO>().HasNoKey().ToView("vwWebSysSharedCaseType");
            builder.Entity<SharedReportResponsibleOfficeLookupDTO>().HasNoKey().ToView("vwWebSysSharedRespOffice");
            builder.Entity<SharedReportCaseNumberLookupDTO>().HasNoKey().ToView("vwWebSysSharedCaseNumber");
            builder.Entity<SharedReportCostTypeLookupDTO>().HasNoKey().ToView("vwWebSysSharedCostType");
            builder.Entity<SharedReportAgentLookupDTO>().HasNoKey().ToView("vwWebSysSharedAgent");
            #endregion Report

            #region Report Scheduler
            builder.ApplyConfiguration(new RSActionTypeMap());
            builder.ApplyConfiguration(new RSCriteriaControlMap());
            builder.ApplyConfiguration(new RSFrequencyTypeMap());
            builder.ApplyConfiguration(new RSOrderByControlMap());
            builder.ApplyConfiguration(new RSDateTypeControlMap());
            builder.ApplyConfiguration(new RSPrintOptionControlMap());
            builder.ApplyConfiguration(new RSReportTypeMap());
            builder.ApplyConfiguration(new RSMainMap());
            builder.ApplyConfiguration(new RSHistoryMap());
            builder.ApplyConfiguration(new RSPrintOptionMap());
            builder.ApplyConfiguration(new RSPrintOptionHistoryMap());
            builder.ApplyConfiguration(new RSCriteriaMap());
            builder.ApplyConfiguration(new RSCriteriaHistoryMap());
            builder.ApplyConfiguration(new RSActionMap());
            builder.ApplyConfiguration(new RSActionHistoryMap());
            #endregion Report Scheduler

            #region Letters
            builder.ApplyConfiguration(new LetterMainMap());
            builder.ApplyConfiguration(new LetterCategoryMap());
            builder.ApplyConfiguration(new LetterDataSourceMap());
            builder.ApplyConfiguration(new LetterRecordSourceMap());
            builder.ApplyConfiguration(new LetterRecordSourceFilterMap());
            builder.ApplyConfiguration(new LetterRecordSourceFilterUserMap());
            builder.ApplyConfiguration(new LetterEntitySettingsMap());
            builder.ApplyConfiguration(new LetterUserDataMap());
            builder.Entity<LetterLog>().ToTable("tblLetLog");
            builder.Entity<LetterLogDetail>().ToTable("tblLetLogDtl").HasOne(ll => ll.LetterLog)
                   .WithMany(l => l.LetterLogDetails).HasForeignKey(ll => ll.LetLogId).HasPrincipalKey(l => l.LetLogId);
            builder.ApplyConfiguration(new LetterCustomFieldMap());
            builder.ApplyConfiguration(new LetterSubCategoryMap());
            builder.ApplyConfiguration(new LetterTagMap());

            #endregion

            #region DOCX

            builder.ApplyConfiguration(new DOCXMainMap());
            builder.ApplyConfiguration(new DOCXCategoryMap());
            builder.ApplyConfiguration(new DOCXDataSourceMap());
            builder.ApplyConfiguration(new DOCXRecordSourceMap());
            builder.ApplyConfiguration(new DOCXRecordSourceFilterMap());
            builder.ApplyConfiguration(new DOCXRecordSourceFilterUserMap());
            //builder.ApplyConfiguration(new DOCXEntitySettingsMap());
            builder.ApplyConfiguration(new DOCXUserDataMap());
            builder.ApplyConfiguration(new DOCXUSPTOHeaderMap());
            builder.ApplyConfiguration(new DOCXUSPTOHeaderKeywordMap());
            builder.Entity<DOCXLog>().ToTable("tblDOCXLog");
            //builder.Entity<DOCXLogDetail>().ToTable("tblDOCXLogDtl").HasOne(ll => ll.DOCXLog)
            //       .WithMany(l => l.DOCXLogDetails).HasForeignKey(ll => ll.DOCXLogId).HasPrincipalKey(l => l.DOCXLogId);

            #endregion

            #region Data Query
            builder.ApplyConfiguration(new DataQueryMainMap());
            builder.ApplyConfiguration(new DataQueryAllowedFunctionMap());
            builder.ApplyConfiguration(new DataQueryCategoryMap());
            builder.ApplyConfiguration(new DataQueryTagMap());
            #endregion

            #region Security & System Logs
            builder.Entity<CPiSystem>().ToTable("tblCPiSystems");
            builder.Entity<CPiRole>().ToTable("tblCPiRoles");

            builder.ApplyConfiguration(new CPiUserMap());
            builder.ApplyConfiguration(new CPiSystemMap());
            builder.ApplyConfiguration(new CPiRoleMap());
            builder.ApplyConfiguration(new CPiSystemRoleMap());
            builder.ApplyConfiguration(new CPiUserTypeSystemRoleMap());
            builder.ApplyConfiguration(new CPiUserTypeDefaultPageMap());
            builder.ApplyConfiguration(new CPiUserTypeDefaultWidgetMap());
            builder.ApplyConfiguration(new CPiUserSystemRoleMap());
            builder.ApplyConfiguration(new CPiUserPasswordHistoryMap());
            builder.ApplyConfiguration(new CPiUserClaimMap());
            builder.ApplyConfiguration(new CPiUserEntityFilterMap());
            builder.ApplyConfiguration(new CPiRespOfficeMap());
            builder.ApplyConfiguration(new CPiSSOClaimSystemRoleMap());
            builder.ApplyConfiguration(new CPiSSOClaimUserMap());

            builder.ApplyConfiguration(new ErrorMappingMap());

            #endregion

            #region System Tables
            //builder.Entity<FileHandler>().ToTable("tblPubFileHandler");
            builder.Entity<ModuleMain>().ToTable("tblModule");

            builder.ApplyConfiguration(new CPiMenuItemMap());
            builder.ApplyConfiguration(new CPiMenuPageMap());

            builder.ApplyConfiguration(new LocalizationRecordsGroupingMap());
            builder.ApplyConfiguration(new LocalizationRecordsMap());
            builder.ApplyConfiguration(new NotificationMap());
            builder.ApplyConfiguration(new NotificationConnectionMap());

            builder.ApplyConfiguration(new CPiDefaultPageMap());
            builder.ApplyConfiguration(new CPiSettingMap());
            builder.ApplyConfiguration(new CPiUserSettingMap());
            builder.ApplyConfiguration(new CPiSystemSettingMap());
            builder.ApplyConfiguration(new CPiUserSettingLogMap());

            builder.ApplyConfiguration(new CPiWidgetMap());
            builder.ApplyConfiguration(new CPiUserWidgetMap());

            builder.ApplyConfiguration(new OptionMap());
            builder.ApplyConfiguration(new SystemScreenMap());
            builder.ApplyConfiguration(new GlobalUpdateFieldsMap());
            builder.ApplyConfiguration(new PatGlobalUpdateLogMap());
            builder.ApplyConfiguration(new TmkGlobalUpdateLogMap());
            builder.ApplyConfiguration(new GMGlobalUpdateLogMap());

            builder.ApplyConfiguration(new ActivityLogMap());
            builder.ApplyConfiguration(new ApiLogMap());
            builder.ApplyConfiguration(new SysCustomFieldSettingMap());

            builder.ApplyConfiguration(new CPiGroupMap());
            builder.ApplyConfiguration(new CPiUserGroupMap());

            builder.ApplyConfiguration(new ScheduledTaskMap());
            #endregion

            #region TradeSecret
            builder.ApplyConfiguration(new TradeSecretRequestMap());
            builder.ApplyConfiguration(new TradeSecretActivityMap());
            builder.ApplyConfiguration(new TradeSecretAuditLogMap());
            #endregion

            #region Email Template
            builder.ApplyConfiguration(new EmailTemplateMap());
            builder.ApplyConfiguration(new EmailTypeMap());
            builder.ApplyConfiguration(new EmailSetupMap());
            builder.ApplyConfiguration(new EmailDataModelMap());
            #endregion

            #region Data Import
            builder.ApplyConfiguration(new DataImportHistoryMap());
            builder.ApplyConfiguration(new DataImportTypeMap());
            builder.ApplyConfiguration(new DataImportMappingMap());
            builder.ApplyConfiguration(new DataImportTypeColumnMap());
            builder.ApplyConfiguration(new DataImportErrorMap());

            #endregion

            #region Documents
            builder.ApplyConfiguration(new DocSystemMap());
            builder.ApplyConfiguration(new DocMatterTreeMap());
            builder.ApplyConfiguration(new DocFolderMap());
            builder.ApplyConfiguration(new DocDocumentMap());
            builder.ApplyConfiguration(new DocDocumentTagMap());
            builder.ApplyConfiguration(new DocFileMap());
            builder.ApplyConfiguration(new DocFileSignatureMap());
            builder.ApplyConfiguration(new SharePointFileSignatureMap());
            builder.ApplyConfiguration(new DocFileSignatureRecipientMap());
            builder.ApplyConfiguration(new DocIconMap());
            builder.ApplyConfiguration(new DocTypeMap());
            builder.ApplyConfiguration(new DocFixedFolderMap());

            builder.ApplyConfiguration(new DocGmailCaseLinkMap());
            builder.ApplyConfiguration(new DocOutlookMap());
            builder.ApplyConfiguration(new DocOutlookCaseLinkMap());
            builder.ApplyConfiguration(new DocOutlookIdMap());

            builder.ApplyConfiguration(new MailDownloadLogMap());
            builder.ApplyConfiguration(new MailDownloadLogDetailMap());
            builder.ApplyConfiguration(new MailDownloadRuleMap());
            builder.ApplyConfiguration(new MailDownloadRuleConditionMap());
            builder.ApplyConfiguration(new MailDownloadRuleResponsibleMap());
            builder.ApplyConfiguration(new MailDownloadActionMap());
            builder.ApplyConfiguration(new MailDownloadActionFilterMap());
            builder.ApplyConfiguration(new MailDownloadDataMapMap());
            builder.ApplyConfiguration(new MailDownloadDataMapPatternMap());
            builder.ApplyConfiguration(new MailDownloadDataAttributeMap());

            builder.ApplyConfiguration(new DocVerificationMap());
            builder.ApplyConfiguration(new DocVerificationSearchFieldMap());

            builder.ApplyConfiguration(new DocResponsibleLogMap());
            builder.ApplyConfiguration(new DocResponsibleDocketingMap());
            builder.ApplyConfiguration(new DocResponsibleReportingMap());

            builder.ApplyConfiguration(new DocQuickEmailLogMap());
            #endregion

            #region Form Extract
            builder.ApplyConfiguration(new FormSystemMap());
            builder.ApplyConfiguration(new FormSourceMap());
            builder.ApplyConfiguration(new FormIFWFormTypeMap());
            builder.ApplyConfiguration(new FormIFWDocTypeMap());
            builder.ApplyConfiguration(new FormIFWDataExtractMap());
            builder.ApplyConfiguration(new FormIFWFieldUsageMap());
            builder.ApplyConfiguration(new FormIFWActionMapMap());
            builder.ApplyConfiguration(new FormIFWActMapMap());
            builder.ApplyConfiguration(new FormIFWActMapPatMap());
            builder.ApplyConfiguration(new FormIFWActMapTmkMap());

            #endregion

            #region Global Search 
            builder.ApplyConfiguration(new GSSystemMap());
            builder.ApplyConfiguration(new GSScreenMap());
            builder.ApplyConfiguration(new GSTableMap());
            builder.ApplyConfiguration(new GSFieldMap());
            #endregion

            #region API
            builder.ApplyConfiguration(new AMSInstrxWebSvcMap());
            builder.ApplyConfiguration(new InventionWebSvcMap());
            builder.ApplyConfiguration(new CountryApplicationWebSvcMap());
            builder.ApplyConfiguration(new PatInventorInvWebSvcMap());
            builder.ApplyConfiguration(new PatCostTrackWebSvcMap());
            builder.ApplyConfiguration(new PatActionDueWebSvcMap());
            builder.ApplyConfiguration(new PatDueDateWebSvcMap());
            builder.ApplyConfiguration(new PatAssignmentHistoryWebSvcMap());
            builder.ApplyConfiguration(new PatIDSDownloadWebSvcMap());
            builder.ApplyConfiguration(new TmkTrademarkWebSvcMap());
            builder.ApplyConfiguration(new TmkActionDueWebSvcMap());
            builder.ApplyConfiguration(new TmkDueDateWebSvcMap());
            builder.ApplyConfiguration(new TmkCostTrackWebSvcMap());
            builder.ApplyConfiguration(new TmkTrademarkClassWebSvcMap());
            builder.ApplyConfiguration(new TmkAssignmentHistoryWebSvcMap());
            builder.ApplyConfiguration(new WebServiceLogMap());
            builder.ApplyConfiguration(new DocWebSvcMap());
            builder.ApplyConfiguration(new PatOwnerAppWebSvcMap());
            builder.ApplyConfiguration(new PatPriorityWebSvcMap());
            builder.ApplyConfiguration(new TmkOwnerWebSvcMap());
            #endregion

            #region Family Trees
            builder.ApplyConfiguration(new FamilyTreeParentCaseDTOMap());

            #endregion

            // Fix breaking changes in EF7
            // SqlFunctionExpression.Create is obsolete
            // builder.HasDbFunction(typeof(SqlHelper).GetMethod(nameof(SqlHelper.JsonValue)))
            //    .HasTranslation(e => SqlFunctionExpression.Create(
            //        "JSON_VALUE", e, typeof(string), null));
            //https://github.com/dotnet/efcore/issues/11295
            builder.HasDbFunction(typeof(SqlHelper).GetMethod(nameof(SqlHelper.JsonValue)))
                .HasTranslation(e => new SqlFunctionExpression(
                    "JSON_VALUE",
                    e,
                    nullable: true,
                    argumentsPropagateNullability: new[] { false, false },
                    typeof(string),
                    null));

            base.OnModelCreating(builder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Mitigate breaking changes in EF7
            // SQL Server tables with triggers or certain computed columns now require special EF Core configuration
            configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
        }

        public void DetachAllEntities()
        {
            var undetachedEntriesCopy = this.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Detached)
                .ToList();

            foreach (var entry in undetachedEntriesCopy)
                entry.State = EntityState.Detached;
        }

        public List<EntityEntry> GetAllTrackedEntities()
        {
            return this.ChangeTracker.Entries().ToList();
                
        }
    }
}
