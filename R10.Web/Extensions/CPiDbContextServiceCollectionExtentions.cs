using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using R10.Core.Interfaces;
using R10.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Extensions
{
    public static class CPiDbContextServiceCollectionExtentions
    {
        public static IServiceCollection AddCPiDbContext<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            services.AddScoped<IRepositoryFactory, CPiDbContext<TContext>>();
            services.AddScoped<ICPiDbContext, CPiDbContext<TContext>>();
            services.AddScoped<ICPiDbContext<TContext>, CPiDbContext<TContext>>();
            return services;
        }
    }
}
