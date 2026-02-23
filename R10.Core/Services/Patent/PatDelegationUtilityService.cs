using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;

namespace R10.Core.Services
{
    public class PatDelegationUtilityService : IPatDelegationUtilityService
    {
        
        private readonly IPatDelegationUtilityRepository _patDelegationUtilityRepository;
        private readonly IDueDateService<PatActionDue, PatDueDate> _dueDateService;
        private readonly IActionDueService<PatActionDue, PatDueDate> _actionDueService;

        public PatDelegationUtilityService(IPatDelegationUtilityRepository patDelegationUtilityRepository,
                                           IDueDateService<PatActionDue, PatDueDate> dueDateService,
                                           IActionDueService<PatActionDue, PatDueDate> actionDueService)
        {
            _patDelegationUtilityRepository = patDelegationUtilityRepository;
            _dueDateService = dueDateService;
            _actionDueService = actionDueService;
        }

        public IQueryable<DelegationUserDTO> PatDelegationUserDTO {
            get {
                return _patDelegationUtilityRepository.CPiUsers.Where(u => 
                _patDelegationUtilityRepository.PatDueDateDelegations.Any(ddd => ddd.UserId==u.Id && 
                (_dueDateService.QueryableList.Any(dd=> dd.DDId==ddd.DDId) || _actionDueService.QueryableList.Any(ad=> ad.ActId==ddd.ActId))
                )).Select(u => new DelegationUserDTO { UserId = u.Id, DelegatedUser = u.FirstName + " " + u.LastName + "(" + u.Email + ")" });
            }
        }

        public IQueryable<DelegationGroupDTO> PatDelegationGroupDTO {
            get
            {
                return _patDelegationUtilityRepository.CPiGroups.Where(g =>
                _patDelegationUtilityRepository.PatDueDateDelegations.Any(ddd => ddd.GroupId == g.Id &&
                (_dueDateService.QueryableList.Any(dd => dd.DDId == ddd.DDId) || _actionDueService.QueryableList.Any(ad => ad.ActId == ddd.ActId))
                )).Select(u => new DelegationGroupDTO { GroupId = u.Id, DelegatedGroup = u.Name });
            }
        }

        public IQueryable<DelegationActionTypeDTO> PatDelegationActionTypeDTO
        {
            get
            {
               return  _actionDueService.QueryableList.Where(ad => _patDelegationUtilityRepository.PatDueDateDelegations.Any(ddd => ddd.ActId == ad.ActId || _dueDateService.QueryableList.Any(dd => dd.DDId == ddd.DDId && dd.ActId==ad.ActId)))
                .Select(ad => new DelegationActionTypeDTO {ActionType=ad.ActionType}).Distinct();
            }
        }

        public IQueryable<DelegationActionDueDTO> PatDelegationActionDueDTO
        {
            get
            {
                return _dueDateService.QueryableList.Where(dd => _patDelegationUtilityRepository.PatDueDateDelegations.Any(ddd => ddd.DDId==dd.DDId))
                 .Select(dd => new DelegationActionDueDTO { ActionDue = dd.ActionDue}).Distinct();
            }
        }

        public IQueryable<DelegationIndicatorDTO> PatDelegationIndicatorDTO
        {
            get
            {
                return _dueDateService.QueryableList.Where(dd => _patDelegationUtilityRepository.PatDueDateDelegations.Any(ddd => ddd.DDId == dd.DDId || ddd.ActId == dd.ActId))
                 .Select(dd => new DelegationIndicatorDTO { Indicator = dd.Indicator }).Distinct();
            }
        }

        public IQueryable<DelegationActionTypeDTO> PatDelegationActionTypeDelegateDTO
        {
            get
            {
                return _actionDueService.QueryableList.Where(ad => _dueDateService.QueryableList.Any(dd => dd.ActId == ad.ActId && dd.DateTaken == null))
                 .Select(ad => new DelegationActionTypeDTO { ActionType = ad.ActionType }).Distinct();
            }
        }

        public IQueryable<DelegationActionDueDTO> PatDelegationActionDueDelegateDTO {
            get
            {
                return _dueDateService.QueryableList.Where(dd => dd.DateTaken ==null)
                 .Select(dd => new DelegationActionDueDTO { ActionDue = dd.ActionDue }).Distinct();
            }
        }
        public IQueryable<DelegationIndicatorDTO> PatDelegationIndicatorDelegateDTO
        {
            get
            {
                return _dueDateService.QueryableList.Where(dd => dd.DateTaken == null)
                 .Select(dd => new DelegationIndicatorDTO { Indicator = dd.Indicator }).Distinct();
            }
        }

        public async Task<List<DelegationUtilityPreviewDTO>> GetPreviewList(DelegationUtilityCriteriaDTO searchCriteria) { 
            var query = _patDelegationUtilityRepository.GetPreviewList(searchCriteria);

            //entity or resp office filter
            var result = await query.Where(d => _actionDueService.QueryableList.Any(ad => ad.ActId == d.ActId)).ToListAsync();
            return result;
        }

        public async Task<DelegationUtilityResultDTO> RunUpdate(string updateMode, int[] delegationIds, string[] delegateTo, string userName, string? fromUser, int fromGroup, bool reassign)
        {
            return await _patDelegationUtilityRepository.RunUpdate(updateMode, delegationIds, delegateTo,userName, fromUser, fromGroup, reassign);
        }

        
    }
}

