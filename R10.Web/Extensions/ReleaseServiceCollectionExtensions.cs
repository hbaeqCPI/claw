using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Core.Services;
using R10.Web.Interfaces;
using R10.Web.Services;

namespace R10.Web.Extensions
{
    public static class ReleaseServiceCollectionExtensions
    {
        public static IServiceCollection AddRelease(this IServiceCollection services)
        {
            services.AddScoped<IViewModelService<Release>, ViewModelService<Release>>();
            services.AddScoped<IEntityService<Release>, AuxService<Release>>();
            return services;
        }
    }
}
