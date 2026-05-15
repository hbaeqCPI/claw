using Microsoft.Extensions.DependencyInjection;
using LawPortal.Core.Entities;
using LawPortal.Core.Interfaces;
using LawPortal.Core.Services;
using LawPortal.Web.Interfaces;
using LawPortal.Web.Services;

namespace LawPortal.Web.Extensions
{
    public static class ReleaseServiceCollectionExtensions
    {
        public static IServiceCollection AddRelease(this IServiceCollection services)
        {
            services.AddScoped<IViewModelService<Release>, ViewModelService<Release>>();
            services.AddScoped<IEntityService<Release>, AuxService<Release>>();
            services.AddScoped<IViewModelService<DeployPassword>, ViewModelService<DeployPassword>>();
            services.AddScoped<IEntityService<DeployPassword>, AuxService<DeployPassword>>();
            return services;
        }
    }
}
