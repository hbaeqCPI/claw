using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Core.Services;
using R10.Infrastructure.Data;
using R10.Infrastructure.Data.Trademark;
using R10.Web.Interfaces;
using R10.Web.Services;

namespace R10.Web.Extensions
{
    public static class TrademarkServiceCollectionExtensions
    {
        public static IServiceCollection AddTrademark(this IServiceCollection services)
        {
            services.AddScoped<ITmkCountryLawService, TmkCountryLawService>();
            services.AddScoped<IAsyncRepository<TmkCountryLaw>, EFRepository<TmkCountryLaw>>();
            services.AddScoped<ITmkCountryDueRepository, TmkCountryDueRepository>();
            services.AddScoped<IViewModelService<TmkCountryLaw>, ViewModelService<TmkCountryLaw>>();

            services.AddScoped<ITmkWorkflowService, TmkWorkflowService>();
            services.AddScoped<IAsyncRepository<TmkWorkflow>, EFRepository<TmkWorkflow>>();
            services.AddScoped<IViewModelService<TmkWorkflow>, ViewModelService<TmkWorkflow>>();

            services.AddScoped<ITmkDesignationRepository, TmkDesignationRepository>();
            services.AddScoped<ITmkDesignationService, TmkDesignationService>();

            services.AddScoped<ITmkConflictRepository, TmkConflictRepository>();

            services.AddScoped<IViewModelService<TmkCountry>, ViewModelService<TmkCountry>>();
            services.AddScoped<IParentEntityService<TmkCountry, TmkAreaCountry>, ParentEntityService<TmkCountry, TmkAreaCountry>>();
            services.AddScoped<IEntityService<TmkCountry>, AuxService<TmkCountry>>();

            services.AddScoped<IViewModelService<TmkArea>, ViewModelService<TmkArea>>();
            services.AddScoped<IParentEntityService<TmkArea, TmkAreaCountry>, ParentEntityService<TmkArea, TmkAreaCountry>>();

            services.AddScoped<IViewModelService<TmkCaseType>, ViewModelService<TmkCaseType>>();
            services.AddScoped<IEntityService<TmkCaseType>, AuxService<TmkCaseType>>();

            services.AddScoped<IViewModelService<TmkConflictStatus>, ViewModelService<TmkConflictStatus>>();
            services.AddScoped<IEntityService<TmkConflictStatus>, AuxService<TmkConflictStatus>>();

            services.AddScoped<IViewModelService<TmkActionType>, ViewModelService<TmkActionType>>();
            services.AddScoped<IParentEntityService<TmkActionType, TmkActionParameter>, ParentEntityService<TmkActionType, TmkActionParameter>>();

            services.AddScoped<IBaseService<TmkDesCaseType>, BaseService<TmkDesCaseType>>();

            services.AddScoped<IEntityService<TmkCountryLawUpdate>, AuxService<TmkCountryLawUpdate>>();

            //indicator
            services.AddScoped<IViewModelService<TmkIndicator>, ViewModelService<TmkIndicator>>();
            services.AddScoped<IEntityService<TmkIndicator>, AuxService<TmkIndicator>>();

            return services;
        }
    }
}
