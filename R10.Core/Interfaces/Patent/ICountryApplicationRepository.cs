using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Identity;

namespace R10.Core.Interfaces
{

    public interface ICountryApplicationRepository
    {
        Task Update(CountryApplication entity, PatIDSRelatedCasesInfo idsInfo, ApplicationModifiedFields modifiedFields, DateTime dateCreated, bool hasRelatedCasesMassCopy,string? sessionKey);
        Task<CountryApplication> Add(CountryApplication application, PatIDSRelatedCasesInfo idsInfo, ApplicationModifiedFields modifiedFields, DateTime dateCreated, bool hasRelatedCasesMassCopy, string? sessionKey);
        Task Delete(CountryApplication application);
        Task CopyCountryApplication(int oldAppId, int newAppId, bool copyImages, bool copyAssignments,
            bool copyInventors, bool copyLicenses, bool copyOwners, bool copyCosts, bool copyIDS, bool copyRelatedCases,
            bool copyRelatedTrademarks, bool copyInventorAward, bool copyProducts, bool copyterminalDisclaimer, string userName);

        Task GenerateCountryLawFromPriority(int invId, string userName);
        Task UpdateExpirationDate(List<PatTerminalDisclaimerChildDTO> children, string updatedBy);
        Task UpdateChild<T>(CountryApplication application, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task SyncChildToDesignatedApplications(int appId, string country, string caseType, string userName, Type childType);
        Task AddCustomFieldsAsCopyFields();
        Task<List<PatActionMultipleBasedOnDTO>> GetActionsWithMultipleBasedOn(int appId, string? sessionKey);
        Task GenerateActionsWithMultipleBasedOn(List<PatActionMultipleBasedOnSelectionDTO> list, string? createdBy);
        Task InsertEPDesignatedCountriesOwner(int parentAppId, string updatedBy);
        #region Designation
        Task<bool> CanHaveDesignatedCountry(string country, string caseType);
        Task<object[]> GetSelectableDesignatedCountries(string country, string caseType, int appId);
        Task<string[]> GetSelectableDesignatedCaseTypes(string country, string caseType, string desCountry);
        Task<List<PatParentCaseDTO>> GetPossibleFamilyReferences(int appId, string caseNumber);
        Task<List<PatParentCaseDTO>> GetAllPossibleTerminalDisclaimer(int appId);
        Task<int> GetActiveTerminalDisclaimerAppId(int appId);
        Task<List<PatTerminalDisclaimerChildDTO>> GetTerminalDisclaimerChildren(int appId);
        Task DesignateCountries(int appId, bool fromCountryLaw, string createdBy);
        Task GenerateApplications(int parentAppId, string desCountries, string updatedBy);
        Task<List<PatDesignatedCountry>> GetSelectableCountries(int appId);
        Task MarkDesCountriesWithExistingApps(int appId);
        #endregion

        #region RelatedCases
        Task<List<PatRelatedCaseDTO>> GetRelatedCases(int appId);
        Task RelatedCasesMassCopy(int appId, int invId, string? createdBy);
        #endregion

        #region IDS
        Task IDSUpdateFilDate(int appId, string filDateType, string recordType, string userName, DateTime? filDate, DateTime? specificFilDate, bool consideredByExaminer);
        Task UpdateConsideredByExaminer(int appId, string filDateType, string recordType, DateTime? filDateFrom, DateTime? filDateTo, DateTime? specificFilDate, string userName);
            #endregion

            #region Family Tree View

            Task<IEnumerable<FamilyTreeDTO>> GetFamilyTree(string paramType, string paramValue, string paramParent);
        FamilyTreePatDTO GetNodeDetails(string paramType, string paramValue);
        void UpdateParent(int childAppId, int newParentId, string parentInfo, string userName);

        //string GetExpandedNodes(string paramType, string paramValue);
        Task<List<FamilyTreeParentCaseDTO>> GetPossibleFamilyTreeReferences(int appId, string caseNumber);

        #endregion
        #region Action
        Task<List<DelegationEmailDTO>> GetDelegationEmails(int delegationId);
        Task MarkDelegationasEmailed(int delegationId);
        Task<List<LookupIntDTO>> GetDelegatedDdIds(int action, int[] recIds);
        Task<List<DelegationEmailDTO>> GetDeletedDelegationEmails(int delegationId);
        Task<List<LookupIntDTO>> GetDuedateChangedDelegationIds(int action, List<PatDueDate> updated);
        Task<DelegationDetailDTO> GetDeletedDelegation(int delegationId);
        #endregion

        #region Unitary Patent
        Task<int> ShouldShowUnitaryPatentFields(int action, string country, string caseType, int appId);
        Task<List<PatDesignationDTO>> GetDesignatedCountries(int appId);
        #endregion

    }

}
