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
using R10.Web.Interfaces.Shared;
using R10.Web.Areas.Shared.Services;
using R10.Core.Services.Documents;
using Microsoft.Extensions.Configuration;
using R10.Web.Services.DocumentStorage;
using R10.Core.Entities.Documents;
using R10.Web.Services.EmailAddIn;
using R10.Web.Services.DocumentSearch;
using R10.Core.DTOs;
using R10.Core.Services.FormExtract;
using R10.Web.Services.FormExtract;
using R10.Core.Entities.FormExtract;
using R10.Core.Entities.MailDownload;
using R10.Web.Services.MailDownload;
using R10.Web.Services.SharePoint;
using R10.Web.Services.iManage;
using R10.Web.Services.NetDocuments;

namespace R10.Web.Extensions
{
    public static class SharedServiceCollectionExtensions
    {
        public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddScoped<IClientViewModelService, ClientViewModelService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IClientContactService, ClientContactService>();

            services.AddScoped<IClientDesignatedCountryService, ClientDesignatedCountryService>();
            services.AddScoped<IEntitySyncRepository, EntitySyncRepository>();

            services.AddScoped<IAgentViewModelService, AgentViewModelService>();
            services.AddScoped<IAgentService, AgentService>();
            services.AddScoped<IAgentContactService, AgentContactService>();
            services.AddScoped<IChildEntityService<Agent, AgentCEFee>, ChildEntityService<Agent, AgentCEFee>>();

            services.AddScoped<IOwnerViewModelService, OwnerViewModelService>();
            services.AddScoped<IOwnerService, OwnerService>();

            services.AddScoped<IViewModelService<Attorney>, ViewModelService<Attorney>>();
            services.AddScoped<IAttorneyService, AttorneyService>();
            services.AddScoped<IEntityService<TimeTracker>, AuxService<TimeTracker>>();
            services.AddScoped<IParentEntityService<Attorney, TimeTracker>, ParentEntityService<Attorney, TimeTracker>>();
            services.AddScoped<IEntityService<TimeTrack>, AuxService<TimeTrack>>();
            services.AddScoped<ITimeTrackerService, TimeTrackerService>();


            services.AddScoped<IEntityService<Language>, AuxService<Language>>();
            services.AddScoped<IViewModelService<Language>, ViewModelService<Language>>();

            services.AddScoped<IAsyncRepository<Log>, EFRepository<Log>>();

            services.AddScoped<IViewModelService<ContactPerson>, ViewModelService<ContactPerson>>();
            services.AddScoped<IContactPersonService, ContactPersonService>();

            services.AddScoped<IAsyncRepository<QELog>, EFRepository<QELog>>();
            services.AddScoped<IImageTypeRepository, ImageTypeRepository>();

            services.AddScoped<IOuickDocketViewModelService, QuickDocketViewModelService>();
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

            //letters
            services.AddScoped<ILetterService, LetterService>();
            services.AddScoped<ILetterViewModelService, LetterViewModelService>();
            services.AddScoped<ILetterEntitySettingRepository, LetterEntitySettingRepository>();
            services.AddScoped<IAsyncRepository<LetterCategory>, EFRepository<LetterCategory>>();
            services.AddScoped<IViewModelService<LetterCategory>, ViewModelService<LetterCategory>>();
            services.AddScoped<IAsyncRepository<LetterLog>, EFRepository<LetterLog>>();
            services.AddScoped<ILetterDataSourceViewModelService, LetterDataSourceViewModelService>();
            services.AddScoped<IAsyncRepository<LetterSubCategory>, EFRepository<LetterSubCategory>>();
            services.AddScoped<IViewModelService<LetterSubCategory>, ViewModelService<LetterSubCategory>>();
            services.AddScoped<IEntityService<LetterSubCategory>, AuxService<LetterSubCategory>>();
            services.AddScoped<IViewModelService<LetterSubCategory>, ViewModelService<LetterSubCategory>>();
            services.AddScoped<IChildEntityService<LetterMain, LetterTag>, ChildEntityService<LetterMain, LetterTag>>();

            // DOCX
            services.AddScoped<IDOCXService, DOCXService>();
            services.AddScoped<IDOCXViewModelService, DOCXViewModelService>();
            //services.AddScoped<IDOCXEntitySettingRepository, DOCXEntitySettingRepository>();
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
            //services.AddScoped<IAsyncRepository<QERecipient>, EFRepository<QERecipient>>();
            services.AddScoped<IQEDataSourceRepository, QEDataSourceRepository>();
            //services.AddScoped<IQERecipientRepository, QERecipientRepository>();
            
