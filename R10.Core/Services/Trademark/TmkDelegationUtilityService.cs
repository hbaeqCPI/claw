using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Trademark;

namespace R10.Core.Services
{
    public class TmkDelegationUtilityService : ITmkDelegationUtilityService
    {
        
        private readonly ITmkDelegationUtilityRepository _tmkDelegationUtilityRepository;
        private readonly IDueDateService<TmkActionDue, TmkDueDate> _dueDateService;
        private readonly IActionDueService<TmkActionDue, TmkDueDate> _actionDueService;

        public TmkDelegationUtilityService(ITmkDelegationUtilityRepository tmkDelegationUtilityRepository,
                                           IDueDateService<TmkActionDue, TmkDueDate> dueDateService,
                                           IActionDueService<TmkActionDue, TmkDueDate> actionDueService)
        {
            _tmkDelegationUtilityRepository = tmkDelegationUtilityRepository;
            _dueDateService = dueDateService;
            _actionDueService = actionDueService;
        }

        public IQueryable<DelegationUserDTO> TmkDelegationUserDTO {
            get {
                return _tmkDelegationUtilityRepository.CPiUsers.Where(u => 
                _tmkDelegationUtilityRepository.TmkDueDateDelegations.Any(ddd => ddd.UserId==u.Id && 
                (_dueDateService.QueryableList.Any(dd=> dd.DDId==ddd.DDId) || _actionDueService.QueryableList.Any(ad=> ad.ActId==ddd.ActId))
                )).Select(u => new DelegationUserDTO { UserId = u.Id, DelegatedUser = u.FirstName + " " + u.LastName + "(" + u.Email + ")" });
            }
        }

        public IQueryable<DelegationGroupDTO> TmkDelegationGroupDTO {
            get
            {
                return _tmkDelegationUtilityRepository.CPiGroups.Where(g =>
                _tmkDelegationUtilityRepository.TmkDueDateDelegations.Any(ddd => ddd.GroupId == g.Id &&
                (_dueDateService.QueryableList.Any(dd => dd.DDId == ddd.DDId) || _actionDueService.QueryableList.Any(ad => ad.ActId == ddd.ActId))
                )).Select(u => new DelegationGroupDTO { GroupId = u.Id, DelegatedGroup = u.Name });
            }
        }

        public IQueryable<DelegationActionTypeDTO> TmkDelegationActionTypeDTO
        {
            get
            {
               return  _actionDueService.QueryableList.Where(ad => _tmkDelegationUtilityRepository.TmkDueDateDelegations.Any(ddd => ddd.ActId == ad.ActId || _dueDateService.QueryableList.Any(dd => dd.DDId == ddd.DDId && dd.ActId==ad.ActId)))
                .Select(ad => new DelegationActionTypeDTO {ActionType=ad.ActionType}).Distinct();
            }
        }

        public IQueryable<DelegationActionDueDTO> TmkDelegationActionDueDTO
        {
            get
            {
                return _dueDateService.QueryableList.Where(dd => _tmkDelegationUtilityRepository.TmkDueDateDelegations.Any(ddd => ddd.DDId==dd.DDId))
                 .Select(dd => new DelegationActionDueDTO { ActionDue = dd.ActionDue}).Distinct();
            }
        }

        public IQueryable<DelegationIndicatorDTO> TmkDelegationIndicatorDTO
        {
            get
            {
                return _dueDateService.QueryableList.Where(dd => _tmkDelegationUtilityRepository.TmkDueDateDelegations.Any(ddd => ddd.DDId == dd.DDId || ddd.ActId == dd.ActId))
                 .Select(dd => new DelegationIndicatorDTO { Indicator = dd.Indicator }).Distinct();
            }
        }

        public IQueryable<DelegationActionTypeDTO> TmkDelegationActionTypeDelegateDTO
        {
            get
            {
                return _actionDueService.QueryableList.Where(ad => _dueDateService.QueryableList.Any(dd => dd.ActId == ad.ActId && dd.DateTaken == null))
                 .Select(ad => new DelegationActionTypeDTO { ActionType = ad.ActionType }).Distinct();
            }
        }

        public IQueryable<DelegationActionDueDTO> TmkDelegationActionDueDelegateDTO {
            get
            {
                return _dueDateService.QueryableList.Where(dd => dd.DateTaken ==null)
                 .Select(dd => new DelegationActionDueDTO { ActionDue = dd.ActionDue }).Distinct();
            }
        }
        public IQueryable<DelegationIndicatorDTO> TmkDelegationIndicatorDelegateDTO
        {
            get
            {
                return _dueDateService.QueryableList.Where(dd => dd.DateTaken == null)
                 .Select(dd => new DelegationIndicatorDTO { Indicator = dd.Indicator }).Distinct();
            }
        }

        public async Task<List<DelegationUtilityPreviewDTO>> GetPreviewList(DelegationUtilityCriteriaDTO searchCriteria) { 
            var query = _tmkDelegationUtilityRepository.GetPreviewList(searchCriteria);

            //entity or resp office filter
            var result = await query.Where(d => _actionDueService.QueryableList.Any(ad => ad.ActId == d.ActId)).ToListAsync();
            return result;
        }

        public async Task<DelegationUtilityResultDTO> RunUpdate(string updateMode, int[] delegationIds, string[] delegateTo, string userName, string? fromUser, int fromGroup, bool reassign) {
            return await _tmkDelegationUtilityRepository.RunUpdate(updateMode, delegationIds, delegateTo, userName, fromUser, fromGroup, reassign);
        }


    }
}

