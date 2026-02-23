using R10.Core.DTOs;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Identity;

namespace R10.Core.Interfaces.GeneralMatter
{
    public interface IGMDelegationUtilityRepository
    {
        IQueryable<DelegationUtilityPreviewDTO> GetPreviewList(DelegationUtilityCriteriaDTO searchCriteria);
        Task<DelegationUtilityResultDTO> RunUpdate(string updateMode, int[] ids, string[] delegateTo, string userName, string? fromUser, int fromGroup, bool reassign);
        IQueryable<CPiUser> CPiUsers { get; }
        IQueryable<GMDueDateDelegation> GMDueDateDelegations { get; }
        IQueryable<CPiGroup> CPiGroups { get; }
        
    }
}
