using R10.Core.DTOs;

namespace R10.Core.Interfaces.GeneralMatter
{
    public interface IGMDelegationUtilityService 
    {
        Task<List<DelegationUtilityPreviewDTO>> GetPreviewList(DelegationUtilityCriteriaDTO searchCriteria);
        Task<DelegationUtilityResultDTO> RunUpdate(string updateMode, int[] delegationIds, string[] delegateTo, string userName, string? fromUser, int fromGroup, bool reassign);
     
        IQueryable<DelegationUserDTO> GMDelegationUserDTO { get; }
        IQueryable<DelegationGroupDTO> GMDelegationGroupDTO { get; }
        IQueryable<DelegationActionTypeDTO> GMDelegationActionTypeDTO { get; }
        IQueryable<DelegationActionDueDTO> GMDelegationActionDueDTO { get; }
        IQueryable<DelegationIndicatorDTO> GMDelegationIndicatorDTO { get; }

        IQueryable<DelegationActionTypeDTO> GMDelegationActionTypeDelegateDTO { get; }
        IQueryable<DelegationActionDueDTO> GMDelegationActionDueDelegateDTO { get; }
        IQueryable<DelegationIndicatorDTO> GMDelegationIndicatorDelegateDTO { get; }
    }
}
