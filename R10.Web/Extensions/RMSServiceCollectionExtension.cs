using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities.RMS;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Core.Interfaces.RMS;
using R10.Core.Services;
using R10.Core.Services.RMS;
//using R10.Web.Areas.RMS.Services; // Removed: Web Area services no longer exist
using R10.Web.Interfaces;
//using R10.Web.Interfaces.RMS; // Removed: Web Interfaces no longer exist
using R10.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Extensions
{
    public static class RMSServiceCollectionExtension
    {
        public static IServiceCollection AddRMS(this IServiceCollection services)
        {
            services.AddScoped<IRMSDueService, RMSDueService>();
            services.AddScoped<IRMSInstrxTypeService, RMSInstrxTypeService>();
            services.AddScoped<IViewModelService<RMSInstrxType>, ViewModelService<RMSInstrxType>>();
            services.AddScoped<IRMSReminderSetupService, RMSReminderSetupService>();
            services.AddScoped<IViewModelService<RMSReminderSetup>, ViewModelService<RMSReminderSetup>>();
            //services.AddScoped<IRMSReminderViewModelService, RMSReminderViewModelService>(); // Removed: Web Area service
            services.AddScoped<IReminderLogService<TmkDueDate, RMSRemLogDue>, RMSReminderLogService>();
            services.AddScoped<IRMSActionCloseService, RMSActionCloseService>();
            //services.AddScoped<IRMSActionCloseViewModelService, RMSActionCloseViewModelService>(); // Removed: Web Area service
            //services.AddScoped<IRMSActionCloseLogViewModelService, RMSActionCloseLogViewModelService>(); // Removed: Web Area service
            //services.AddScoped<IActionCloseLetterService, ActionCloseLetterService>(); // Removed: Web Area service
            services.AddScoped<IBaseService<RMSDoc>, BaseService<RMSDoc>>();
            services.AddScoped<IRMSDueDocService, RMSDueDocService>();
            //services.AddScoped<IRMSDocViewModelService, RMSDocViewModelService>(); // Removed: Web Area service
            services.AddScoped<IBaseService<RMSReminderSetupDoc>, BaseService<RMSReminderSetupDoc>>();
            services.AddScoped<IParentEntityService<RMSInstrxTypeAction, RMSInstrxTypeActionDetail>, ParentEntityService<RMSInstrxTypeAction, RMSInstrxTypeActionDetail>>();
            services.AddScoped<IViewModelService<RMSDoc>, ViewModelService<RMSDoc>>();
            services.AddScoped<IViewModelService<RMSInstrxTypeAction>, ViewModelService<RMSInstrxTypeAction>>();
            //services.AddScoped<IRMSAgentResponsibilityViewModelService, RMSAgentResponsibilityViewModelService>(); // Removed: Web Area service

            //settings
            services.AddScoped<ISystemSettings<RMSSetting>, SystemSettings<RMSSetting>>();

            return services;
        }
    }
}
