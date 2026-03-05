using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Services;
using R10.Infrastructure.Data;
using R10.Infrastructure.Data.Patent;
using R10.Web.Interfaces;
using R10.Web.Services;

namespace R10.Web.Extensions
{
    public static class PatentServiceCollectionExtensions
    {
        public static IServiceCollection AddPatent(this IServiceCollection services)
        {
            services.AddScoped<IPatCountryLawService, PatCountryLawService>();
            services.AddScoped<IAsyncRepository<PatCountryLaw>, EFRepository<PatCountryLaw>>();
            services.AddScoped<IPatCountryDueRepository, PatCountryDueRepository>();
            services.AddScoped<IViewModelService<PatCountryLaw>, ViewModelService<PatCountryLaw>>();

            services.AddScoped<IViewModelService<PatCountry>, ViewModelService<PatCountry>>();
            services.AddScoped<IParentEntityService<PatCountry, PatAreaCountry>, ParentEntityService<PatCountry, PatAreaCountry>>();
            services.AddScoped<IEntityService<PatCountry>, AuxService<PatCountry>>();

            services.AddScoped<IViewModelService<PatDisclosureStatus>, ViewModelService<PatDisclosureStatus>>();
            services.AddScoped<IEntityService<PatDisclosureStatus>, AuxService<PatDisclosureStatus>>();

            services.AddScoped<IViewModelService<PatArea>, ViewModelService<PatArea>>();
            services.AddScoped<IParentEntityService<PatArea, PatAreaCountry>, ParentEntityService<PatArea, PatAreaCountry>>();

            services.AddScoped<IViewModelService<PatApplicationStatus>, ViewModelService<PatApplicationStatus>>();
            services.AddScoped<IEntityService<PatApplicationStatus>, AuxService<PatApplicationStatus>>();

            services.AddScoped<IViewModelService<PatActionType>, ViewModelService<PatActionType>>();
            services.AddScoped<IParentEntityService<PatActionType, PatActionParameter>, ParentEntityService<PatActionType, PatActionParameter>>();

            services.AddScoped<IViewModelService<PatCaseType>, ViewModelService<PatCaseType>>();
            services.AddScoped<IEntityService<PatCaseType>, AuxService<PatCaseType>>();

            services.AddScoped<IBaseService<PatDesCaseType>, BaseService<PatDesCaseType>>();

            //settings
            services.AddScoped<ISystemSettings<PatSetting>, SystemSettings<PatSetting>>();

            services.AddScoped<IEntityService<PatCountryLawUpdate>, AuxService<PatCountryLawUpdate>>();

            //workflow
            services.AddScoped<IPatWorkflowService, PatWorkflowService>();
            services.AddScoped<IAsyncRepository<PatWorkflow>, EFRepository<PatWorkflow>>();
            services.AddScoped<IViewModelService<PatWorkflow>, ViewModelService<PatWorkflow>>();

            //patent score
            services.AddScoped<IViewModelService<PatScoreCategory>, ViewModelService<PatScoreCategory>>();
            services.AddScoped<IEntityService<PatScoreCategory>, AuxService<PatScoreCategory>>();

            return services;
        }
    }
}
