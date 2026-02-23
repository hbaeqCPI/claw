using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Services.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.GeneralMatter
{
    public class GMDueDateService : ChildEntityService<GMActionDue, GMDueDate>, IDueDateService<GMActionDue, GMDueDate>
    {
        private readonly IActionDueService<GMActionDue, GMDueDate> _actionDueService;
        private readonly IGMMatterService _matterService;
        private readonly IDueDateExtensionService _dueDateExtensionService;
        private readonly IDocumentService _docService;
        private readonly ICPiSystemSettingManager _systemSettingManager;
        private readonly ISystemSettings<GMSetting> _settings;

        public GMDueDateService(
            ICPiDbContext cpiDbContext, 
            IActionDueService<GMActionDue, GMDueDate> actionDueService, 
            IGMMatterService matterService,
            ClaimsPrincipal user, IDueDateExtensionService dueDateExtensionService,
            IDocumentService docService,
            ICPiSystemSettingManager systemSettingManager,
            ISystemSettings<GMSetting> settings) : base(cpiDbContext, user)
        {
            _actionDueService = actionDueService;
            _matterService = matterService;
            _dueDateExtensionService = dueDateExtensionService;
            _docService = docService;
            _systemSettingManager = systemSettingManager;
            _settings = settings;
        }

        public IQueryable<GMActionDue> ActionsDue => _actionDueService.QueryableList;

        public override IQueryable<GMDueDate> QueryableList
        {
            get
            {
                var dueDates = _cpiDbContext.GetRepository<GMDueDate>().QueryableList;

                if (_user.HasRespOfficeFilter(SystemType.GeneralMatter) || _user.HasEntityFilter())
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
        public override async Task<bool> Update(object key, string userName, IEnumerable<GMDueDate> updated, IEnumerable<GMDueDate> added, IEnumerable<GMDueDate> deleted)
        {
            var actId = (int)key;
            var actionDue = await ValidateActionDue(actId);
            var matter = await ValidateMatter(actionDue.MatId);
            var settings = await _settings.GetSetting();

            if (updated.Any() || added.Any())
            {
                var permissions = CPiPermissions.FullModify;
                if (updated.Any())
                    permissions.AddRange(CPiPermissions.RemarksOnly);

                if (settings.IsSoftDocketOn && updated.Any(d => d.Indicator?.ToLower() == "soft docket"))
                    ValidateSoftDocket(actionDue.ResponsibleID);
                else
                    await ValidatePermission(permissions, matter.RespOffice);

            }

            if (deleted.Any())
            {
                if (settings.IsSoftDocketOn && deleted.Any(d => d.Indicator?.ToLower() == "soft docket"))
                    ValidateSoftDocket(actionDue.ResponsibleID);
                else
                    await ValidatePermission(CPiPermissions.CanDelete, matter.RespOffice);
            }

            _cpiDbContext.GetRepository<GMActionDue>().Attach(actionDue);
            actionDue.UpdatedBy = userName;
            actionDue.LastUpdate = DateTime.Now;

            _cpiDbContext.GetRepository<GMMatter>().Attach(matter);
            matter.UpdatedBy = actionDue.UpdatedBy;
            matter.LastUpdate = actionDue.LastUpdate;

            foreach (var dueDate in updated)
            {
                dueDate.UpdatedBy = actionDue.UpdatedBy;
                dueDate.LastUpdate = actionDue.LastUpdate;

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

            _cpiDbContext.GetRepository<GMDueDate>().Delete(deleted);
            _cpiDbContext.GetRepository<GMDueDate>().Update(updated);
            _cpiDbContext.GetRepository<GMDueDate>().Update(added);

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

            _cpiDbContext.Detach(actionDue);
            _cpiDbContext.Detach(matter);

            return true;
        }

        /// <summary>
        /// Update from matter detail screen.
        /// </summary>
        /// <param name="parentId">MatId of parent matter record.</param>
        /// <param name="userName">User id stamp.</param>
        /// <param name="updated">Updated due dates.</param>
        /// <param name="deleted">Deleted due dates.</param>
        /// <returns></returns>
        public async Task<bool> Update(int parentId, string userName, IEnumerable<GMDueDate> updated, IEnumerable<GMDueDate> deleted)
        {
            var matter = await ValidateMatter(parentId);

            if (updated.Any()) {
                var permissions = CPiPermissions.FullModify;
                permissions.AddRange(CPiPermissions.RemarksOnly);
                await ValidatePermission(permissions, matter.RespOffice);
            }

            if (deleted.Any())
                await ValidatePermission(CPiPermissions.CanDelete, matter.RespOffice);

            _cpiDbContext.GetRepository<GMMatter>().Attach(matter);
            matter.UpdatedBy = userName;
            matter.LastUpdate = DateTime.Now;

            List<int> updatedActIds = new List<int>();
            if (updated.Any())
                updatedActIds.AddRange(updated.Select(d => d.ActId).Distinct().ToList());

            if (deleted.Any())
                updatedActIds.AddRange(deleted.Select(d => d.ActId).Distinct().ToList());

            var actionsDue = await ActionsDue.Where(a => updatedActIds.Any(actId => actId == a.ActId)).ToListAsync();
            _cpiDbContext.GetRepository<GMActionDue>().Attach(actionsDue);
            foreach (var actionDue in actionsDue)
            {
                actionDue.UpdatedBy = matter.UpdatedBy;
                actionDue.LastUpdate = matter.LastUpdate;
            }

            foreach (var dueDate in updated)
            {
                dueDate.UpdatedBy = matter.UpdatedBy;
                dueDate.LastUpdate = matter.LastUpdate;
            }

            _cpiDbContext.GetRepository<GMDueDate>().Delete(deleted);
            _cpiDbContext.GetRepository<GMDueDate>().Update(updated);
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
        public async Task<bool> UpdateDeDocket(string userName, IEnumerable<GMDueDate> updated)
        {
            var addedDeDockets = new List<GMDueDateDeDocketOutstanding>();
            var updatedDeDockets = new List<GMDueDateDeDocketOutstanding>();
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
                    var gmDueDate = await QueryableList.Where(d => d.DDId == dueDate.DDId).FirstOrDefaultAsync();
                    if (gmDueDate != null && gmDueDate.Remarks != dueDate.Remarks)
                    {
                        _cpiDbContext.GetRepository<GMDueDate>().Update(gmDueDate);
                        gmDueDate.Remarks = dueDate.Remarks;
                        gmDueDate.UpdatedBy = userName;
                        gmDueDate.LastUpdate = DateTime.Now;
                    }
                }
            }
            _cpiDbContext.GetRepository<GMDueDateDeDocketOutstanding>().Update(updatedDeDockets);
            _cpiDbContext.GetRepository<GMDueDateDeDocketOutstanding>().Update(addedDeDockets);
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
                await _cpiDbContext.GetRepository<GMDueDateExtension>().UpdateAsync((GMDueDateExtension)setting);
            }
            else
            {
                await _cpiDbContext.GetRepository<GMDueDateExtension>().AddAsync((GMDueDateExtension)setting);
            }

            await _cpiDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<DueDateExtension> GetExtensionSetting(int ddId, int parentId)
        {
            var setting = await _cpiDbContext.GetRepository<GMDueDateExtension>().QueryableList.FirstOrDefaultAsync(e => e.DDId == ddId);
            if (setting == null)
            {
                var parent = await _cpiDbContext.GetRepository<GMMatter>().QueryableList.FirstOrDefaultAsync(t => t.MatId == parentId);
                if (parent.ClientID > 0)
                {
                    var client = await _cpiDbContext.GetRepository<Client>().QueryableList.FirstOrDefaultAsync(e => e.ClientID == parentId);
                    if (client != null)
                    {
                        setting = new GMDueDateExtension
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

        public IQueryable<DueDateExtension> DueDateExtensions => _cpiDbContext.GetRepository<GMDueDateExtension>().QueryableList;
        public IQueryable<DueDateDeDocket> DueDateDeDockets => _cpiDbContext.GetRepository<GMDueDateDeDocket>().QueryableList;
        public IQueryable<DueDateDeDocketResp> DueDateDeDocketResps => _cpiDbContext.GetRepository<GMDueDateDeDocketResp>().QueryableList;

        private async Task ValidatePermission(List<string> roles, string respOffice)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.GeneralMatter, roles, respOffice));
        }

        private async Task<GMActionDue> ValidateActionDue(int actId)
        {
            var actionDue = await ActionsDue.SingleOrDefaultAsync(a => a.ActId == actId);
            Guard.Against.NoRecordPermission(actionDue != null);

            return actionDue;
        }

        private async Task<GMMatter> ValidateMatter(int matId)
        {
            var matter = await _matterService.QueryableList.SingleOrDefaultAsync(m => m.MatId == matId);
            Guard.Against.NoRecordPermission(matter != null);

            return matter;
        }

        private void ValidateSoftDocket(int? responsibleId)
        {
            if (_user.IsSoftDocketUser())
                Guard.Against.NoRecordPermission(responsibleId == _user.GetEntityId());
            else
                Guard.Against.NoRecordPermission(_user.IsInRoles(SystemType.GeneralMatter, CPiPermissions.SoftDocket));
        }

        /// <summary>
        /// Delete action without due dates.
        /// </summary>
        /// <param name="actionDue">The action due record to delete if it has no due dates.</param>
        /// <returns></returns>
        private async Task DeleteEmptyAction(GMActionDue actionDue)
        {
            if (!await QueryableList.AnyAsync(d => d.ActId == actionDue.ActId))
            {
                //Update ActId to ActionTypeID in tblDocVerification before removing tblPatActionDue record
                var actionTypeId = await _cpiDbContext.GetRepository<GMActionType>().QueryableList
                                        .Where(d => EF.Functions.Like(d.ActionType, actionDue.ActionType))
                                        .Select(d => d.ActionTypeID).FirstOrDefaultAsync();
                if (actionTypeId > 0)
                    await _docService.UpdateVerificationActionTypeId("G", actionDue.MatId, actionDue.ActId, actionTypeId);

                _cpiDbContext.GetRepository<GMActionDue>().Delete(actionDue);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Apply dedocket logic to due date record
        /// </summary>
        /// <param name="userName">User id stamp.</param>
        /// <param name="dueDate">The source duedate record</param>
        /// <param name="deDocket">The source dedocket record</param>
        private async Task ApplyDeDocketLogic(string userName, GMDueDate dueDate, GMDueDateDeDocketOutstanding deDocket)
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
            else
            {
                dueDate.DeDocketOutstanding = null;
            }
        }

        public async Task<DueDateDeDocket> UpdateDeDocketFileInfo(int ddId, int deDocketId, string? docFile, int fileId, string userName, string? driveItemId)
        {
            var deDocket = new GMDueDateDeDocket();
            if (deDocketId > 0)
                deDocket = await _cpiDbContext.GetRepository<GMDueDateDeDocket>().QueryableList.FirstOrDefaultAsync(ddk => ddk.DeDocketId == deDocketId);
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
                    await _cpiDbContext.GetRepository<GMDueDateDeDocket>().UpdateAsync(deDocket);
                else
                    await _cpiDbContext.GetRepository<GMDueDateDeDocket>().AddAsync(deDocket);

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
            
            var existingResps = await _cpiDbContext.GetRepository<GMDueDateDeDocketResp>().QueryableList.Where(d => d.DeDocketId == deDocketId).ToListAsync();

            DateTime today = DateTime.Now;  
            if (idList.Any())
            {
                var selectedUsers = idList.Where(d => !d.isInt)
                    .Select(d => new GMDueDateDeDocketResp
                    {
                        DeDocketId = deDocketId,                        
                        UserId = d.strVal,
                        GroupId = null,
                    }).ToList();

                var selectedGroups = idList.Where(d => d.isInt)
                    .Select(d => new GMDueDateDeDocketResp
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
                    _cpiDbContext.GetRepository<GMDueDateDeDocketResp>().Add(added);

                    ////Log new added                                   
                    //_repository.GMDueDateDeDocketRespLogs.Add(new GMDueDateDeDocketRespLog()
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
                    _cpiDbContext.GetRepository<GMDueDateDeDocketResp>().Delete(deleted);

                    ////Log deleted 
                    //_repository.GMDueDateDeDocketRespLogs.Add(new GMDueDateDeDocketRespLog()
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
            var deDockets = await _cpiDbContext.GetRepository<GMDueDateDeDocket>().QueryableList
                .Where(d => deDocketIds.Contains(d.DeDocketId) && d.InstructionCompleted == false).ToListAsync(); 
            
            var instructions = await _cpiDbContext.GetRepository<DeDocketInstruction>().QueryableList.AsNoTracking().ToListAsync();

            foreach (var deDocket in deDockets)
            {
                var dueDate = await QueryableList.FirstOrDefaultAsync(d => d.DDId == deDocket.DDId);
                if (dueDate != null) 
                {
                    _cpiDbContext.GetRepository<GMDueDate>().Update(dueDate);
                    _cpiDbContext.GetRepository<GMDueDateDeDocket>().Update(deDocket);
                    
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
