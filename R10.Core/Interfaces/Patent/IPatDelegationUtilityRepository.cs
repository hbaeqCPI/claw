using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using R10.Core.Identity;

namespace R10.Core.Interfaces.Patent
{
    public interface IPatDelegationUtilityRepository
    {
        IQueryable<DelegationUtilityPreviewDTO> GetPreviewList(DelegationUtilityCriteriaDTO searchCriteria);
        Task<DelegationUtilityResultDTO> RunUpdate(string updateMode, int[] delegationIds, string[] delegateTo, string userName, string? fromUser, int fromGroup, bool reassign);

        IQueryable<CPiUser> CPiUsers { get; }
        IQueryable<PatDueDateDelegation> PatDueDateDelegations { get; }
        IQueryable<CPiGroup> CPiGroups { get; }
        
    }
}
