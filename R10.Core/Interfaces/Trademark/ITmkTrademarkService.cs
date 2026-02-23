using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using R10.Core.Entities.Documents;
using R10.Core.Identity;
using R10.Core.Entities.Patent;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace R10.Core.Interfaces
{
    public interface ITmkTrademarkService
    {
        Task<TmkTrademark> GetByIdAsync(int tmkId);
        Task AddTrademark(TmkTrademark tmkTrademark, DateTime dateCreated);
        Task<int> UpdateTrademark(TmkTrademark tmkTrademark, DateTime? dateCreated);
        Task UpdateDeDocket(TmkTrademark tmkTrademark);
        Task DeleteTrademark(TmkTrademark tmkTrademark, bool validateRecordFilter = true);
        Task<Tuple<string, string, string,string>> CopyTrademark(int oldTmkId, string newCaseNumber, string newSubCase, List<int> countryIds,
                           bool copyCaseInfo, bool copyRemarks, bool copyAssignments, bool copyGoods, bool copyImages,
                           bool copyKeywords, bool copyDesCountries, bool copyLicenses, bool copyRelatedCases, string createdBy, string relationship, bool copyProducts, bool copyOwners);
        Task UpdateChild<T>(int tmkId, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task SyncChildToDesignatedTrademarks(TmkTrademark trademark, TmkTrademarkModifiedFields modifiedFields, Type childType);

        Task<List<LookupDTO>> GetAllowedRespOffices(List<string> roles);
        Task<List<SysCustomFieldSetting>> GetCustomFields();
        Task<bool> HasParentClassNotInChild(int parentTmkId, int tmkId);
        Task<List<TmkTrademarkClass>> GetParentClassNotInChild(int parentTmkId, int tmkId);
        Task<List<TmkTrademarkClass>> GetTrademarkClass(string? caseNumber, string? country, string? subCase, string? appNo, string? regNo, string? trademark, int classId, string? goods);
        Task<List<TmkTrademarkClass>> GetTrademarkClass();
        IQueryable<TmkTrademarkClass> TmkTrademarkClasses { get; }


        #region Workflow
        Task GenerateWorkflowFromEmailSent(int tmkId, int qeSetupId);
        Task GenerateWorkflowFromActionEmailSent(int actId, int qeSetupId);
        Task GenerateWorkflowAction(int tmkId, int actionTypeId, DateTime? baseDate = null);
        Task <List<TmkActionDue>>CloseWorkflowAction(int tmkId, int actionTypeId);
        Task<bool> HasWorkflowEnabled(TmkWorkflowTriggerType triggerType);
        Task<List<TmkWorkflowAction>> CheckWorkflowAction(TmkWorkflowTriggerType triggerType);
        Task<List<TmkWorkflowActionParameter>> CheckWorkflowActionParameters(TmkWorkflowTriggerType triggerType);
        Task<List<TmkDueDate>> GenerateDueDateFromActionParameterWorkflow(TmkActionDue? newActionDue, List<TmkDueDate> dueDates, TmkWorkflowTriggerType triggerType, bool clearBase = true);
        Task<List<TmkDueDate>> GetUpdatedDueDateIndicator(int actId, List<TmkDueDate> dueDates);
        #endregion

        bool IsOwnerRequired { get; }

        IQueryable<TmkTrademark> TmkTrademarks { get; }
        IQueryable<T> QueryableChildList<T>() where T : BaseEntity;
        IQueryable<TmkStandardGood> TmkStandardGoods { get; }
        IQueryable<TmkCountryDue> TmkCountryDues { get; }
        IQueryable<TmkActionType> TmkActionTypes { get; }

        // renewal date generation
        TmkTrademarkRenewalFields GetTrademarkRenewal(TmkTrademarkRenewalParameters param);
        DateTime? GetTrademarkRenewalDate(TmkTrademarkRenewalParameters param);
        bool AnyRenewalDateParametersModified(TmkTrademark trademark);


        // family/designation
        Task<bool> CanHaveDesignatedCountry(string country, string caseType);
        Task<List<PatParentCaseDTO>> GetPossibleFamilyReferences(int tmkId, string trademarkName);      // main screen, designation tab
        Task<List<PatParentCaseDTO>> ParentTrademarks();                                                // search screen


        Task ValidatePermission(int tmkId);
        Task<bool> CanModifyAttorney(int attorneyId);


        //child service - actions
        Task<List<ActionTabDTO>> GetActions(int tmkId, ActionDisplayOption actionDisplayOption);
        Task ActionsUpdate(int tmkId, string userName, IEnumerable<TmkDueDate> updatedActions, IEnumerable<TmkDueDate> deletedActions);
        Task ActionDelete(TmkDueDate deletedAction);
        Task<List<DelegationEmailDTO>> GetDelegationEmails(int delegationId);
        Task MarkDelegationasEmailed(int delegationId);
        Task<List<LookupIntDTO>> GetDelegatedDdIds(int action, int[] recIds);
        Task<List<DelegationEmailDTO>> GetDeletedDelegationEmails(int delegationId);
        Task<List<LookupIntDTO>> GetDuedateChangedDelegationIds(int action, List<TmkDueDate> updated);
        Task<DelegationDetailDTO> GetDeletedDelegation(int delegationId);
        IQueryable<DeDocketInstruction> DeDocketInstructions { get; }
        Task<bool> HasOutstandingDedocket(int tmkId);

        //child service - costs 
        Task<List<TmkCostTrack>> GetCosts(int tmkId);
        Task CostsUpdate(int tmkId, string userName, IEnumerable<TmkCostTrack> updatedCostTracks, IEnumerable<TmkCostTrack> deletedCostTracks);
        Task CostDelete(TmkCostTrack deletedCostTrack);

        //child service - licensees 
        Task<List<TmkLicensee>> GetLicensees(int tmkId);
        Task LicenseesUpdate(int tmkId, string userName, IEnumerable<TmkLicensee> updatedLicensees, IEnumerable<TmkLicensee> newLicensees, IEnumerable<TmkLicensee> deletedLicensees);
        Task LicenseeDelete(TmkLicensee deletedLicensee);
        Task<bool> HasLicensees(int tmkId);

        //family tree view
        Task<IEnumerable<FamilyTreeDTO>> GetFamilyTree(string paramType, string paramValue, string paramParent);
        Task<FamilyTreeTmkDTO> GetNodeDetails(string paramType, string paramValue);
        void UpdateParent(int childTmkId, int newParentId, string parentInfo, string userName);

        //product
        Task<bool> HasProducts(int tmkId);
        IQueryable<Product> Products { get; }

        //assignment
        IQueryable<TmkAssignmentHistory> TmkAssignmentsHistory { get; }

        //copy setting
        IQueryable<TmkTrademarkCopySetting> TmkTrademarkCopySettings { get; }
        Task UpdateCopySetting(TmkTrademarkCopySetting setting);
        Task<CPiUserSetting> GetMainCopySettings(string userId);
        Task UpdateMainCopySettings(CPiUserSetting userSetting);
        Task<int> GetMainCopySettingId();
        Task AddCopySettings(List<TmkTrademarkCopySetting> settings);
        Task AddCustomFieldsAsCopyFields();

        //documents
        IQueryable<DocDocument> Documents { get; }

        void DetachAllEntities();
        List<EntityEntry> GetAllTrackedEntities();

        Task<int> GetRequestDocketPendingCount(int tmkId);
        Task<List<TmkDocketRequest>> GetRequestDockets(int tmkId, bool outstandingOnly);

    }
}
