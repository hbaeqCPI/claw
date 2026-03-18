using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Infrastructure.Data.Documents.mappings;
using R10.Infrastructure.Data.mappings;
using R10.Infrastructure.Data.Patent.mappings;
using R10.Infrastructure.Data.Shared.mappings;
using R10.Infrastructure.Data.Trademark.mappings;
using R10.Infrastructure.Identity.Mappings;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using R10.Core.Helpers;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace R10.Infrastructure.Data
{

    public class ApplicationDbContext : DbContext, IApplicationDbContext, IDataProtectionKeyContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        #region Entities Declaration
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
        public DbSet<DeleteLog> DeleteLogs { get; set; }

        #region Shared Auxiliaries

        public DbSet<Language> Languages { get; set; }
        public DbSet<CurrencyType> CurrencyTypes { get; set; }
        public DbSet<CPiLanguage> CPiLanguages { get; set; }
        public DbSet<SearchCriteria> SearchCriteria { get; set; }
        public DbSet<SearchCriteriaDetail> SearchCriteriaDetails { get; set; }
        public DbSet<SharedCountryLookupDTO> SharedCountryLookupDTO { get; set; }

        //YX 20210806

        #endregion

        #region Patent

        public DbSet<PatDesignatedCountry> PatDesignatedCountries { get; set; }

        public DbSet<PatDisclosureStatus> PatDisclosureStatuses { get; set; }
        public DbSet<PatCountry> PatCountries { get; set; }
        public DbSet<PatArea> PatAreas { get; set; }
        public DbSet<PatAreaCountry> PatAreasCountries { get; set; }
        public DbSet<PatCountryLaw> PatCountryLaws { get; set; }
        public DbSet<PatCountryDue> PatCountryDues { get; set; }
        public DbSet<PatCountryExp> PatCountryExpirations { get; set; }
        public DbSet<PatCaseType> PatCaseTypes { get; set; }
        public DbSet<PatActionType> PatActionTypes { get; set; }
        public DbSet<PatActionParameter> PatActionParameters { get; set; }
        public DbSet<PatApplicationStatus> ApplicationStatuses { get; set; }
        public DbSet<PatDesignationDTO> PatDesignationDTO { get; set; }
        public DbSet<PatDesCaseType> PatDesCaseTypes { get; set; }
        public DbSet<PatDesCaseTypeFields> PatDesCaseTypeFields { get; set; }

        public DbSet<PatWorkflow> PatWorkflows { get; set; }
        public DbSet<PatWorkflowAction> PatWorkflowActions { get; set; }
        public DbSet<PatWorkflowActionParameter> PatWorkflowActionParameters { get; set; }
        public DbSet<LookupDescDTO> PatActionTypeDTO { get; set; }
        public DbSet<PatScoreCategory> PatScoreCategories { get; set; }
        public DbSet<PatScore> PatScores { get; set; }
        public DbSet<PatScoreDTO> PatScoreDTO { get; set; }
        public DbSet<PatAverageScoreDTO> PatAverageScoreDTO { get; set; }

        public DbSet<PatCountryLawUpdate> PatCountryLawUpdate { get; set; }
        public DbSet<PatIndicator> PatIndicators { get; set; }

        #endregion

        #endregion

        #region Trademark
        //trademark
        public DbSet<TmkCountry> TmkCountries { get; set; }
        public DbSet<TmkArea> TmkAreas { get; set; }
        public DbSet<TmkAreaCountry> TmkAreasCountries { get; set; }

        public DbSet<TmkCaseType> TmkCaseTypes { get; set; }
        public DbSet<TmkDesCaseType> TmkDesCaseTypes { get; set; }
        public DbSet<TmkDesCaseTypeFields> TmkDesCaseTypeFields { get; set; }
        public DbSet<TmkConflictStatus> TmkConflictStatuses { get; set; }
        public DbSet<TmkCountryLaw> TmkCountryLaws { get; set; }
        public DbSet<TmkCountryDue> TmkCountryDues { get; set; }
        public DbSet<TmkActionType> TmkActionTypes { get; set; }
        public DbSet<TmkActionParameter> TmkActionParameters { get; set; }

        public DbSet<TmkConflict> TmkConflicts { get; set; }
        public DbSet<TmkDesignatedCountry> TmkDesignatedCountries { get; set; }

        public DbSet<TmkCountryLawUpdate> TmkCountryLawUpdate { get; set; }
        public DbSet<TmkWorkflow> TmkWorkflows { get; set; }
        public DbSet<TmkWorkflowAction> TmkWorkflowActions { get; set; }
        public DbSet<TmkWorkflowActionParameter> TmkWorkflowActionParameters { get; set; }
        public DbSet<LookupDescDTO> TmkActionTypeDTO { get; set; }
        public DbSet<TmkIndicator> TmkIndicators { get; set; }
        public DbSet<TmkStandardGood> TmkStandardGoods { get; set; }

        #endregion

        #region Shared
        public DbSet<AppSystem> AppSystems { get; set; }
        #endregion

        #region Images
        public DbSet<ImageType> ImageTypes { get; set; }
        #endregion

        #region Letters
        public DbSet<LookupDTO> LetterFilterLookUpDTO { get; set; }
        #endregion Letters

        #region DOCX
        public DbSet<LookupDTO> DOCXFilterLookUpDTO { get; set; }
        #endregion

        #region Audit Trail
        public DbSet<LookupDTO> AuditLookupDTO { get; set; }
        #endregion AuditTrail

        #region Web Links
        public DbSet<WebLinksDTO> WebLinksDTO { get; set; }
        public DbSet<WebLinksUrlDTO> WebLinksUrlDTO { get; set; }
        public DbSet<WebLinksNumberTemplateDTO> WebLinksNumberTemplateDTO { get; set; }
        #endregion

        #region Security & System Logs
        public DbSet<CPiUser> CPiUser { get; set; }
        public DbSet<CPiUserPasswordHistory> CPiUserPasswordHistory { get; set; }
        public DbSet<CPiUserEntityFilter> CPiUserEntityFilters { get; set; }
        public DbSet<CPiUserSystemRole> CPiUserSystemRoles { get; set; }
        public DbSet<CPiRespOffice> CPiRespOffices { get; set; }
        public DbSet<CPiGroup> CPiGroups { get; set; }

        public DbSet<ErrorMapping> ErrorMappings { get; set; }
        public DbSet<Log> Logs { get; set; }

        #endregion

        #region System Tables
        public DbSet<CPiMenuItem> CPiMenuItems { get; set; }
        public DbSet<CPiMenuPage> CPiMenuPages { get; set; }

        public DbSet<CPiDefaultPage> CPiDefaultPages { get; set; }
        public DbSet<CPiSetting> CPiSettings { get; set; }
        public DbSet<CPiUserSetting> CPiUserSettings { get; set; }
        public DbSet<CPiSystemSetting> CPiSystemSettings { get; set; }
        public DbSet<CPiUserSettingLog> CPiUserSettingLog { get; set; }

        public DbSet<CPiWidget> CPiWidgets { get; set; }
        public DbSet<CPiUserWidget> CPiUserWidgets { get; set; }

        public DbSet<LocalizationRecords> LocalizationRecords { get; set; }
        public DbSet<LocalizationRecordsGrouping> LocalizationRecordsGrouping { get; set; }

        public DbSet<Option> Options { get; set; }
        public DbSet<ModuleMain> ModulesMain { get; set; }
        public DbSet<SystemScreen> SystemScreens { get; set; }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationConnection> NotificationConnections { get; set; }

        //DTOs for dashboard widgets that use stored procs
        //Use view models if not using stored procs
        public DbSet<ChartDTO> ChartDTO { get; set; }
        public DbSet<ListDTO> ListDTO { get; set; }
        public DbSet<CaseListDTO> CaseListDTO { get; set; }
        public DbSet<SysCustomFieldSetting> SysCustomFieldSettings { get; set; }

        #endregion

        #region Documents
        public DbSet<DocSystem> DocSystems { get; set; }
        public DbSet<DocMatterTree> DocMatterTrees { get; set; }
        public DbSet<DocTreeDTO> DocTreeDTO { get; set; }
        public DbSet<DocTreeEmailApiDTO> DocTreeEmailApiDTO { get; set; }
        public DbSet<DocImageDetailDTO> DocImageDetailDTO { get; set; }
        public DbSet<DocIDSRelCasesDTO> DocIDSRelCasesDTO { get; set; }
        public DbSet<DocIDSNonPatLitDTO> DocIDSNonPatLitDTO { get; set; }
        public DbSet<DocViewDTO> DocViewDTO { get; set; }
        public DbSet<DocInfoDTO> DocInfoDTO { get; set; }

        public DbSet<DocFolder> DocFolders { get; set; }
        public DbSet<DocDocument> DocDocuments { get; set; }
        public DbSet<DocDocumentTag> DocDocumentTags { get; set; }
        public DbSet<DocFile> DocFiles { get; set; }
        public DbSet<DocFileSignature> DocFileSignatures { get; set; }
        public DbSet<SharePointFileSignature> SharePointFileSignatures { get; set; }
        public DbSet<DocFileSignatureRecipient> DocFileSignatureRecipients { get; set; }
        public DbSet<DocIcon> DocIcons { get; set; }
        public DbSet<DocType> DocTypes { get; set; }
        public DbSet<DocFixedFolder> DocFixedFolders { get; set; }

        public DbSet<DocGmailCaseLink> DocGmailCaseLinks { get; set; }

        public DbSet<DocOutlook> DocOutlook { get; set; }
        public DbSet<DocOutlookCaseLink> DocOutlookCaseLinks { get; set; }
        public DbSet<DocOutlookId> DocOutlookIds { get; set; }
        public DbSet<DocReviewDTO> DocReviewDTO { get; set; }

        public DbSet<DocVerification> DocVerifications { get; set; }
        public DbSet<DocVerificationSearchField> DocVerificationSearchFields { get; set; }
        public DbSet<DocumentVerificationNewDTO> DocumentVerificationNewDTO { get; set; }
        public DbSet<DocumentVerificationDTO> DocumentVerificationDTO { get; set; }
        public DbSet<DocumentVerificationActionDTO> DocumentVerificationActionDTO { get; set; }
        public DbSet<DocumentVerificationCommunicationDTO> DocumentVerificationCommunicationDTO { get; set; }

        public DbSet<DocResponsibleLog> DocResponsibleLogs { get; set; }
        public DbSet<DocResponsibleDocketing> DocRespDocketings { get; set; }
        public DbSet<DocResponsibleReporting> DocRespReportings { get; set; }

        public DbSet<DocQuickEmailLog> DocQuickEmailLogs { get; set; }
        #endregion

        #region Others
        public DbSet<ActionTabDTO> ActionTabDTO { get; set; }
        public DbSet<LookupDTO> LookupDTO { get; set; }
        public DbSet<LookupDescDTO> LookupDescDTO { get; set; }
        public DbSet<LookupIntDTO> LookupIntDTO { get; set; }
        #endregion

        #region API
        public DbSet<WebServiceLog> WebServiceLogs { get; set; }
        #endregion

        #region Task Scheduler
        // Add ScheduledTask to include it in ModelBuilder
        // to enable EncryptionConverter for Password property
        public DbSet<ScheduledTask> ScheduledTasks { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //Column encryption
            builder.UseEncryption();

            builder.Entity<CPiLanguage>().ToTable("tblCPiLanguage");

            builder.ApplyConfiguration(new DeleteLogMap());

            #region Shared Auxiliaries
            //shared
            builder.ApplyConfiguration(new LanguageMap());
            builder.ApplyConfiguration(new CurrencyTypeMap());
            builder.ApplyConfiguration(new ImageTypeMap());
            builder.ApplyConfiguration(new SearchCriteriaMap());
            builder.ApplyConfiguration(new SearchCriteriaDetailMap());

            builder.ApplyConfiguration(new HelpMap());
            #endregion

            #region Patent
            //patent
            builder.ApplyConfiguration(new PatDisclosureStatusMap());
            builder.ApplyConfiguration(new PatCountryLawMap());
            builder.ApplyConfiguration(new PatCountryDueMap());
            builder.ApplyConfiguration(new PatCountryExpMap());
            builder.ApplyConfiguration(new PatCountryMap());
            builder.ApplyConfiguration(new PatAreaMap());
            builder.ApplyConfiguration(new PatAreaCountryMap());
            builder.ApplyConfiguration(new PatCaseTypeMap());
            builder.ApplyConfiguration(new PatApplicationStatusMap());
            builder.ApplyConfiguration(new PatDesCaseTypeMap());
            builder.ApplyConfiguration(new PatDesCaseTypeFieldsMap());
            builder.ApplyConfiguration(new PatDesignatedCountryMap());
            builder.ApplyConfiguration(new PatActionTypeMap());
            builder.ApplyConfiguration(new PatActionParameterMap());

            builder.ApplyConfiguration(new PatCountryLawUpdateMap());

            builder.ApplyConfiguration(new PatWorkflowMap());
            builder.ApplyConfiguration(new PatWorkflowActionMap());
            builder.ApplyConfiguration(new PatWorkflowActionParameterMap());
            builder.ApplyConfiguration(new PatScoreCategoryMap());
            builder.ApplyConfiguration(new PatScoreMap());
            builder.ApplyConfiguration(new PatScoreDTOMap());
            builder.ApplyConfiguration(new PatAverageScoreDTOMap());
            builder.ApplyConfiguration(new PatIndicatorMap());

            #endregion

            #region Trademark
            //trademark
            builder.ApplyConfiguration(new TmkCountryMap());
            builder.ApplyConfiguration(new TmkAreaMap());
            builder.ApplyConfiguration(new TmkAreaCountryMap());
            builder.ApplyConfiguration(new TmkCaseTypeMap());
            builder.ApplyConfiguration(new TmkDesCaseTypeMap());
            builder.ApplyConfiguration(new TmkDesCaseTypeFieldsMap());
            builder.ApplyConfiguration(new TmkConflictStatusMap());
            builder.ApplyConfiguration(new TmkCountryLawMap());
            builder.ApplyConfiguration(new TmkCountryDueMap());
            builder.ApplyConfiguration(new TmkActionTypeMap());
            builder.ApplyConfiguration(new TmkActionParameterMap());
            builder.ApplyConfiguration(new TmkConflictMap());
            builder.ApplyConfiguration(new TmkDesignatedCountryMap());

            builder.ApplyConfiguration(new TmkCountryLawUpdateMap());
            builder.ApplyConfiguration(new TmkWorkflowMap());
            builder.ApplyConfiguration(new TmkWorkflowActionMap());
            builder.ApplyConfiguration(new TmkWorkflowActionParameterMap());
            builder.ApplyConfiguration(new TmkIndicatorMap());
            builder.ApplyConfiguration(new TmkStandardGoodMap());

            #endregion

            #region Shared
            builder.ApplyConfiguration(new AppSystemMap());
            #endregion

            #region Security & System Logs
            builder.Entity<CPiSystem>().ToTable("tblCPiSystems");
            builder.Entity<CPiRole>().ToTable("tblCPiRoles");

            builder.ApplyConfiguration(new CPiUserMap());
            builder.ApplyConfiguration(new CPiSystemMap());
            builder.ApplyConfiguration(new CPiRoleMap());
            builder.ApplyConfiguration(new CPiSystemRoleMap());
            builder.ApplyConfiguration(new CPiUserTypeSystemRoleMap());
            builder.ApplyConfiguration(new CPiUserTypeDefaultPageMap());
            builder.ApplyConfiguration(new CPiUserTypeDefaultWidgetMap());
            builder.ApplyConfiguration(new CPiUserSystemRoleMap());
            builder.ApplyConfiguration(new CPiUserPasswordHistoryMap());
            builder.ApplyConfiguration(new CPiUserClaimMap());
            builder.ApplyConfiguration(new CPiUserEntityFilterMap());
            builder.ApplyConfiguration(new CPiRespOfficeMap());
            builder.ApplyConfiguration(new CPiSSOClaimSystemRoleMap());
            builder.ApplyConfiguration(new CPiSSOClaimUserMap());

            builder.ApplyConfiguration(new ErrorMappingMap());

            #endregion

            #region System Tables
            builder.Entity<ModuleMain>().ToTable("tblModule");

            builder.ApplyConfiguration(new CPiMenuItemMap());
            builder.ApplyConfiguration(new CPiMenuPageMap());

            builder.ApplyConfiguration(new LocalizationRecordsGroupingMap());
            builder.ApplyConfiguration(new LocalizationRecordsMap());
            builder.ApplyConfiguration(new NotificationMap());
            builder.ApplyConfiguration(new NotificationConnectionMap());

            builder.ApplyConfiguration(new CPiDefaultPageMap());
            builder.ApplyConfiguration(new CPiSettingMap());
            builder.ApplyConfiguration(new CPiUserSettingMap());
            builder.ApplyConfiguration(new CPiSystemSettingMap());
            builder.ApplyConfiguration(new CPiUserSettingLogMap());

            builder.ApplyConfiguration(new CPiWidgetMap());
            builder.ApplyConfiguration(new CPiUserWidgetMap());

            builder.ApplyConfiguration(new OptionMap());
            builder.ApplyConfiguration(new SystemScreenMap());

            builder.ApplyConfiguration(new ActivityLogMap());
            builder.ApplyConfiguration(new ApiLogMap());
            builder.ApplyConfiguration(new SysCustomFieldSettingMap());

            builder.ApplyConfiguration(new CPiGroupMap());
            builder.ApplyConfiguration(new CPiUserGroupMap());

            builder.ApplyConfiguration(new ScheduledTaskMap());
            #endregion

            #region Documents
            builder.ApplyConfiguration(new DocSystemMap());
            builder.ApplyConfiguration(new DocMatterTreeMap());
            builder.ApplyConfiguration(new DocFolderMap());
            builder.ApplyConfiguration(new DocDocumentMap());
            builder.ApplyConfiguration(new DocDocumentTagMap());
            builder.ApplyConfiguration(new DocFileMap());
            builder.ApplyConfiguration(new DocFileSignatureMap());
            builder.ApplyConfiguration(new SharePointFileSignatureMap());
            builder.ApplyConfiguration(new DocFileSignatureRecipientMap());
            builder.ApplyConfiguration(new DocIconMap());
            builder.ApplyConfiguration(new DocTypeMap());
            builder.ApplyConfiguration(new DocFixedFolderMap());

            builder.ApplyConfiguration(new DocGmailCaseLinkMap());
            builder.ApplyConfiguration(new DocOutlookMap());
            builder.ApplyConfiguration(new DocOutlookCaseLinkMap());
            builder.ApplyConfiguration(new DocOutlookIdMap());

            builder.ApplyConfiguration(new DocVerificationMap());
            builder.ApplyConfiguration(new DocVerificationSearchFieldMap());

            builder.ApplyConfiguration(new DocResponsibleLogMap());
            builder.ApplyConfiguration(new DocResponsibleDocketingMap());
            builder.ApplyConfiguration(new DocResponsibleReportingMap());

            builder.ApplyConfiguration(new DocQuickEmailLogMap());
            #endregion

            #region API
            builder.ApplyConfiguration(new DocWebSvcMap());
            builder.ApplyConfiguration(new WebServiceLogMap());
            #endregion

            // Fix breaking changes in EF7
            // SqlFunctionExpression.Create is obsolete
            // builder.HasDbFunction(typeof(SqlHelper).GetMethod(nameof(SqlHelper.JsonValue)))
            //    .HasTranslation(e => SqlFunctionExpression.Create(
            //        "JSON_VALUE", e, typeof(string), null));
            //https://github.com/dotnet/efcore/issues/11295
            builder.HasDbFunction(typeof(SqlHelper).GetMethod(nameof(SqlHelper.JsonValue)))
                .HasTranslation(e => new SqlFunctionExpression(
                    "JSON_VALUE",
                    e,
                    nullable: true,
                    argumentsPropagateNullability: new[] { false, false },
                    typeof(string),
                    null));

            base.OnModelCreating(builder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Mitigate breaking changes in EF7
            // SQL Server tables with triggers or certain computed columns now require special EF Core configuration
            configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
        }

        public void DetachAllEntities()
        {
            var undetachedEntriesCopy = this.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Detached)
                .ToList();

            foreach (var entry in undetachedEntriesCopy)
                entry.State = EntityState.Detached;
        }

        public List<EntityEntry> GetAllTrackedEntities()
        {
            return this.ChangeTracker.Entries().ToList();

        }
    }
}
