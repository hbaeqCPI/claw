using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities.DMS;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.DMS;
using R10.Core.Services;
using R10.Infrastructure.Data;
using R10.Infrastructure.Data.DMS;
using R10.Infrastructure.Data.Patent;
//using R10.Web.Areas.DMS.Services; // Removed: Web Area services no longer exist
using R10.Web.Interfaces;
using R10.Web.Services;

namespace R10.Web.Extensions
{
    public static class RTSServiceCollectionExtension
    {
        public static IServiceCollection AddRTS(this IServiceCollection services)
        {
            services.AddScoped<IRTSService, RTSService>();
            services.AddScoped<IRTSInfoRepository, RTSInfoRepository>();
            services.AddScoped<IRTSPTOActionMappingService, RTSPTOActionMappingService>();
            services.AddScoped<IRTSActionSearchService, RTSActionSearchService>();

            return services;
        }
    }
}
