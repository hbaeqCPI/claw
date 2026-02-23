using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.DMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class DMSDueDateService : ChildEntityService<DMSActionDue, DMSDueDate>, IDueDateService<DMSActionDue, DMSDueDate>
    {
        private readonly IActionDueService<DMSActionDue, DMSDueDate> _actionDueService;
        private readonly IDisclosureService _disclosureService;
        private readonly IDueDateExtensionService _dueDateExtensionService;

        public DMSDueDateService(
            ICPiDbContext cpiDbContext,
            IActionDueService<DMSActionDue, DMSDueDate> actionDueService,
            IDisclosureService disclosureService,
            ClaimsPrincipal user,
            IDueDateExtensionService dueDateExtensionService) : base(cpiDbContext, user)
        {
            _actionDueService = actionDueService;
            _disclosureService = disclosureService;
            _dueDateExtensionService = dueDateExtensionService;
        }

        public IQueryable<DMSActionDue> ActionsDue => _actionDueService.QueryableList;

        public override IQueryable<DMSDueDate> QueryableList
        {
            get
            {
                var dueDates = _cpiDbContext.GetRepository<DMSDueDate>().QueryableList;

                if (_user.HasRespOfficeFilter(SystemType.DMS) || _user.HasEntityFilter() || !_user.CanAccessDMSTradeSecret())
                    dueDates = dueDates.Where(d => ActionsDue.Any(a => a.ActId == d.ActId));

                return dueDates;
            }
        }


        public override async Task<bool> Update(object key, string userName, IEnumerable<DMSDueDate> updated, IEnumerable<DMSDueDate> added, IEnumerable<DMSDueDate> deleted)
        {
            var actId = (int)key;
            var actionDue = await ValidatePermission(new DMSDueDate()
            {
                ActId = actId,
                UpdatedBy = userName,
                LastUpdate = DateTime.Now
            });

            foreach (var dueDate in updated)
            {
                dueDate.UpdatedBy = actionDue.UpdatedBy;
                dueDate.LastUpdate = actionDue.LastUpdate;
            }

            foreach (var dueDate in added)
            {
                dueDate.ActId = actId;
                dueDate.CreatedBy = actionDue.UpdatedBy;
                dueDate.DateCreated = actionDue.LastUpdate;
                dueDate.UpdatedBy = actionDue.UpdatedBy;
                dueDate.LastUpdate = actionDue.LastUpdate;
            }

            //todo: recurring actions?

            var repository = _cpiDbContext.GetRepository<DMSDueDate>();
            repository.Delete(deleted);
            repository.Update(updated);
            repository.Update(added);

            await _cpiDbContext.SaveChangesAsync();

            if (deleted.Any())
                await DeleteEmptyAction(actionDue);

            return true;
        }

        public async Task<bool> Update(int parentId, string userName, IEnumerable<DMSDueDate> updated, IEnumerable<DMSDueDate> deleted)
        {
            var disclosure = await ValidateDisclosure(parentId);
            _cpiDbContext.GetRepository<Disclosure>().Attach(disclosure);
            disclosure.UpdatedBy = userName;
            disclosure.LastUpdate = DateTime.Now;

            List<int> updatedActIds = new List<int>();
            if (updated.Any())
                updatedActIds.AddRange(updated.Select(d => d.ActId).Distinct().ToList());

            if (deleted.Any())
                updatedActIds.AddRange(deleted.Select(d => d.ActId).Distinct().ToList());

            var actionsDue = await ActionsDue.Where(a => updatedActIds.Any(actId => actId == a.ActId)).ToListAsync();
            _cpiDbContext.GetRepository<DMSActionDue>().Attach(actionsDue);
            foreach (var actionDue in actionsDue)
            {
                actionDue.UpdatedBy = disclosure.UpdatedBy;
                actionDue.LastUpdate = disclosure.LastUpdate;
            }

            foreach (var dueDate in updated)
            {
                dueDate.UpdatedBy = disclosure.UpdatedBy;
                dueDate.LastUpdate = disclosure.LastUpdate;
            }

            //todo: recurring actions?

            _cpiDbContext.GetRepository<DMSDueDate>().Delete(deleted);
            _cpiDbContext.GetRepository<DMSDueDate>().Update(updated);
            await _cpiDbContext.SaveChangesAsync();

            if (deleted.Any())
                foreach (var actId in deleted.Select(d => d.ActId).Distinct())
                {
                    await DeleteEmptyAction(actionsDue.FirstOrDefault(a => a.ActId == actId));
                }

            return true;
        }

        public async Task<bool> UpdateDeDocket(string userName, IEnumerable<DMSDueDate> updated)
        {
            throw  new NotImplementedException();
        }

        public async Task<bool> UpdateExtensionSetting(DueDateExtension setting)
        {
            if (setting.IsEnabled)
                setting.NextRunDate = _dueDateExtensionService.ComputeRunDate(setting);
            else
                setting.NextRunDate = null;

            if (setting.ExtensionId > 0)
            {
                await _cpiDbContext.GetRepository<DMSDueDateExtension>().UpdateAsync((DMSDueDateExtension)setting);
            }
            else
            {
                await _cpiDbContext.GetRepository<DMSDueDateExtension>().AddAsync((DMSDueDateExtension)setting);
            }

            await _cpiDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<DueDateExtension> GetExtensionSetting(int ddId, int parentId)
        {
            var setting =  await _cpiDbContext.GetRepository<DMSDueDateExtension>().QueryableList.FirstOrDefaultAsync(e => e.DDId == ddId);
            if (setting == null)
            {
                var parent = await _cpiDbContext.GetRepository<Disclosure>().QueryableList.FirstOrDefaultAsync(t => t.DMSId == parentId);
                if (parent.ClientID > 0)
                {
                    var client = await _cpiDbContext.GetRepository<Client>().QueryableList.FirstOrDefaultAsync(e => e.ClientID == parentId);
                    if (client != null)
                    {
                        setting = new DMSDueDateExtension
                        {
                            DDId = ddId,
                            ExtendDay = client.DueDateExtendDay,
                            ExtendWeek = client.DueDateExtendWeek,
                            ExtendMonth = client.DueDateExtendMonth,
                            RepeatInterval = client.DueDateExtendRepeatInterval,
                            RepeatRecurrence = client.DueDateExtendRepeatRecurrence ?? "D",
                            RepeatOnDay = client.DueDateExtendRepeatOnDay,
                            StopIndicator = client.DueDateExtendStopIndicator,
                            StopAfterCount = client.DueDateExtendStopAfterCount,
                            StopDate = client.DueDateExtendStopDate
                        };
                    }
                }
            }
            return setting;
        }

        public IQueryable<DueDateExtension> DueDateExtensions => _cpiDbContext.GetRepository<DMSDueDateExtension>().QueryableList;
        public IQueryable<DueDateDeDocket> DueDateDeDockets => throw new NotImplementedException();
        public IQueryable<DueDateDeDocketResp> DueDateDeDocketResps => throw new NotImplementedException();

        private async Task<DMSActionDue> ValidatePermission(DMSDueDate dueDate)
        {
            var actionDue = await ValidateActionDue(dueDate.ActId);
            var disclosure = await ValidateDisclosure(actionDue.DMSId);

            _cpiDbContext.GetRepository<DMSActionDue>().Attach(actionDue);
            _cpiDbContext.GetRepository<Disclosure>().Attach(disclosure);

            actionDue.UpdatedBy = dueDate.UpdatedBy;
            actionDue.LastUpdate = dueDate.LastUpdate;

            disclosure.UpdatedBy = actionDue.UpdatedBy;
            disclosure.LastUpdate = actionDue.LastUpdate;

            return actionDue;
        }

        private async Task<DMSActionDue> ValidateActionDue(int actId)
        {
            var actionDue = await ActionsDue.SingleOrDefaultAsync(a => a.ActId == actId);
            Guard.Against.NoRecordPermission(actionDue != null);

            return actionDue;
        }

        private async Task<Disclosure> ValidateDisclosure(int dmsId)
        {
            var disclosure = await _disclosureService.QueryableList.SingleOrDefaultAsync(d => d.DMSId == dmsId);
            Guard.Against.NoRecordPermission(disclosure != null);

            return disclosure;
        }

        private async Task DeleteEmptyAction(DMSActionDue actionDue)
        {
            if (!await QueryableList.AnyAsync(d => d.ActId == actionDue.ActId))
            {
                _cpiDbContext.GetRepository<DMSActionDue>().Delete(actionDue);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task<DueDateDeDocket> UpdateDeDocketFileInfo(int ddId, int deDocketId, string? docFile, int fileId, string userName,string driveItemId)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateDeDocketResp(List<string> responsibleList, string userName, int deDocketId)
        {
            throw new NotImplementedException();
        }

        public async Task MarkDeDocketInstructionsAsCompleted(List<int> deDocketIds, DateTime? completedDate)
        {
            throw new NotImplementedException();          
        }
    }
}
