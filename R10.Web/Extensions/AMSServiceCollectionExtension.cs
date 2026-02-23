using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities.AMS;
using R10.Core.Interfaces;
using R10.Core.Interfaces.AMS;
using R10.Core.Services;
using R10.Core.Services.AMS;
//using R10.Web.Areas.AMS.Services; // Removed: Web Area services no longer exist
using R10.Web.Interfaces;
using R10.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Extensions
{
    public static class AMSServiceCollectionExtension
    {
        public static IServiceCollection AddAMS(this IServiceCollection services)
        {
            services.AddScoped<IAMSMainService, AMSMainService>();
            //services.AddScoped<IAMSMainViewModelService, AMSMainViewModelService>(); // Removed: Web Area service
            services.AddScoped<IAMSDueService, AMSDueService>();
            //services.AddScoped<IAMSDueViewModelService, AMSDueViewModelService>(); // Removed: Web Area service
            //services.AddScoped<IAMSInstructionViewModelService, AMSInstructionViewModelService>(); // Removed: Web Area service
            //services.AddScoped<IAMSInstructionsToCPiViewModelService, AMSInstructionsToCPiViewModelService>(); // Removed: Web Area service
            //services.AddScoped<IAMSInstrxCPiLogViewModelService, AMSInstrxCPiLogViewModelService>(); // Removed: Web Area service
            //services.AddScoped<IAMSReminderViewModelService, AMSReminderViewModelService>(); // Removed: Web Area service
            //services.AddScoped<IAMSPrepayViewModelService, AMSPrepayViewModelService>(); // Removed: Web Area service
            services.AddScoped<IAMSInstrxTypeService, AMSInstrxTypeService>();
            services.AddScoped<IReminderLogService<AMSDue, AMSRemLogDue>, AMSReminderLogService>();
            services.AddScoped<IViewModelService<AMSInstrxType>, ViewModelService<AMSInstrxType>>();
            services.AddScoped<IAMSFeeService, AMSFeeService>();
            services.AddScoped<IAMSVATRateService, AMSVATRateService>();
            services.AddScoped<IViewModelService<AMSFee>, ViewModelService<AMSFee>>();
            services.AddScoped<IViewModelService<AMSVATRate>, ViewModelService<AMSVATRate>>();
            //services.AddScoped<ISendToCPiService, SendToCPiService>(); // Removed: service no longer exists
            //services.AddScoped<ISendToCPiLetterService, SendToCPiLetterService>(); // Removed: service no longer exists
            services.AddScoped<IAMSInstrxCPiLogService, AMSInstrxCPiLogService>();
            services.AddScoped<IAMSStatusChangeLogService, AMSStatusChangeLogService>();
            //services.AddScoped<IAMSMainQuickEmailService, AMSMainQuickEmailService>(); // Removed: service no longer exists
            //services.AddScoped<IAMSStatusUpdateViewModelService, AMSStatusUpdateViewModelService>(); // Removed: Web Area service
            services.AddScoped<IChildEntityService<AMSMain, AMSProduct>, AMSMainChildService<AMSProduct>>();
            services.AddScoped<IChildEntityService<AMSMain, AMSLicensee>, AMSMainChildService<AMSLicensee>>();
            services.AddScoped<IAMSCostExportService, AMSCostExportService>();
            //services.AddScoped<IAMSCostExportViewModelService, AMSCostExportViewModelService>(); // Removed: Web Area service
            services.AddScoped<IEntityService<AMSInstrxCPiViewLog>, EntityService<AMSInstrxCPiViewLog>>();

            //API
            services.AddScoped<IAMSInstrxApiService, AMSInstrxApiService>();

            //settings
            services.AddScoped<ISystemSettings<AMSSetting>, SystemSettings<AMSSetting>>();

            return services;
        }
    }
}
