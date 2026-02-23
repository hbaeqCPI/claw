using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Interfaces;
using R10.Core.Interfaces.GeneralMatter;

namespace R10.Core.Services
{
    public class GMDelegationUtilityService : IGMDelegationUtilityService
    {
        private readonly IGMDelegationUtilityRepository _tmkDelegationUtilityRepository;
        private readonly IDueDateService<GMActionDue, GMDueDate> _dueDateService;
        private readonly IActionDueService<GMActionDue, GMDueDate> _actionDueService;

        public GMDelegationUtilityService(IGMDelegationUtilityRepository tmkDelegationUtilityRepository,
                                           IDueDateService<GMActionDue, GMDueDate> dueDateService,
                                           IActionDueService<GMActionDue, GMDueDate> actionDueService)
        {
            _tmkDelegationUtilityRepository = tmkDelegationUtilityRepository;
            _dueDateService = dueDateService;
            _actionDueService = actionDueService;
        }

        public IQueryable<DelegationUserDTO> GMDelegationUserDTO {
            get {
                return _tmkDelegationUtilityRepository.CPiUsers.Where(u => 
                _tmkDelegationUtilityRepository.GMDueDateDelegations.Any(ddd => ddd.UserId==u.Id && 
                (_dueDateService.QueryableList.Any(dd=> dd.DDId==ddd.DDId) || _actionDueService.QueryableList.Any(ad=> ad.ActId==ddd.ActId))
                )).Select(u => new DelegationUserDTO { UserId = u.Id, DelegatedUser = u.FirstName + " " + u.LastName + "(" + u.Email + ")" });
            }
        }

        public IQueryable<DelegationGroupDTO> GMDelegationGroupDTO {
            get
            {
                return _tmkDelegationUtilityRepository.CPiGroups.Where(g =>
                _tmkDelegationUtilityRepository.GMDueDateDelegations.Any(ddd => ddd.GroupId == g.Id &&
                (_dueDateService.QueryableList.Any(dd => dd.DDId == ddd.DDId) || _actionDueService.QueryableList.Any(ad => ad.ActId == ddd.ActId))
                )).Select(u => new DelegationGroupDTO { GroupId = u.Id, DelegatedGroup = u.Name });
            }
        }

        public IQueryable<DelegationActionTypeDTO> GMDelegationActionTypeDTO
        {
            get
            {
               return  _actionDueService.QueryableList.Where(ad => _tmkDelegationUtilityRepository.GMDueDateDelegations.Any(ddd => ddd.ActId == ad.ActId || _dueDateService.QueryableList.Any(dd => dd.DDId == ddd.DDId && dd.ActId==ad.ActId)))
                .Select(ad => new DelegationActionTypeDTO {ActionType=ad.ActionType}).Distinct();
            }
        }

        public IQueryable<DelegationActionDueDTO> GMDelegationActionDueDTO
        {
            get
            {
                return _dueDateService.QueryableList.Where(dd => _tmkDelegationUtilityRepository.GMDueDateDelegations.Any(ddd => ddd.DDId==dd.DDId))
                 .Select(dd => new DelegationActionDueDTO { ActionDue = dd.ActionDue}).Distinct();
            }
        }

        public IQueryable<DelegationIndicatorDTO> GMDelegationIndicatorDTO
        {
            get
            {
                return _dueDateService.QueryableList.Where(dd => _tmkDelegationUtilityRepository.GMDueDateDelegations.Any(ddd => ddd.DDId == dd.DDId || ddd.ActId == dd.ActId))
                 .Select(dd => new DelegationIndicatorDTO { Indicator = dd.Indicator }).Distinct();
            }
        }

        public IQueryable<DelegationActionTypeDTO> GMDelegationActionTypeDelegateDTO
        {
            get
            {
                return _actionDueService.QueryableList.Where(ad => _dueDateService.QueryableList.Any(dd => dd.ActId == ad.ActId && dd.DateTaken == null))
                 .Select(ad => new DelegationActionTypeDTO { ActionType = ad.ActionType }).Distinct();
            }
        }

        public IQueryable<DelegationActionDueDTO> GMDelegationActionDueDelegateDTO {
            get
            {
                return _dueDateService.QueryableList.Where(dd => dd.DateTaken ==null)
                 .Select(dd => new DelegationActionDueDTO { ActionDue = dd.ActionDue }).Distinct();
            }
        }
        public IQueryable<DelegationIndicatorDTO> GMDelegationIndicatorDelegateDTO
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

        public async Task<DelegationUtilityResultDTO> RunUpdate(string updateMode, int[] delegationIds, string[] delegateTo, string userName, string? fromUser, int fromGroup, bool reassign)
        {
            return await _tmkDelegationUtilityRepository.RunUpdate(updateMode, delegationIds, delegateTo,userName, fromUser, fromGroup,  reassign);
        }

        
    }
}

