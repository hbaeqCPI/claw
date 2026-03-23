using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Core.Services;
using R10.Core.Services.Shared;
using R10.Infrastructure.Data;
using R10.Infrastructure.Data.Admin;
using R10.Web.Interfaces;
using R10.Web.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Interfaces.Shared;
using R10.Web.Helpers;
using R10.Core.Entities.Shared;
using R10.Core.Services.Documents;
using Microsoft.Extensions.Configuration;
using R10.Core.Entities.Documents;
using R10.Core.DTOs;

namespace R10.Web.Extensions
{
    public static class SharedServiceCollectionExtensions
    {
        public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration Configuration)
        {

            services.AddScoped<IEntitySyncRepository, EntitySyncRepository>();

            services.AddScoped<IEntityService<Language>, AuxService<Language>>();
            services.AddScoped<IViewModelService<Language>, ViewModelService<Language>>();

            services.AddScoped<IAsyncRepository<Log>, EFRepository<Log>>();

            services.AddScoped<IImageTypeRepository, ImageTypeRepository>();

            services.AddScoped<IEntityService<CurrencyType>, AuxService<CurrencyType>>();
            services.AddScoped<IViewModelService<CurrencyType>, ViewModelService<CurrencyType>>();

            services.AddScoped<IWebLinksRepository, WebLinksRepository>();
            services.AddScoped<IWebLinksService, WebLinksService>();
            services.AddScoped<INumberFormatService, NumberFormatService>();


            services.AddScoped<IAsyncRepository<SystemScreen>, EFRepository<SystemScreen>>();
            services.AddScoped<IViewModelService<SystemScreen>, ViewModelService<SystemScreen>>();

            services.AddScoped<IAsyncRepository<ModuleMain>, EFRepository<ModuleMain>>();
            services.AddScoped<IAsyncRepository<CPiLanguage>, EFRepository<CPiLanguage>>();

            services.AddScoped<IAsyncRepository<ImageType>, EFRepository<ImageType>>();

            services.AddScoped<ICountryLookupViewModelService, CountryLookupViewModelService>();
            services.AddScoped<IEFSHelper, EFSHelper>();

            //settings
            services.AddScoped<ISystemSettings<DefaultSetting>, SystemSettings<DefaultSetting>>();

            //reports
            services.AddScoped<IReportService, ReportService>();

            // documents
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IDocumentHelper, DocumentHelper>();
            services.AddScoped<IAsyncRepository<DocFixedFolder>, EFRepository<DocFixedFolder>>();
            services.AddScoped<IChildEntityService<DocDocument, DocDocumentTag>, ChildEntityService<DocDocument, DocDocumentTag>>();
            services.AddScoped<IEntityService<DocDocumentTag>, AuxService<DocDocumentTag>>();

            //utilities for AMS CPiEARSCommunication web service calls
            services.AddScoped<ICPiEncryption, CPiEncryption>();
            services.AddScoped<ICPiCompression, CPiCompression>();

            services.AddScoped<ExportHelper, ExportHelper>();

            // Product, DataImport, Map, RSCTM, Email, FormIFW, DocuSign, DueDateExtension,

            //user setting
            services.AddScoped<IUserSettingsService, UserSettingsService>();

            //API
            services.AddScoped<IEntityService<WebServiceLog>, AuxService<WebServiceLog>>();

            //Help
            services.AddScoped<IBaseService<Help>, BaseService<Help>>();

            //system
            services.AddScoped<IViewModelService<AppSystem>, ViewModelService<AppSystem>>();
            services.AddScoped<IEntityService<AppSystem>, AuxService<AppSystem>>();

            //Document Verification
            services.AddScoped<IDocumentVerificationRepository, DocumentVerificationRepository>();

            return services;
        }
    }
}
