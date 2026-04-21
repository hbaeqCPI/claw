using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Services;
using R10.Infrastructure.Data;
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
            services.AddScoped<IEntityService<PatCountry>, AuxService<PatCountry>>();

            services.AddScoped<IViewModelService<PatArea>, ViewModelService<PatArea>>();
            services.AddScoped<IEntityService<PatArea>, AuxService<PatArea>>();
            services.AddScoped<IViewModelService<PatAreaCountry>, ViewModelService<PatAreaCountry>>();
            services.AddScoped<IEntityService<PatAreaCountry>, AuxService<PatAreaCountry>>();

            services.AddScoped<IViewModelService<PatActionType>, ViewModelService<PatActionType>>();
            services.AddScoped<IEntityService<PatActionType>, AuxService<PatActionType>>();
            services.AddScoped<IChildEntityService<PatActionType, PatActionParameter>, ChildEntityService<PatActionType, PatActionParameter>>();
            services.AddScoped<IEntityService<PatActionParameter>, AuxService<PatActionParameter>>();

            services.AddScoped<IViewModelService<PatCaseType>, ViewModelService<PatCaseType>>();
            services.AddScoped<IEntityService<PatCaseType>, AuxService<PatCaseType>>();

            services.AddScoped<IBaseService<PatDesCaseType>, BaseService<PatDesCaseType>>();
            services.AddScoped<IViewModelService<PatDesCaseType>, ViewModelService<PatDesCaseType>>();
            services.AddScoped<IEntityService<PatDesCaseType>, AuxService<PatDesCaseType>>();

            services.AddScoped<IViewModelService<PatCountryDue>, ViewModelService<PatCountryDue>>();
            services.AddScoped<IEntityService<PatCountryDue>, AuxService<PatCountryDue>>();

            services.AddScoped<IViewModelService<PatCountryExp>, ViewModelService<PatCountryExp>>();
            services.AddScoped<IEntityService<PatCountryExp>, AuxService<PatCountryExp>>();

            services.AddScoped<IViewModelService<PatDesCaseTypeFields>, ViewModelService<PatDesCaseTypeFields>>();
            services.AddScoped<IEntityService<PatDesCaseTypeFields>, AuxService<PatDesCaseTypeFields>>();

            // Delete/Ext tables
            services.AddScoped<IViewModelService<PatAreaDelete>, ViewModelService<PatAreaDelete>>();
            services.AddScoped<IEntityService<PatAreaDelete>, AuxService<PatAreaDelete>>();
            services.AddScoped<IViewModelService<PatAreaCountryDelete>, ViewModelService<PatAreaCountryDelete>>();
            services.AddScoped<IEntityService<PatAreaCountryDelete>, AuxService<PatAreaCountryDelete>>();
            services.AddScoped<IViewModelService<PatCountryExpDelete>, ViewModelService<PatCountryExpDelete>>();
            services.AddScoped<IEntityService<PatCountryExpDelete>, AuxService<PatCountryExpDelete>>();
            services.AddScoped<IViewModelService<PatCountryLawExt>, ViewModelService<PatCountryLawExt>>();
            services.AddScoped<IEntityService<PatCountryLawExt>, AuxService<PatCountryLawExt>>();
            services.AddScoped<IViewModelService<PatDesCaseTypeExt>, ViewModelService<PatDesCaseTypeExt>>();
            services.AddScoped<IEntityService<PatDesCaseTypeExt>, AuxService<PatDesCaseTypeExt>>();
            services.AddScoped<IViewModelService<PatDesCaseTypeDelete>, ViewModelService<PatDesCaseTypeDelete>>();
            services.AddScoped<IEntityService<PatDesCaseTypeDelete>, AuxService<PatDesCaseTypeDelete>>();
            services.AddScoped<IViewModelService<PatDesCaseTypeDeleteExt>, ViewModelService<PatDesCaseTypeDeleteExt>>();
            services.AddScoped<IEntityService<PatDesCaseTypeDeleteExt>, AuxService<PatDesCaseTypeDeleteExt>>();
            services.AddScoped<IViewModelService<PatDesCaseTypeFieldsExt>, ViewModelService<PatDesCaseTypeFieldsExt>>();
            services.AddScoped<IEntityService<PatDesCaseTypeFieldsExt>, AuxService<PatDesCaseTypeFieldsExt>>();
            services.AddScoped<IViewModelService<PatDesCaseTypeFieldsDelete>, ViewModelService<PatDesCaseTypeFieldsDelete>>();
            services.AddScoped<IEntityService<PatDesCaseTypeFieldsDelete>, AuxService<PatDesCaseTypeFieldsDelete>>();
            services.AddScoped<IViewModelService<PatDesCaseTypeFieldsDeleteExt>, ViewModelService<PatDesCaseTypeFieldsDeleteExt>>();
            services.AddScoped<IEntityService<PatDesCaseTypeFieldsDeleteExt>, AuxService<PatDesCaseTypeFieldsDeleteExt>>();

            //settings
            services.AddScoped<ISystemSettings<PatSetting>, SystemSettings<PatSetting>>();

            services.AddScoped<IEntityService<PatCountryLawUpdate>, AuxService<PatCountryLawUpdate>>();

            //indicator
            services.AddScoped<IViewModelService<PatIndicator>, ViewModelService<PatIndicator>>();
            services.AddScoped<IEntityService<PatIndicator>, AuxService<PatIndicator>>();

            return services;
        }
    }
}
