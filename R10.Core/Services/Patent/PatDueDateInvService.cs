using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class PatDueDateInvService : ChildEntityService<PatActionDueInv, PatDueDateInv>, IDueDateService<PatActionDueInv, PatDueDateInv>
    {
        private readonly IActionDueService<PatActionDueInv, PatDueDateInv> _actionDueService;
        private readonly IInventionService _inventionService;
        private readonly IClientService _clientService;
        private readonly IDocumentService _docService;
        private readonly IDueDateExtensionService _dueDateExtensionService;
        private readonly ICPiSystemSettingManager _systemSettingManager;
        private readonly ISystemSettings<PatSetting> _settings;

        public PatDueDateInvService(
            ICPiDbContext cpiDbContext,
            IActionDueService<PatActionDueInv, PatDueDateInv> actionDueService,
            IInventionService inventionService,
            IClientService clientService,
            ClaimsPrincipal user,
            IDocumentService docService,
            IDueDateExtensionService dueDateExtensionService,
            ICPiSystemSettingManager systemSettingManager,
            ISystemSettings<PatSetting> settings) : base(cpiDbContext, user)
        {
            _actionDueService = actionDueService;
            _inventionService = inventionService;
            _clientService = clientService;
            _docService = docService;
            _dueDateExtensionService = dueDateExtensionService;
            _systemSettingManager = systemSettingManager;
            _settings = settings;
        }

        public IQueryable<PatActionDueInv> ActionsDue => _actionDueService.QueryableList;

        public override IQueryable<PatDueDateInv> QueryableList
        {
            get
            {
                var dueDates = _cpiDbContext.GetRepository<PatDueDateInv>().QueryableList;

                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Patent) || _user.RestrictExportControl())
                    dueDates = dueDates.Where(d => ActionsDue.Any(a => a.ActId == d.ActId));

                return dueDates;
            }
        }

        /// <summary>
        /// Update from action due detail screen.
        /// </summary>
        /// <param name="key">ActId of parent action due record.</param>
        /// <param name="userName">User id stamp.</param>
        /// <param name="updated">Updated due dates.</param>
        /// <param name="added">New due dates.</param>
        /// <param name="deleted">Deleted due dates.</param>
        /// <returns></returns>
        public override async Task<bool> Update(object key, string userName, IEnumerable<PatDueDateInv> updated, IEnumerable<PatDueDateInv> added, IEnumerable<PatDueDateInv> deleted)
        {
            var actId = (int)key;
            var actionDue = await ValidateActionDue(actId);
            var countryApp = await ValidateInvention(actionDue.InvId);
            var settings = await _settings.GetSetting();

            if (updated.Any() || added.Any())
            {
                var permissions = CPiPermissions.FullModify;
                if (updated.Any())
                    permissions.AddRange(CPiPermissions.RemarksOnly);

                if (settings.IsSoftDocketOn && updated.Any(d => d.Indicator?.ToLower() == "soft docket"))
                    ValidateSoftDocket(actionDue.ResponsibleID);
                else
                    await ValidatePermission(permissions, countryApp.RespOffice);
            }

            if (deleted.Any())
            {
                if (settings.IsSoftDocketOn && deleted.Any(d => d.Indicator?.ToLower() == "soft docket"))
                    ValidateSoftDocket(actionDue.ResponsibleID);
                else
                    await ValidatePermission(CPiPermissions.CanDelete, countryApp.RespOffice);
            }

            _cpiDbContext.GetRepository<PatActionDueInv>().Attach(actionDue);
            actionDue.UpdatedBy = userName;
            actionDue.LastUpdate = DateTime.Now;

            _cpiDbContext.GetRepository<Invention>().Attach(countryApp);
            countryApp.UpdatedBy = actionDue.UpdatedBy;
            countryApp.LastUpdate = actionDue.LastUpdate;

            var recurringDueDates = new List<PatDueDateInv>();
            foreach (var dueDate in updated)
            {
                dueDate.UpdatedBy = actionDue.UpdatedBy;
                dueDate.LastUpdate = actionDue.LastUpdate;

                var recurringDueDate = await GetRecurringDueDate(actionDue, dueDate);
                if (recurringDueDate != null)
                    recurringDueDates.Add(recurringDueDate);

                var deDocket = dueDate.DeDocketOutstanding;
                if (deDocket != null)
                    ApplyDeDocketLogic(userName, dueDate, deDocket);
            }

            foreach (var dueDate in added)
            {
                dueDate.ActId = actId;
                dueDate.CreatedBy = actionDue.UpdatedBy;
                dueDate.DateCreated = actionDue.LastUpdate;
                dueDate.UpdatedBy = actionDue.UpdatedBy;
                dueDate.LastUpdate = actionDue.LastUpdate;

                var deDocket = dueDate.DeDocketOutstanding;
                if (deDocket != null)
                    ApplyDeDocketLogic(userName, dueDate, deDocket);
            }

            _cpiDbContext.GetRepository<PatDueDateInv>().Delete(deleted);
            _cpiDbContext.GetRepository<PatDueDateInv>().Update(updated);
            _cpiDbContext.GetRepository<PatDueDateInv>().Update(added);
            _cpiDbContext.GetRepository<PatDueDateInv>().Update(recurringDueDates);

            bool concurrencyFailure;
            do
            {
                concurrencyFailure = false;
                try
                {
                    await _cpiDbContext.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    concurrencyFailure = true;

                    //client wins (overwrite current db values)
                    foreach (var entry in ex.Entries)
                    {
                        var currentdbValues = await entry.GetDatabaseValuesAsync();
                        if (currentdbValues != null)
                        {
                            entry.OriginalValues.SetValues(currentdbValues);
                        }
                        //record is no longer available
                        else
                        {
                            throw;
                        }
                    }
                }
            } while (concurrencyFailure);

            if (deleted.Any())
                await DeleteEmptyAction(actionDue);

            //detach to allow series of updates
            _cpiDbContext.Detach(actionDue);
            _cpiDbContext.Detach(countryApp);

            return true;
        }

        /// <summary>
        /// Update from country application detail screen.
        /// </summary>
        /// <param name="parentId">AppId of parent country application record.</param>
        /// <param name="userName">User id stamp.</param>
        /// <param name="updated">Updated due dates.</param>
        /// <param name="deleted">Deleted due dates.</param>
        /// <returns></returns>
        public async Task<bool> Update(int parentId, string userName, IEnumerable<PatDueDateInv> updated, IEnumerable<PatDueDateInv> deleted)
        {
            var countryApp = await ValidateInvention(parentId);

            if (updated.Any())
            {
                var permissions = CPiPermissions.FullModify;
                permissions.AddRange(CPiPermissions.RemarksOnly);
                await ValidatePermission(permissions, countryApp.RespOffice);
            }

            if (deleted.Any())
                await ValidatePermission(CPiPermissions.CanDelete, countryApp.RespOffice);

            _cpiDbContext.GetRepository<Invention>().Attach(countryApp);
            countryApp.UpdatedBy = userName;
            countryApp.LastUpdate = DateTime.Now;

            List<int> updatedActIds = new List<int>();
            if (updated.Any())
                updatedActIds.AddRange(updated.Select(d => d.ActId).Distinct().ToList());

            if (deleted.Any())
                updatedActIds.AddRange(deleted.Select(d => d.ActId).Distinct().ToList());

            var actionsDue = await ActionsDue.Where(a => updatedActIds.Any(actId => actId == a.ActId)).ToListAsync();
            _cpiDbContext.GetRepository<PatActionDueInv>().Attach(actionsDue);
            foreach (var actionDue in actionsDue)
            {
                actionDue.UpdatedBy = countryApp.UpdatedBy;
                actionDue.LastUpdate = countryApp.LastUpdate;
            }

            var recurringDueDates = new List<PatDueDateInv>();
            foreach (var dueDate in updated)
            {
                dueDate.UpdatedBy = countryApp.UpdatedBy;
                dueDate.LastUpdate = countryApp.LastUpdate;

                var actionDue = actionsDue.FirstOrDefault(a => a.ActId == dueDate.ActId);
                var recurringDueDate = await GetRecurringDueDate(actionDue, dueDate);
                if (recurringDueDate != null)
                    recurringDueDates.Add(recurringDueDate);
            }

            _cpiDbContext.GetRepository<PatDueDateInv>().Delete(deleted);
            _cpiDbContext.GetRepository<PatDueDateInv>().Update(updated);
            _cpiDbContext.GetRepository<PatDueDateInv>().Update(recurringDueDates);
            await _cpiDbContext.SaveChangesAsync();

            if (deleted.Any())
                foreach (var actId in deleted.Select(d => d.ActId).Distinct())
                {
                    await DeleteEmptyAction(actionsDue.FirstOrDefault(a => a.ActId == actId));
                }

            return true;
        }

        /// <summary>
        /// Update from action due detail screen.
        /// </summary>
        /// <param name="userName">User id stamp.</param>
        /// <param name="updated">Updated due dates.</param>
        /// <returns></returns>
        public async Task<bool> UpdateDeDocket(string userName, IEnumerable<PatDueDateInv> updated)
        {

            var addedDeDockets = new List<PatDueDateInvDeDocketOutstanding>();
            var updatedDeDockets = new List<PatDueDateInvDeDocketOutstanding>();
            foreach (var dueDate in updated)
            {
                var deDocket = dueDate.DeDocketOutstanding;
                if (deDocket != null)
                {
                    ApplyDeDocketLogic(userName, dueDate, deDocket);
                    if (deDocket != null && deDocket.DeDocketId > 0)
                        updatedDeDockets.Add(deDocket);
                    else if (deDocket != null)
                        addedDeDockets.Add(deDocket);
                }

                //due date remarks
                var deDocketFields = await _systemSettingManager.GetSystemSetting<DeDocketFields>();
                if (deDocketFields?.PatentDueDate != null && deDocketFields.PatentDueDate.Remarks)
                {
                    var patDueDateInv = await QueryableList.Where(d => d.DDId == dueDate.DDId).FirstOrDefaultAsync();
                    if (patDueDateInv != null && patDueDateInv.Remarks != dueDate.Remarks)
                    {
                        _cpiDbContext.GetRepository<PatDueDateInv>().Update(patDueDateInv);
                        patDueDateInv.Remarks = dueDate.Remarks;
                        patDueDateInv.UpdatedBy = userName;
                        patDueDateInv.LastUpdate = DateTime.Now;
                    }
                }
            }
            _cpiDbContext.GetRepository<PatDueDateInvDeDocketOutstanding>().Update(updatedDeDockets);
            _cpiDbContext.GetRepository<PatDueDateInvDeDocketOutstanding>().Update(addedDeDockets);
            await _cpiDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateExtensionSetting(DueDateExtension setting)
        {
            if (setting.IsEnabled)
                setting.NextRunDate = _dueDateExtensionService.ComputeRunDate(setting);
            else
                setting.NextRunDate = null;

            if (setting.ExtensionId > 0)
            {
                await _cpiDbContext.GetRepository<PatDueDateInvExtension>().UpdateAsync((PatDueDateInvExtension)setting);
            }
            else
            {
                await _cpiDbContext.GetRepository<PatDueDateInvExtension>().AddAsync((PatDueDateInvExtension)setting);
            }
            await _cpiDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<DueDateExtension> GetExtensionSetting(int ddId, int parentId)
        {
            var setting = await _cpiDbContext.GetRepository<PatDueDateInvExtension>().QueryableList.FirstOrDefaultAsync(e => e.DDId == ddId);
            if (setting == null)
            {
                var parent = await _inventionService.Inventions.FirstOrDefaultAsync(c => c.InvId == parentId);
                if (parent.ClientID > 0)
                {
                    var client = await _clientService.ClearanceQueryableList.Where(c => c.ClientID == parent.ClientID).FirstOrDefaultAsync();
                    if (client != null)
                    {
                        setting = new PatDueDateInvExtension
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

        public IQueryable<DueDateExtension> DueDateExtensions => _cpiDbContext.GetRepository<PatDueDateInvExtension>().QueryableList;
        public IQueryable<DueDateDeDocket> DueDateDeDockets => _cpiDbContext.GetRepository<PatDueDateInvDeDocket>().QueryableList;
        public IQueryable<DueDateDeDocketResp> DueDateDeDocketResps => throw new NotImplementedException();

        private async Task ValidatePermission(List<string> roles, string respOffice)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Patent, roles, respOffice));
        }

        private async Task<PatActionDueInv> ValidateActionDue(int actId)
        {
            var actionDue = await ActionsDue.SingleOrDefaultAsync(a => a.ActId == actId);
            Guard.Against.NoRecordPermission(actionDue != null);

            return actionDue;
        }

        private async Task<Invention> ValidateInvention(int invId)
        {
            var countryApp = await _inventionService.Inventions.SingleOrDefaultAsync(ca => ca.InvId == invId);
            Guard.Against.NoRecordPermission(countryApp != null);

            return countryApp;
        }

        private void ValidateSoftDocket(int? responsibleId)
        {
            if (_user.IsSoftDocketUser())
                Guard.Against.NoRecordPermission(responsibleId == _user.GetEntityId());
            else
                Guard.Against.NoRecordPermission(_user.IsInRoles(SystemType.Patent, CPiPermissions.SoftDocket));
        }

        /// <summary>
        /// Delete action without due dates.
        /// </summary>
        /// <param name="actionDue">The action due record to delete if it has no due dates.</param>
        /// <returns></returns>
        private async Task DeleteEmptyAction(PatActionDueInv actionDue)
        {
            if (!await QueryableList.AnyAsync(d => d.ActId == actionDue.ActId))
            {
                //Update ActId to ActionTypeID in tblDocVerification before removing tblPatActionDue record
                var actionTypeId = await _cpiDbContext.GetRepository<PatActionType>().QueryableList
                                        .Where(d => EF.Functions.Like(d.ActionType, actionDue.ActionType))
                                        .Select(d => d.ActionTypeID).FirstOrDefaultAsync();
                if (actionTypeId > 0)
                    await _docService.UpdateVerificationActionTypeId("P", actionDue.InvId, actionDue.ActId, actionTypeId);

                _cpiDbContext.GetRepository<PatActionDueInv>().Delete(actionDue);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Get recurring country law due date if date taken is updated.
        /// </summary>
        /// <param name="actionDue">The parent action due record that will link back to PatCountryDue.</param>
        /// <param name="dueDate">The due date record that the recurring due date will be based on.</param>
        /// <returns></returns>
        private async Task<PatDueDateInv> GetRecurringDueDate(PatActionDueInv actionDue, PatDueDateInv dueDate)
        {
            if (actionDue.ComputerGenerated && dueDate.DateTaken != null)
            {
                var oldDateTaken = await QueryableList.Where(d => d.DDId == dueDate.DDId).Select(d => d.DateTaken).FirstOrDefaultAsync();
                if (dueDate.DateTaken != oldDateTaken)
                {
                    return await _actionDueService.GetRecurringDueDate(actionDue, dueDate);
                }
            }

            return null;
        }

        /// <summary>
        /// Apply dedocket logic to due date record
        /// </summary>
        /// <param name="userName">User id stamp.</param>
        /// <param name="dueDate">The source duedate record</param>
        /// <param name="deDocket">The source dedocket record</param>
        private async Task ApplyDeDocketLogic(string userName, PatDueDateInv dueDate, PatDueDateInvDeDocketOutstanding deDocket)
        {
            var lastUpdate = DateTime.Now;
            if (deDocket.DeDocketId > 0)
            {
                if (deDocket.OldInstruction != deDocket.Instruction)
                {
                    deDocket.InstructedBy = userName;
                    deDocket.InstructionDate = lastUpdate;
                }

                if (deDocket.InstructionCompleted && !deDocket.OldInstructionCompleted)
                {
                    deDocket.CompletedBy = userName;
                    deDocket.CompletedDate = lastUpdate;

                    var instruction = await _cpiDbContext.GetRepository<DeDocketInstruction>().QueryableList.FirstOrDefaultAsync(i => i.Instruction == deDocket.Instruction);
                    if (instruction != null)
                    {
                        if (instruction.CloseDeadlineWith.ToUpper() == "C")
                            dueDate.DateTaken = DateTime.Now;
                        else if (instruction.CloseDeadlineWith.ToUpper() == "I")
                            dueDate.DateTaken = deDocket.InstructionDate.Value.Date;
                    }
                }

                deDocket.UpdatedBy = userName;
                deDocket.LastUpdate = lastUpdate;

            }
            else if (!(string.IsNullOrEmpty(deDocket.Instruction) && string.IsNullOrEmpty(deDocket.DeDocketRemarks)))
            {
                deDocket.InstructedBy = userName;
                deDocket.InstructionDate = lastUpdate;
                deDocket.CreatedBy = userName;
                deDocket.DateCreated = lastUpdate;
                deDocket.UpdatedBy = userName;
                deDocket.LastUpdate = lastUpdate;

                if (deDocket.InstructionCompleted)
                {
                    deDocket.CompletedBy = userName;
                    deDocket.CompletedDate = lastUpdate;

                    var instruction = await _cpiDbContext.GetRepository<DeDocketInstruction>().QueryableList.FirstOrDefaultAsync(i => i.Instruction == deDocket.Instruction);
                    if (instruction != null)
                    {
                        if (instruction.CloseDeadlineWith.ToUpper() == "C")
                            dueDate.DateTaken = DateTime.Now;
                        else if (instruction.CloseDeadlineWith.ToUpper() == "I")
                            dueDate.DateTaken = deDocket.InstructionDate;
                    }
                }
            }
            else
            {
                dueDate.DeDocketOutstanding = null;
            }
        }

        public async Task<DueDateDeDocket> UpdateDeDocketFileInfo(int ddId, int deDocketId, string? docFile, int fileId, string userName, string? driveItemId)
        {
            var deDocket = new PatDueDateInvDeDocket();
            if (deDocketId > 0)
                deDocket = await _cpiDbContext.GetRepository<PatDueDateInvDeDocket>().QueryableList.FirstOrDefaultAsync(ddk => ddk.DeDocketId == deDocketId);
            else
            {
                deDocket.DDId = ddId;
                deDocket.CreatedBy = userName;
                deDocket.DateCreated = DateTime.Now;
                deDocket.UpdatedBy = userName;
                deDocket.LastUpdate = DateTime.Now;
            }

            if (deDocket != null)
            {
                deDocket.FileId = fileId;
                deDocket.DocFile = docFile;
                deDocket.DriveItemId = driveItemId;
                deDocket.UpdatedBy = userName;
                deDocket.LastUpdate = DateTime.Now;

                if (deDocketId > 0)
                    await _cpiDbContext.GetRepository<PatDueDateInvDeDocket>().UpdateAsync(deDocket);
                else
                    await _cpiDbContext.GetRepository<PatDueDateInvDeDocket>().AddAsync(deDocket);

                await _cpiDbContext.SaveChangesAsync();
                _cpiDbContext.Detach(deDocket);
                return deDocket;
            }
            return null;
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
