using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.Trademark
{
    public class TmkDueDateService : ChildEntityService<TmkActionDue, TmkDueDate>, IDueDateService<TmkActionDue, TmkDueDate>
    {
        private readonly IActionDueService<TmkActionDue, TmkDueDate> _actionDueService;
        private readonly ITmkTrademarkService _trademarkService;
        private readonly IDueDateExtensionService _dueDateExtensionService;
        private readonly IDocumentService _docService;
        private readonly ICPiSystemSettingManager _systemSettingManager;
        private readonly ISystemSettings<TmkSetting> _settings;

        public TmkDueDateService(
            ICPiDbContext cpiDbContext,
            IActionDueService<TmkActionDue, TmkDueDate> actionDueService,
            ITmkTrademarkService trademarkService,
            ClaimsPrincipal user,
            IDueDateExtensionService dueDateExtensionService,
            IDocumentService docService,
            ICPiSystemSettingManager systemSettingManager,
            ISystemSettings<TmkSetting> settings) : base(cpiDbContext, user)
        {
            _actionDueService = actionDueService;
            _trademarkService = trademarkService;
            _dueDateExtensionService = dueDateExtensionService;
            _docService = docService;
            _systemSettingManager = systemSettingManager;
            _settings = settings;
        }
        public IQueryable<TmkActionDue> ActionsDue => _actionDueService.QueryableList;

        public override IQueryable<TmkDueDate> QueryableList
        {
            get
            {
                var dueDates = _cpiDbContext.GetRepository<TmkDueDate>().QueryableList;

                if (_user.HasRespOfficeFilter(SystemType.Trademark) || _user.HasEntityFilter())
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
        public override async Task<bool> Update(object key, string userName, IEnumerable<TmkDueDate> updated, IEnumerable<TmkDueDate> added, IEnumerable<TmkDueDate> deleted)
        {
            var actId = (int)key;
            var actionDue = await ValidateActionDue(actId); //includes Trademark validation
            //var trademark = await ValidateTrademark(actionDue.TmkId);
            var trademark = await _cpiDbContext.GetRepository<TmkTrademark>().QueryableList.Where(t => t.TmkId == actionDue.TmkId).FirstOrDefaultAsync();
            var settings = await _settings.GetSetting();

            if (updated.Any() || added.Any()) {
                var permissions = CPiPermissions.FullModify;
                if (updated.Any())
                    permissions.AddRange(CPiPermissions.RemarksOnly);

                if (settings.IsSoftDocketOn && updated.Any(d => d.Indicator?.ToLower() == "soft docket"))
                    ValidateSoftDocket(actionDue.ResponsibleID);
                else
                    await ValidatePermission(permissions, trademark.RespOffice);
            }

            if (deleted.Any())
            {
                if (settings.IsSoftDocketOn && deleted.Any(d => d.Indicator?.ToLower() == "soft docket"))
                    ValidateSoftDocket(actionDue.ResponsibleID);
                else
                    await ValidatePermission(CPiPermissions.CanDelete, trademark.RespOffice);
            }

            _cpiDbContext.GetRepository<TmkActionDue>().Attach(actionDue);
            actionDue.UpdatedBy = userName;
            actionDue.LastUpdate = DateTime.Now;

            _cpiDbContext.GetRepository<TmkTrademark>().Attach(trademark);
            trademark.UpdatedBy = actionDue.UpdatedBy;
            trademark.LastUpdate = actionDue.LastUpdate;

            var recurringDueDates = new List<TmkDueDate>();
            foreach (var dueDate in updated)
            {
                dueDate.UpdatedBy = actionDue.UpdatedBy;
                dueDate.LastUpdate = actionDue.LastUpdate;

                var recurringDueDate = await GetRecurringDueDate(actionDue, dueDate);
                if (recurringDueDate != null)
                    recurringDueDates.Add(recurringDueDate);

                var deDocket = dueDate.DeDocketOutstanding;
                if (deDocket != null)
                    ApplyDeDocketLogic(userName,dueDate, deDocket);
                
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
                    ApplyDeDocketLogic(userName,dueDate, deDocket);
            }

            _cpiDbContext.GetRepository<TmkDueDate>().Delete(deleted);
            _cpiDbContext.GetRepository<TmkDueDate>().Update(updated);
            _cpiDbContext.GetRepository<TmkDueDate>().Update(added);
            _cpiDbContext.GetRepository<TmkDueDate>().Update(recurringDueDates);

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

            _cpiDbContext.Detach(actionDue);
            _cpiDbContext.Detach(trademark);

            if (deleted.Any())
                await DeleteEmptyAction(actionDue);

            return true;
        }

        /// <summary>
        /// Update from trademark detail screen.
        /// </summary>
        /// <param name="parentId">TmkId of parent trademark record.</param>
        /// <param name="userName">User id stamp.</param>
        /// <param name="updated">Updated due dates.</param>
        /// <param name="deleted">Deleted due dates.</param>
        /// <returns></returns>
        public async Task<bool> Update(int parentId, string userName, IEnumerable<TmkDueDate> updated, IEnumerable<TmkDueDate> deleted)
        {
            var trademark = await ValidateTrademark(parentId);

            if (updated.Any()) {
                var permissions = CPiPermissions.FullModify;
                permissions.AddRange(CPiPermissions.RemarksOnly);
                await ValidatePermission(permissions, trademark.RespOffice);
            }

            if (deleted.Any())
                await ValidatePermission(CPiPermissions.CanDelete, trademark.RespOffice);

            _cpiDbContext.GetRepository<TmkTrademark>().Attach(trademark);
            trademark.UpdatedBy = userName;
            trademark.LastUpdate = DateTime.Now;

            List<int> updatedActIds = new List<int>();
            if (updated.Any())
                updatedActIds.AddRange(updated.Select(d => d.ActId).Distinct().ToList());

            if (deleted.Any())
                updatedActIds.AddRange(deleted.Select(d => d.ActId).Distinct().ToList());

            var actionsDue = await ActionsDue.Where(a => updatedActIds.Any(actId => actId == a.ActId)).ToListAsync();
            _cpiDbContext.GetRepository<TmkActionDue>().Attach(actionsDue);
            foreach (var actionDue in actionsDue)
            {
                actionDue.UpdatedBy = trademark.UpdatedBy;
                actionDue.LastUpdate = trademark.LastUpdate;
            }

            var recurringDueDates = new List<TmkDueDate>();
            foreach (var dueDate in updated)
            {
                dueDate.UpdatedBy = trademark.UpdatedBy;
                dueDate.LastUpdate = trademark.LastUpdate;

                var actionDue = actionsDue.FirstOrDefault(a => a.ActId == dueDate.ActId);
                var recurringDueDate = await GetRecurringDueDate(actionDue, dueDate);
                if (recurringDueDate != null)
                    recurringDueDates.Add(recurringDueDate);
            }

            _cpiDbContext.GetRepository<TmkDueDate>().Delete(deleted);
            _cpiDbContext.GetRepository<TmkDueDate>().Update(updated);
            _cpiDbContext.GetRepository<TmkDueDate>().Update(recurringDueDates);
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
        public async Task<bool> UpdateDeDocket(string userName, IEnumerable<TmkDueDate> updated)
        {
            var addedDeDockets = new List<TmkDueDateDeDocketOutstanding>();
            var updatedDeDockets = new List<TmkDueDateDeDocketOutstanding>();
            foreach (var dueDate in updated)
            {
                var deDocket = dueDate.DeDocketOutstanding;
                if (deDocket != null) {
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
                    var tmkDueDate = await QueryableList.Where(d => d.DDId == dueDate.DDId).FirstOrDefaultAsync();
                    if (tmkDueDate != null && tmkDueDate.Remarks != dueDate.Remarks)
                    {
                        _cpiDbContext.GetRepository<TmkDueDate>().Update(tmkDueDate);
                        tmkDueDate.Remarks = dueDate.Remarks;
                        tmkDueDate.UpdatedBy = userName;
                        tmkDueDate.LastUpdate = DateTime.Now;
                    }
                }
            }
            _cpiDbContext.GetRepository<TmkDueDateDeDocketOutstanding>().Update(updatedDeDockets);
            _cpiDbContext.GetRepository<TmkDueDateDeDocketOutstanding>().Update(addedDeDockets);
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
                await _cpiDbContext.GetRepository<TmkDueDateExtension>().UpdateAsync((TmkDueDateExtension)setting);
            }
            else
            {
                await _cpiDbContext.GetRepository<TmkDueDateExtension>().AddAsync((TmkDueDateExtension)setting);
            }

            await _cpiDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<DueDateExtension> GetExtensionSetting(int ddId, int parentId)
        {
            var setting =  await _cpiDbContext.GetRepository<TmkDueDateExtension>().QueryableList.FirstOrDefaultAsync(e => e.DDId == ddId);
            if (setting == null)
            {
                var parent = await _trademarkService.TmkTrademarks.FirstOrDefaultAsync(t => t.TmkId == parentId);
                if (parent.ClientID > 0)
                {
                    var client = await _cpiDbContext.GetRepository<Client>().QueryableList.FirstOrDefaultAsync(e => e.ClientID == parentId);
                    if (client != null)
                    {
                        setting = new TmkDueDateExtension
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

        public IQueryable<DueDateExtension> DueDateExtensions => _cpiDbContext.GetRepository<TmkDueDateExtension>().QueryableList;
        public IQueryable<DueDateDeDocket> DueDateDeDockets => _cpiDbContext.GetRepository<TmkDueDateDeDocket>().QueryableList;
        public IQueryable<DueDateDeDocketResp> DueDateDeDocketResps => _cpiDbContext.GetRepository<TmkDueDateDeDocketResp>().QueryableList;

        private async Task ValidatePermission(List<string> roles, string respOffice)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Trademark, roles, respOffice));
        }

        private async Task<TmkActionDue> ValidateActionDue(int actId)
        {
            var actionDue = await ActionsDue.SingleOrDefaultAsync(a => a.ActId == actId);
            Guard.Against.NoRecordPermission(actionDue != null);

            return actionDue;
        }

        private async Task<TmkTrademark> ValidateTrademark(int tmkId)
        {
            var trademark = await _trademarkService.TmkTrademarks.SingleOrDefaultAsync(t => t.TmkId == tmkId);
            Guard.Against.NoRecordPermission(trademark != null);

            return trademark;
        }

        private void ValidateSoftDocket(int? responsibleId)
        {
            if (_user.IsSoftDocketUser())
                Guard.Against.NoRecordPermission(responsibleId == _user.GetEntityId());
            else
                Guard.Against.NoRecordPermission(_user.IsInRoles(SystemType.Trademark, CPiPermissions.SoftDocket));
        }

        /// <summary>
        /// Delete action without due dates.
        /// </summary>
        /// <param name="actionDue">The action due record to delete if it has no due dates.</param>
        /// <returns></returns>
        private async Task DeleteEmptyAction(TmkActionDue actionDue)
        {
            if (!await QueryableList.AnyAsync(d => d.ActId == actionDue.ActId))
            {
                //Update ActId to ActionTypeID in tblDocVerification before removing tblPatActionDue record
                var actionTypeId = await _cpiDbContext.GetRepository<TmkActionType>().QueryableList
                                        .Where(d => EF.Functions.Like(d.ActionType, actionDue.ActionType))
                                        .Select(d => d.ActionTypeID).FirstOrDefaultAsync();
                if (actionTypeId > 0)
                    await _docService.UpdateVerificationActionTypeId("T", actionDue.TmkId, actionDue.ActId, actionTypeId);


                _cpiDbContext.GetRepository<TmkActionDue>().Delete(actionDue);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Get recurring country law due date if date taken is updated.
        /// </summary>
        /// <param name="actionDue">The parent action due record that will link back to TmkCountryDue.</param>
        /// <param name="dueDate">The due date record that the recurring due date will be based on.</param>
        /// <returns></returns>
        private async Task<TmkDueDate> GetRecurringDueDate(TmkActionDue actionDue, TmkDueDate dueDate)
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
        private async Task ApplyDeDocketLogic(string userName, TmkDueDate dueDate, TmkDueDateDeDocketOutstanding deDocket)
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
                            dueDate.DateTaken = deDocket.InstructionDate.Value.Date;
                    }
                }
            }
            else {
                dueDate.DeDocketOutstanding = null;
            }
        }

        public async Task<DueDateDeDocket> UpdateDeDocketFileInfo(int ddId, int deDocketId, string? docFile, int fileId, string userName, string? driveItemId)
        {
            var deDocket = new TmkDueDateDeDocket();
            if (deDocketId > 0)
                deDocket = await _cpiDbContext.GetRepository<TmkDueDateDeDocket>().QueryableList.FirstOrDefaultAsync(ddk => ddk.DeDocketId == deDocketId);
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
                deDocket.DriveItemId = driveItemId;
                deDocket.DocFile = docFile;
                deDocket.UpdatedBy = userName;
                deDocket.LastUpdate = DateTime.Now;

                if (deDocketId > 0)
                    await _cpiDbContext.GetRepository<TmkDueDateDeDocket>().UpdateAsync(deDocket);
                else
                    await _cpiDbContext.GetRepository<TmkDueDateDeDocket>().AddAsync(deDocket);

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
            
            var existingResps = await _cpiDbContext.GetRepository<TmkDueDateDeDocketResp>().QueryableList.Where(d => d.DeDocketId == deDocketId).ToListAsync();

            DateTime today = DateTime.Now;  
            if (idList.Any())
            {
                var selectedUsers = idList.Where(d => !d.isInt)
                    .Select(d => new TmkDueDateDeDocketResp
                    {
                        DeDocketId = deDocketId,                        
                        UserId = d.strVal,
                        GroupId = null,
                    }).ToList();

                var selectedGroups = idList.Where(d => d.isInt)
                    .Select(d => new TmkDueDateDeDocketResp
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
                    _cpiDbContext.GetRepository<TmkDueDateDeDocketResp>().Add(added);

                    ////Log new added                                   
                    //_repository.TmkDueDateDeDocketRespLogs.Add(new TmkDueDateDeDocketRespLog()
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
                    _cpiDbContext.GetRepository<TmkDueDateDeDocketResp>().Delete(deleted);

                    ////Log deleted 
                    //_repository.TmkDueDateDeDocketRespLogs.Add(new TmkDueDateDeDocketRespLog()
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
            var deDockets = await _cpiDbContext.GetRepository<TmkDueDateDeDocket>().QueryableList
                .Where(d => deDocketIds.Contains(d.DeDocketId) && d.InstructionCompleted == false).ToListAsync(); 
            
            var instructions = await _cpiDbContext.GetRepository<DeDocketInstruction>().QueryableList.AsNoTracking().ToListAsync();

            foreach (var deDocket in deDockets)
            {
                var dueDate = await QueryableList.FirstOrDefaultAsync(d => d.DDId == deDocket.DDId);
                if (dueDate != null) 
                {
                    _cpiDbContext.GetRepository<TmkDueDate>().Update(dueDate);
                    _cpiDbContext.GetRepository<TmkDueDateDeDocket>().Update(deDocket);
                    
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
