using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Interfaces;
using R10.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Extensions
{
    public static class ReportSchedulerServiceCollectionExtensions
    {
        public static IServiceCollection AddReportScheduler(this IServiceCollection services)
        {
            services.AddScoped<IRSActionService, RSActionService>();
            services.AddScoped<IRSCriteriaService, RSCriteriaService>();
            services.AddScoped<IRSPrintOptionService, RSPrintOptionService>();

            services.AddScoped<IRSMainService, RSMainService>();

            services.AddScoped<IRSActionHistoryService, RSActionHistoryService>();
            services.AddScoped<IRSCriteriaHistoryService, RSCriteriaHistoryService>();
            services.AddScoped<IRSPrintOptionHistoryService, RSPrintOptionHistoryService>();

            services.AddScoped<IRSHistoryService, RSHistoryService>();
            services.AddScoped<IEntityService<RSHistory>, AuxService<RSHistory>>();

            // RSMainViewModelService removed in debloat

            services.AddScoped<IRSLogService, RSLogService>();
            services.AddScoped<IEntityService<RSLog>, AuxService<RSLog>>();

            return services;
        }
    }
}
