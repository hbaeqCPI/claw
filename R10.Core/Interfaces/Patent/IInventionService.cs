using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.Patent
{
    public interface IInventionService : IEntityService<Invention>
    {
        IQueryable<InventionCopySetting> InventionCopySettings { get; }

        Task<bool> CanModifyAttorney(int attorneyId);
        bool IsOwnerRequired { get; }
        bool IsInventorRequired { get; }
        Task Add(Invention invention, List<int> requiredEntityIds);
        Task Update(Invention invention, bool hasRelatedCasesMassCopy);
        Task ValidatePermission(int invId, List<string> roles);
        Task CopyInvention(int oldInvId, int newInvId, string userName, bool copyCaseInfo,
            bool CopyOwners, bool CopyInventors, bool CopyPriorities, bool CopyAbstract, 
            bool CopyKeywords, bool CopyImages, bool CopyRelatedInventions, bool CopyProducts, bool copyCosts);
        Task RefreshCopySetting(List<InventionCopySetting> added, List<InventionCopySetting> deleted);
        Task UpdateCopySetting(InventionCopySetting setting);
        Task AddCopySettings(List<InventionCopySetting> settings);
        Task<List<SysCustomFieldSetting>> GetCustomFields();
        Task<CPiUserSetting> GetMainCopySettings(string userId);
        Task UpdateMainCopySettings(CPiUserSetting userSetting);
         Task<int> GetMainCopySettingId();

        Task UpdateDeDocket(Invention invention);

        Task<bool> HasProducts(int invId);
        IQueryable<Product> Products { get; }
        Task RelatedCasesMassCopy(int invId, string? createdBy);
        IQueryable<Invention> Inventions { get; }
        Task AddCustomFieldsAsCopyFields();
        #region Action
        Task<List<DelegationEmailDTO>> GetDelegationEmails(int delegationId);
        Task MarkDelegationasEmailed(int delegationId);
        Task<List<LookupIntDTO>> GetDelegatedDdIds(int action, int[] recIds);
        Task<List<DelegationEmailDTO>> GetDeletedDelegationEmails(int delegationId);
        Task<List<LookupIntDTO>> GetDuedateChangedDelegationIds(int action, List<PatDueDateInv> updated);
        Task<DelegationDetailDTO> GetDeletedDelegation(int delegationId);
        IQueryable<DeDocketInstruction> DeDocketInstructions { get; }
        #endregion

        Task RefreshTradeSecret(int invId);
    }
}
