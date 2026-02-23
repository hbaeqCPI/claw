using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Core.Services;
using R10.Infrastructure.Data;

namespace R10.Web.Extensions
{
    public static class TLServiceCollectionExtension
    {
        public static IServiceCollection AddTL(this IServiceCollection services)
        {
            services.AddScoped<ITLInfoService, TLInfoService>();
            services.AddScoped<ITLInfoRepository, TLInfoRepository>();
            services.AddScoped<ITLPTOActionMappingService, TLPTOActionMappingService>();
            services.AddScoped<ITLUpdateService, TLUpdateService>();
            services.AddScoped<ITLUpdateLookupService, TLUpdateLookupService>();
            services.AddScoped<ITLUpdateRepository, TLUpdateRepository>();
            services.AddScoped<ITLActionSearchService, TLActionSearchService>();
            services.AddScoped<IEntityService<TLSearchTTABParty>, AuxService<TLSearchTTABParty>>();
            services.AddScoped<IEntityService<TLSearchTTAB>, AuxService<TLSearchTTAB>>();
            services.AddScoped<IEntityService<TLSearch>, AuxService<TLSearch>>();
            services.AddScoped<IEntityService<TLSearchAction>, AuxService<TLSearchAction>>();

            //settings
            services.AddScoped<ISystemSettings<TLSetting>, SystemSettings<TLSetting>>();
            return services;
        }
    }
}
