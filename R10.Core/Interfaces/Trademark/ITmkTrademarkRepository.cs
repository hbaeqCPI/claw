using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;

using R10.Core.Identity;
namespace R10.Core.Interfaces
{
    public interface ITmkTrademarkRepository : IAsyncRepository<TmkTrademark>, IEntityFilterRepository
    {
        Task<TmkTrademark> AddAsync(TmkTrademark trademark, TmkTrademarkModifiedFields modifiedFields, DateTime dateCreated);
        Task<int> UpdateAsync(TmkTrademark trademark, TmkTrademarkModifiedFields modifiedFields, DateTime? dateCreated,bool isMultipleOwnersOn);
        Task UpdateChild<T>(TmkTrademark trademark, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity;
        Task SyncChildToDesignatedTrademarks(TmkTrademark trademark, TmkTrademarkModifiedFields modifiedFields, Type childType);

        Task<Tuple<string, string, string,string>> CopyTrademark(int oldTmkId, string newCaseNumber, string newSubCase, List<int> countryIds,
                           bool copyCaseInfo, bool copyRemarks, bool copyAssignments, bool copyGoods, bool copyImages,
                           bool copyKeywords, bool copyDesCountries, bool copyLicenses, bool copyRelatedCases, string createdBy, string relationship, bool copyProducts, bool copyOwners);
        Task AddCustomFieldsAsCopyFields();

        // family/designation
        Task<bool> CanHaveDesignatedCountry(string country, string caseType);

        // renewal date generation
        TmkTrademarkRenewalFields GetTrademarkRenewal(TmkTrademarkRenewalParameters param);
        bool AnyActionFieldsModified(TmkTrademarkModifiedFields modifiedFields);


        // action
        Task<List<ActionTabDTO>> GetActions(int tmkId, ActionDisplayOption actionDisplayOption);
        Task ActionsUpdate(int tmkId, string userName, IEnumerable<TmkDueDate> updatedActions, IEnumerable<TmkDueDate> deletedActions);
        Task ActionDelete(TmkDueDate deletedAction);
        Task CheckChildlessActionDue(IEnumerable<int> affectedIds);
        Task<List<DelegationEmailDTO>> GetDelegationEmails(int delegationId);
        Task MarkDelegationasEmailed(int delegationId);
        Task<List<LookupIntDTO>> GetDelegatedDdIds(int action, int[] recIds);
        Task<List<DelegationEmailDTO>> GetDeletedDelegationEmails(int delegationId);
        Task<List<LookupIntDTO>> GetDuedateChangedDelegationIds(int action, List<TmkDueDate> updated);
        Task<DelegationDetailDTO> GetDeletedDelegation(int delegationId);

        // cost
        Task<List<TmkCostTrack>> GetCosts(int tmkId);
        Task CostsUpdate(int tmkId, string userName, IEnumerable<TmkCostTrack> updatedCostTracks, IEnumerable<TmkCostTrack> deletedCostTracks);
        Task CostDelete(TmkCostTrack deletedCostTrack);

        // licensee
        Task<List<TmkLicensee>> GetLicensees(int tmkId);
        Task LicenseesUpdate(int tmkId, string userName, IEnumerable<TmkLicensee> updatedLicensees, IEnumerable<TmkLicensee> newLicensees, IEnumerable<TmkLicensee> deletedLicensees);
        Task LicenseeDelete(TmkLicensee deletedLicensee);
        IQueryable<TmkLicensee> TmkLicensees { get; }

        //family tree view
        Task<IEnumerable<FamilyTreeDTO>> GetFamilyTree(string paramType, string paramValue, string paramParent);
        Task<FamilyTreeTmkDTO> GetNodeDetails(string paramType, string paramValue);
        void UpdateParent(int childTmkId, int newParentId, string parentInfo, string userName);

        //product
        IQueryable<TmkProduct> TmkProducts { get; }

        //filters
        Task<bool> RespOfficeAllowed(string userIdentifier, string respOffice, string systemType);
        Task<bool> EntityFilterAllowed(string userIdentifier, int? entityId);

    }
}
