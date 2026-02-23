using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities;
using R10.Web.Areas.Admin.Services;
using R10.Web.Interfaces;
using R10.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Extensions
{
    public static class AdminServiceCollectionExtension
    {
        public static IServiceCollection AddAdmin(this IServiceCollection services)
        {
            services.AddScoped<IUserAccountService, UserAccountService>();
            services.AddScoped<IViewModelService<ActivityLog>, ViewModelService<ActivityLog>>();
            services.AddScoped<IActionIndicatorService, ActionIndicatorService>();
            services.AddScoped<ICatalogService, CatalogService>();

            return services;
        }
    }
}
