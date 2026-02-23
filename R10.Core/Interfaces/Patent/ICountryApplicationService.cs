using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.GeneralMatter;
using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace R10.Core.Interfaces.Patent
{
    public interface ICountryApplicationService
    {
        Task<CountryApplication> GetById(int appId);
        Task ValidateRecordFilterPermission(int appId);
        Task AddCountryApplication(CountryApplication countryApplication, PatIDSRelatedCasesInfo idsInfo, DateTime dateCreated, bool hasRelatedCasesMassCopy,string? sessionKey);
        Task UpdateCountryApplication(CountryApplication countryApplication, PatIDSRelatedCasesInfo idsInfo, DateTime dateCreated, bool hasRelatedCasesMassCopy=false, string? sessionKey="");
        Task DeleteCountryApplication(CountryApplication countryApplication, bool validateRecordFilter);
        Task CopyCountryApplication(int oldAppId, int newAppId, bool copyImages, bool copyAssignments,
            bool copyInventors, bool copyLicenses, bool copyOwners, bool copyCosts, bool copyIDS, bool copyRelatedCases,
            bool copyRelatedTrademarks, bool copyInventorAward, bool copyProducts, bool copyterminalDisclaimer, string userName);
        Task GenerateCountryLawFromPriority(int invId, string userName);
        Task UpdateExpirationDate(List<PatTerminalDisclaimerChildDTO> children, string updatedBy);
        Task UpdateChild<T>(int appId, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task SyncChildToDesignatedApplications(int appId, string country, string caseType, string userName, Type childType);
        Task<CountryApplication> GetInventorAwardInfo(int appId);
        Task<List<PatActionMultipleBasedOnDTO>> GetActionsWithMultipleBasedOn(int appId, string? sessionKey);
        Task GenerateActionsWithMultipleBasedOn(List<PatActionMultipleBasedOnSelectionDTO> list, string? createdBy);

        IQueryable<T> QueryableChildList<T>() where T : BaseEntity;
        IQueryable<CountryApplication> CountryApplications { get; }
        IQueryable<PatParentCaseDTO> ParentApplications { get; }
        IQueryable<PatParentCaseTDDTO> TerminalDisclaimerParents { get; }
        IQueryable<PatCountry> PatCountries { get; }
        IQueryable<Agent> Agents { get; }
        IQueryable<Attorney> Attorneys { get; }
        IQueryable<Client> Clients { get; }
        IQueryable<Owner> Owners { get; }
        IQueryable<Product> Products { get; }
        IQueryable<GMMatterPatent> GMMatterPatents { get; }
        IQueryable<PatCountryLaw> PatCountryLaws { get; }
        IQueryable<PatCountryDue> PatCountryDues { get; }
        IQueryable<PatActionType> PatActionTypes { get; }
        IQueryable<PatActionParameter> PatActionParameters { get; }
        IQueryable<PatIndicator> PatIndicators { get; }
        IQueryable<PatActionDue> PatActionDues { get; }
        IQueryable<PatDueDate> PatDueDates { get; }
        IQueryable<PatAssignmentHistory> PatAssignmentsHistory { get; }
        IQueryable<PatRelatedCaseDTO> PatRelatedCaseDTO { get; }
        IQueryable<PatApplicationStatus> ApplicationStatuses { get; }
        IQueryable<CountryApplicationCopySetting> CountryApplicationCopySettings { get; }
        List<CountryApplicationCopySettingChild> CountryApplicationCopySettingsChild { get; }

        Task<List<LookupDTO>> GetAllowedRespOffices();
        Task<List<CPiUserEntityFilter>> GetUserEntityFilters();
        Task<string> GetTaxScheduleLabel(string country, string caseType);
        Task<bool> ShouldLockRecord(int appId);
        Task RefreshCopySetting(List<CountryApplicationCopySetting> added, List<CountryApplicationCopySetting> deleted);
        Task UpdateCopySetting(CountryApplicationCopySetting setting);
        Task AddCopySettings(List<CountryApplicationCopySetting> settings);
        Task<CPiUserSetting> GetMainCopySettings(string userId);
        Task UpdateMainCopySettings(CPiUserSetting userSetting);
        Task<int> GetMainCopySettingId();
        Task AddCustomFieldsAsCopyFields();

        Task TerminalDisclaimerAddAction(PatActionDue actionDue, DateTime expirationDate);
        Task<bool> HasTerminalDisclaimerAction(int appId);
        Task<List<PatTerminalDisclaimerChildDTO>> GetTerminalDisclaimerChildren(int appId);
        Task<List<PatParentCaseDTO>> GetAllPossibleTerminalDisclaimer(int appId);
        Task<int> GetActiveTerminalDisclaimerAppId(int appId);

        bool IsOwnerRequired { get; }
        bool IsInventorRequired { get; }
        Task<ApplicationModifiedFields> GetModifiedFields(CountryApplication modified);

        #region Workflow
        Task GenerateWorkflowAction(int appId, int actionTypeId, DateTime baseDate);
        Task <List<PatActionDue>> CloseWorkflowAction(int appId, int actionTypeId);
        Task GenerateWorkflowFromEmailSent(int appId, int qeSetupId);
        Task GenerateWorkflowFromActionEmailSent(int actId, int qeSetupId);
        Task<bool> HasWorkflowEnabled(PatWorkflowTriggerType triggerType);
        Task<List<PatWorkflowAction>> CheckWorkflowAction(PatWorkflowTriggerType triggerType);
        Task<List<PatWorkflowActionParameter>> CheckWorkflowActionParameters(PatWorkflowTriggerType triggerType);
        Task<List<PatDueDate>> GenerateDueDateFromActionParameterWorkflow(PatActionDue? newActionDue, List<PatDueDate> dueDates, PatWorkflowTriggerType triggerType, bool clearBase = true);
        Task<List<PatDueDate>> GetUpdatedDueDateIndicator(int actId, List<PatDueDate> dueDates);
        #endregion

        #region Designation
        Task<bool> CanHaveDesignatedCountry(string country, string caseType);
        Task<object[]> GetSelectableDesignatedCountries(string country, string caseType, int appId);
        Task<string[]> GetSelectableDesignatedCaseTypes(string country, string caseType, string desCountry);
        Task DesignateCountries(int appId, bool fromCountryLaw, string createdBy);
        Task<List<PatDesignatedCountry>> GetSelectableCountries(int appId);
        Task GenerateApplications(int parentAppId, string desCountries, string updatedBy);
        Task MarkDesCountriesWithExistingApps(int appId);
        #endregion

        #region Related Case
            Task<List<PatRelatedCaseDTO>> GetRelatedCases(int appId);
        Task<bool> HasRelatedCases(int appId);
        #endregion

        #region Licensees
        Task<bool> HasLicensees(int appId);
        #endregion

        #region Products
        Task<bool> HasProducts(int appId);
        #endregion

        #region Family Tree View

        Task<IEnumerable<FamilyTreeDTO>> GetFamilyTree(string paramType, string paramValue, string paramParent);
        FamilyTreePatDTO GetNodeDetails(string paramType, string paramValue);
        void UpdateParent(int childAppId, int newParentId, string parentInfo, string userName);
        Task<List<PatParentCaseDTO>> GetPossibleFamilyReferences(int appId, string caseNumber);
        Task<List<FamilyTreeParentCaseDTO>> GetPossibleFamilyTreeReferences(int appId, string caseNumber);

        //string GetExpandedNodes(string paramType, string paramValue);

        #endregion

        #region Action
        Task<List<DelegationEmailDTO>> GetDelegationEmails(int delegationId);
        Task MarkDelegationasEmailed(int delegationId);
        Task<List<LookupIntDTO>> GetDelegatedDdIds(int action, int[] recIds);
        Task<List<DelegationEmailDTO>> GetDeletedDelegationEmails(int delegationId);
        Task<List<LookupIntDTO>> GetDuedateChangedDelegationIds(int action, List<PatDueDate> updated);
        Task<DelegationDetailDTO> GetDeletedDelegation(int delegationId);
        IQueryable<DeDocketInstruction> DeDocketInstructions { get; }
        Task UpdateDeDocket(CountryApplication countryApplication);
        Task<bool> HasOutstandingDedocket(int appId);
        #endregion

        #region Unitary Patent
        Task<bool> ShouldShowUnitaryPatentFields(int action, string country, string caseType, int appId);
        Task<int> GetUnitaryPatentDesignatedCount(int action, string country, string caseType, int appId);
        Task<List<PatDesignationDTO>> GetDesignatedCountries(int appId);
        #endregion

        

        void DetachAllEntities();
        List<EntityEntry> GetAllTrackedEntities();

        Task<List<int>> GenerateEPODocMappedAction(int appId, string documentCode, DateTime baseDate);
        Task<List<int>> GenerateEPOActMappedAction(int appId, string termKey, DateTime epoDueDate);

        Task<int> GetRequestDocketPendingCount(int appId);
        Task<List<PatDocketRequest>> GetRequestDockets(int appId,bool outstandingOnly);
    }
}