            services.AddScoped<IAsyncRepository<QELayout>, EFRepository<QELayout>>();
            services.AddScoped<IAsyncRepository<QERoleSource>, EFRepository<QERoleSource>>();
            services.AddScoped<IAsyncRepository<QELog>, EFRepository<QELog>>();
            services.AddScoped<IAsyncRepository<CPiLanguage>, EFRepository<CPiLanguage>>();
            services.AddScoped<IQuickEmailRepository, QuickEmailRepository>();
            services.AddScoped<IOuickEmailViewModelService, QuickEmailViewModelServiceStub>();
            services.AddScoped<IOuickEmailSetupViewModelService, OuickEmailSetupViewModelService>();
            services.AddScoped<IQuickEmailService, QuickEmailService>();
            services.AddScoped<IQuickEmailSetupService, QuickEmailSetupService>();
            services.AddScoped<IViewModelService<QEDataSource>, ViewModelService<QEDataSource>>();
            services.AddScoped<IViewModelService<QERoleSource>, ViewModelService<QERoleSource>>();
            services.AddScoped<IQEDataSourceViewModelService, QEDataSourceViewModelService>();
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

            // data query
            services.AddScoped<IDataQueryService, DataQueryService>();
            services.AddScoped<IDataQueryViewModelService, DataQueryViewModelService>();
            services.AddScoped<IAsyncRepository<DataQueryCategory>, EFRepository<DataQueryCategory>>();
            services.AddScoped<IViewModelService<DataQueryCategory>, ViewModelService<DataQueryCategory>>();
            services.AddScoped<IEntityService<DataQueryCategory>, AuxService<DataQueryCategory>>();
            services.AddScoped<IViewModelService<DataQueryCategory>, ViewModelService<DataQueryCategory>>();
            services.AddScoped<IChildEntityService<DataQueryMain, DataQueryTag>, ChildEntityService<DataQueryMain, DataQueryTag>>();

            //settings
            services.AddScoped<ISystemSettings<DefaultSetting>, SystemSettings<DefaultSetting>>();
            // Note: Settings and services for other modules (AMS, DMS, GM, FF, RMS, etc.)
            // are registered via their own extension files called in Startup.cs

            //reports
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IReportDeployService, ReportDeployService>();
            services.AddScoped<ISharedReportRepository, SharedReportRepository>();
            services.AddScoped<ISharedReportViewModelService, SharedReportViewModelService>();
            services.AddScoped<IReportParameterService, ReportParameterService>();

            services.AddScoped<ICustomReportService, CustomReportService>();
            services.AddScoped<IEntityService<CustomReport>, AuxService<CustomReport>>();
            services.AddScoped<ICustomReportViewModelService, CustomReportViewModelService>();

            //delegation
            services.AddScoped<IDelegationService, DelegationService>();

            //email setup
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<IParentEntityService<EmailType, EmailSetup>, EmailTypeService>();
            services.AddScoped<IViewModelService<EmailType>, ViewModelService<EmailType>>();
            services.AddScoped<IViewModelService<EmailSetup>, ViewModelService<EmailSetup>>();
            services.AddScoped<IViewModelService<EmailTemplate>, ViewModelService<EmailTemplate>>();

            //map
            services.AddScoped<IMapService, MapService>();
            services.AddScoped<IMapRepository, MapRepository>();

            //dashboard
            services.AddScoped<IDashboardManager, DashboardManager>();
            services.AddScoped<IWidgetDataService, WidgetDataService>();
            // Shared/Patent/Trademark widget services removed with debloat; dashboard will return null for those widgets

            //CTM
            services.AddScoped<IRSCTMService, RSCTMService>();

            //data import
            services.AddScoped<IDataImportService, DataImportService>();
            services.AddScoped<IDataImportRepository,DataImportRepository>();

            // documents
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IDocumentViewModelService, DocumentViewModelService>();
            services.AddScoped<IDocumentsViewModelService, DocumentsViewModelService>();
            services.AddScoped<IDocumentsAIViewModelService, DocumentsAIViewModelService>();
            services.AddScoped<IAsyncRepository<DocFixedFolder>, EFRepository<DocFixedFolder>>();
            services.AddScoped<IOutlookService, OutlookService>();
            services.AddScoped<IChildEntityService<DocDocument, DocDocumentTag>, ChildEntityService<DocDocument, DocDocumentTag>>();
            services.AddScoped<IEntityService<DocDocumentTag>, AuxService<DocDocumentTag>>();
            services.AddScoped<ISignatureService, SignatureService>();
            services.AddScoped<ISignatureRepository, SignatureRepository>();

            //utilities for AMS CPiEARSCommunication web service calls
            services.AddScoped<ICPiEncryption, CPiEncryption>();
            services.AddScoped<ICPiCompression, CPiCompression>();

            services.AddScoped<ExportHelper, ExportHelper>();
            //services.AddScoped<DocumentHelper, DocumentHelper>();

