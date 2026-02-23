using R10.Core.DTOs;

namespace R10.Core.Interfaces.Patent
{
    public interface IPatDelegationUtilityService 
    {
        Task<List<DelegationUtilityPreviewDTO>> GetPreviewList(DelegationUtilityCriteriaDTO searchCriteria);
        Task<DelegationUtilityResultDTO> RunUpdate(string updateMode, int[] delegationIds, string[] delegateTo, string userName, string? fromUser, int fromGroup, bool reassign);

        IQueryable<DelegationUserDTO> PatDelegationUserDTO { get; }
        IQueryable<DelegationGroupDTO> PatDelegationGroupDTO { get; }
        IQueryable<DelegationActionTypeDTO> PatDelegationActionTypeDTO { get; }
        IQueryable<DelegationActionDueDTO> PatDelegationActionDueDTO { get; }
        IQueryable<DelegationIndicatorDTO> PatDelegationIndicatorDTO { get; }

        IQueryable<DelegationActionTypeDTO> PatDelegationActionTypeDelegateDTO { get; }
        IQueryable<DelegationActionDueDTO> PatDelegationActionDueDelegateDTO { get; }
        IQueryable<DelegationIndicatorDTO> PatDelegationIndicatorDelegateDTO { get; }
    }
}
