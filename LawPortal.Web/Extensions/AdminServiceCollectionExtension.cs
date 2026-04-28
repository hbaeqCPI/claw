using Microsoft.Extensions.DependencyInjection;
using LawPortal.Web.Areas.Admin.Services;
using LawPortal.Web.Interfaces;

namespace LawPortal.Web.Extensions
{
    public static class AdminServiceCollectionExtension
    {
        public static IServiceCollection AddAdmin(this IServiceCollection services)
        {
            services.AddScoped<IUserAccountService, UserAccountService>();

            return services;
        }
    }
}
