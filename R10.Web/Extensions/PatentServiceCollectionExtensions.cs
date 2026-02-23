using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Core.Services;
using R10.Core.Services.Shared;
using R10.Infrastructure.Data;
using R10.Infrastructure.Data.Patent;
using R10.Web.Areas.Patent.Services;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Services.Patent;
using R10.Web.Areas.Shared.Services;

namespace R10.Web.Extensions
{
    public static class PatentServiceCollectionExtensions
    {
        public static IServiceCollection AddPatent(this IServiceCollection services)
        {
            services.AddScoped<IInventionViewModelService, InventionViewModelService>();
            services.AddScoped<IInventionService, InventionService>();
            services.AddScoped<IViewModelService<PatCountry>, ViewModelService<PatCountry>>();
            services.AddScoped<IParentEntityService<PatCountry, PatAreaCountry>, ParentEntityService<PatCountry, PatAreaCountry>>();
            services.AddScoped<IPatCountryLawService, PatCountryLawService>();
            services.AddScoped<IAsyncRepository<PatCountryLaw>, EFRepository<PatCountryLaw>>();
            services.AddScoped<IPatCountryDueRepository, PatCountryDueRepository>();
            services.AddScoped<IViewModelService<PatCountryLaw>, ViewModelService<PatCountryLaw>>();
            services.AddScoped<IInventionRepository, InventionRepository>();

            services.AddScoped<IViewModelService<PatDisclosureStatus>, ViewModelService<PatDisclosureStatus>>();
            services.AddScoped<IEntityService<PatDisclosureStatus>, AuxService<PatDisclosureStatus>>();
            services.AddScoped<IViewModelService<PatArea>, ViewModelService<PatArea>>();
            services.AddScoped<IParentEntityService<PatArea, PatAreaCountry>, ParentEntityService<PatArea, PatAreaCountry>>();

            services.AddScoped<IViewModelService<PatAssignmentStatus>, ViewModelService<PatAssignmentStatus>>();
            services.AddScoped<IEntityService<PatAssignmentStatus>, AuxService<PatAssignmentStatus>>();
            services.AddScoped<IViewModelService<PatCostType>, ViewModelService<PatCostType>>();
            services.AddScoped<IEntityService<PatCostType>, AuxService<PatCostType>>();
            services.AddScoped<IViewModelService<PatIDSReferenceSource>, ViewModelService<PatIDSReferenceSource>>();
            services.AddScoped<IEntityService<PatIDSReferenceSource>, AuxService<PatIDSReferenceSource>>();

            services.AddScoped<IViewModelService<PatActionType>, ViewModelService<PatActionType>>();
            services.AddScoped<IParentEntityService<PatActionType, PatActionParameter>, ParentEntityService<PatActionType, PatActionParameter>>();

            services.AddScoped<IViewModelService<PatCaseType>, ViewModelService<PatCaseType>>();
            services.AddScoped<IEntityService<PatCaseType>, AuxService<PatCaseType>>();
            services.AddScoped<IEntityService<PatCountry>, AuxService<PatCountry>>();

            services.AddScoped<IViewModelService<PatIREmployeePosition>, ViewModelService<PatIREmployeePosition>>();
            services.AddScoped<IEntityService<PatIREmployeePosition>, AuxService<PatIREmployeePosition>>();
            services.AddScoped<IViewModelService<PatIRTurnOver>, ViewModelService<PatIRTurnOver>>();
            services.AddScoped<IEntityService<PatIRTurnOver>, AuxService<PatIRTurnOver>>();
            services.AddScoped<IViewModelService<PatIRStaggering>, ViewModelService<PatIRStaggering>>();
            services.AddScoped<IEntityService<PatIRStaggering>, AuxService<PatIRStaggering>>();
            services.AddScoped<IChildEntityService<PatIRStaggering, PatIRStaggeringDetail>, ChildEntityService<PatIRStaggering, PatIRStaggeringDetail>>();
            services.AddScoped<IViewModelService<PatIREuroExchangeRate>, ViewModelService<PatIREuroExchangeRate>>();
            services.AddScoped<IEntityService<PatIREuroExchangeRate>, AuxService<PatIREuroExchangeRate>>();
            services.AddScoped<IChildEntityService<PatIREuroExchangeRate, PatIREuroExchangeRateYearly>, ChildEntityService<PatIREuroExchangeRate, PatIREuroExchangeRateYearly>>();
            services.AddScoped<IViewModelService<PatIRValorizationRule>, ViewModelService<PatIRValorizationRule>>();
            services.AddScoped<IEntityService<PatIRValorizationRule>, AuxService<PatIRValorizationRule>>();
            services.AddScoped<IEntityService<PatIRDistribution>, AuxService<PatIRDistribution>>();
            services.AddScoped<IEntityService<PatIRRemuneration>, AuxService<PatIRRemuneration>>();
            services.AddScoped<IChildEntityService<PatIRRemuneration, PatInventorInv>, ChildEntityService<PatIRRemuneration, PatInventorInv>>();
            services.AddScoped<IChildEntityService<PatIRRemuneration, PatIRProductSale>, ChildEntityService<PatIRRemuneration, PatIRProductSale>>();
            services.AddScoped<IViewModelService<PatIRRemunerationFormula>, ViewModelService<PatIRRemunerationFormula>>();
            services.AddScoped<IEntityService<PatIRRemunerationFormula>, AuxService<PatIRRemunerationFormula>>();
            services.AddScoped<IEntityService<PatIRRemunerationType>, AuxService<PatIRRemunerationType>>();
            services.AddScoped<IViewModelService<PatIRRemunerationFormulaFactor>, ViewModelService<PatIRRemunerationFormulaFactor>>();
            services.AddScoped<IEntityService<PatIRRemunerationFormulaFactor>, AuxService<PatIRRemunerationFormulaFactor>>();
            services.AddScoped<IViewModelService<PatIRRemunerationValuationMatrix>, ViewModelService<PatIRRemunerationValuationMatrix>>();
            services.AddScoped<IEntityService<PatIRRemunerationValuationMatrix>, AuxService<PatIRRemunerationValuationMatrix>>();
            services.AddScoped<IEntityService<PatIRRemunerationValuationMatrixType>, AuxService<PatIRRemunerationValuationMatrixType>>();
            services.AddScoped<IChildEntityService<PatIRRemunerationValuationMatrix, PatIRRemunerationValuationMatrixCriteria>, ChildEntityService<PatIRRemunerationValuationMatrix, PatIRRemunerationValuationMatrixCriteria>>();
            services.AddScoped<IChildEntityService<PatIRRemuneration, PatIRRemunerationValuationMatrixData>, ChildEntityService<PatIRRemuneration, PatIRRemunerationValuationMatrixData>>();

            services.AddScoped<IViewModelService<PatIRFREmployeePosition>, ViewModelService<PatIRFREmployeePosition>>();
            services.AddScoped<IEntityService<PatIRFREmployeePosition>, AuxService<PatIRFREmployeePosition>>();
            services.AddScoped<IViewModelService<PatIRFRTurnOver>, ViewModelService<PatIRFRTurnOver>>();
            services.AddScoped<IEntityService<PatIRFRTurnOver>, AuxService<PatIRFRTurnOver>>();
            services.AddScoped<IViewModelService<PatIRFRStaggering>, ViewModelService<PatIRFRStaggering>>();
            services.AddScoped<IEntityService<PatIRFRStaggering>, AuxService<PatIRFRStaggering>>();
            services.AddScoped<IChildEntityService<PatIRFRStaggering, PatIRFRStaggeringDetail>, ChildEntityService<PatIRFRStaggering, PatIRFRStaggeringDetail>>();
            services.AddScoped<IViewModelService<PatIRFRValorizationRule>, ViewModelService<PatIRFRValorizationRule>>();
            services.AddScoped<IEntityService<PatIRFRValorizationRule>, AuxService<PatIRFRValorizationRule>>();
            services.AddScoped<IEntityService<PatIRFRDistribution>, AuxService<PatIRFRDistribution>>();
            services.AddScoped<IEntityService<PatIRFRRemuneration>, AuxService<PatIRFRRemuneration>>();
            services.AddScoped<IChildEntityService<PatIRFRRemuneration, PatInventorInv>, ChildEntityService<PatIRFRRemuneration, PatInventorInv>>();
            services.AddScoped<IChildEntityService<PatIRFRRemuneration, PatIRFRProductSale>, ChildEntityService<PatIRFRRemuneration, PatIRFRProductSale>>();
            services.AddScoped<IViewModelService<PatIRFRRemunerationFormula>, ViewModelService<PatIRFRRemunerationFormula>>();
            services.AddScoped<IEntityService<PatIRFRRemunerationFormula>, AuxService<PatIRFRRemunerationFormula>>();
            services.AddScoped<IEntityService<PatIRFRRemunerationType>, AuxService<PatIRFRRemunerationType>>();
            services.AddScoped<IViewModelService<PatIRFRRemunerationFormulaFactor>, ViewModelService<PatIRFRRemunerationFormulaFactor>>();
            services.AddScoped<IEntityService<PatIRFRRemunerationFormulaFactor>, AuxService<PatIRFRRemunerationFormulaFactor>>();
            services.AddScoped<IViewModelService<PatIRFRRemunerationValuationMatrix>, ViewModelService<PatIRFRRemunerationValuationMatrix>>();
            services.AddScoped<IEntityService<PatIRFRRemunerationValuationMatrix>, AuxService<PatIRFRRemunerationValuationMatrix>>();
            services.AddScoped<IEntityService<PatIRFRRemunerationValuationMatrixType>, AuxService<PatIRFRRemunerationValuationMatrixType>>();
            services.AddScoped<IChildEntityService<PatIRFRRemunerationValuationMatrix, PatIRFRRemunerationValuationMatrixCriteria>, ChildEntityService<PatIRFRRemunerationValuationMatrix, PatIRFRRemunerationValuationMatrixCriteria>>();
            services.AddScoped<IChildEntityService<PatIRFRRemuneration, PatIRFRRemunerationValuationMatrixData>, ChildEntityService<PatIRFRRemuneration, PatIRFRRemunerationValuationMatrixData>>();

            services.AddScoped<IViewModelService<PatApplicationStatus>, ViewModelService<PatApplicationStatus>>();
            services.AddScoped<IEntityService<PatApplicationStatus>, AuxService<PatApplicationStatus>>();
            services.AddScoped<IViewModelService<PatUPCStatus>, ViewModelService<PatUPCStatus>>();
            services.AddScoped<IEntityService<PatUPCStatus>, AuxService<PatUPCStatus>>();
            services.AddScoped<IChildEntityService<Invention, PatPriority>, PatPriorityService>();
            services.AddScoped<IPatAbstractService, PatAbstractService>();
            services.AddScoped<IChildEntityService<Invention, PatKeyword>, PatInventionChildService<PatKeyword>>();
            services.AddScoped<IViewModelService<PatIndicator>, ViewModelService<PatIndicator>>();
            services.AddScoped<IEntityService<PatIndicator>, AuxService<PatIndicator>>();
            services.AddScoped<IViewModelService<PatRelatedCaseDTO>, ViewModelService<PatRelatedCaseDTO>>();
            services.AddScoped<IAsyncRepository<PatRelatedCaseDTO>, EFRepository<PatRelatedCaseDTO>>();
            services.AddScoped<IViewModelService<PatIDSRelatedCaseCopyDTO>, ViewModelService<PatIDSRelatedCaseCopyDTO>>();
            services.AddScoped<IAsyncRepository<PatIDSRelatedCaseDTO>, EFRepository<PatIDSRelatedCaseDTO>>();
            services.AddScoped<IViewModelService<PatIDSNonPatLiterature>, ViewModelService<PatIDSNonPatLiterature>>();
            services.AddScoped<IAsyncRepository<PatIDSNonPatLiterature>, EFRepository<PatIDSNonPatLiterature>>();

            services.AddScoped<IViewModelService<PatInventor>, ViewModelService<PatInventor>>();
            services.AddScoped<IPatInventorService, PatInventorService>();

            services.AddScoped<ICountryApplicationRepository, CountryApplicationRepository>();
            services.AddScoped<ICountryApplicationViewModelService, CountryApplicationViewModelService>();
            services.AddScoped<ICountryApplicationService, CountryApplicationService>();
            services.AddScoped<IPatTaxStartExpirationService, PatTaxStartExpirationService>();
            services.AddScoped<IAsyncRepository<PatLicensee>, EFRepository<PatLicensee>>();
            services.AddScoped<IPatTaxStartExpirationRepository, PatTaxStartExpirationRepository>();

            services.AddScoped<IChildEntityService<Invention, InventionRelatedDisclosure>, PatInventionChildService<InventionRelatedDisclosure>>();
            services.AddScoped<IChildEntityService<Invention, InventionRelatedInvention>, PatInventionChildService<InventionRelatedInvention>>();

            services.AddScoped<IPatIDSService, PatIDSService>();
            services.AddScoped<IPatIDSRepository, PatIDSRepository>();
            services.AddScoped<IPatIDSManageService, PatIDSManageService>();

            services.AddScoped<IMultipleEntityService<Invention, PatInventorInv>, PatInventorInvService>();
            services.AddScoped<IMultipleEntityService<Invention, PatOwnerInv>, PatOwnerInvService>();
            services.AddScoped<IChildEntityService<Invention, PatProductInv>, PatInventionChildService<PatProductInv>>();

            services.AddScoped<IEntityService<PatInventorInv>, AuxService<PatInventorInv>>();
            services.AddScoped<IChildEntityService<Invention, PatIRProductSale>, ChildEntityService<Invention, PatIRProductSale>>();
            services.AddScoped<IChildEntityService<Invention, GMMatterPatent>, ChildEntityService<Invention, GMMatterPatent>>();

            services.AddScoped<IMultipleEntityService<PatOwnerApp>, PatOwnerAppService>();
            services.AddScoped<IMultipleEntityService<PatInventorApp>, PatInventorAppService>();

            services.AddScoped<IPatCostTrackingViewModelService, PatCostTrackingViewModelService>();
            services.AddScoped<ICostTrackingService<PatCostTrack>, PatCostTrackingService>();

            services.AddScoped<IPatCostTrackingInvViewModelService, PatCostTrackingInvViewModelService>();
            services.AddScoped<ICostTrackingService<PatCostTrackInv>, PatCostTrackingInvService>();
            services.AddScoped<IPatImageCostInvViewModelService, PatImageCostInvViewModelService>();

            services.AddScoped<IPatActionDueViewModelService, PatActionDueViewModelService>();
            services.AddScoped<IActionDueDeDocketService<PatActionDue, PatDueDate>, PatActionDueService>();
            services.AddScoped<IActionDueService<PatActionDue, PatDueDate>, PatActionDueService>();
            services.AddScoped<IDueDateService<PatActionDue, PatDueDate>, PatDueDateService>();
            services.AddScoped<IEntityService<PatDueDateDelegation>, AuxService<PatDueDateDelegation>>();

            services.AddScoped<IPatActionDueInvViewModelService, PatActionDueInvViewModelService>();
            services.AddScoped<IActionDueDeDocketService<PatActionDueInv, PatDueDateInv>, PatActionDueInvService>();
            services.AddScoped<IActionDueService<PatActionDueInv, PatDueDateInv>, PatActionDueInvService>();
            services.AddScoped<IDueDateService<PatActionDueInv, PatDueDateInv>, PatDueDateInvService>();
            services.AddScoped<IEntityService<PatDueDateInvDelegation>, AuxService<PatDueDateInvDelegation>>();

            services.AddScoped<IViewModelService<PatTaxBase>, ViewModelService<PatTaxBase>>();
            services.AddScoped<IParentEntityService<PatTaxBase, PatTaxYear>, ParentEntityService<PatTaxBase, PatTaxYear>>();

            services.AddScoped<IBaseService<PatDesCaseType>, BaseService<PatDesCaseType>>();

            services.AddScoped<IPatGlobalUpdateRepository, PatGlobalUpdateRepository>();
            services.AddScoped<IPatGlobalUpdateService, PatGlobalUpdateService>();
            services.AddScoped<IPatDelegationUtilityRepository, PatDelegationUtilityRepository>();
            services.AddScoped<IPatDelegationUtilityService, PatDelegationUtilityService>();

            services.AddScoped<IViewModelService<PatInventorAwardCriteria>, ViewModelService<PatInventorAwardCriteria>>();
            services.AddScoped<IEntityService<PatInventorAwardCriteria>, AuxService<PatInventorAwardCriteria>>();
            services.AddScoped<IViewModelService<PatInventorAppAward>, ViewModelService<PatInventorAppAward>>();
            services.AddScoped<IEntityService<PatInventorAppAward>, AuxService<PatInventorAppAward>>();
            services.AddScoped<IParentEntityService<PatInventor, PatInventorAppAward>, ParentEntityService<PatInventor, PatInventorAppAward>>();
            services.AddScoped<IParentEntityService<CountryApplication, PatInventorAppAward>, ParentEntityService<CountryApplication, PatInventorAppAward>>();
            services.AddScoped<IViewModelService<PatInventorAwardType>, ViewModelService<PatInventorAwardType>>();
            services.AddScoped<IEntityService<PatInventorAwardType>, AuxService<PatInventorAwardType>>();
            services.AddScoped<IParentEntityService<PatInventorAwardType, PatInventorAwardCriteria>, ParentEntityService<PatInventorAwardType, PatInventorAwardCriteria>>();
            services.AddScoped<IEntityService<PatInventorApp>, AuxService<PatInventorApp>>();
            services.AddScoped<IPatInventorAppAwardUpdateService, PatInventorAppRewardUpdateService>();
            services.AddScoped<IPatInventorRemunerationService, PatInventorRemunerationService>();
            services.AddScoped<IPatInventorFRRemunerationService, PatInventorFRRemunerationService>();
            services.AddScoped<IParentEntityService<PatCostType, PatBudgetManagement>, ParentEntityService<PatCostType, PatBudgetManagement>>();
            services.AddScoped<IEntityService<PatBudgetManagement>, AuxService<PatBudgetManagement>>();

            services.AddScoped<IEntityService<LSDText>, AuxService<LSDText>>();

            // pat invention images
            services.AddScoped<IPatImageInvViewModelService, PatImageInvViewModelService>();

            // pat ctryapp images
            services.AddScoped<IPatImageAppViewModelService, PatImageAppViewModelService>();

            // pat action images
            services.AddScoped<IPatImageActViewModelService, PatImageActViewModelService>();
            services.AddScoped<IPatImageActInvViewModelService, PatImageActInvViewModelService>();

            // pat cost tracking images
            services.AddScoped<IPatImageCostViewModelService, PatImageCostViewModelService>();

            //settings
            services.AddScoped<ISystemSettings<PatSetting>, SystemSettings<PatSetting>>();

            //cost tracking import
            services.AddScoped<IPatCostTrackingImportService, PatCostTrackingImportService>();
            services.AddScoped<IPatCostTrackingImportRepository, PatCostTrackingImportRepository>();

            //notification
            services.AddScoped<INotificationHub, NotificationHub>();

            services.AddScoped<IEntityService<PatCountryLawUpdate>, AuxService<PatCountryLawUpdate>>();

            //workflow
            services.AddScoped<IPatWorkflowService, PatWorkflowService>();
            services.AddScoped<IAsyncRepository<PatWorkflow>, EFRepository<PatWorkflow>>();
            services.AddScoped<IViewModelService<PatWorkflow>, ViewModelService<PatWorkflow>>();

            //API
            services.AddScoped<IInventionApiService, InventionApiService>();
            services.AddScoped<ICountryApplicationApiService, CountryApplicationApiService>();
            services.AddScoped<IPatAssignmentApiService, PatAssignmentApiService>();
            services.AddScoped<IWebApiBaseService<PatCostTrackWebSvc, PatCostTrack>, PatCostTrackingApiService>();
            services.AddScoped<IPatActionDueApiService, PatActionDueApiService>();
            services.AddScoped<IEntityService<PatIDSDownloadWebSvc>, AuxService<PatIDSDownloadWebSvc>>();
            services.AddScoped<IPatOwnerAppApiService, PatOwnerAppApiService>();
            services.AddScoped<IPatPriorityApiService, PatPriorityApiService>();

            //patent score
            services.AddScoped<IViewModelService<PatScoreCategory>, ViewModelService<PatScoreCategory>>();
            services.AddScoped<IEntityService<PatScoreCategory>, AuxService<PatScoreCategory>>();

            //cost estimator & setup
            services.AddScoped<IPatCEAnnuitySetupService, PatCEAnnuitySetupService>();
            services.AddScoped<IAsyncRepository<PatCEAnnuitySetup>, EFRepository<PatCEAnnuitySetup>>();
            services.AddScoped<IViewModelService<PatCEAnnuitySetup>, ViewModelService<PatCEAnnuitySetup>>();
            services.AddScoped<IChildEntityService<PatCEAnnuitySetup, PatCEAnnuityCost>, ChildEntityService<PatCEAnnuitySetup, PatCEAnnuityCost>>();

            services.AddScoped<IPatCECountrySetupService, PatCECountrySetupService>();
            services.AddScoped<IAsyncRepository<PatCECountrySetup>, EFRepository<PatCECountrySetup>>();
            services.AddScoped<IViewModelService<PatCECountrySetup>, ViewModelService<PatCECountrySetup>>();

            services.AddScoped<IPatCEGeneralSetupService, PatCEGeneralSetupService>();
            services.AddScoped<IAsyncRepository<PatCEGeneralSetup>, EFRepository<PatCEGeneralSetup>>();
            services.AddScoped<IViewModelService<PatCEGeneralSetup>, ViewModelService<PatCEGeneralSetup>>();

            services.AddScoped<IViewModelService<PatCEFee>, ViewModelService<PatCEFee>>();
            services.AddScoped<IEntityService<PatCEFee>, AuxService<PatCEFee>>();
            services.AddScoped<IParentEntityService<PatCEFee, PatCEFeeDetail>, ParentEntityService<PatCEFee, PatCEFeeDetail>>();

            services.AddScoped<IViewModelService<PatCEStage>, ViewModelService<PatCEStage>>();
            services.AddScoped<IEntityService<PatCEStage>, AuxService<PatCEStage>>();

            services.AddScoped<IPatCostEstimatorService, PatCostEstimatorService>();
            services.AddScoped<IAsyncRepository<PatCostEstimator>, EFRepository<PatCostEstimator>>();
            services.AddScoped<IViewModelService<PatCostEstimator>, ViewModelService<PatCostEstimator>>();
            services.AddScoped<IChildEntityService<PatCostEstimator, PatCostEstimatorCountry>, PatCostEstimatorChildService<PatCostEstimatorCountry>>();
            services.AddScoped<IChildEntityService<PatCostEstimator, PatCostEstimatorCountryCost>, PatCostEstimatorChildService<PatCostEstimatorCountryCost>>();
            services.AddScoped<IChildEntityService<PatCostEstimator, PatCEQuestionGeneral>, PatCostEstimatorChildService<PatCEQuestionGeneral>>();

            services.AddScoped<IEntityService<PatEGrantDownloaded>, AuxService<PatEGrantDownloaded>>();
            services.AddScoped<IEntityService<PatTerminalDisclaimerChecked>, AuxService<PatTerminalDisclaimerChecked>>();

            services.AddScoped<IChildEntityService<Invention, GMMatterPatent>, ChildEntityService<Invention, GMMatterPatent>>();
            services.AddScoped<IChildEntityService<CountryApplication, GMMatterPatent>, ChildEntityService<CountryApplication, GMMatterPatent>>();

            //EPO
            services.AddScoped<IEPOService, EPOService>();
            services.AddScoped<IEntityService<EPOPortfolio>, AuxService<EPOPortfolio>>();
            services.AddScoped<IEntityService<EPOApplication>, AuxService<EPOApplication>>();
            services.AddScoped<IEntityService<EPODueDate>, AuxService<EPODueDate>>();

            services.AddScoped<IEntityService<EPOCommunication>, AuxService<EPOCommunication>>();
            services.AddScoped<IParentEntityService<EPOCommunication, EPOCommunicationDoc>, ParentEntityService<EPOCommunication, EPOCommunicationDoc>>();
            services.AddScoped<IEntityService<PatEPODocumentCombined>, AuxService<PatEPODocumentCombined>>();
            services.AddScoped<IEntityService<PatEPOMailLog>, AuxService<PatEPOMailLog>>();

            services.AddScoped<IViewModelService<PatEPODocumentMerge>, ViewModelService<PatEPODocumentMerge>>();
            services.AddScoped<IParentEntityService<PatEPODocumentMerge, PatEPODocumentMergeGuide>, ParentEntityService<PatEPODocumentMerge, PatEPODocumentMergeGuide>>();
            services.AddScoped<IParentEntityService<PatEPODocumentMergeGuide, PatEPODocumentMergeGuideSub>, ParentEntityService<PatEPODocumentMergeGuide, PatEPODocumentMergeGuideSub>>();

            services.AddScoped<IViewModelService<PatEPODocumentMap>, ViewModelService<PatEPODocumentMap>>();
            services.AddScoped<IEntityService<PatEPODocumentMap>, AuxService<PatEPODocumentMap>>();
            services.AddScoped<IChildEntityService<PatEPODocumentMap, PatEPODocumentMapAct>, PatEPODocumentMapChildService<PatEPODocumentMapAct>>();
            services.AddScoped<IChildEntityService<PatEPODocumentMap, PatEPODocumentMapTag>, PatEPODocumentMapChildService<PatEPODocumentMapTag>>();

            services.AddScoped<IEntityService<EPODueDateTerm>, AuxService<EPODueDateTerm>>();
            services.AddScoped<IViewModelService<EPODueDateTerm>, ViewModelService<EPODueDateTerm>>();
            services.AddScoped<IParentEntityService<EPODueDateTerm, PatEPOActionMapAct>, ParentEntityService<EPODueDateTerm, PatEPOActionMapAct>>();
            services.AddScoped<IEntityService<PatEPOAppLog>, AuxService<PatEPOAppLog>>();
            services.AddScoped<IEntityService<PatEPOCommActLog>, AuxService<PatEPOCommActLog>>();
            services.AddScoped<IEntityService<PatEPODDActLog>, AuxService<PatEPODDActLog>>();

            services.AddScoped<IEntityService<PatOPSLog>, AuxService<PatOPSLog>>();

            return services;
        }
    }
}
