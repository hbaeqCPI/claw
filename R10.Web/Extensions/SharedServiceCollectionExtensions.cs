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
using R10.Core.Entities.FormExtract;
using R10.Core.Entities.MailDownload;

namespace R10.Web.Extensions
{
    public static class SharedServiceCollectionExtensions
    {
        public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration Configuration)
        {
            // Client/Agent/Owner core services (ViewModelServices removed in debloat)
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IClientContactService, ClientContactService>();

            services.AddScoped<IClientDesignatedCountryService, ClientDesignatedCountryService>();
            services.AddScoped<IEntitySyncRepository, EntitySyncRepository>();

            services.AddScoped<IAgentService, AgentService>();
            services.AddScoped<IAgentContactService, AgentContactService>();
            services.AddScoped<IChildEntityService<Agent, AgentCEFee>, ChildEntityService<Agent, AgentCEFee>>();

            services.AddScoped<IOwnerService, OwnerService>();

            services.AddScoped<IViewModelService<Attorney>, ViewModelService<Attorney>>();
            services.AddScoped<IAttorneyService, AttorneyService>();
            services.AddScoped<IEntityService<TimeTracker>, AuxService<TimeTracker>>();
            services.AddScoped<IParentEntityService<Attorney, TimeTracker>, ParentEntityService<Attorney, TimeTracker>>();
            services.AddScoped<IEntityService<TimeTrack>, AuxService<TimeTrack>>();


            services.AddScoped<IEntityService<Language>, AuxService<Language>>();
            services.AddScoped<IViewModelService<Language>, ViewModelService<Language>>();

            services.AddScoped<IAsyncRepository<Log>, EFRepository<Log>>();

            services.AddScoped<IViewModelService<ContactPerson>, ViewModelService<ContactPerson>>();
            services.AddScoped<IContactPersonService, ContactPersonService>();

            services.AddScoped<IAsyncRepository<QELog>, EFRepository<QELog>>();
            services.AddScoped<IImageTypeRepository, ImageTypeRepository>();

            // QuickDocket core (ViewModelService removed in debloat)
            services.AddScoped<IQuickDocketService, QuickDocketService>();
            services.AddScoped<IQuickDocketRepository, QuickDocketRepository>();

            services.AddScoped<IViewModelService<DeDocketInstruction>, ViewModelService<DeDocketInstruction>>();
            services.AddScoped<IEntityService<DeDocketInstruction>, AuxService<DeDocketInstruction>>();

            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IAuditRepository, AuditRepository>();

            services.AddScoped<IEntityService<CurrencyType>, AuxService<CurrencyType>>();
            services.AddScoped<IViewModelService<CurrencyType>, ViewModelService<CurrencyType>>();

            services.AddScoped<IWebLinksRepository, WebLinksRepository>();
            services.AddScoped<IWebLinksService, WebLinksService>();
            services.AddScoped<INumberFormatService, NumberFormatService>();

            //letters (ViewModelServices removed in debloat)
            services.AddScoped<ILetterService, LetterService>();
            services.AddScoped<ILetterEntitySettingRepository, LetterEntitySettingRepository>();
            services.AddScoped<IAsyncRepository<LetterCategory>, EFRepository<LetterCategory>>();
            services.AddScoped<IViewModelService<LetterCategory>, ViewModelService<LetterCategory>>();
            services.AddScoped<IAsyncRepository<LetterLog>, EFRepository<LetterLog>>();
            services.AddScoped<IAsyncRepository<LetterSubCategory>, EFRepository<LetterSubCategory>>();
            services.AddScoped<IViewModelService<LetterSubCategory>, ViewModelService<LetterSubCategory>>();
            services.AddScoped<IEntityService<LetterSubCategory>, AuxService<LetterSubCategory>>();
            services.AddScoped<IViewModelService<LetterSubCategory>, ViewModelService<LetterSubCategory>>();
            services.AddScoped<IChildEntityService<LetterMain, LetterTag>, ChildEntityService<LetterMain, LetterTag>>();

            // DOCX (ViewModelService removed in debloat)
            services.AddScoped<IDOCXService, DOCXService>();
            services.AddScoped<IAsyncRepository<DOCXCategory>, EFRepository<DOCXCategory>>();
            services.AddScoped<IViewModelService<DOCXCategory>, ViewModelService<DOCXCategory>>();
            services.AddScoped<IAsyncRepository<DOCXLog>, EFRepository<DOCXLog>>();

            services.AddScoped<IAsyncRepository<SystemScreen>, EFRepository<SystemScreen>>();
            services.AddScoped<IViewModelService<SystemScreen>, ViewModelService<SystemScreen>>();

            services.AddScoped<IEFSRepository, EFSRepository>();
            services.AddScoped<IEFSService, EFSService>();
            services.AddScoped<IAsyncRepository<EFSLog>, EFRepository<EFSLog>>();

            services.AddScoped<IAsyncRepository<ModuleMain>, EFRepository<ModuleMain>>();
            services.AddScoped<IAsyncRepository<QEMain>, EFRepository<QEMain>>();
            services.AddScoped<IAsyncRepository<QEDataSource>, EFRepository<QEDataSource>>();
            services.AddScoped<IQEDataSourceRepository, QEDataSourceRepository>();

            services.AddScoped<IAsyncRepository<QELayout>, EFRepository<QELayout>>();
            services.AddScoped<IAsyncRepository<QERoleSource>, EFRepository<QERoleSource>>();
            services.AddScoped<IAsyncRepository<QELog>, EFRepository<QELog>>();
            services.AddScoped<IAsyncRepository<CPiLanguage>, EFRepository<CPiLanguage>>();
            services.AddScoped<IQuickEmailRepository, QuickEmailRepository>();
            services.AddScoped<IQuickEmailService, QuickEmailService>();
            services.AddScoped<IQuickEmailSetupService, QuickEmailSetupService>();
            services.AddScoped<IViewModelService<QEDataSource>, ViewModelService<QEDataSource>>();
            services.AddScoped<IViewModelService<QERoleSource>, ViewModelService<QERoleSource>>();
            services.AddScoped<IAsyncRepository<QECategory>, EFRepository<QECategory>>();
            services.AddScoped<IViewModelService<QECategory>, ViewModelService<QECategory>>();
            services.AddScoped<IEntityService<QECategory>, AuxService<QECategory>>();
            services.AddScoped<IViewModelService<QECategory>, ViewModelService<QECategory>>();
            services.AddScoped<IChildEntityService<QEMain, QETag>, ChildEntityService<QEMain, QETag>>();


            services.AddScoped<IDocsOutRepository, DocsOutRepository>();
            services.AddScoped<IDocsOutService, DocsOutService>();

            // global update
            services.AddScoped<IGlobalUpdateRepository, GlobalUpdateRepository>();

            services.AddScoped<IAsyncRepository<ImageType>, EFRepository<ImageType>>();

            services.AddScoped<ICountryLookupViewModelService, CountryLookupViewModelService>();
            services.AddScoped<IEFSHelper, EFSHelper>();

            // data query (ViewModelService removed in debloat)
            services.AddScoped<IDataQueryService, DataQueryService>();
            services.AddScoped<IAsyncRepository<DataQueryCategory>, EFRepository<DataQueryCategory>>();
            services.AddScoped<IViewModelService<DataQueryCategory>, ViewModelService<DataQueryCategory>>();
            services.AddScoped<IEntityService<DataQueryCategory>, AuxService<DataQueryCategory>>();
            services.AddScoped<IViewModelService<DataQueryCategory>, ViewModelService<DataQueryCategory>>();
            services.AddScoped<IChildEntityService<DataQueryMain, DataQueryTag>, ChildEntityService<DataQueryMain, DataQueryTag>>();

            //settings
            services.AddScoped<ISystemSettings<DefaultSetting>, SystemSettings<DefaultSetting>>();

            //reports (ViewModelServices and deploy removed in debloat)
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<ISharedReportRepository, SharedReportRepository>();
            services.AddScoped<IReportParameterService, ReportParameterService>();

            services.AddScoped<IEntityService<CustomReport>, AuxService<CustomReport>>();

            //delegation (DelegationService removed in debloat)

            //email setup
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<IParentEntityService<EmailType, EmailSetup>, EmailTypeService>();
            services.AddScoped<IViewModelService<EmailType>, ViewModelService<EmailType>>();
            services.AddScoped<IViewModelService<EmailSetup>, ViewModelService<EmailSetup>>();
            services.AddScoped<IViewModelService<EmailTemplate>, ViewModelService<EmailTemplate>>();

            //map
            services.AddScoped<IMapService, MapService>();
            services.AddScoped<IMapRepository, MapRepository>();

            //CTM
            services.AddScoped<IRSCTMService, RSCTMService>();

            //data import
            services.AddScoped<IDataImportService, DataImportService>();
            services.AddScoped<IDataImportRepository,DataImportRepository>();

            // documents (OutlookService, SignatureService removed in debloat)
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IAsyncRepository<DocFixedFolder>, EFRepository<DocFixedFolder>>();
            services.AddScoped<IChildEntityService<DocDocument, DocDocumentTag>, ChildEntityService<DocDocument, DocDocumentTag>>();
            services.AddScoped<IEntityService<DocDocumentTag>, AuxService<DocDocumentTag>>();

            //utilities for AMS CPiEARSCommunication web service calls
            services.AddScoped<ICPiEncryption, CPiEncryption>();
            services.AddScoped<ICPiCompression, CPiCompression>();

            services.AddScoped<ExportHelper, ExportHelper>();

            //Product Aux
            services.AddScoped<IViewModelService<ProductCategory>, ViewModelService<ProductCategory>>();
            services.AddScoped<IEntityService<ProductCategory>, AuxService<ProductCategory>>();
            services.AddScoped<IViewModelService<ProductGroup>, ViewModelService<ProductGroup>>();
            services.AddScoped<IEntityService<ProductGroup>, AuxService<ProductGroup>>();

            services.AddScoped<IProductService, ProductService>();

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductSaleService, ProductSaleService>();


            services.AddScoped<IViewModelService<RelatedProductDTO>, ViewModelService<RelatedProductDTO>>();
            services.AddScoped<IAsyncRepository<RelatedProductDTO>, EFRepository<RelatedProductDTO>>();

            services.AddScoped<IViewModelService<Brand>, ViewModelService<Brand>>();
            services.AddScoped<IEntityService<Brand>, AuxService<Brand>>();

            //Product import
            services.AddScoped<IProductImportService, ProductImportService>();
            services.AddScoped<IProductImportRepository, ProductImportRepository>();

            // DocumentStorage, GlobalSearch, AzureSearch, FormExtract removed in debloat

            services.AddScoped<IFormIFWService, FormIFWService>();
            services.AddScoped<IParentEntityService<FormIFWActMap, FormIFWActMapPat>, ParentEntityService<FormIFWActMap, FormIFWActMapPat>>();
            services.AddScoped<IParentEntityService<FormIFWActMap, FormIFWActMapTmk>, ParentEntityService<FormIFWActMap, FormIFWActMapTmk>>();

            //user setting
            services.AddScoped<IUserSettingsService, UserSettingsService>();

            //API
            services.AddScoped<IEntityService<WebServiceLog>, AuxService<WebServiceLog>>();

            //deletelog
            services.AddScoped<IDeleteLogService, DeleteLogService>();

            //Help
            services.AddScoped<IBaseService<Help>, BaseService<Help>>();

            //Mail Rules
            services.AddScoped<IParentEntityService<MailDownloadRule, MailDownloadRuleCondition>, ParentEntityService<MailDownloadRule, MailDownloadRuleCondition>>();
            services.AddScoped<IParentEntityService<MailDownloadAction, MailDownloadActionFilter>, ParentEntityService<MailDownloadAction, MailDownloadActionFilter>>();
            services.AddScoped<IBaseService<MailDownloadRuleResponsible>, BaseService<MailDownloadRuleResponsible>>();


            services.AddScoped<IMyFavoriteService, MyFavoriteService>();

            //SharePoint removed in debloat

            //DocuSign (Service removed in debloat)
            services.AddScoped<IViewModelService<DocuSignAnchor>, ViewModelService<DocuSignAnchor>>();
            services.AddScoped<IParentEntityService<DocuSignAnchor, DocuSignAnchorTab>, ParentEntityService<DocuSignAnchor, DocuSignAnchorTab>>();

            //workflow (WorkflowViewModelService removed in debloat)

            //Document Verification (removed)
            services.AddScoped<IDocumentVerificationRepository, DocumentVerificationRepository>();

            //Due date extension
            services.AddScoped<IDueDateExtensionService, DueDateExtensionService>();

            //MS Graph, Mailbox, SharePoint, iManage, NetDocuments removed in debloat

            services.AddScoped<ITradeSecretService, TradeSecretService>();
            services.AddScoped<ISoftDocketService, SoftDocketService>();
            services.AddScoped<IDocketRequestService, DocketRequestService>();


            // Google Patent (removed in debloat)

            return services;
        }
    }
}
