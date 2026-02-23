
using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Interfaces;
using R10.Core.Interfaces.GeneralMatter;
using R10.Core.Interfaces.Patent;
using R10.Core.Services;
using R10.Core.Services.GeneralMatter;
using R10.Core.Services.Shared;
using R10.Infrastructure.Data;
using R10.Infrastructure.Data.GeneralMatter;
using R10.Infrastructure.Data.Patent;
//using R10.Web.Areas.GeneralMatter.Services; // Removed: Web Area services no longer exist
using R10.Web.Interfaces;
using R10.Web.Services;

namespace R10.Web.Extensions
{
    public static class GeneralMatterServiceCollectionExtension
    {
        public static IServiceCollection AddGeneralMatter(this IServiceCollection services)
        {
            services.AddScoped<IGMMatterCountryService, GMMatterCountryService>();
            services.AddScoped<IGMMatterAttorneyService, GMMatterAttorneyService>();
            services.AddScoped<IMultipleEntityService<GMMatter, GMMatterAttorney>, GMMatterAttorneyService>();
            services.AddScoped<IChildEntityService<GMMatter, GMMatterKeyword>, GMMatterChildService<GMMatterKeyword>>();
            services.AddScoped<IChildEntityService<GMMatter, GMMatterOtherParty>, GMMatterChildService<GMMatterOtherParty>>();
            services.AddScoped<IChildEntityService<GMMatter, GMMatterPatent>, GMMatterPatentService>();
            services.AddScoped<IChildEntityService<GMMatter, GMMatterTrademark>, GMMatterTrademarkService>();
            services.AddScoped<IChildEntityService<GMMatter, GMMatterOtherPartyPatent>, GMMatterChildService<GMMatterOtherPartyPatent>>();
            services.AddScoped<IChildEntityService<GMMatter, GMMatterOtherPartyTrademark>, GMMatterChildService<GMMatterOtherPartyTrademark>>();
            services.AddScoped<IChildEntityService<GMMatter, GMMatterRelatedMatter>, GMMatterChildService<GMMatterRelatedMatter>>();
            services.AddScoped<IChildEntityService<GMMatter, GMProduct>, GMMatterChildService<GMProduct>>();

            services.AddScoped<IGMMatterService, GMMatterService>();
            services.AddScoped<ICostTrackingService<GMCostTrack>, GMCostTrackingService>();
            //services.AddScoped<IGMCostTrackingViewModelService, GMCostTrackingViewModelService>(); // Removed: Web Area service

            //services.AddScoped<IGMMatterViewModelService, GMMatterViewModelService>(); // Removed: Web Area service

            //services.AddScoped<IGMActionDueViewModelService, GMActionDueViewModelService>(); // Removed: Web Area service
            services.AddScoped<IActionDueDeDocketService<GMActionDue, GMDueDate>, GMActionDueService>();
            services.AddScoped<IActionDueService<GMActionDue, GMDueDate>, GMActionDueService>();
            services.AddScoped<IDueDateService<GMActionDue, GMDueDate>, GMDueDateService>();
            services.AddScoped<IEntityService<GMDueDateDelegation>, AuxService<GMDueDateDelegation>>();

            services.AddScoped<IParentEntityService<GMCountry, GMAreaCountry>, ParentEntityService<GMCountry, GMAreaCountry>>();
            services.AddScoped<IParentEntityService<GMArea, GMAreaCountry>, ParentEntityService<GMArea, GMAreaCountry>>();

            services.AddScoped<IViewModelService<GMMatterType>, ViewModelService<GMMatterType>>();
            services.AddScoped<IEntityService<GMMatterType>, AuxService<GMMatterType>>();
            services.AddScoped<IViewModelService<GMMatterStatus>, ViewModelService<GMMatterStatus>>();
            services.AddScoped<IEntityService<GMMatterStatus>, AuxService<GMMatterStatus>>();
            services.AddScoped<IViewModelService<GMCountry>, ViewModelService<GMCountry>>();
            services.AddScoped<IViewModelService<GMArea>, ViewModelService<GMArea>>();
            services.AddScoped<IViewModelService<GMAgreementType>, ViewModelService<GMAgreementType>>();
            services.AddScoped<IEntityService<GMAgreementType>, AuxService<GMAgreementType>>();
            services.AddScoped<IViewModelService<GMExtent>, ViewModelService<GMExtent>>();
            services.AddScoped<IEntityService<GMExtent>, AuxService<GMExtent>>();
            services.AddScoped<IViewModelService<GMOtherPartyType>, ViewModelService<GMOtherPartyType>>();
            services.AddScoped<IEntityService<GMOtherPartyType>, AuxService<GMOtherPartyType>>();
            services.AddScoped<IViewModelService<GMOtherParty>, ViewModelService<GMOtherParty>>();
            services.AddScoped<IEntityService<GMOtherParty>, GMOtherPartyService>();
            services.AddScoped<IViewModelService<GMIndicator>, ViewModelService<GMIndicator>>();
            services.AddScoped<IEntityService<GMIndicator>, AuxService<GMIndicator>>();
            services.AddScoped<IViewModelService<GMCostType>, ViewModelService<GMCostType>>();
            services.AddScoped<IEntityService<GMCostType>, AuxService<GMCostType>>();
            services.AddScoped<IParentEntityService<GMCostType, GMBudgetManagement>, ParentEntityService<GMCostType, GMBudgetManagement>>();
            services.AddScoped<IEntityService<GMBudgetManagement>, AuxService<GMBudgetManagement>>();

            services.AddScoped<IViewModelService<GMActionType>, ViewModelService<GMActionType>>();
            services.AddScoped<IParentEntityService<GMActionType, GMActionParameter>, ParentEntityService<GMActionType, GMActionParameter>>();
            services.AddScoped<IEntityService<GMCountry>, AuxService<GMCountry>>();

            // gm images
            //services.AddScoped<IGMMatterImageViewModelService, GMMatterImageViewModelService>(); // Removed: Web Area service

            // gm cost tracking images
            //services.AddScoped<IGMMatterImageCostViewModelService, GMMatterImageCostViewModelService>(); // Removed: Web Area service

            // gm action images
            //services.AddScoped<IGMMatterImageActViewModelService, GMMatterImageActViewModelService>(); // Removed: Web Area service

            //settings
            services.AddScoped<ISystemSettings<GMSetting>, SystemSettings<GMSetting>>();

            //global update
            services.AddScoped<IGMGlobalUpdateRepository, GMGlobalUpdateRepository>();
            services.AddScoped<IGMGlobalUpdateService, GMGlobalUpdateService>();
            services.AddScoped<IGMDelegationUtilityRepository, GMDelegationUtilityRepository>();
            services.AddScoped<IGMDelegationUtilityService, GMDelegationUtilityService>();

            //workflow
            services.AddScoped<IGMWorkflowService, GMWorkflowService>();
            services.AddScoped<IAsyncRepository<GMWorkflow>, EFRepository<GMWorkflow>>();
            services.AddScoped<IViewModelService<GMWorkflow>, ViewModelService<GMWorkflow>>();
            services.AddScoped<IEntityService<GMActionType>, AuxService<GMActionType>>();

            //cost tracking import
            services.AddScoped<IGMCostTrackingImportService, GMCostTrackingImportService>();
            services.AddScoped<IGMCostTrackingImportRepository, GMCostTrackingImportRepository>();

            return services;
        }
    }
}
