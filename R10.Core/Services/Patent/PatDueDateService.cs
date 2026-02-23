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
    public class PatDueDateService : ChildEntityService<PatActionDue, PatDueDate>, IDueDateService<PatActionDue, PatDueDate>
    {
        private readonly IActionDueService<PatActionDue, PatDueDate> _actionDueService;
        private readonly ICountryApplicationService _countryApplicationService;
        private readonly IDocumentService _docService;
        private readonly IDueDateExtensionService _dueDateExtensionService;
        private readonly ICPiSystemSettingManager _systemSettingManager;
        private readonly ISystemSettings<PatSetting> _settings;

        public PatDueDateService(
            ICPiDbContext cpiDbContext,
            IActionDueService<PatActionDue, PatDueDate> actionDueService,
            ICountryApplicationService countryApplicationService,
            ClaimsPrincipal user,
            IDocumentService docService,
            IDueDateExtensionService dueDateExtensionService,
            ICPiSystemSettingManager systemSettingManager,
            ISystemSettings<PatSetting> settings) : base(cpiDbContext, user)
        {
            _actionDueService = actionDueService;
            _countryApplicationService = countryApplicationService;
            _docService = docService;
            _dueDateExtensionService = dueDateExtensionService;
            _systemSettingManager = systemSettingManager;
            _settings = settings;
        }

        public IQueryable<PatActionDue> ActionsDue => _actionDueService.QueryableList;

        public override IQueryable<PatDueDate> QueryableList
        {
            get
            {
                var dueDates = _cpiDbContext.GetRepository<PatDueDate>().QueryableList;

                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Patent) || _user.RestrictExportControl() || !_user.CanAccessPatTradeSecret())
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
        public override async Task<bool> Update(object key, string userName, IEnumerable<PatDueDate> updated, IEnumerable<PatDueDate> added, IEnumerable<PatDueDate> deleted)
        {
            var actId = (int)key;
            var actionDue = await ValidateActionDue(actId); //includes country app validation
            //var countryApp = await ValidateCountryApplication(actionDue.AppId);
            var countryApp = await _cpiDbContext.GetRepository<CountryApplication>().QueryableList.Where(c => c.AppId == actionDue.AppId).FirstOrDefaultAsync();
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

            _cpiDbContext.GetRepository<PatActionDue>().Attach(actionDue);
            actionDue.UpdatedBy = userName;
            actionDue.LastUpdate = DateTime.Now;

            _cpiDbContext.GetRepository<CountryApplication>().Attach(countryApp);
            countryApp.UpdatedBy = actionDue.UpdatedBy;
            countryApp.LastUpdate = actionDue.LastUpdate;

            var recurringDueDates = new List<PatDueDate>();
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

            _cpiDbContext.GetRepository<PatDueDate>().Delete(deleted);
            _cpiDbContext.GetRepository<PatDueDate>().Update(updated);
            _cpiDbContext.GetRepository<PatDueDate>().Update(added);
            _cpiDbContext.GetRepository<PatDueDate>().Update(recurringDueDates);

            bool concurrencyFailure;
            int counter = 1;
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
                catch (InvalidOperationException ex)
                {
                    if (ex.Message.Contains("A second operation") && counter < 3)
                    {
                        concurrencyFailure = true;
                        counter++;
                        await Task.Delay(500);
                    }
                    else
                    {
                        throw;
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
        public async Task<bool> Update(int parentId, string userName, IEnumerable<PatDueDate> updated, IEnumerable<PatDueDate> deleted)
        {
            var countryApp = await ValidateCountryApplication(parentId);

            if (updated.Any())
            {
                var permissions = CPiPermissions.FullModify;
                permissions.AddRange(CPiPermissions.RemarksOnly);
                await ValidatePermission(permissions, countryApp.RespOffice);
            }

            if (deleted.Any())
                await ValidatePermission(CPiPermissions.CanDelete, countryApp.RespOffice);

            _cpiDbContext.GetRepository<CountryApplication>().Attach(countryApp);
            countryApp.UpdatedBy = userName;
            countryApp.LastUpdate = DateTime.Now;

            List<int> updatedActIds = new List<int>();
            if (updated.Any())
                updatedActIds.AddRange(updated.Select(d => d.ActId).Distinct().ToList());

            if (deleted.Any())
                updatedActIds.AddRange(deleted.Select(d => d.ActId).Distinct().ToList());

            var actionsDue = await ActionsDue.Where(a => updatedActIds.Any(actId => actId == a.ActId)).ToListAsync();
            _cpiDbContext.GetRepository<PatActionDue>().Attach(actionsDue);
            foreach (var actionDue in actionsDue)
            {
                actionDue.UpdatedBy = countryApp.UpdatedBy;
                actionDue.LastUpdate = countryApp.LastUpdate;
            }

            var recurringDueDates = new List<PatDueDate>();
            foreach (var dueDate in updated)
            {
                dueDate.UpdatedBy = countryApp.UpdatedBy;
                dueDate.LastUpdate = countryApp.LastUpdate;

                var actionDue = actionsDue.FirstOrDefault(a => a.ActId == dueDate.ActId);
                var recurringDueDate = await GetRecurringDueDate(actionDue, dueDate);
                if (recurringDueDate != null)
                    recurringDueDates.Add(recurringDueDate);
            }

            _cpiDbContext.GetRepository<PatDueDate>().Delete(deleted);
            _cpiDbContext.GetRepository<PatDueDate>().Update(updated);
            _cpiDbContext.GetRepository<PatDueDate>().Update(recurringDueDates);
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
        public async Task<bool> UpdateDeDocket(string userName, IEnumerable<PatDueDate> updated)
        {

            var addedDeDockets = new List<PatDueDateDeDocketOutstanding>();
            var updatedDeDockets = new List<PatDueDateDeDocketOutstanding>();
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
                    var patDueDate = await QueryableList.Where(d => d.DDId == dueDate.DDId).FirstOrDefaultAsync();
                    if (patDueDate != null && patDueDate.Remarks != dueDate.Remarks)
                    {
                        _cpiDbContext.GetRepository<PatDueDate>().Update(patDueDate);
                        patDueDate.Remarks = dueDate.Remarks;
                        patDueDate.UpdatedBy = userName;
                        patDueDate.LastUpdate = DateTime.Now;
                    }
                }
            }
            _cpiDbContext.GetRepository<PatDueDateDeDocketOutstanding>().Update(updatedDeDockets);
            _cpiDbContext.GetRepository<PatDueDateDeDocketOutstanding>().Update(addedDeDockets);
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
                await _cpiDbContext.GetRepository<PatDueDateExtension>().UpdateAsync((PatDueDateExtension)setting);
            }
            else
            {
                await _cpiDbContext.GetRepository<PatDueDateExtension>().AddAsync((PatDueDateExtension)setting);
            }
            await _cpiDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<DueDateExtension> GetExtensionSetting(int ddId, int parentId)
        {
            var setting = await _cpiDbContext.GetRepository<PatDueDateExtension>().QueryableList.FirstOrDefaultAsync(e => e.DDId == ddId);
            if (setting == null)
            {
                var parent = await _countryApplicationService.CountryApplications.Include(c => c.Invention).FirstOrDefaultAsync(c => c.AppId == parentId);
                if (parent.Invention.ClientID > 0)
                {
                    var client = await _countryApplicationService.Clients.Where(c => c.ClientID == parent.Invention.ClientID).FirstOrDefaultAsync();
                    if (client != null)
                    {
                        setting = new PatDueDateExtension
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

        public IQueryable<DueDateExtension> DueDateExtensions => _cpiDbContext.GetRepository<PatDueDateExtension>().QueryableList;
        public IQueryable<DueDateDeDocket> DueDateDeDockets => _cpiDbContext.GetRepository<PatDueDateDeDocket>().QueryableList;
        public IQueryable<DueDateDeDocketResp> DueDateDeDocketResps => _cpiDbContext.GetRepository<PatDueDateDeDocketResp>().QueryableList;

        private async Task ValidatePermission(List<string> roles, string respOffice)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Patent, roles, respOffice));
        }

        private async Task<PatActionDue> ValidateActionDue(int actId)
        {
            var actionDue = await ActionsDue.SingleOrDefaultAsync(a => a.ActId == actId);
            Guard.Against.NoRecordPermission(actionDue != null);

            return actionDue;
        }

        private async Task<CountryApplication> ValidateCountryApplication(int appId)
        {
            var countryApp = await _countryApplicationService.CountryApplications.SingleOrDefaultAsync(ca => ca.AppId == appId);
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
        private async Task DeleteEmptyAction(PatActionDue actionDue)
        {
            if (!await QueryableList.AnyAsync(d => d.ActId == actionDue.ActId))
            {
                //Update ActId to ActionTypeID in tblDocVerification before removing tblPatActionDue record
                var actionTypeId = await _cpiDbContext.GetRepository<PatActionType>().QueryableList
                                        .Where(d => EF.Functions.Like(d.ActionType, actionDue.ActionType))
                                        .Select(d => d.ActionTypeID).FirstOrDefaultAsync();
                if (actionTypeId > 0)
                    await _docService.UpdateVerificationActionTypeId("P", actionDue.AppId, actionDue.ActId, actionTypeId);

                _cpiDbContext.GetRepository<PatActionDue>().Delete(actionDue);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Get recurring country law due date if date taken is updated.
        /// </summary>
        /// <param name="actionDue">The parent action due record that will link back to PatCountryDue.</param>
        /// <param name="dueDate">The due date record that the recurring due date will be based on.</param>
        /// <returns></returns>
        private async Task<PatDueDate> GetRecurringDueDate(PatActionDue actionDue, PatDueDate dueDate)
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
        private async Task ApplyDeDocketLogic(string userName, PatDueDate dueDate, PatDueDateDeDocketOutstanding deDocket)
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

        public async Task<DueDateDeDocket> UpdateDeDocketFileInfo(int ddId, int deDocketId, string? docFile, int fileId, string userName,string? driveItemId)
        {
            var deDocket = new PatDueDateDeDocket();
            if (deDocketId > 0)
                deDocket = await _cpiDbContext.GetRepository<PatDueDateDeDocket>().QueryableList.FirstOrDefaultAsync(ddk => ddk.DeDocketId == deDocketId);
            else {
                deDocket.DDId = ddId;
                deDocket.CreatedBy = userName;
                deDocket.DateCreated = DateTime.Now;
                deDocket.UpdatedBy = userName;
                deDocket.LastUpdate = DateTime.Now;
            }
            
            if (deDocket != null) {
                deDocket.FileId = fileId;
                deDocket.DocFile = docFile;
                deDocket.DriveItemId = driveItemId;
                deDocket.UpdatedBy = userName;
                deDocket.LastUpdate = DateTime.Now;

                if (deDocketId > 0)
                    await _cpiDbContext.GetRepository<PatDueDateDeDocket>().UpdateAsync(deDocket);
                else
                    await _cpiDbContext.GetRepository<PatDueDateDeDocket>().AddAsync(deDocket);

                await _cpiDbContext.SaveChangesAsync();
                _cpiDbContext.Detach(deDocket);
                return deDocket;
            }
            return null;
        }

        public async Task UpdateDeDocketResp(List<string> responsibleList, string userName, int deDocketId)
        {
            var idList = responsibleList.Select(d => 
                                        { 
                                            int intVal; string strVal = d;
                                            bool isInt = int.TryParse(d, out intVal);                                             
                                            return new { intVal, strVal, isInt }; 
                                        })                                  
                                        .ToList();
            
            var existingResps = await _cpiDbContext.GetRepository<PatDueDateDeDocketResp>().QueryableList.Where(d => d.DeDocketId == deDocketId).ToListAsync();

            DateTime today = DateTime.Now;  
            if (idList.Any())
            {
                var selectedUsers = idList.Where(d => !d.isInt)
                    .Select(d => new PatDueDateDeDocketResp
                    {
                        DeDocketId = deDocketId,                        
                        UserId = d.strVal,
                        GroupId = null,
                    }).ToList();

                var selectedGroups = idList.Where(d => d.isInt)
                    .Select(d => new PatDueDateDeDocketResp
                    {
                        DeDocketId = deDocketId,                        
                        UserId = null,
                        GroupId = d.intVal,
                    }).ToList();

                //Get deleted users/groups - existing users/groups not in selected users/groups
                var deleted = existingResps.Where(d => (!string.IsNullOrEmpty(d.UserId) && !selectedUsers.Any(s => s.UserId == d.UserId)) 
                                                    || (d.GroupId > 0 && !selectedGroups.Any(s => s.GroupId == d.GroupId))
                                            ).ToList();                
                
                //Get added users/groups - selected users/groups not in existing users/groups
                var added = selectedUsers.Where(d => !string.IsNullOrEmpty(d.UserId) && !existingResps.Any(s => s.UserId == d.UserId)).ToList();
                added.AddRange(selectedGroups.Where(d => d.GroupId > 0 && !existingResps.Any(s => s.GroupId == d.GroupId)).ToList());

                if (added.Any())
                {
                    added.ForEach(d => { d.CreatedBy = userName; d.UpdatedBy = userName; d.DateCreated = today; d.LastUpdate = today; });                    
                    _cpiDbContext.GetRepository<PatDueDateDeDocketResp>().Add(added);

                    ////Log new added                                   
                    //_repository.PatDueDateDeDocketRespLogs.Add(new PatDueDateDeDocketRespLog()
                    //{
                    //    DocId = docId,                        
                    //    UserIds = string.Join("|", added.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).ToList()),
                    //    GroupIds = string.Join("|", added.Where(d => d.GroupId > 0).Select(d => d.GroupId.ToString()).ToList()),
                    //    RespType = DocRespLogType.Docketing,
                    //    TransxType = DocRespLogTransxType.Update,
                    //    CreatedBy = userName,
                    //    UpdatedBy = userName,
                    //    DateCreated = today,
                    //    LastUpdate = today
                    //});
                }

                if (deleted.Any())
                {                    
                    _cpiDbContext.GetRepository<PatDueDateDeDocketResp>().Delete(deleted);

                    ////Log deleted 
                    //_repository.PatDueDateDeDocketRespLogs.Add(new PatDueDateDeDocketRespLog()
                    //{
                    //    DocId = docId,                        
                    //    UserIds = string.Join("|", deleted.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).ToList()),
                    //    GroupIds = string.Join("|", deleted.Where(d => d.GroupId > 0).Select(d => d.GroupId.ToString()).ToList()),
                    //    RespType = DocRespLogType.Docketing,
                    //    TransxType = DocRespLogTransxType.Delete,
                    //    CreatedBy = userName,
                    //    UpdatedBy = userName,
                    //    DateCreated = today,
                    //    LastUpdate = today
                    //});
                }                    
                
                await _cpiDbContext.SaveChangesAsync();
                _cpiDbContext.Detach(added);
                _cpiDbContext.Detach(deleted);
            }
        }

        public async Task MarkDeDocketInstructionsAsCompleted(List<int> deDocketIds, DateTime? completedDate)
        {
            var userName = _user.GetUserName();
            var deDockets = await _cpiDbContext.GetRepository<PatDueDateDeDocket>().QueryableList
                .Where(d => deDocketIds.Contains(d.DeDocketId) && d.InstructionCompleted == false).ToListAsync();

            var instructions = await _cpiDbContext.GetRepository<DeDocketInstruction>().QueryableList.AsNoTracking().ToListAsync();

            foreach (var deDocket in deDockets)
            {
                var dueDate = await QueryableList.FirstOrDefaultAsync(d => d.DDId == deDocket.DDId);
                if (dueDate != null) 
                {
                    _cpiDbContext.GetRepository<PatDueDate>().Update(dueDate);
                    _cpiDbContext.GetRepository<PatDueDateDeDocket>().Update(deDocket);
                    
                    deDocket.InstructionCompleted = true;
                    deDocket.CompletedBy = userName;
                    deDocket.CompletedDate = completedDate;
                    deDocket.LastUpdate = DateTime.Now;
                    deDocket.UpdatedBy = userName;

                    dueDate.UpdatedBy = userName;
                    dueDate.LastUpdate = DateTime.Now;

                    var instruction = instructions.FirstOrDefault(d => d.Instruction == deDocket.Instruction);
                    if (instruction != null)
                    {
                        if (!string.IsNullOrEmpty(instruction.CloseDeadlineWith) && instruction.CloseDeadlineWith.ToUpper() == "C")
                            dueDate.DateTaken = DateTime.Now;
                        else if (!string.IsNullOrEmpty(instruction.CloseDeadlineWith) && instruction.CloseDeadlineWith.ToUpper() == "I")
                            dueDate.DateTaken = deDocket.InstructionDate != null ? deDocket.InstructionDate.Value.Date : DateTime.Now;
                    }

                    await _cpiDbContext.SaveChangesAsync();
                    _cpiDbContext.Detach(deDocket);
                    _cpiDbContext.Detach(dueDate);
                }                
            }            
        }
    }
}
