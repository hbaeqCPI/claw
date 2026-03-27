using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Identity;

namespace R10.Core.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<DeleteLog> DeleteLogs { get; set; }

        #region Shared Auxiliaries
        DbSet<SharedCountryLookupDTO> SharedCountryLookupDTO { get; set; }
        #endregion

        #region Patent
        DbSet<PatDesignatedCountry> PatDesignatedCountries { get; set; }

        DbSet<PatCountry> PatCountries { get; set; }
        DbSet<PatArea> PatAreas { get; set; }
        DbSet<PatAreaCountry> PatAreasCountries { get; set; }
        DbSet<PatCountryLaw> PatCountryLaws { get; set; }
        DbSet<PatCountryDue> PatCountryDues { get; set; }
        DbSet<PatCountryExp> PatCountryExpirations { get; set; }
        DbSet<PatCaseType> PatCaseTypes { get; set; }
        DbSet<PatActionType> PatActionTypes { get; set; }
        DbSet<PatDesCaseType> PatDesCaseTypes { get; set; }
        DbSet<PatDesCaseTypeFields> PatDesCaseTypeFields { get; set; }

        DbSet<PatCountryLawUpdate> PatCountryLawUpdate { get; set; }

        DbSet<LookupDescDTO> PatActionTypeDTO { get; set; }
        DbSet<PatIndicator> PatIndicators { get; set; }
        DbSet<PatAreaDelete> PatAreaDeletes { get; set; }
        DbSet<PatAreaCountryDelete> PatAreaCountryDeletes { get; set; }
        DbSet<PatCountryExpDelete> PatCountryExpDeletes { get; set; }
        DbSet<PatCountryLawExt> PatCountryLawExts { get; set; }
        DbSet<PatDesCaseTypeExt> PatDesCaseTypeExts { get; set; }
        DbSet<PatDesCaseTypeDelete> PatDesCaseTypeDeletes { get; set; }
        DbSet<PatDesCaseTypeDeleteExt> PatDesCaseTypeDeleteExts { get; set; }
        DbSet<PatDesCaseTypeFieldsExt> PatDesCaseTypeFieldsExts { get; set; }
        DbSet<PatDesCaseTypeFieldsDelete> PatDesCaseTypeFieldsDeletes { get; set; }
        DbSet<PatDesCaseTypeFieldsDeleteExt> PatDesCaseTypeFieldsDeleteExts { get; set; }
        #endregion

        #region Trademark
        DbSet<TmkCountry> TmkCountries { get; set; }
        DbSet<TmkArea> TmkAreas { get; set; }
        DbSet<TmkAreaCountry> TmkAreasCountries { get; set; }

        DbSet<TmkCaseType> TmkCaseTypes { get; set; }
        DbSet<TmkDesCaseType> TmkDesCaseTypes { get; set; }
        DbSet<TmkDesCaseTypeFields> TmkDesCaseTypeFields { get; set; }
        DbSet<TmkCountryLaw> TmkCountryLaws { get; set; }
        DbSet<TmkCountryDue> TmkCountryDues { get; set; }
        DbSet<TmkActionType> TmkActionTypes { get; set; }
        DbSet<TmkDesignatedCountry> TmkDesignatedCountries { get; set; }

        DbSet<TmkCountryLawUpdate> TmkCountryLawUpdate { get; set; }
        DbSet<TmkIndicator> TmkIndicators { get; set; }
        DbSet<LookupDescDTO> TmkActionTypeDTO { get; set; }
        DbSet<TmkStandardGood> TmkStandardGoods { get; set; }
        DbSet<TmkAreaDelete> TmkAreaDeletes { get; set; }
        DbSet<TmkAreaCountryDelete> TmkAreaCountryDeletes { get; set; }
        DbSet<TmkDesCaseTypeExt> TmkDesCaseTypeExts { get; set; }
        DbSet<TmkDesCaseTypeDelete> TmkDesCaseTypeDeletes { get; set; }
        DbSet<TmkDesCaseTypeDeleteExt> TmkDesCaseTypeDeleteExts { get; set; }
        DbSet<TmkDesCaseTypeFieldsExt> TmkDesCaseTypeFieldsExts { get; set; }
        DbSet<TmkDesCaseTypeFieldsDelete> TmkDesCaseTypeFieldsDeletes { get; set; }
        DbSet<TmkDesCaseTypeFieldsDeleteExt> TmkDesCaseTypeFieldsDeleteExts { get; set; }
        #endregion

        #region Releases
        DbSet<Release> Releases { get; set; }
        #endregion

        #region Shared
        DbSet<AppSystem> AppSystems { get; set; }
        #endregion

        #region Images
        DbSet<ImageType> ImageTypes { get; set; }
        #endregion

        #region Shared
        DbSet<CPiLanguage> CPiLanguages { get; set; }
        DbSet<Language> Languages { get; set; }
        DbSet<SearchCriteria> SearchCriteria { get; set; }
        DbSet<SearchCriteriaDetail> SearchCriteriaDetails { get; set; }
        #endregion

        #region Documents
        DbSet<DocSystem> DocSystems { get; set; }
        DbSet<DocMatterTree> DocMatterTrees { get; set; }
        DbSet<DocTreeDTO> DocTreeDTO { get; set; }
        DbSet<DocTreeEmailApiDTO> DocTreeEmailApiDTO { get; set; }

        DbSet<DocImageDetailDTO> DocImageDetailDTO { get; set; }
        DbSet<DocIDSRelCasesDTO> DocIDSRelCasesDTO { get; set; }
        DbSet<DocIDSNonPatLitDTO> DocIDSNonPatLitDTO { get; set; }
        DbSet<DocViewDTO> DocViewDTO { get; set; }
        DbSet<DocInfoDTO> DocInfoDTO { get; set; }

        DbSet<DocFolder> DocFolders { get; set; }
        DbSet<DocDocument> DocDocuments { get; set; }
        DbSet<DocDocumentTag> DocDocumentTags { get; set; }
        DbSet<DocFile> DocFiles { get; set; }
        DbSet<DocFileSignature> DocFileSignatures { get; set; }
        DbSet<SharePointFileSignature> SharePointFileSignatures { get; set; }
        DbSet<DocFileSignatureRecipient> DocFileSignatureRecipients { get; set; }

        DbSet<DocIcon> DocIcons { get; set; }
        DbSet<DocType> DocTypes { get; set; }
        DbSet<DocFixedFolder> DocFixedFolders { get; set; }

        DbSet<DocGmailCaseLink> DocGmailCaseLinks { get; set; }

        DbSet<DocOutlook> DocOutlook { get; set; }
        DbSet<DocOutlookCaseLink> DocOutlookCaseLinks { get; set; }
        DbSet<DocOutlookId> DocOutlookIds { get; set; }
        DbSet<DocReviewDTO> DocReviewDTO { get; set; }

        DbSet<DocVerification> DocVerifications { get; set; }
        DbSet<DocVerificationSearchField> DocVerificationSearchFields { get; set; }

        DbSet<DocResponsibleLog> DocResponsibleLogs { get; set; }
        DbSet<DocResponsibleDocketing> DocRespDocketings { get; set; }
        DbSet<DocResponsibleReporting> DocRespReportings { get; set; }

        DbSet<DocQuickEmailLog> DocQuickEmailLogs { get; set; }
        #endregion

        #region System Tables
        DbSet<CPiMenuItem> CPiMenuItems { get; set; }
        DbSet<CPiMenuPage> CPiMenuPages { get; set; }

        DbSet<CPiDefaultPage> CPiDefaultPages { get; set; }
        DbSet<CPiSetting> CPiSettings { get; set; }
        DbSet<CPiUserSetting> CPiUserSettings { get; set; }
        DbSet<CPiSystemSetting> CPiSystemSettings { get; set; }

        DbSet<CPiWidget> CPiWidgets { get; set; }
        DbSet<CPiUserWidget> CPiUserWidgets { get; set; }

        DbSet<Option> Options { get; set; }
        DbSet<ModuleMain> ModulesMain { get; set; }
        DbSet<SystemScreen> SystemScreens { get; set; }

        DbSet<ChartDTO> ChartDTO { get; set; }
        DbSet<CaseListDTO> CaseListDTO { get; set; }

        DbSet<LocalizationRecords> LocalizationRecords { get; set; }
        DbSet<LocalizationRecordsGrouping> LocalizationRecordsGrouping { get; set; }
        DbSet<Notification> Notifications { get; set; }
        DbSet<NotificationConnection> NotificationConnections { get; set; }
        DbSet<SysCustomFieldSetting> SysCustomFieldSettings { get; set; }

        #endregion

        #region Security & System Logs

        DbSet<CPiUserEntityFilter> CPiUserEntityFilters { get; set; }
        DbSet<CPiUserSystemRole> CPiUserSystemRoles { get; set; }
        DbSet<CPiRespOffice> CPiRespOffices { get; set; }
        DbSet<CPiUser> CPiUser { get; set; }
        DbSet<CPiGroup> CPiGroups { get; set; }
        DbSet<Log> Logs { get; set; }
        #endregion

        #region Others
        DbSet<LookupIntDTO> LookupIntDTO { get; set; }
        #endregion

        #region API
        DbSet<WebServiceLog> WebServiceLogs { get; set; }
        #endregion

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        EntityEntry Entry(object entity);

        DatabaseFacade Database { get; }
        void DetachAllEntities();
        List<EntityEntry> GetAllTrackedEntities();
    }
}