            //Product Aux
            services.AddScoped<IViewModelService<ProductCategory>, ViewModelService<ProductCategory>>();
            services.AddScoped<IEntityService<ProductCategory>, AuxService<ProductCategory>>();
            services.AddScoped<IViewModelService<ProductGroup>, ViewModelService<ProductGroup>>();
            services.AddScoped<IEntityService<ProductGroup>, AuxService<ProductGroup>>();

            services.AddScoped<IProductViewModelService, ProductViewModelService>();
            services.AddScoped<IProductService, ProductService>();

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductSaleService, ProductSaleService>();
            services.AddScoped<IProductImageViewModelService, ProductImageViewModelService>();


            services.AddScoped<IViewModelService<RelatedProductDTO>, ViewModelService<RelatedProductDTO>>();
            services.AddScoped<IAsyncRepository<RelatedProductDTO>, EFRepository<RelatedProductDTO>>();

            services.AddScoped<IViewModelService<Brand>, ViewModelService<Brand>>(); 
            services.AddScoped<IEntityService<Brand>, AuxService<Brand>>();

            //Product import
            services.AddScoped<IProductImportService, ProductImportService>();
            services.AddScoped<IProductImportRepository, ProductImportRepository>();

            var settings = Configuration.GetSection("DocumentStorage").Get<DocumentStorageSettings>();
            if (settings.UseFileSystem)
            {
                services.AddScoped<IDocumentStorage, FileSystemStorage>();
                services.AddScoped<IDocumentHelper, DocumentHelper>();
            }
            else {
                services.AddScoped<IDocumentStorage, AzureStorage>();
                services.AddScoped<IDocumentHelper, AzureDocumentHelper>();
            }
            services.AddScoped<AzureStorage, AzureStorage>();
            services.AddScoped<IDocumentPermission, DocumentPermissionStub>();

            // global search
            services.AddScoped<IGlobalSearchService, GlobalSearchService>();
            services.AddScoped<IGlobalSearchViewModelService, GlobalSearchViewModelService>();
            services.AddScoped<AzureSearch, AzureSearch>();

            // form recognizer/extraction
            services.AddScoped<IFormExtractService, FormExtractService>();
            services.AddScoped<IFormExtractViewModelService, FormExtractViewModelService>();
            services.AddScoped<AzureFormRecognizer, AzureFormRecognizer>();

            services.AddScoped<IFormIFWViewModelService, FormIFWViewModelService>();
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

            //Share Point
            services.AddScoped<ISharePointViewModelService, SharePointViewModelService>();
            services.AddScoped<ISharePointService, SharePointService>();

            //DocuSign
            services.AddScoped<IDocuSignService, DocuSignService>();
            services.AddScoped<IViewModelService<DocuSignAnchor>, ViewModelService<DocuSignAnchor>>();
            services.AddScoped<IParentEntityService<DocuSignAnchor, DocuSignAnchorTab>, ParentEntityService<DocuSignAnchor, DocuSignAnchorTab>>();

            //workflow
            services.AddScoped<IWorkflowViewModelService, WorkflowViewModelServiceStub>();

            //Document Verification
            services.AddScoped<IDocumentVerificationViewModelService, DocumentVerificationViewModelService>();
            services.AddScoped<IDocumentVerificationRepository, DocumentVerificationRepository>();

            //Due date extension
            services.AddScoped<IDueDateExtensionService, DueDateExtensionService>();

            //MS Graph
            services.AddSingleton<IGraphAuthProvider, GraphAuthProvider>();
            services.AddSingleton<IGraphServiceClientFactory, GraphServiceClientFactory>();

            //Mailbox
            services.AddScoped<IMailDownloadService, MailDownloadService>();
            services.AddScoped<IMailDataMapService, MailDataMapService>();

            //Sharepoint utility
            services.AddScoped<ISharePointRepository, SharePointRepository>();
            
            //iManage
            services.AddScoped<IiManageAuthProvider, iManageAuthProvider>();
            services.AddScoped<IiManageClientFactory, iManageClientFactory>();
            services.AddScoped<IiManageViewModelService, iManageViewModelService>();

            //NetDocuments
            services.AddScoped<INetDocumentsAuthProvider, NetDocumentsAuthProvider>();
            services.AddScoped<INetDocumentsClientFactory, NetDocumentsClientFactory>();
            services.AddScoped<INetDocumentsViewModelService, NetDocumentsViewModelService>();

            services.AddScoped<IDocumentImportService, DocumentImportService>();

            services.AddScoped<ITradeSecretService, TradeSecretService>();
            services.AddScoped<ISoftDocketService, SoftDocketService>();
            services.AddScoped<IDocketRequestService, DocketRequestService>();

            
            //Google Patent
            services.AddScoped<ICPIGoogleService, CPIGoogleService>();

            return services;
        }
    }
}
