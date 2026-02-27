using Microsoft.Extensions.DependencyInjection;
using R10.Web.Areas.Admin.Services;
using R10.Web.Interfaces;

namespace R10.Web.Extensions
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
