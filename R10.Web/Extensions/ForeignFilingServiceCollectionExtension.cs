using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities.ForeignFiling;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Interfaces.ForeignFiling;
using R10.Core.Services;
using R10.Core.Services.ForeignFiling;
//using R10.Web.Areas.ForeignFiling.Services; // Removed: Web Area services no longer exist
using R10.Web.Interfaces;
//using R10.Web.Interfaces.ForeignFiling; // Removed: Web Interfaces no longer exist
using R10.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Extensions
{
    public static class ForeignFilingServiceCollectionExtension
    {
        public static IServiceCollection AddForeignFiling(this IServiceCollection services)
        {
            services.AddScoped<IFFDueService, FFDueService>();
            services.AddScoped<IFFDueCountryService, FFDueCountryService>();
            services.AddScoped<IFFInstrxTypeService, FFInstrxTypeService>();
            services.AddScoped<IFFReminderSetupService, FFReminderSetupService>();
            services.AddScoped<IViewModelService<FFReminderSetup>, ViewModelService<FFReminderSetup>>();
            services.AddScoped<IBaseService<FFReminderSetupDoc>, BaseService<FFReminderSetupDoc>>();
            services.AddScoped<IViewModelService<FFInstrxType>, ViewModelService<FFInstrxType>>();
            //services.AddScoped<IFFReminderViewModelService, FFReminderViewModelService>(); // Removed: Web Area service
            services.AddScoped<IReminderLogService<PatDueDate, FFRemLogDue>, FFReminderLogService>();
            services.AddScoped<IFFActionCloseService, FFActionCloseService>();
            //services.AddScoped<IFFActionCloseViewModelService, FFActionCloseViewModelService>(); // Removed: Web Area service
            //services.AddScoped<IActionCloseLetterService, ActionCloseLetterService>(); // Removed: Web Area service
            //services.AddScoped<IFFActionCloseLogViewModelService, FFActionCloseLogViewModelService>(); // Removed: Web Area service
            //services.AddScoped<IFFGenAppViewModelService, FFGenAppViewModelService>(); // Removed: Web Area service
            services.AddScoped<IBaseService<FFDoc>, BaseService<FFDoc>>();
            services.AddScoped<IViewModelService<FFDoc>, ViewModelService<FFDoc>>();
            services.AddScoped<IViewModelService<FFInstrxTypeAction>, ViewModelService<FFInstrxTypeAction>>();
            services.AddScoped<IParentEntityService<FFInstrxTypeAction, FFInstrxTypeActionDetail>, ParentEntityService<FFInstrxTypeAction, FFInstrxTypeActionDetail>>();
            //services.AddScoped<IFFDocViewModelService, FFDocViewModelService>(); // Removed: Web Area service
            services.AddScoped<IFFDueDocService, FFDueDocService>();

            //settings
            services.AddScoped<ISystemSettings<FFSetting>, SystemSettings<FFSetting>>();

            return services;
        }
    }
}
