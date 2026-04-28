using Microsoft.Extensions.DependencyInjection;
using LawPortal.Core.Entities.Trademark;
using LawPortal.Core.Interfaces;
using LawPortal.Core.Services;
using LawPortal.Infrastructure.Data;
using LawPortal.Infrastructure.Data.Trademark;
using LawPortal.Web.Interfaces;
using LawPortal.Web.Services;

namespace LawPortal.Web.Extensions
{
    public static class TrademarkServiceCollectionExtensions
    {
        public static IServiceCollection AddTrademark(this IServiceCollection services)
        {
            services.AddScoped<ITmkCountryLawService, TmkCountryLawService>();
            services.AddScoped<IAsyncRepository<TmkCountryLaw>, EFRepository<TmkCountryLaw>>();
            services.AddScoped<ITmkCountryDueRepository, TmkCountryDueRepository>();
            services.AddScoped<IViewModelService<TmkCountryLaw>, ViewModelService<TmkCountryLaw>>();

            services.AddScoped<ITmkDesignationRepository, TmkDesignationRepository>();
            services.AddScoped<ITmkDesignationService, TmkDesignationService>();

            services.AddScoped<IViewModelService<TmkCountry>, ViewModelService<TmkCountry>>();
            services.AddScoped<IEntityService<TmkCountry>, AuxService<TmkCountry>>();

            services.AddScoped<IViewModelService<TmkArea>, ViewModelService<TmkArea>>();
            services.AddScoped<IEntityService<TmkArea>, AuxService<TmkArea>>();
            services.AddScoped<IViewModelService<TmkAreaCountry>, ViewModelService<TmkAreaCountry>>();
            services.AddScoped<IEntityService<TmkAreaCountry>, AuxService<TmkAreaCountry>>();

            services.AddScoped<IViewModelService<TmkCaseType>, ViewModelService<TmkCaseType>>();
            services.AddScoped<IEntityService<TmkCaseType>, AuxService<TmkCaseType>>();

            services.AddScoped<IViewModelService<TmkActionType>, ViewModelService<TmkActionType>>();
            services.AddScoped<IEntityService<TmkActionType>, AuxService<TmkActionType>>();
            services.AddScoped<IChildEntityService<TmkActionType, TmkActionParameter>, ChildEntityService<TmkActionType, TmkActionParameter>>();
            services.AddScoped<IEntityService<TmkActionParameter>, AuxService<TmkActionParameter>>();

            services.AddScoped<IBaseService<TmkDesCaseType>, BaseService<TmkDesCaseType>>();
            services.AddScoped<IViewModelService<TmkDesCaseType>, ViewModelService<TmkDesCaseType>>();
            services.AddScoped<IEntityService<TmkDesCaseType>, AuxService<TmkDesCaseType>>();

            services.AddScoped<IViewModelService<TmkCountryDue>, ViewModelService<TmkCountryDue>>();
            services.AddScoped<IEntityService<TmkCountryDue>, AuxService<TmkCountryDue>>();

            services.AddScoped<IViewModelService<TmkDesCaseTypeFields>, ViewModelService<TmkDesCaseTypeFields>>();
            services.AddScoped<IEntityService<TmkDesCaseTypeFields>, AuxService<TmkDesCaseTypeFields>>();

            services.AddScoped<IEntityService<TmkCountryLawUpdate>, AuxService<TmkCountryLawUpdate>>();

            //standard goods
            services.AddScoped<IViewModelService<TmkStandardGood>, ViewModelService<TmkStandardGood>>();
            services.AddScoped<IEntityService<TmkStandardGood>, AuxService<TmkStandardGood>>();

            //indicator
            services.AddScoped<IViewModelService<TmkIndicator>, ViewModelService<TmkIndicator>>();
            services.AddScoped<IEntityService<TmkIndicator>, AuxService<TmkIndicator>>();

            // Delete/Ext tables
            services.AddScoped<IViewModelService<TmkAreaDelete>, ViewModelService<TmkAreaDelete>>();
            services.AddScoped<IEntityService<TmkAreaDelete>, AuxService<TmkAreaDelete>>();
            services.AddScoped<IViewModelService<TmkAreaCountryDelete>, ViewModelService<TmkAreaCountryDelete>>();
            services.AddScoped<IEntityService<TmkAreaCountryDelete>, AuxService<TmkAreaCountryDelete>>();
            services.AddScoped<IViewModelService<TmkDesCaseTypeExt>, ViewModelService<TmkDesCaseTypeExt>>();
            services.AddScoped<IEntityService<TmkDesCaseTypeExt>, AuxService<TmkDesCaseTypeExt>>();
            services.AddScoped<IViewModelService<TmkDesCaseTypeDelete>, ViewModelService<TmkDesCaseTypeDelete>>();
            services.AddScoped<IEntityService<TmkDesCaseTypeDelete>, AuxService<TmkDesCaseTypeDelete>>();
            services.AddScoped<IViewModelService<TmkDesCaseTypeDeleteExt>, ViewModelService<TmkDesCaseTypeDeleteExt>>();
            services.AddScoped<IEntityService<TmkDesCaseTypeDeleteExt>, AuxService<TmkDesCaseTypeDeleteExt>>();
            services.AddScoped<IViewModelService<TmkDesCaseTypeFieldsExt>, ViewModelService<TmkDesCaseTypeFieldsExt>>();
            services.AddScoped<IEntityService<TmkDesCaseTypeFieldsExt>, AuxService<TmkDesCaseTypeFieldsExt>>();
            services.AddScoped<IViewModelService<TmkDesCaseTypeFieldsDelete>, ViewModelService<TmkDesCaseTypeFieldsDelete>>();
            services.AddScoped<IEntityService<TmkDesCaseTypeFieldsDelete>, AuxService<TmkDesCaseTypeFieldsDelete>>();
            services.AddScoped<IViewModelService<TmkDesCaseTypeFieldsDeleteExt>, ViewModelService<TmkDesCaseTypeFieldsDeleteExt>>();
            services.AddScoped<IEntityService<TmkDesCaseTypeFieldsDeleteExt>, AuxService<TmkDesCaseTypeFieldsDeleteExt>>();

            return services;
        }
    }
}
