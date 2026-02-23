using R10.Core.DTOs;

namespace R10.Core.Interfaces.Trademark
{
    public interface ITmkDelegationUtilityService 
    {
        Task<List<DelegationUtilityPreviewDTO>> GetPreviewList(DelegationUtilityCriteriaDTO searchCriteria);
        Task<DelegationUtilityResultDTO> RunUpdate(string updateMode, int[] delegationIds, string[] delegateTo, string userName, string? fromUser, int fromGroup, bool reassign);

        IQueryable<DelegationUserDTO> TmkDelegationUserDTO { get; }
        IQueryable<DelegationGroupDTO> TmkDelegationGroupDTO { get; }
        IQueryable<DelegationActionTypeDTO> TmkDelegationActionTypeDTO { get; }
        IQueryable<DelegationActionDueDTO> TmkDelegationActionDueDTO { get; }
        IQueryable<DelegationIndicatorDTO> TmkDelegationIndicatorDTO { get; }

        IQueryable<DelegationActionTypeDTO> TmkDelegationActionTypeDelegateDTO { get; }
        IQueryable<DelegationActionDueDTO> TmkDelegationActionDueDelegateDTO { get; }
        IQueryable<DelegationIndicatorDTO> TmkDelegationIndicatorDelegateDTO { get; }
    }
}
