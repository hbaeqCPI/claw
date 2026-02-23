using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace R10.Core.Services.GeneralMatter
{
    public class GMActionDueService : EntityService<GMActionDue>, IActionDueDeDocketService<GMActionDue, GMDueDate>
    {
        private readonly IGMMatterService _matterService;
        private readonly ISystemSettings<GMSetting> _settings;
        private readonly ICPiUserSettingManager _userSettingManager;
        private readonly ICPiSystemSettingManager _systemSettingManager;

        public GMActionDueService(
            ICPiDbContext cpiDbContext, 
            IGMMatterService matterService,
            ISystemSettings<GMSetting> settings,
            ClaimsPrincipal user,
            ICPiUserSettingManager userSettingManager,
            ICPiSystemSettingManager systemSettingManager) : base(cpiDbContext, user)
        {
            _matterService = matterService;
            _settings = settings;
            _userSettingManager = userSettingManager;
            _systemSettingManager = systemSettingManager;
        }

        public override IQueryable<GMActionDue> QueryableList
        {
            get
            {
                var actionsDue = base.QueryableList;

                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.GeneralMatter))
                    actionsDue = actionsDue.Where(a => _matterService.QueryableList.Any(gm => gm.MatId == a.MatId));

                return actionsDue;
            }
        }

        public override async Task<GMActionDue> GetByIdAsync(int actId)
        {
            return await QueryableList.SingleOrDefaultAsync(a => a.ActId == actId);
        }

        public override async Task Add(GMActionDue actionDue)
        {
            actionDue.ComputerGenerated = false;

            await ValidatePermission(actionDue, CPiPermissions.FullModify);
            await ValidateResponsibleAttorney(actionDue.ResponsibleID ?? 0);

            await ValidateMatter(actionDue);

            if (actionDue.DueDates == null)
                actionDue.DueDates = await GenerateDueDates(actionDue);

            var settings = await _settings.GetSetting();
            if (actionDue.DueDates.Any() && settings.IsWorkflowOn)
            {
                var dueDatesFromWorkflow = await _matterService.GenerateDueDateFromActionParameterWorkflow(actionDue, actionDue.DueDates, GMWorkflowTriggerType.Indicator);
                if (dueDatesFromWorkflow != null && dueDatesFromWorkflow.Any())
                {
                    actionDue.DueDates.AddRange(dueDatesFromWorkflow);
                }
            }

            _cpiDbContext.GetRepository<GMActionDue>().Add(actionDue);

            //if (actionDue.ResponseDate != null)
            //    await GenerateFollowUpAction(actionDue);

            if (actionDue.DocFolders != null)
            {
                await AddCustomDocFolder(actionDue);
            }
            else
                await _cpiDbContext.SaveChangesAsync();
            
            //save the main action first before adding a followup
            if (actionDue.ResponseDate != null)
            {
                await GenerateFollowUpAction(actionDue);
                await _cpiDbContext.SaveChangesAsync();
            }

            _cpiDbContext.Detach(actionDue);
        }

        public override async Task Update(GMActionDue actionDue)
        {
            await ValidatePermission(actionDue, CPiPermissions.FullModify);
            await ValidateResponsibleAttorney(actionDue.ResponsibleID ?? 0);

            await ValidateMatter(actionDue);

            var generateFollowUp = await UpdateDueDates(actionDue);
            if (generateFollowUp)
                await GenerateFollowUpAction(actionDue);

            //Verification - reset CheckDocket if response date changed to empty and there is no due date with "final" or "due date" with date taken != null
            var responseDateChanged = await QueryableList.AsNoTracking().AnyAsync(d => d.ActId == actionDue.ActId && d.ResponseDate != actionDue.ResponseDate);
            var filterIndicatorList = new List<string>() { "due date", "final" };
            var ddIds = new List<int>();
            if (actionDue.DueDates != null)
                ddIds.AddRange(actionDue.DueDates.Select(d => d.DDId).Distinct().ToList());

            if (responseDateChanged && actionDue.ResponseDate == null && actionDue.CheckDocket 
                && (await _settings.GetSetting()).IsDocumentVerificationOn
                && (actionDue.DueDates == null || !actionDue.DueDates.Any() || !(actionDue.DueDates.Any(d => !string.IsNullOrEmpty(d.Indicator) && filterIndicatorList.Contains(d.Indicator.ToLower()) && d.DateTaken != null)))
                && !(await DueDates.AsNoTracking().AnyAsync(d => d.ActId == actionDue.ActId && (!ddIds.Any() || !ddIds.Contains(d.DDId)) && !string.IsNullOrEmpty(d.Indicator) && filterIndicatorList.Contains(d.Indicator.ToLower()) && d.DateTaken != null)))
            {
                actionDue.CheckDocket = false;                        
            } 
            
            await _cpiDbContext.SaveChangesAsync();
            _cpiDbContext.Detach(actionDue);
            
        }

        public override async Task UpdateRemarks(GMActionDue actionDue)
        {
            var updated = await GetByIdAsync(actionDue.ActId);
            Guard.Against.NoRecordPermission(updated != null);

            await ValidatePermission(updated, CPiPermissions.RemarksOnly);

            updated.tStamp = actionDue.tStamp;

            _cpiDbContext.GetRepository<GMActionDue>().Attach(updated);
            updated.Remarks = actionDue.Remarks;
            updated.UpdatedBy = actionDue.UpdatedBy;
            updated.LastUpdate = actionDue.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task UpdateResponseDate(GMActionDue actionDue)
        {
            var updated = await GetByIdAsync(actionDue.ActId);
            Guard.Against.NoRecordPermission(updated != null);

            await ValidatePermission(updated, CPiPermissions.RemarksOnly);

            updated.tStamp = actionDue.tStamp;

            _cpiDbContext.GetRepository<GMActionDue>().Attach(updated);
            updated.ResponseDate = actionDue.ResponseDate;
            updated.UpdatedBy = actionDue.UpdatedBy;
            updated.LastUpdate = actionDue.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();
        }

        public override async Task Delete(GMActionDue actionDue)
        {
            await ValidatePermission(actionDue, CPiPermissions.CanDelete);
            await ValidateResponsibleAttorney(actionDue.ResponsibleID ?? 0);

            await base.Delete(actionDue);
        }

        private async Task ValidatePermission(GMActionDue actionDue, List<string> roles)
        {
            if ((await _userSettingManager.GetUserSetting<UserAccountSettings>(_user.GetUserIdentifier())).RestrictAdhocActions &&
                !string.IsNullOrEmpty(actionDue.ActionType) &&
                (actionDue.ActId == 0 ||
                !actionDue.ActionType.Equals((await GetByIdAsync(actionDue.ActId)).ActionType, StringComparison.InvariantCultureIgnoreCase)))
                Guard.Against.ValueNotAllowed(await _cpiDbContext
                                                        .GetReadOnlyRepositoryAsync<GMActionType>().QueryableList
                                                        .AnyAsync(at => at.ActionType == actionDue.ActionType), "Action Type");

            var actId = actionDue.ActId;
            var respOfc = "";
            if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.GeneralMatter))
            {
                var item = new KeyValuePair<int, string>();
                if (actId > 0)
                {
                    item = (await QueryableList.Where(a => a.ActId == actId)
                                    .Select(a => new { a.MatId, a.GMMatter.RespOffice })
                                    .ToDictionaryAsync(a => a.MatId, c => c.RespOffice)).FirstOrDefault();
                }
                else
                {
                    actionDue.SubCase = actionDue.SubCase ?? "";
                    item = (await _matterService.QueryableList
                                    .Where(m => m.CaseNumber == actionDue.CaseNumber && m.SubCase == actionDue.SubCase)
                                    .Select(m => new { m.MatId, m.RespOffice })
                                    .ToDictionaryAsync(m => m.MatId, c => c.RespOffice)).FirstOrDefault();
                }

                Guard.Against.NoRecordPermission(item.Key > 0);

                respOfc = item.Value;
            }

            var settings = await _settings.GetSetting();
            if (settings.IsSoftDocketOn && actionDue.DueDates != null && actionDue.DueDates.Any(d => !string.IsNullOrEmpty(d.Indicator) && d.Indicator.ToLower() == "soft docket"))
                Guard.Against.NoRecordPermission(_user.IsSoftDocketUser() || _user.IsInRoles(SystemType.GeneralMatter, CPiPermissions.SoftDocket));
            else
                Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.GeneralMatter, roles, respOfc));
        }

        private async Task ValidateResponsibleAttorney(int responsibleId)
        {
            if (responsibleId > 0 && _user.GetEntityFilterType() == CPiEntityType.Attorney)
                Guard.Against.ValueNotAllowed(await base.EntityFilterAllowed(responsibleId), "Responsible Attorney");
        }

        private async Task ValidateMatter(GMActionDue actionDue)
        {
            var settings = await _settings.GetSetting();

            actionDue.SubCase = actionDue.SubCase ?? "";

            var matter = await _matterService.QueryableList
                .Where(m =>
                    m.CaseNumber == actionDue.CaseNumber &&
                    m.SubCase == actionDue.SubCase).
                SingleOrDefaultAsync();

            var caseNumberLabel = settings.LabelCaseNumber;
            Guard.Against.ValueNotAllowed(matter?.MatId > 0, $"{caseNumberLabel}/Sub Case");

            if (_user.IsRespOfficeOn(SystemType.GeneralMatter))
                Guard.Against.ValueNotAllowed(await ValidateRespOffice(SystemType.GeneralMatter, matter.RespOffice), $"{caseNumberLabel}/Sub Case");

            actionDue.MatId = matter.MatId;

            _cpiDbContext.GetRepository<GMMatter>().Attach(matter);
            matter.LastUpdate = actionDue.LastUpdate;
            matter.UpdatedBy = actionDue.UpdatedBy;
            
        }

        public async Task<bool> CanModifyAttorney(int responsibleId)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Attorney && responsibleId > 0)
                return await base.EntityFilterAllowed(responsibleId);
            else
                return true;
        }

        private IQueryable<GMDueDate> DueDates => _cpiDbContext.GetRepository<GMDueDate>().QueryableList;

        /// <summary>
        /// Update or generate due dates.
        /// Returns true if follow up actions need to be generated.
        /// </summary>
        /// <param name="actionDue"></param>
        /// <returns></returns>
        private async Task<bool> UpdateDueDates(GMActionDue actionDue)
        {
            var dueDates = new List<GMDueDate>();
            var recurringDueDates = new List<GMDueDate>();

            var oldActionDue = await QueryableList.SingleOrDefaultAsync(a => a.ActId == actionDue.ActId);
            var responseDateChanged = oldActionDue.ResponseDate != actionDue.ResponseDate;
            var generateDueDates = oldActionDue.ActionType.Trim().ToLower() != actionDue.ActionType.Trim().ToLower() || oldActionDue.BaseDate != actionDue.BaseDate;
            var generateFollowUp = (responseDateChanged && actionDue.ResponseDate != null);

            if (generateDueDates)
            {
                //actionType or baseDate changed
                //regenerate due dates
                generateFollowUp = actionDue.ResponseDate != null;
                dueDates = await GenerateDueDates(actionDue);

                var oldDueDates = await DueDates.Where(d => d.ActId == actionDue.ActId).ToListAsync();
                if (oldDueDates.Any())
                {
                    foreach (var dueDate in dueDates)
                    {
                        var oldDueDate = oldDueDates.Where(d => d.ActionDue == dueDate.ActionDue && d.Indicator == dueDate.Indicator).FirstOrDefault();
                        if (oldDueDate != null)
                        {
                            //restore remarks
                            dueDate.Remarks = oldDueDate.Remarks;

                            //responseDate is updated
                            //restore dateTaken if old dateTaken is different from old responseDate
                            if (responseDateChanged && actionDue.ResponseDate == null)
                                if (oldDueDate.DateTaken != oldActionDue.ResponseDate)
                                    dueDate.DateTaken = oldDueDate.DateTaken;
                        }
                    }
                    //delete old DueDates
                    _cpiDbContext.GetRepository<GMDueDate>().Delete(oldDueDates);
                }
            }
            else
            {
                //update DueDates when ResponseDate changed
                dueDates = await DueDates.Where(d => d.ActId == actionDue.ActId &&
                                                        ((responseDateChanged && (d.DateTaken == null || d.DateTaken == oldActionDue.ResponseDate)) || (actionDue.CloseDueDates && d.DateTaken == null)) 
                                                        ).ToListAsync();

                if (responseDateChanged && actionDue.ResponseDate == null)
                {
                    //ResponseDate is updated to blank
                    //remove old follow up due date
                    var oldFollowUpAction = await GetFollowUpAction(oldActionDue);
                    if (oldFollowUpAction != null && oldFollowUpAction.ActId == oldActionDue.ActId)
                    {
                        var oldFollowUpDueDate = oldFollowUpAction.DueDates.FirstOrDefault();
                        var oldFollowUps = await DueDates.Where(d => d.ActId == actionDue.ActId && d.ActionDue == oldFollowUpDueDate.ActionDue).ToListAsync();

                        if (oldFollowUps.Any())
                        {
                            _cpiDbContext.GetRepository<GMDueDate>().Delete(oldFollowUps);
                            dueDates = dueDates.Where(d => !oldFollowUps.Any(f => f.ActionDue == d.ActionDue && f.DueDate == d.DueDate)).ToList();
                        }
                    }
                    else if (oldFollowUpAction != null)
                    {
                        var oldFollowUpActionDue = await _cpiDbContext.GetRepository<GMActionDue>().QueryableList
                                  .Where(a => a.MatId == oldFollowUpAction.MatId && a.ActionType == oldFollowUpAction.ActionType && a.BaseDate == oldFollowUpAction.BaseDate)
                                  .FirstOrDefaultAsync();

                        if (oldFollowUpActionDue != null)
                        {
                            var oldFollowUpDueDates = await DueDates.Where(d => d.ActId == oldFollowUpActionDue.ActId).ToListAsync();

                            _cpiDbContext.GetRepository<GMDueDate>().Delete(oldFollowUpDueDates);
                            _cpiDbContext.GetRepository<GMActionDue>().Delete(oldFollowUpActionDue);
                        }
                    }

                    else
                    {
                        var settings = await _settings.GetSetting();
                        var autoFollowUp = settings.IsGenFollowUpOn;
                        if (autoFollowUp)
                        {
                            var autoFollowUpActionDesc = $"{actionDue.ActionType.Substring(0, actionDue.ActionType.Length >= 45 ? 45 : actionDue.ActionType.Length)} Follow Up Date";
                            var existing = dueDates.Where(d => d.ActId == actionDue.ActId && d.ActionDue == autoFollowUpActionDesc).FirstOrDefault();
                            if (existing != null)
                            {
                                _cpiDbContext.GetRepository<GMDueDate>().Delete(existing);
                                dueDates.Remove(existing);
                            }
                        }
                    }
                }

                //update DueDates
                if (dueDates.Any()) {
                    _cpiDbContext.GetRepository<GMDueDate>().Attach(dueDates);
                }
                foreach (var dueDate in dueDates)
                {
                    var oldDateTaken = dueDate.DateTaken;

                    if (actionDue.CloseDueDates)
                    {
                        dueDate.DateTaken = DateTime.Now.Date;
                    }

                    //update DateTaken with ResponseDate if ResponseDate changed
                    //and when DateTaken is blank or DateTaken is the same as old ResponseDate
                    dueDate.DateTaken = responseDateChanged && (dueDate.DateTaken == null || dueDate.DateTaken == oldActionDue.ResponseDate) ? actionDue.ResponseDate : dueDate.DateTaken;
                    dueDate.LastUpdate = actionDue.LastUpdate;
                    dueDate.UpdatedBy = actionDue.UpdatedBy;
                }
            }

            //add recurring due dates
            foreach (var recurringDueDate in recurringDueDates)
            {
                if (!dueDates.Exists(d => d.ActionDue == recurringDueDate.ActionDue && d.DueDate == recurringDueDate.DueDate))
                    dueDates.Add(recurringDueDate);
            }

            //attach new and updated DueDates to actionDue
            if (dueDates.Any()) {
                actionDue.DueDates = dueDates;
            }
            _cpiDbContext.GetRepository<GMActionDue>().Update(actionDue);

            return generateFollowUp;
        }

        /// <summary>
        /// Generates DueDates based on ActionParameters when actionDue is based on ActionType.
        /// Generates DueDate based on actionDue when actionDue is not based on any ActionType.
        /// </summary>
        /// <param name="actionDue">The action due record that the due dates will be based on.</param>
        /// <returns></returns>
        private async Task<List<GMDueDate>> GenerateDueDates(GMActionDue actionDue)
        {
            var dueDates = new List<GMDueDate>();
            var actionType = await GetActionType(actionDue.ActionType);
            var actionParams = new List<GMActionParameter>();

            //actionDue is based on an ActionType
            //get ActionParameters
            if (actionType != null)
                actionParams = await _cpiDbContext.GetReadOnlyRepositoryAsync<GMActionParameter>().QueryableList
                                    .Where(ap => ap.ActionTypeID == actionType.ActionTypeID)
                                    .ToListAsync();

            if (actionParams.Any())
                //generate DueDates based on ActionParameters
                dueDates = actionParams.Select(ap => new GMDueDate()
                {
                    ActId = actionDue.ActId,
                    ActionDue = ap.ActionDue,
                    //DueDate = actionDue.BaseDate.AddDays((double)ap.Dy).AddMonths(ap.Mo).AddYears(ap.Yr),
                    //DueDate = actionDue.BaseDate.AddMonths(ap.Mo).AddYears(ap.Yr).AddDays((double)ap.Dy),

                    //proper leap year handling
                    DueDate = actionDue.BaseDate.AddYears(ap.Yr).AddMonths(ap.Mo).AddDays((double)ap.Dy),

                    DateTaken = actionDue.ResponseDate,
                    //IsVerifyDate = actionDue.VerifyDate,
                    Indicator = ap.Indicator,
                    AttorneyID = actionDue.ResponsibleID,
                    CreatedBy = actionDue.UpdatedBy,
                    DateCreated = actionDue.LastUpdate,
                    UpdatedBy = actionDue.UpdatedBy,
                    LastUpdate = actionDue.LastUpdate
                }).ToList();
            else
                //generate DueDate based on actionDue
                dueDates.Add(new GMDueDate()
                {
                    ActId = actionDue.ActId,
                    ActionDue = actionDue.ActionType,
                    DueDate = actionDue.BaseDate,
                    DateTaken = actionDue.ResponseDate,
                    //IsVerifyDate = actionDue.VerifyDate,
                    Indicator = "Due Date",
                    AttorneyID = actionDue.ResponsibleID,
                    CreatedBy = actionDue.UpdatedBy,
                    DateCreated = actionDue.LastUpdate,
                    UpdatedBy = actionDue.UpdatedBy,
                    LastUpdate = actionDue.LastUpdate
                });

            return dueDates;
        }

        private async Task<GMActionType> GetActionType(string actionType)
        {
            return await _cpiDbContext.GetReadOnlyRepositoryAsync<GMActionType>().QueryableList.FirstOrDefaultAsync(a => a.ActionType == actionType);
        }

        /// <summary>
        /// Generates follow up action.
        /// Generates both ActionDue and DueDates when actionDue is based on ActionType with FollowUpMsg, and FollowUpMsg is based on another ActionType.
        /// Generates DueDate only when actionDue is based on ActionType with no FollowUpMsg, or the FollowUpMsg is not based on any ActionType.
        /// Will not generate anything when actionDue is based on ActionType with FollowUpGen set to Don't Generate.
        /// Will not generate anything when actionDue is not based on any ActionType.
        /// Will not generate anything when follow up ActionDue or DueDate already exists.
        /// </summary>
        /// <param name="actionDue">The action due record that the follow up action will be based on.</param>
        /// <returns></returns>
        private async Task GenerateFollowUpAction(GMActionDue actionDue)
        {
            var settings = await _settings.GetSetting();
            var followUpAction = await GetFollowUpAction(actionDue);
            if (followUpAction != null)
            {
                //if (followUpAction.ActId == actionDue.ActId) //foreign key error
                if (followUpAction.ActionType == actionDue.ActionType)
                {
                    var followUpDueDate = followUpAction.DueDates.FirstOrDefault();

                    //insert one DueDate if DueDate does not exist
                    if (!actionDue.DueDates.Any(d => d.ActionDue == followUpDueDate.ActionDue && d.DueDate == followUpDueDate.DueDate)) {
                        followUpDueDate.ActId = actionDue.ActId;
                        _cpiDbContext.GetRepository<GMDueDate>().Add(followUpDueDate);
                        if (settings.IsWorkflowOn)
                        {
                            var dueDatesFromWorkflow = await _matterService.GenerateDueDateFromActionParameterWorkflow(actionDue, new List<GMDueDate> { followUpDueDate }, GMWorkflowTriggerType.Indicator);
                            if (dueDatesFromWorkflow != null && dueDatesFromWorkflow.Any())
                            {
                                dueDatesFromWorkflow.ForEach(dd => { dd.ActId = actionDue.ActId; dd.DateTaken = null; });
                                _cpiDbContext.GetRepository<GMDueDate>().Add(dueDatesFromWorkflow);
                            }
                        }
                    }
                        
                }
                else
                {
                    //insert new ActionDue with DueDates if ActionDue does not exist
                    var actions = await _cpiDbContext.GetRepository<GMActionDue>().QueryableList
                                    .Where(a => a.MatId == followUpAction.MatId && a.ActionType == followUpAction.ActionType && a.BaseDate == followUpAction.BaseDate)
                                    .ToListAsync();

                    if (!actions.Any() && !(actionDue.ActionType == followUpAction.ActionType && actionDue.BaseDate == followUpAction.BaseDate)) {
                        if (settings.IsWorkflowOn)
                        {
                            var dueDatesFromWorkflow = await _matterService.GenerateDueDateFromActionParameterWorkflow(followUpAction, followUpAction.DueDates, GMWorkflowTriggerType.Indicator);
                            if (dueDatesFromWorkflow != null && dueDatesFromWorkflow.Any())
                            {
                                followUpAction.DueDates.AddRange(dueDatesFromWorkflow);
                            }
                        }
                        _cpiDbContext.GetRepository<GMActionDue>().Add(followUpAction);
                        actionDue.FollowUpAction = followUpAction.ActionType;
                    }
                        
                }
            }

            //office action auto followup is handled above
            //Jeff Nichol's email dated 3/20/2024
            //else if (actionDue.ResponseDate != null)
            //{
            //    var autoFollowUp = settings.IsGenFollowUpOn;
            //    if (autoFollowUp)
            //    {
            //        var termMonth = settings.FollowUpActionTermMon;
            //        var termDay = settings.FollowUpActionTermDay;
            //        var indicator = settings.FollowUpActionIndicator;

            //        if (string.IsNullOrEmpty(indicator))
            //            indicator = "Due Date";

            //        var followUpActionDesc = $"{actionDue.ActionType.Substring(0, actionDue.ActionType.Length >= 45 ? 45 : actionDue.ActionType.Length)} Follow Up Date";

            //        var existing = await _cpiDbContext.GetRepository<GMDueDate>().QueryableList
            //                           .Where(d => d.ActId == actionDue.ActId && d.ActionDue == followUpActionDesc)
            //                           .FirstOrDefaultAsync();
            //        if (existing != null)
            //            _cpiDbContext.GetRepository<GMDueDate>().Delete(existing);

            //        var followUpDueDate = new GMDueDate
            //        {
            //            ActId = actionDue.ActId,
            //            ActionDue = followUpActionDesc,
            //            Indicator = indicator,
            //            DueDate = ((DateTime)actionDue.ResponseDate).AddMonths(termMonth).AddDays(termDay),
            //            DateCreated = DateTime.Now,
            //            LastUpdate = DateTime.Now,
            //            CreatedBy = actionDue.UpdatedBy,
            //            UpdatedBy = actionDue.UpdatedBy
            //        };
            //        //_cpiDbContext.GetRepository<GMDueDate>().Add(followUpDueDate); //foreign key error
            //        actionDue.DueDates.Add(followUpDueDate);
            //    }
            //}
        }

        /// <summary>
        /// Returns follow up ActionDue.
        /// Returns new ActionDue with DueDates when actionDue is based on ActionType with FollowUpMsg, and FollowUpMsg is based on another ActionType.
        /// Returns ActionDue with same ActId with one new DueDate when actionDue is based on ActionType with no FollowUpMsg, or the FollowUpMsg is not based on any ActionType.
        /// Return null when actionDue is not based on any ActionType.
        /// Return null when actionDue is based on ActionType with FollowUpGen set to Don't Generate.
        /// </summary>
        /// <param name="actionDue">The action due record that the follow up action will be based on.</param>
        /// <returns></returns>
        private async Task<GMActionDue> GetFollowUpAction(GMActionDue actionDue)
        {
            GMActionDue followUpAction = null;
            GMActionType actionType = await GetActionType(actionDue.ActionType);

            if (actionType != null && actionType.FollowUpGen != (short)FollowUpOption.DontGenerate)
            {
                if (string.IsNullOrEmpty(actionType.FollowUpIndicator))
                    actionType.FollowUpIndicator = "Due Date";

                //ActionType exists
                //get follow up ActionType based on FollowUpMsg
                GMActionType followUpActionType = null;

                if (!string.IsNullOrEmpty(actionType.FollowUpMsg))
                    followUpActionType = await GetActionType(actionType.FollowUpMsg);

                if (followUpActionType != null)
                {
                    //follow up ActionType exists
                    //create new ActionDue
                    followUpAction = new GMActionDue()
                    {
                        MatId = actionDue.MatId,
                        CaseNumber = actionDue.CaseNumber,
                        SubCase = actionDue.SubCase,
                        ActionType = followUpActionType.ActionType,
                        BaseDate = actionType.FollowUpGen == (short)FollowUpOption.BaseDate ? actionDue.BaseDate : (DateTime)actionDue.ResponseDate,
                        //ResponsibleID = actionDue.ResponsibleID,
                        ResponsibleID = followUpActionType.ResponsibleID > 0 ? followUpActionType.ResponsibleID : actionDue.ResponsibleID,
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now,
                        CreatedBy = actionDue.UpdatedBy,
                        UpdatedBy = actionDue.UpdatedBy
                    };
                    //create new DueDates
                    followUpAction.DueDates = await GenerateDueDates(followUpAction);
                }
                else
                {
                    //adhoc
                    if (!string.IsNullOrEmpty(actionType.FollowUpMsg))
                    {
                        followUpAction = new GMActionDue()
                        {
                            MatId = actionDue.MatId,
                            CaseNumber = actionDue.CaseNumber,
                            SubCase = actionDue.SubCase,
                            ActionType = actionType.FollowUpMsg,
                            BaseDate = actionType.FollowUpGen == (short)FollowUpOption.BaseDate ? actionDue.BaseDate : (DateTime)actionDue.ResponseDate,
                            DateCreated = DateTime.Now,
                            LastUpdate = DateTime.Now,
                            CreatedBy = actionDue.UpdatedBy,
                            UpdatedBy = actionDue.UpdatedBy
                        };
                    }

                    //FollowUpMsg is blank 
                    var followUpDueDate = actionType.FollowUpGen == (short)FollowUpOption.BaseDate ? actionDue.BaseDate : (DateTime)actionDue.ResponseDate;
                    var followUpActionDue = actionType.FollowUpMsg;

                    //generate follow up ActionDue description
                    if (string.IsNullOrEmpty(followUpActionDue))
                    {
                        followUpActionDue = actionDue.ActionType.Length >= 45 ? actionDue.ActionType.Substring(0, 45) : actionDue.ActionType;
                        followUpActionDue = $"{followUpActionDue} Follow Up Date";

                        //do not create new ActionDue
                        //use same actId
                        followUpAction = new GMActionDue()
                        {
                            ActId = actionDue.ActId,
                            ActionType = actionDue.ActionType
                        };
                    }

                    if (followUpAction != null) {
                        //create new follow up DueDate
                        followUpAction.DueDates = new List<GMDueDate>() {
                        new GMDueDate()
                        {
                            ActId = actionDue.ActId,
                            ActionDue = followUpActionDue,
                            DueDate = followUpDueDate.AddMonths(actionType.FollowUpMonth).AddDays(actionType.FollowUpDay),
                            //IsVerifyDate = actionDue.VerifyDate,
                            Indicator = actionType.FollowUpIndicator,
                            AttorneyID = actionDue.ResponsibleID,
                            CreatedBy = actionDue.UpdatedBy,
                            DateCreated = actionDue.LastUpdate,
                            UpdatedBy = actionDue.UpdatedBy,
                            LastUpdate = actionDue.LastUpdate
                        }};
                    }
                    
                }
            }
            //todo: more follow up

            return followUpAction;
        }

        public Task<GMDueDate> GetRecurringDueDate(GMActionDue actionDue, GMDueDate dueDate)
        {
            throw new NotImplementedException();
        }

        private async Task AddCustomDocFolder(GMActionDue actionDue)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                await _cpiDbContext.SaveChangesAsync();

                actionDue.DocFolders.ForEach(f => {
                    f.CreatedBy = actionDue.CreatedBy;
                    f.UpdatedBy = actionDue.UpdatedBy;
                    f.DateCreated = actionDue.DateCreated;
                    f.LastUpdate = actionDue.LastUpdate;
                    f.DataKeyValue = actionDue.ActId;
                    f.FolderId = 0;
                    f.DocDocuments.ForEach(d => {
                        d.DocId = 0;
                        d.FolderId = 0;
                        d.CreatedBy = actionDue.CreatedBy;
                        d.UpdatedBy = actionDue.UpdatedBy;
                        d.DateCreated = actionDue.DateCreated;
                        d.LastUpdate = actionDue.LastUpdate;
                    });

                });
                _cpiDbContext.GetRepository<DocFolder>().Add(actionDue.DocFolders);
                await _cpiDbContext.SaveChangesAsync();
                scope.Complete();
            }
        }

        public async Task RetroGenerateActionDues(ActionDueRetroParam criteria)
        {
            var actionType = await _cpiDbContext.GetReadOnlyRepositoryAsync<GMActionType>().QueryableList.Where(at => at.ActionTypeID == criteria.ActionTypeID).AsNoTracking().FirstOrDefaultAsync();
            if (actionType == null) return;

            var actionParam = await _cpiDbContext.GetReadOnlyRepositoryAsync<GMActionParameter>().QueryableList
                                            .Where(ap => ap.ActionTypeID == actionType.ActionTypeID && ap.ActionDue == criteria.ActionDue)
                                            .FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(criteria.ActionDue) && actionParam == null) return;

            var userName = _user.GetUserName();
            var today = DateTime.Now;
            
            //Generate new action records if generating from ActionType level
            if (criteria.ActParamId <= 0)
            {
                var matters = await _matterService.QueryableList
                                    .Where(d => (!criteria.ActiveOnly || (d.GMMatterStatus != null && d.GMMatterStatus.ActiveSwitch))
                                        && (string.IsNullOrEmpty(criteria.Country) || (d.Countries != null && d.Countries.Any(c => c.Country == criteria.Country)))
                                        && (criteria.CaseTypes == null || (d.MatterType != null && criteria.CaseTypes.Contains(d.MatterType)))
                                        && (criteria.EffectiveOpenDateFrom == null || d.EffectiveOpenDate >= criteria.EffectiveOpenDateFrom)
                                        && (criteria.EffectiveOpenDateTo == null || d.EffectiveOpenDate <= criteria.EffectiveOpenDateTo)
                                        && (criteria.TerminationEndDateFrom == null || d.TerminationEndDate >= criteria.TerminationEndDateFrom)
                                        && (criteria.TerminationEndDateTo == null || d.TerminationEndDate <= criteria.TerminationEndDateTo)                                    
                                    )
                                    .ToListAsync();

                var newActionDues = new List<GMActionDue>();

                foreach (var matter in matters)
                {
                    var dupActionDue = await QueryableList.Where(d => d.MatId == matter.MatId && d.BaseDate.Date == criteria.BaseDate.Date && d.ActionType == actionType.ActionType)
                                            .Include(d => d.DueDates).AsNoTracking().FirstOrDefaultAsync();

                    if (dupActionDue == null)
                    {
                        GMActionDue actionDue = new GMActionDue()
                        {
                            MatId = matter.MatId,
                            CaseNumber = matter.CaseNumber,                        
                            SubCase = matter.SubCase,
                            ActionType = actionType.ActionType,
                            BaseDate = criteria.BaseDate,
                            ResponsibleID = actionType.ResponsibleID,
                            CreatedBy = userName,
                            UpdatedBy = userName,
                            DateCreated = today,
                            LastUpdate = today
                        };

                        //Generate all action parameters
                        if (string.IsNullOrEmpty(criteria.ActionDue))
                        {
                            actionDue.DueDates = await GenerateDueDates(actionDue);
                        }
                        ////Generate specific ActionDue/ActionParameter
                        //else if (!string.IsNullOrEmpty(criteria.ActionDue) && actionParam != null)
                        //{
                        //    actionDue.DueDates = new List<GMDueDate>()
                        //    {
                        //        new GMDueDate()
                        //        {
                        //            ActionDue = actionParam.ActionDue,
                        //            //proper leap year handling
                        //            DueDate = actionDue.BaseDate.AddYears(actionParam.Yr).AddMonths(actionParam.Mo).AddDays((double)actionParam.Dy),
                        //            DateTaken = actionDue.ResponseDate,                               
                        //            Indicator = actionParam.Indicator,
                        //            AttorneyID = actionDue.ResponsibleID,
                        //            CreatedBy = actionDue.UpdatedBy,
                        //            DateCreated = actionDue.LastUpdate,
                        //            UpdatedBy = actionDue.UpdatedBy,
                        //            LastUpdate = actionDue.LastUpdate
                        //        }
                        //    };
                        //}

                        if (criteria.DueDateCutOff != null && actionDue.DueDates != null) actionDue.DueDates.RemoveAll(d => d.DueDate.Date <= criteria.DueDateCutOff.Value.Date);

                        if (actionDue != null && actionDue.DueDates != null) newActionDues.Add(actionDue);
                    }
                    else
                    {
                        //Generate all ActionDues/ActionParameters that are not in the existing ActionDue record yet
                        if (string.IsNullOrEmpty(criteria.ActionDue))
                        {
                            var actionParams = await _cpiDbContext.GetReadOnlyRepositoryAsync<GMActionParameter>().QueryableList
                                                .Where(ap => ap.ActionTypeID == actionType.ActionTypeID)
                                                .Select(d => new
                                                {
                                                    d.ActionDue,
                                                    DueDate = dupActionDue.BaseDate.AddYears(d.Yr).AddMonths(d.Mo).AddDays((double)d.Dy),
                                                    d.Indicator
                                                })
                                                .ToListAsync();
                            var newDueDates = actionParams.Where(d => dupActionDue.DueDates == null || !dupActionDue.DueDates.Any()
                                                || !dupActionDue.DueDates.Any(a => a.ActionDue == d.ActionDue && a.DueDate == d.DueDate))
                                                .Select(d => new GMDueDate()
                                                {
                                                    ActId = dupActionDue.ActId,
                                                    ActionDue = d.ActionDue,
                                                    DueDate = d.DueDate,
                                                    Indicator = d.Indicator,
                                                    AttorneyID = dupActionDue.ResponsibleID,
                                                    CreatedBy = userName,
                                                    UpdatedBy = userName,
                                                    DateCreated = today,
                                                    LastUpdate = today
                                                }).ToList();

                            if (criteria.DueDateCutOff != null && newDueDates != null) newDueDates.RemoveAll(d => d.DueDate.Date <= criteria.DueDateCutOff.Value.Date);

                            if (newDueDates != null && newDueDates.Count > 0)
                            {
                                _cpiDbContext.GetRepository<GMDueDate>().Add(newDueDates);
                                await _cpiDbContext.SaveChangesAsync();
                            }
                        }
                        ////Generate specific ActionDue/ActionParameter that is not in the existing ActionDue record yet
                        //else if (!string.IsNullOrEmpty(criteria.ActionDue) && actionParam != null)
                        //{
                        //    var newDueDate = new GMDueDate()
                        //    {
                        //        ActId = dupActionDue.ActId,
                        //        ActionDue = actionParam.ActionDue,
                        //        DueDate = dupActionDue.BaseDate.AddYears(actionParam.Yr).AddMonths(actionParam.Mo).AddDays((double)actionParam.Dy),
                        //        Indicator = actionParam.Indicator,
                        //        AttorneyID = dupActionDue.ResponsibleID,
                        //        CreatedBy = userName,
                        //        UpdatedBy = userName,
                        //        DateCreated = today,
                        //        LastUpdate = today
                        //    };

                        //    if ((dupActionDue.DueDates == null || !dupActionDue.DueDates.Any(d => d.ActionDue == newDueDate.ActionDue && d.DueDate == newDueDate.DueDate)) && (criteria.DueDateCutOff == null || newDueDate.DueDate.Date > criteria.DueDateCutOff.Value.Date))
                        //    {
                        //        _cpiDbContext.GetRepository<GMDueDate>().Add(newDueDate);
                        //        await _cpiDbContext.SaveChangesAsync();
                        //    }
                        //}
                    }
                }

                if (newActionDues.Any())
                {
                    _cpiDbContext.GetRepository<GMActionDue>().Add(newActionDues);
                    await _cpiDbContext.SaveChangesAsync();
                }
            }
            //Generate new due dates for all existing action records
            else if (criteria.ActParamId > 0 && !string.IsNullOrEmpty(criteria.ActionDue) && actionParam != null)
            {                
                var existingActions = await QueryableList
                    .Where(ad => ad.GMMatter != null 
                        && (!criteria.ActiveOnly || (ad.GMMatter.GMMatterStatus != null && ad.GMMatter.GMMatterStatus.ActiveSwitch))
                        && (string.IsNullOrEmpty(criteria.Country) || (ad.GMMatter.Countries != null && ad.GMMatter.Countries.Any(c => c.Country == criteria.Country)))
                        && (criteria.CaseTypes == null || (ad.GMMatter.MatterType != null && criteria.CaseTypes.Contains(ad.GMMatter.MatterType)))
                        && ad.ActionType == actionType.ActionType
                        && (ad.DueDates == null 
                                || !ad.DueDates.Any(dd => dd.ActionDue == actionParam.ActionDue 
                                    && dd.DueDate == ad.BaseDate.AddYears(actionParam.Yr).AddMonths(actionParam.Mo).AddDays((double)actionParam.Dy)) 
                            )
                        && (criteria.DueDateCutOff == null || ad.BaseDate.AddYears(actionParam.Yr).AddMonths(actionParam.Mo).AddDays((double)actionParam.Dy).Date > criteria.DueDateCutOff.Value.Date)
                    )
                    //.Include(d => d.DueDates)
                    .AsNoTracking().ToListAsync();

                var newDueDates = new List<GMDueDate>();

                foreach (var existingAct in existingActions) {
                    var newDueDate = new GMDueDate()
                    {
                        ActId = existingAct.ActId,
                        ActionDue = actionParam.ActionDue,
                        DueDate = existingAct.BaseDate.AddYears(actionParam.Yr).AddMonths(actionParam.Mo).AddDays((double)actionParam.Dy),
                        Indicator = actionParam.Indicator,
                        AttorneyID = existingAct.ResponsibleID,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = today,
                        LastUpdate = today
                    };

                    newDueDates.Add(newDueDate);
                }

                if (newDueDates != null && newDueDates.Count > 0)
                {
                    _cpiDbContext.GetRepository<GMDueDate>().Add(newDueDates);
                    await _cpiDbContext.SaveChangesAsync();
                }
            }

        }

        public async Task UpdateDeDocket(GMActionDue actionDue)
        {
            var deDocketFields = await _systemSettingManager.GetSystemSetting<DeDocketFields>();
            var updated = await GetByIdAsync(actionDue.ActId);
            Guard.Against.NoRecordPermission(updated != null);

            await ValidatePermission(updated, CPiPermissions.DeDocketer);

            if (updated != null && deDocketFields.GeneralMatterActionDue != null)
            {
                if (deDocketFields.GeneralMatterActionDue.Remarks)
                    updated.Remarks = actionDue.Remarks;

                updated.LastUpdate = actionDue.LastUpdate;
                updated.UpdatedBy = actionDue.UpdatedBy;
                updated.tStamp = actionDue.tStamp;

                _cpiDbContext.GetRepository<GMActionDue>().Update(updated);
                await _cpiDbContext.SaveChangesAsync();
            }
            else
                Guard.Against.UnAuthorizedAccess(false);
        }

        public async Task UpdateCheckDocket(GMActionDue actionDue)
        {
            var updated = await GetByIdAsync(actionDue.ActId);
            if (updated != null)
            {
                updated.tStamp = actionDue.tStamp;
                _cpiDbContext.GetRepository<GMActionDue>().Attach(updated);
                updated.CheckDocket = actionDue.CheckDocket;               
                updated.UpdatedBy = actionDue.UpdatedBy;
                updated.LastUpdate = actionDue.LastUpdate;
                await _cpiDbContext.SaveChangesAsync();
                _cpiDbContext.Detach(updated);
            }
        }
    }
}
