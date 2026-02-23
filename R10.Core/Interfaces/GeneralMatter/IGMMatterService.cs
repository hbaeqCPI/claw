using Microsoft.EntityFrameworkCore.ChangeTracking;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IGMMatterService : IEntityService<GMMatter>
    {
        IQueryable<GMMatterCopySetting> GMMatterCopySettings { get; }

        bool IsAttorneyRequired { get; }
        Task Add(GMMatter matter, List<int> attorneyIds);
        Task ValidatePermission(int matId, List<string> roles);
        Task CopyMatter(int oldMatId, int newMatId, string userName, bool copyCaseInfo, 
            bool CopyCountries, bool CopyAttorney, bool CopyOtherParties, bool CopyTrademarks, 
            bool CopyPatents, bool CopyKeywords, bool CopyImages, bool CopyRelatedCases, bool CopyProducts);

        Task RefreshCopySetting(List<GMMatterCopySetting> added, List<GMMatterCopySetting> deleted);
        Task UpdateCopySetting(GMMatterCopySetting setting);
        Task<CPiUserSetting> GetMainCopySettings(string userId);
        Task UpdateMainCopySettings(CPiUserSetting userSetting);
        Task<int> GetMainCopySettingId();
        Task AddCopySettings(List<GMMatterCopySetting> settings);
        Task AddCustomFieldsAsCopyFields();

        Task<bool> HasProducts(int matId);
        IQueryable<T> QueryableChildList<T>() where T : BaseEntity;
        IQueryable<Product> Products { get; }

        Task<List<SysCustomFieldSetting>> GetCustomFields();

        Task UpdateDeDocket(GMMatter matter);
        Task<bool> HasOutstandingDedocket(int matId);

        #region Workflow
        Task GenerateWorkflowFromEmailSent(int matId, int qeSetupId);
        Task GenerateWorkflowFromActionEmailSent(int actId, int qeSetupId);
        Task GenerateWorkflowAction(int matId, int actionTypeId, DateTime? baseDate = null);
        Task <List<GMActionDue>>CloseWorkflowAction(int matId, int actionTypeId);
        Task<List<DelegationEmailDTO>> GetDelegationEmails(int delegationId);
        Task MarkDelegationasEmailed(int delegationId);
        Task<List<LookupIntDTO>> GetDelegatedDdIds(int action, int[] recIds);
        Task<List<DelegationEmailDTO>> GetDeletedDelegationEmails(int delegationId);
        Task<List<LookupIntDTO>> GetDuedateChangedDelegationIds(int action, List<GMDueDate> updated);
        Task<DelegationDetailDTO> GetDeletedDelegation(int delegationId);
        IQueryable<DeDocketInstruction> DeDocketInstructions { get; }
        Task<bool> HasWorkflowEnabled(GMWorkflowTriggerType triggerType);
        Task<List<GMWorkflowAction>> CheckWorkflowAction(GMWorkflowTriggerType triggerType);
        Task<List<GMWorkflowActionParameter>> CheckWorkflowActionParameters(GMWorkflowTriggerType triggerType);
        Task<List<GMDueDate>> GenerateDueDateFromActionParameterWorkflow(GMActionDue? newActionDue, List<GMDueDate> dueDates, GMWorkflowTriggerType triggerType, bool clearBase = true);
        Task<List<GMDueDate>> GetUpdatedDueDateIndicator(int actId, List<GMDueDate> dueDates);


        #endregion

        void DetachAllEntities();
        List<EntityEntry> GetAllTrackedEntities();

        Task<int> GetRequestDocketPendingCount(int matId);
        Task<List<GMDocketRequest>> GetRequestDockets(int matId, bool outstandingOnly);
    }
}
