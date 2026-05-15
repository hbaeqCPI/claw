using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LawPortal.Core.Entities;
using LawPortal.Core.Interfaces;
using LawPortal.Core.Services;
using LawPortal.Core.Services.Shared;
using LawPortal.Infrastructure.Data;
using LawPortal.Infrastructure.Data.Admin;
using LawPortal.Web.Interfaces;
using LawPortal.Web.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using LawPortal.Core.Interfaces.Shared;
using LawPortal.Web.Helpers;
using LawPortal.Core.Entities.Shared;
using LawPortal.Core.Services.Documents;
using Microsoft.Extensions.Configuration;
using LawPortal.Core.Entities.Documents;
using LawPortal.Core.DTOs;
using LawPortal.Web.Services.DocumentStorage;

namespace LawPortal.Web.Extensions
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
            services.AddScoped<IAsyncRepository<DocFixedFolder>, EFRepository<DocFixedFolder>>();

            // Document storage — choose backend based on appsettings DocumentStorage section.
            // Azure is selected only when UseFileSystem=false AND every required credential is
            // populated. This means the same appsettings.json works in both environments:
            // locally where credentials are empty (falls back to FileSystem), and in
            // staging/prod where the creds are filled in via per-environment overrides or
            // Key Vault (uses Azure Blob).
            var docStorageSettings = Configuration.GetSection("DocumentStorage").Get<DocumentStorageSettings>();
            bool useFileSystem;
            if (docStorageSettings == null)
            {
                useFileSystem = true;
            }
            else if (docStorageSettings.UseFileSystem)
            {
                useFileSystem = true;
            }
            else
            {
                // Azure is only viable if the service-principal creds + storage account are present.
                var azureReady =
                    !string.IsNullOrWhiteSpace(docStorageSettings.StorageADTenantID) &&
                    !string.IsNullOrWhiteSpace(docStorageSettings.StorageAppClientID) &&
                    !string.IsNullOrWhiteSpace(docStorageSettings.StorageAppClientSecret) &&
                    !string.IsNullOrWhiteSpace(docStorageSettings.StorageAccountName) &&
                    !string.IsNullOrWhiteSpace(docStorageSettings.StorageContainerName);
                useFileSystem = !azureReady;
            }
            if (useFileSystem)
            {
                services.AddScoped<IDocumentStorage, FileSystemStorage>();
                services.AddScoped<IDocumentHelper, DocumentHelper>();
            }
            else
            {
                services.AddScoped<IDocumentStorage, AzureStorage>();
                services.AddScoped<IDocumentHelper, AzureDocumentHelper>();
            }
            // AzureStorage is also resolvable concretely (AzureDocumentHelper depends on the
            // concrete type, and ReleaseController will use it directly to download blobs to
            // local temp files for the 32-bit MDB sidecar).
            services.AddScoped<AzureStorage>();
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
