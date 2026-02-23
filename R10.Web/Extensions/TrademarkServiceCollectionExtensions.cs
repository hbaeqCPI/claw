using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Core.Interfaces.Trademark;
using R10.Core.Services;
using R10.Core.Services.Shared;
using R10.Core.Services.Trademark;
using R10.Infrastructure.Data;
using R10.Infrastructure.Data.Patent;
using R10.Infrastructure.Data.Trademark;
using R10.Web.Areas.Trademark.Services;
using R10.Web.Interfaces;
using R10.Web.Services;

namespace R10.Web.Extensions
{
    public static class TrademarkServiceCollectionExtensions
    {
        public static IServiceCollection AddTrademark(this IServiceCollection services)
        {
            services.AddScoped<IViewModelService<TmkCountry>, ViewModelService<TmkCountry>>();
            services.AddScoped<IParentEntityService<TmkCountry, TmkAreaCountry>, ParentEntityService<TmkCountry, TmkAreaCountry>>();

            services.AddScoped<IViewModelService<TmkArea>, ViewModelService<TmkArea>>();
            services.AddScoped<IParentEntityService<TmkArea, TmkAreaCountry>, ParentEntityService<TmkArea, TmkAreaCountry>>();

            services.AddScoped<IViewModelService<TmkAssignmentStatus>, ViewModelService<TmkAssignmentStatus>>();
            services.AddScoped<IEntityService<TmkAssignmentStatus>, AuxService<TmkAssignmentStatus>>();

            services.AddScoped<IViewModelService<TmkCaseType>, ViewModelService<TmkCaseType>>();
            services.AddScoped<IEntityService<TmkCaseType>, AuxService<TmkCaseType>>();

            services.AddScoped<IEntityService<TmkCountry>, AuxService<TmkCountry>>();

            services.AddScoped<IViewModelService<TmkConflictStatus>, ViewModelService<TmkConflictStatus>>();
            services.AddScoped<IEntityService<TmkConflictStatus>, AuxService<TmkConflictStatus>>();

            services.AddScoped<IViewModelService<TmkCostType>, ViewModelService<TmkCostType>>();
            services.AddScoped<IEntityService<TmkCostType>, AuxService<TmkCostType>>();

            services.AddScoped<IViewModelService<TmkIndicator>, ViewModelService<TmkIndicator>>();
            services.AddScoped<IEntityService<TmkIndicator>, AuxService<TmkIndicator>>();

            services.AddScoped<IViewModelService<TmkMarkType>, ViewModelService<TmkMarkType>>();
            services.AddScoped<IEntityService<TmkMarkType>, AuxService<TmkMarkType>>();

            services.AddScoped<IViewModelService<TmkStandardGood>, ViewModelService<TmkStandardGood>>();
            services.AddScoped<IEntityService<TmkStandardGood>, EntityService<TmkStandardGood>>();

            services.AddScoped<IViewModelService<TmkTrademarkStatus>, ViewModelService<TmkTrademarkStatus>>();
            services.AddScoped<IEntityService<TmkTrademarkStatus>, AuxService<TmkTrademarkStatus>>();

            services.AddScoped<IViewModelService<TmkActionType>, ViewModelService<TmkActionType>>();
            services.AddScoped<IParentEntityService<TmkActionType, TmkActionParameter>, ParentEntityService<TmkActionType, TmkActionParameter>>();

            services.AddScoped<ITmkTrademarkRepository, TmkTrademarkRepository>();
            services.AddScoped<ITmkTrademarkViewModelService, TmkTrademarkViewModelService>();
            services.AddScoped<ITmkTrademarkService, TmkTrademarkService>();

            services.AddScoped<IChildEntityService<TmkTrademark, TmkLicensee>, TmkTrademarkChildService<TmkLicensee>>();
            services.AddScoped<IChildEntityService<TmkTrademark, TmkAssignmentHistory>, TmkTrademarkChildService<TmkAssignmentHistory>>();
            services.AddScoped<IChildEntityService<TmkTrademark, TmkTrademarkClass>, TmkTrademarkChildService<TmkTrademarkClass>>();
            services.AddScoped<IChildEntityService<TmkTrademark, TmkKeyword>, TmkTrademarkChildService<TmkKeyword>>();
            services.AddScoped<IChildEntityService<TmkTrademark, PatRelatedTrademark>, TmkTrademarkChildService<PatRelatedTrademark>>();

            services.AddScoped<ITmkCostTrackingViewModelService, TmkCostTrackingViewModelService>();
            services.AddScoped<ICostTrackingService<TmkCostTrack>, TmkCostTrackingService>();

            services.AddScoped<ITmkActionDueViewModelService, TmkActionDueViewModelService>();
            services.AddScoped<IActionDueDeDocketService<TmkActionDue, TmkDueDate>, TmkActionDueService>();
            services.AddScoped<IActionDueService<TmkActionDue, TmkDueDate>, TmkActionDueService>();
            services.AddScoped<IDueDateService<TmkActionDue, TmkDueDate>, TmkDueDateService>();
            services.AddScoped<IEntityService<TmkDueDateDelegation>, AuxService<TmkDueDateDelegation>>();

            services.AddScoped<ITmkConflictViewModelService, TmkConflictViewModelService>();
            services.AddScoped<ITmkConflictService, TmkConflictService>();
            services.AddScoped<ITmkConflictRepository, TmkConflictRepository>();

            services.AddScoped<ITmkDesignationRepository, TmkDesignationRepository>();
            services.AddScoped<ITmkDesignationService, TmkDesignationService>();

            services.AddScoped<IBaseService<TmkDesCaseType>, BaseService<TmkDesCaseType>>();

            services.AddScoped<IParentEntityService<TmkCostType, TmkBudgetManagement>, ParentEntityService<TmkCostType, TmkBudgetManagement>>();
            services.AddScoped<IEntityService<TmkBudgetManagement>, AuxService<TmkBudgetManagement>>();
            services.AddScoped<IMultipleEntityService<TmkTrademark, TmkOwner>, TmkOwnerService>();

            // tmk images
            services.AddScoped<ITmkImageViewModelService, TmkImageViewModelService>();

            // tmk cost tracking images
            services.AddScoped<ITmkImageCostViewModelService, TmkImageCostViewModelService>();

            // tmk action images
            services.AddScoped<ITmkImageActViewModelService, TmkImageActViewModelService>();

            //settings
            services.AddScoped<ISystemSettings<TmkSetting>, SystemSettings<TmkSetting>>();

            //country law
            services.AddScoped<ITmkCountryLawService, TmkCountryLawService>();
            services.AddScoped<IAsyncRepository<TmkCountryLaw>, EFRepository<TmkCountryLaw>>();
            services.AddScoped<ITmkCountryDueRepository, TmkCountryDueRepository>();
            services.AddScoped<IViewModelService<TmkCountryLaw>, ViewModelService<TmkCountryLaw>>();

            //global update
            services.AddScoped<ITmkGlobalUpdateRepository, TmkGlobalUpdateRepository>();
            services.AddScoped<ITmkGlobalUpdateService, TmkGlobalUpdateService>();
            services.AddScoped<ITmkDelegationUtilityRepository, TmkDelegationUtilityRepository>();
            services.AddScoped<ITmkDelegationUtilityService, TmkDelegationUtilityService>();

            //cost tracking import
            services.AddScoped<ITmkCostTrackingImportService, TmkCostTrackingImportService>();
            services.AddScoped<ITmkCostTrackingImportRepository, TmkCostTrackingImportRepository>();

            services.AddScoped<IEntityService<TmkCountryLawUpdate>, AuxService<TmkCountryLawUpdate>>();
            services.AddScoped<IEntityService<TmkOwner>, AuxService<TmkOwner>>();

            //workflow
            services.AddScoped<ITmkWorkflowService, TmkWorkflowService>();
            services.AddScoped<IAsyncRepository<TmkWorkflow>, EFRepository<TmkWorkflow>>();
            services.AddScoped<IViewModelService<TmkWorkflow>, ViewModelService<TmkWorkflow>>();

            //API
            services.AddScoped<ITmkTrademarkApiService, TmkTrademarkApiService>();
            services.AddScoped<ITmkActionDueApiService, TmkActionDueApiService>();
            services.AddScoped<ITmkTrademarkClassApiService, TmkTrademarkClassApiService>();
            services.AddScoped<ITmkAssignmentApiService, TmkAssignmentApiService>();
            services.AddScoped<IWebApiBaseService<TmkCostTrackWebSvc, TmkCostTrack>, TmkCostTrackingApiService>();
            services.AddScoped<ITmkOwnerApiService, TmkOwnerApiService>();

            //cost estimator & setup           
            services.AddScoped<ITmkCECountrySetupService, TmkCECountrySetupService>();
            services.AddScoped<IAsyncRepository<TmkCECountrySetup>, EFRepository<TmkCECountrySetup>>();
            services.AddScoped<IViewModelService<TmkCECountrySetup>, ViewModelService<TmkCECountrySetup>>();

            services.AddScoped<ITmkCEGeneralSetupService, TmkCEGeneralSetupService>();
            services.AddScoped<IAsyncRepository<TmkCEGeneralSetup>, EFRepository<TmkCEGeneralSetup>>();
            services.AddScoped<IViewModelService<TmkCEGeneralSetup>, ViewModelService<TmkCEGeneralSetup>>();

            services.AddScoped<IViewModelService<TmkCEFee>, ViewModelService<TmkCEFee>>();
            services.AddScoped<IEntityService<TmkCEFee>, AuxService<TmkCEFee>>();
            services.AddScoped<IParentEntityService<TmkCEFee, TmkCEFeeDetail>, ParentEntityService<TmkCEFee, TmkCEFeeDetail>>();

            services.AddScoped<IViewModelService<TmkCEStage>, ViewModelService<TmkCEStage>>();
            services.AddScoped<IEntityService<TmkCEStage>, AuxService<TmkCEStage>>();

            services.AddScoped<ITmkCostEstimatorService, TmkCostEstimatorService>();
            services.AddScoped<IAsyncRepository<TmkCostEstimator>, EFRepository<TmkCostEstimator>>();
            services.AddScoped<IViewModelService<TmkCostEstimator>, ViewModelService<TmkCostEstimator>>();
            services.AddScoped<IChildEntityService<TmkCostEstimator, TmkCostEstimatorCountry>, TmkCostEstimatorChildService<TmkCostEstimatorCountry>>();
            services.AddScoped<IChildEntityService<TmkCostEstimator, TmkCostEstimatorCountryCost>, TmkCostEstimatorChildService<TmkCostEstimatorCountryCost>>();
            services.AddScoped<IChildEntityService<TmkCostEstimator, TmkCEQuestionGeneral>, TmkCostEstimatorChildService<TmkCEQuestionGeneral>>();
            
            services.AddScoped<IChildEntityService<TmkTrademark, GMMatterTrademark>, ChildEntityService<TmkTrademark, GMMatterTrademark>>();

            return services;
        }
    }


}
