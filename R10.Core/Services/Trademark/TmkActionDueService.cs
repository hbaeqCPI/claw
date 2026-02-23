using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using BasedOnOption = R10.Core.Entities.Trademark.BasedOnOption;
using RecurringOption = R10.Core.Entities.Trademark.RecurringOption;

namespace R10.Core.Services.Trademark
{
    public class TmkActionDueService : EntityService<TmkActionDue>, IActionDueDeDocketService<TmkActionDue, TmkDueDate>
    {
        private readonly ITmkTrademarkService _trademarkService;
        private readonly ISystemSettings<TmkSetting> _settings;
        private readonly ICPiUserSettingManager _userSettingManager;
        private readonly ICPiSystemSettingManager _systemSettingManager;

        public TmkActionDueService(
            ICPiDbContext cpiDbContext,
            ITmkTrademarkService trademarkService,
            ISystemSettings<TmkSetting> settings,
            ClaimsPrincipal user,
            ICPiUserSettingManager userSettingManager,
            ICPiSystemSettingManager systemSettingManager) : base(cpiDbContext, user)
        {
            _trademarkService = trademarkService;
            _settings = settings;
            _userSettingManager = userSettingManager;
            _systemSettingManager = systemSettingManager;
        }

        public override IQueryable<TmkActionDue> QueryableList
        {
            get
            {
                var actionsDue = base.QueryableList;

                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Trademark))
                    actionsDue = actionsDue.Where(a => _trademarkService.TmkTrademarks.Any(tm => tm.TmkId == a.TmkId));

                return actionsDue;
            }
        }

        public override async Task<TmkActionDue> GetByIdAsync(int actId)
        {
            return await QueryableList.SingleOrDefaultAsync(a => a.ActId == actId);
        }

        public async Task<List<TmkActionDue>> GetByParentIdAsync(int parentId)
        {
            return await QueryableList.Where(a => a.TmkId == parentId).ToListAsync();
        }

        public override async Task Add(TmkActionDue actionDue)
        {
            actionDue.ComputerGenerated = false;
            actionDue.IsElectronic = false;

            await ValidatePermission(actionDue, CPiPermissions.FullModify);
            await ValidateResponsibleAttorney(actionDue.ResponsibleID ?? 0);

            var tmk = await ValidateTrademark(actionDue);

            //Web API can add due dates
            if (actionDue.DueDates == null)
                actionDue.DueDates = await GenerateDueDates(actionDue);

            var settings = await _settings.GetSetting();
            if (actionDue.DueDates.Any() && settings.IsWorkflowOn)
            {
                var dueDatesFromWorkflow = await _trademarkService.GenerateDueDateFromActionParameterWorkflow(actionDue, actionDue.DueDates, TmkWorkflowTriggerType.Indicator);
                if (dueDatesFromWorkflow != null && dueDatesFromWorkflow.Any())
                {
                    actionDue.DueDates.AddRange(dueDatesFromWorkflow);
                }
            }
            _cpiDbContext.GetRepository<TmkActionDue>().Add(actionDue);

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
            _cpiDbContext.Detach(tmk);
        }

        public override async Task Update(TmkActionDue actionDue)
        {
            await ValidatePermission(actionDue, CPiPermissions.FullModify);
            await ValidateResponsibleAttorney(actionDue.ResponsibleID ?? 0);

            await ValidateComputerGenerated(actionDue);
            var tmk = await ValidateTrademark(actionDue);

            var generateFollowUp = await UpdateDueDates(actionDue);
            if (generateFollowUp)
                await GenerateFollowUpAction(actionDue);

            bool concurrencyFailure;
            do
            {
                concurrencyFailure = false;
                try
                {
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
                    _cpiDbContext.Detach(tmk);
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
            
        }

        public override async Task UpdateRemarks(TmkActionDue actionDue)
        {
            var updated = await GetByIdAsync(actionDue.ActId);
            Guard.Against.NoRecordPermission(updated != null);

            await ValidatePermission(updated, CPiPermissions.RemarksOnly);

            updated.tStamp = actionDue.tStamp;

            _cpiDbContext.GetRepository<TmkActionDue>().Attach(updated);
            updated.Remarks = actionDue.Remarks;
            updated.UpdatedBy = actionDue.UpdatedBy;
            updated.LastUpdate = actionDue.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task UpdateResponseDate(TmkActionDue actionDue)
        {
            var updated = await GetByIdAsync(actionDue.ActId);
            Guard.Against.NoRecordPermission(updated != null);

            await ValidatePermission(updated, CPiPermissions.RemarksOnly);

            updated.tStamp = actionDue.tStamp;

            _cpiDbContext.GetRepository<TmkActionDue>().Attach(updated);
            updated.ResponseDate = actionDue.ResponseDate;
            updated.UpdatedBy = actionDue.UpdatedBy;
            updated.LastUpdate = actionDue.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();
        }

        public override async Task Delete(TmkActionDue actionDue)
        {
            await ValidatePermission(actionDue, CPiPermissions.CanDelete);
            await ValidateResponsibleAttorney(actionDue.ResponsibleID ?? 0);

            await base.Delete(actionDue);
        }

        private async Task ValidatePermission(TmkActionDue actionDue, List<string> roles)
        {
            if ((await _userSettingManager.GetUserSetting<UserAccountSettings>(_user.GetUserIdentifier())).RestrictAdhocActions &&
                !string.IsNullOrEmpty(actionDue.ActionType) &&
                (actionDue.ActId == 0 ||
                !actionDue.ActionType.Equals((await GetByIdAsync(actionDue.ActId)).ActionType, StringComparison.InvariantCultureIgnoreCase)))
                Guard.Against.ValueNotAllowed(await _cpiDbContext
                                                        .GetReadOnlyRepositoryAsync<TmkActionType>().QueryableList
                                                        .AnyAsync(at => (at.Country == actionDue.Country || (at.Country ?? "") == "") &&
                                                                         at.ActionType == actionDue.ActionType), "Action Type");

            var actId = actionDue.ActId;
            var respOfc = "";
            if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Trademark))
            {
                var item = new KeyValuePair<int, string>();
                if (actId > 0)
                {
                    item = (await QueryableList.Where(a => a.ActId == actId)
                                    .Select(a => new { a.TmkId, a.TmkTrademark.RespOffice })
                                    .ToDictionaryAsync(a => a.TmkId, c => c.RespOffice)).FirstOrDefault();
                }
                else
                {
                    actionDue.SubCase = actionDue.SubCase ?? "";
                    item = (await _trademarkService.TmkTrademarks
                                    .Where(t => t.CaseNumber == actionDue.CaseNumber && t.Country == actionDue.Country && t.SubCase == actionDue.SubCase)
                                    .Select(t => new { t.TmkId, t.RespOffice })
                                    .ToDictionaryAsync(t => t.TmkId, c => c.RespOffice)).FirstOrDefault();
                }

                Guard.Against.NoRecordPermission(item.Key > 0);

                respOfc = item.Value;
            }

            var settings = await _settings.GetSetting();
            if (settings.IsSoftDocketOn && actionDue.DueDates != null && actionDue.DueDates.Any(d => !string.IsNullOrEmpty(d.Indicator) && d.Indicator.ToLower() == "soft docket"))
                Guard.Against.NoRecordPermission(_user.IsSoftDocketUser() || _user.IsInRoles(SystemType.Trademark, CPiPermissions.SoftDocket));
            else
                Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Trademark, roles, respOfc));
        }

        private async Task ValidateResponsibleAttorney(int responsibleId)
        {
            if (responsibleId > 0 && _user.GetEntityFilterType() == CPiEntityType.Attorney)
                Guard.Against.ValueNotAllowed(await base.EntityFilterAllowed(responsibleId), "Responsible Attorney");
        }

        private async Task ValidateComputerGenerated(TmkActionDue actionDue)
        {
            if (actionDue.ComputerGenerated)
            {
                var notAllowed = await QueryableList.AnyAsync(a => a.ActId == actionDue.ActId &&
                                            (
                                                a.CaseNumber != actionDue.CaseNumber ||
                                                a.Country != actionDue.Country ||
                                                a.SubCase != (actionDue.SubCase ?? "") ||
                                                a.ActionType != actionDue.ActionType ||
                                                a.BaseDate != actionDue.BaseDate
                                            ));
                Guard.Against.NoRecordPermission(!notAllowed);
            }
        }

        private async Task<TmkTrademark> ValidateTrademark(TmkActionDue actionDue)
        {
            var settings = await _settings.GetSetting();

            actionDue.SubCase = actionDue.SubCase ?? "";

            var trademark = await _trademarkService.TmkTrademarks
                .Where(t =>
                    t.CaseNumber == actionDue.CaseNumber &&
                    t.Country == actionDue.Country &&
                    t.SubCase == actionDue.SubCase)
                .SingleOrDefaultAsync();

            var caseNumberLabel = settings.LabelCaseNumber;
            Guard.Against.ValueNotAllowed(trademark?.TmkId > 0, $"{caseNumberLabel}/Country/Sub Case");

            if (_user.IsRespOfficeOn(SystemType.Trademark))
                Guard.Against.ValueNotAllowed(await ValidateRespOffice(SystemType.Trademark, trademark.RespOffice), $"{caseNumberLabel}/Country/Sub Case");

            actionDue.TmkId = trademark.TmkId;

            _cpiDbContext.GetRepository<TmkTrademark>().Attach(trademark);
            trademark.LastUpdate = actionDue.LastUpdate;
            trademark.UpdatedBy = actionDue.UpdatedBy;

            return trademark;
        }

        public async Task<bool> CanModifyAttorney(int responsibleId)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Attorney && responsibleId > 0)
                return await EntityFilterAllowed(responsibleId);
            else
                return true;
        }

        private IQueryable<TmkDueDate> DueDates => _cpiDbContext.GetRepository<TmkDueDate>().QueryableList;

        /// <summary>
        /// Update or generate due dates.
        /// Returns true if follow up actions need to be generated.
        /// </summary>
        /// <param name="actionDue"></param>
        /// <returns></returns>
        private async Task<bool> UpdateDueDates(TmkActionDue actionDue)
        {
            var dueDates = new List<TmkDueDate>();
            var recurringDueDates = new List<TmkDueDate>();

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

                            //generate recurring due date 
                            //if action is country law generated and date taken is updated
                            if (actionDue.ComputerGenerated && (dueDate.DateTaken != null && dueDate.DateTaken != oldDueDate.DateTaken))
                            {
                                var generateRecurring = true;

                                if (responseDateChanged)
                                {
                                    generateRecurring = (await _settings.GetSetting()).CanResponseDateTriggerRecurring;
                                }
                                if (generateRecurring) {
                                    var recurringDueDate = await GetRecurringDueDate(actionDue, dueDate);
                                    if (recurringDueDate != null)
                                        recurringDueDates.Add(recurringDueDate);
                                }
                            }
                        }
                    }
                    //delete old DueDates
                    _cpiDbContext.GetRepository<TmkDueDate>().Delete(oldDueDates);
                }
            }
            else
            {
                //update DueDates when VerifyDate or ResponseDate changed
                var verifyDateChanged = oldActionDue.VerifyDate != actionDue.VerifyDate;
                dueDates = await DueDates.Where(d => d.ActId == actionDue.ActId &&
                                                        ((responseDateChanged && (d.DateTaken == null || d.DateTaken == oldActionDue.ResponseDate)) ||
                                                            verifyDateChanged || (actionDue.CloseDueDates && d.DateTaken == null)
                                                        )).ToListAsync();

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
                            _cpiDbContext.GetRepository<TmkDueDate>().Delete(oldFollowUps);
                            dueDates = dueDates.Where(d => !oldFollowUps.Any(f => f.ActionDue == d.ActionDue && f.DueDate == d.DueDate)).ToList();
                        }
                    }
                    else if (oldFollowUpAction != null)
                    {
                        var oldFollowUpActionDue = await _cpiDbContext.GetRepository<TmkActionDue>().QueryableList
                                  .Where(a => a.TmkId == oldFollowUpAction.TmkId && a.ActionType == oldFollowUpAction.ActionType && a.BaseDate == oldFollowUpAction.BaseDate)
                                  .FirstOrDefaultAsync();

                        if (oldFollowUpActionDue != null)
                        {
                            var oldFollowUpDueDates = await DueDates.Where(d => d.ActId == oldFollowUpActionDue.ActId).ToListAsync();

                            _cpiDbContext.GetRepository<TmkDueDate>().Delete(oldFollowUpDueDates);
                            _cpiDbContext.GetRepository<TmkActionDue>().Delete(oldFollowUpActionDue);
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
                                _cpiDbContext.GetRepository<TmkDueDate>().Delete(existing);
                                dueDates.Remove(existing);
                            }
                        }
                    }
                }


                //update DueDates
                if (dueDates.Any())
                {
                    _cpiDbContext.GetRepository<TmkDueDate>().Attach(dueDates);
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

                    //generate recurring due date 
                    //if action is country law generated and date taken is updated
                    if (actionDue.ComputerGenerated && (dueDate.DateTaken != null && dueDate.DateTaken != oldDateTaken))
                    {
                        var generateRecurring = true;

                        if (responseDateChanged)
                        {
                            generateRecurring = (await _settings.GetSetting()).CanResponseDateTriggerRecurring;
                        }
                        if (generateRecurring)
                        {
                            var recurringDueDate = await GetRecurringDueDate(actionDue, dueDate);
                            if (recurringDueDate != null)
                                recurringDueDates.Add(recurringDueDate);
                        }
                    }
                }
            }

            //add recurring due dates
            foreach (var recurringDueDate in recurringDueDates)
            {
                if (!dueDates.Exists(d => d.ActionDue == recurringDueDate.ActionDue && d.DueDate == recurringDueDate.DueDate))
                    dueDates.Add(recurringDueDate);
            }

            //attach new and updated DueDates to actionDue
            if (dueDates.Any())
            {
                actionDue.DueDates = dueDates;
            }
            _cpiDbContext.GetRepository<TmkActionDue>().Update(actionDue);

            return generateFollowUp;
        }

        /// <summary>
        /// Generates DueDates based on ActionParameters when actionDue is based on ActionType.
        /// Generates DueDate based on actionDue when actionDue is not based on any ActionType.
        /// </summary>
        /// <param name="actionDue">The action due record that the due dates will be based on.</param>
        /// <returns></returns>
        private async Task<List<TmkDueDate>> GenerateDueDates(TmkActionDue actionDue)
        {
            var dueDates = new List<TmkDueDate>();
            var actionType = await GetActionType(actionDue.ActionType, actionDue.Country);
            var actionParams = new List<TmkActionParameter>();

            //actionDue is based on an ActionType
            //get ActionParameters
            if (actionType != null)
                actionParams = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmkActionParameter>().QueryableList
                                    .Where(ap => ap.ActionTypeID == actionType.ActionTypeID)
                                    .ToListAsync();

            if (actionParams.Any())
                //generate DueDates based on ActionParameters
                dueDates = actionParams.Select(ap => new TmkDueDate()
                {
                    ActId = actionDue.ActId,
                    ActionDue = ap.ActionDue,
                    //DueDate = actionDue.BaseDate.AddDays((double)ap.Dy).AddMonths(ap.Mo).AddYears(ap.Yr),
                    //DueDate = actionDue.BaseDate.AddMonths(ap.Mo).AddYears(ap.Yr).AddDays((double)ap.Dy),

                    //proper leap year handling
                    DueDate = actionDue.BaseDate.AddYears(ap.Yr).AddMonths(ap.Mo).AddDays((double)ap.Dy),
                    DateTaken = actionDue.ResponseDate,
                    Indicator = ap.Indicator,
                    AttorneyID = actionDue.ResponsibleID,
                    CreatedBy = actionDue.UpdatedBy,
                    DateCreated = actionDue.LastUpdate,
                    UpdatedBy = actionDue.UpdatedBy,
                    LastUpdate = actionDue.LastUpdate
                }).ToList();
            else
                //generate DueDate based on actionDue
                dueDates.Add(new TmkDueDate()
                {
                    ActId = actionDue.ActId,
                    ActionDue = actionDue.ActionType,
                    DueDate = actionDue.BaseDate,
                    DateTaken = actionDue.ResponseDate,
                    Indicator = "Due Date",
                    AttorneyID = actionDue.ResponsibleID,
                    CreatedBy = actionDue.UpdatedBy,
                    DateCreated = actionDue.LastUpdate,
                    UpdatedBy = actionDue.UpdatedBy,
                    LastUpdate = actionDue.LastUpdate
                });

            return dueDates;
        }

        /// <summary>
        /// Returns TmkActionType based on ActionType and Country.
        /// Retrieves ActionType with matching Country first then ActionType with blank Country.
        /// </summary>
        /// <param name="actionType">The action type to search.</param>
        /// <param name="country">The country to search</param>
        /// <returns></returns>
        private async Task<TmkActionType> GetActionType(string actionType, string country)
        {
            //exlude actiontypes used as country law followup
            var actionTypes = _cpiDbContext.GetReadOnlyRepositoryAsync<TmkActionType>().QueryableList.Where(a => (a.CDueId ?? 0) == 0);

            //get actiontype with matching country first
            var hasCountry = actionTypes.Where(a => a.ActionType == actionType && a.Country == country);

            //exlude actiontypes without country if same actiontype with country exists 
            var noCountry = actionTypes.Where(a => a.ActionType == actionType && string.IsNullOrEmpty(a.Country) && !hasCountry.Any(c => c.ActionType == a.ActionType));

            return await hasCountry.Union(noCountry).FirstOrDefaultAsync();
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
        private async Task GenerateFollowUpAction(TmkActionDue actionDue)
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
                        _cpiDbContext.GetRepository<TmkDueDate>().Add(followUpDueDate);

                        if (settings.IsWorkflowOn)
                        {
                            var dueDatesFromWorkflow = await _trademarkService.GenerateDueDateFromActionParameterWorkflow(actionDue, new List<TmkDueDate> { followUpDueDate }, TmkWorkflowTriggerType.Indicator);
                            if (dueDatesFromWorkflow != null && dueDatesFromWorkflow.Any())
                            {
                                dueDatesFromWorkflow.ForEach(dd => { dd.ActId = actionDue.ActId; dd.DateTaken = null; });
                                _cpiDbContext.GetRepository<TmkDueDate>().Add(dueDatesFromWorkflow);
                            }
                        }
                    }
                        
                }
                else
                {
                    //insert new ActionDue with DueDates if ActionDue does not exist
                    var actions = await _cpiDbContext.GetRepository<TmkActionDue>().QueryableList
                                    .Where(a => a.TmkId == followUpAction.TmkId && a.ActionType == followUpAction.ActionType && a.BaseDate == followUpAction.BaseDate)
                                    .ToListAsync();

                    if (!actions.Any() && !(actionDue.ActionType == followUpAction.ActionType && actionDue.BaseDate == followUpAction.BaseDate)) {
                        if (settings.IsWorkflowOn)
                        {
                            var dueDatesFromWorkflow = await _trademarkService.GenerateDueDateFromActionParameterWorkflow(followUpAction, followUpAction.DueDates, TmkWorkflowTriggerType.Indicator);
                            if (dueDatesFromWorkflow != null && dueDatesFromWorkflow.Any())
                            {
                                followUpAction.DueDates.AddRange(dueDatesFromWorkflow);
                            }
                        }
                        _cpiDbContext.GetRepository<TmkActionDue>().Add(followUpAction);
                        actionDue.FollowUpAction = followUpAction.ActionType;
                    }
                        
                }
            }
            //office action auto followup is handled above
            //Jeff Nichol's email dated 3/20/2024
            else if (actionDue.ResponseDate != null && actionDue.ComputerGenerated)
            {
                var autoFollowUp = settings.IsGenFollowUpOn;
                if (autoFollowUp)
                {
                    var termMonth = settings.FollowUpActionTermMon;
                    var termDay = settings.FollowUpActionTermDay;
                    var indicator = settings.FollowUpActionIndicator;

                    if (string.IsNullOrEmpty(indicator))
                        indicator = "Due Date";

                    var followUpActionDesc = $"{actionDue.ActionType.Substring(0, actionDue.ActionType.Length >= 45 ? 45 : actionDue.ActionType.Length)} Follow Up Date";

                    var existing = await _cpiDbContext.GetRepository<TmkDueDate>().QueryableList
                                       .Where(d => d.ActId == actionDue.ActId && d.ActionDue == followUpActionDesc)
                                       .FirstOrDefaultAsync();
                    if (existing != null)
                        _cpiDbContext.GetRepository<TmkDueDate>().Delete(existing);

                    var followUpDueDate = new TmkDueDate
                    {
                        ActId = actionDue.ActId,
                        ActionDue = followUpActionDesc,
                        Indicator = indicator,
                        DueDate = ((DateTime)actionDue.ResponseDate).AddMonths(termMonth).AddDays(termDay),
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now,
                        CreatedBy = actionDue.UpdatedBy,
                        UpdatedBy = actionDue.UpdatedBy
                    };
                    
                    actionDue.DueDates.Add(followUpDueDate);
                    if (settings.IsWorkflowOn)
                    {
                        var dueDatesFromWorkflow = await _trademarkService.GenerateDueDateFromActionParameterWorkflow(actionDue, new List<TmkDueDate> { followUpDueDate }, TmkWorkflowTriggerType.Indicator);
                        if (dueDatesFromWorkflow != null && dueDatesFromWorkflow.Any())
                        {
                            dueDatesFromWorkflow.ForEach(dd => { dd.ActId = actionDue.ActId; dd.DateTaken = null; });
                            _cpiDbContext.GetRepository<TmkDueDate>().Add(dueDatesFromWorkflow);
                        }
                    }
                }
            }
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
        private async Task<TmkActionDue> GetFollowUpAction(TmkActionDue actionDue)
        {
            TmkActionDue followUpAction = null;
            TmkActionType actionType = null;

            if (actionDue.ComputerGenerated)
            {
                var trademark = _cpiDbContext.GetReadOnlyRepositoryAsync<TmkTrademark>().QueryableList.Where(t => t.TmkId == actionDue.TmkId);
                var countryDue = _cpiDbContext.GetReadOnlyRepositoryAsync<TmkCountryDue>().QueryableList
                                            .Where(cd =>
                                                    trademark.Any(t => t.Country == cd.Country && t.CaseType == cd.CaseType) &&
                                                    cd.ActionType == actionDue.ActionType
                                                    //&& cd.ActionDue == actionDue.ActionType //some are not equal (Renewal, First Renewal)
                                                    );
                //get ActionType using CDueId
                //todo: check effective period?
                actionType = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmkActionType>().QueryableList
                                            .Where(at => countryDue.Any(cd => cd.CDueId == at.CDueId))
                                            .FirstOrDefaultAsync();
                if (actionType != null)
                {
                    var sourceActionType = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmkActionType>().QueryableList
                                            .Where(at => (at.CDueId == 0 || at.CDueId==null) && at.ActionType == actionType.ActionType && (at.Country == actionType.Country || string.IsNullOrEmpty(at.Country)))
                                            .OrderByDescending(at => at.Country).FirstOrDefaultAsync();
                    if (sourceActionType != null)
                    {
                        actionType.IsOfficeAction = sourceActionType.IsOfficeAction;
                        actionType.ResponsibleID = sourceActionType.ResponsibleID;
                    }
                }

            }
            else
                //get ActionType using ActionType and country
                actionType = await GetActionType(actionDue.ActionType, actionDue.Country);

            if (actionType != null  && actionType.FollowUpGen != (short)FollowUpOption.DontGenerate)
            {
                if (string.IsNullOrEmpty(actionType.FollowUpIndicator))
                    actionType.FollowUpIndicator = "Due Date";

                //ActionType exists
                //get follow up ActionType based on FollowUpMsg
                TmkActionType followUpActionType = null;

                if (actionDue.ComputerGenerated)
                    followUpActionType = actionType;

                else if (!string.IsNullOrEmpty(actionType.FollowUpMsg))
                    followUpActionType = await GetActionType(actionType.FollowUpMsg, actionDue.Country);

                if (followUpActionType != null)
                {
                    //follow up ActionType exists
                    //create new ActionDue
                    followUpAction = new TmkActionDue()
                    {
                        TmkId = actionDue.TmkId,
                        CaseNumber = actionDue.CaseNumber,
                        Country = actionDue.Country,
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
                        followUpAction = new TmkActionDue()
                        {
                            TmkId = actionDue.TmkId,
                            CaseNumber = actionDue.CaseNumber,
                            Country = actionDue.Country,
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
                        followUpAction = new TmkActionDue()
                        {
                            ActId = actionDue.ActId,
                            ActionType = actionDue.ActionType
                        };

                    }

                    if (followUpAction != null) {
                        //create new follow up DueDate
                        followUpAction.DueDates = new List<TmkDueDate>() {
                        new TmkDueDate()
                        {
                            ActId = actionDue.ActId,
                            ActionDue = followUpActionDue,
                            DueDate = followUpDueDate.AddMonths(actionType.FollowUpMonth).AddDays(actionType.FollowUpDay),
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

        public async Task<TmkDueDate> GetRecurringDueDate(TmkActionDue actionDue, TmkDueDate dueDate)
        {
            if (actionDue.ComputerGenerated && dueDate.DateTaken != null)
            {
                var trademark = _cpiDbContext.GetReadOnlyRepositoryAsync<TmkTrademark>().QueryableList.Where(t => t.TmkId == actionDue.TmkId);
                var countryDue = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmkCountryDue>().QueryableList
                                            .Where(cd =>
                                                    trademark.Any(t => t.Country == cd.Country && t.CaseType == cd.CaseType) &&
                                                    cd.ActionType == actionDue.ActionType &&
                                                    cd.ActionDue == dueDate.ActionDue &&
                                                    cd.Recurring != (short)RecurringOption.NonRecurring
                                                    )
                                            .FirstOrDefaultAsync();

                if (countryDue != null)
                {
                    var hasRecurringDueDate = true;
                    if (!string.IsNullOrEmpty(countryDue.EffBasedOn))
                    {
                        var effBasedOn = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmkTrademark>().QueryableList.Where(t => t.TmkId == actionDue.TmkId)
                                                .Select(t => countryDue.EffBasedOn == BasedOnOption.Allowance ? t.AllowanceDate :
                                                                countryDue.EffBasedOn == BasedOnOption.Filing ? t.FilDate :
                                                                countryDue.EffBasedOn == BasedOnOption.Priority ? t.PriDate :
                                                                countryDue.EffBasedOn == BasedOnOption.Publication ? t.PubDate :
                                                                countryDue.EffBasedOn == BasedOnOption.Registration ? t.RegDate :
                                                                countryDue.EffBasedOn == BasedOnOption.Renewal ? t.LastRenewalDate : null)
                                                .FirstOrDefaultAsync();

                        hasRecurringDueDate = (effBasedOn != null &&
                            (countryDue.EffStartDate == null || effBasedOn >= countryDue.EffStartDate) &&
                            (countryDue.EffEndDate == null || effBasedOn <= countryDue.EffEndDate));
                    }

                    if (hasRecurringDueDate)
                    {
                        var baseDate = countryDue.Recurring == (short)RecurringOption.BasedOnDueDate ? dueDate.DueDate : (DateTime)dueDate.DateTaken;

                        return new TmkDueDate()
                        {
                            ActId = dueDate.ActId,
                            ActionDue = dueDate.ActionDue,
                            //DueDate = baseDate.AddDays((double)countryDue.Dy).AddMonths(countryDue.Mo).AddYears(countryDue.Yr),
                            //DueDate = baseDate.AddMonths(countryDue.Mo).AddYears(countryDue.Yr).AddDays((double)countryDue.Dy),

                            //proper leap year handling
                            DueDate = baseDate.AddYears(countryDue.Yr).AddMonths(countryDue.Mo).AddDays((double)countryDue.Dy),
                            Indicator = countryDue.Indicator,
                            AttorneyID = dueDate.AttorneyID,
                            CreatedBy = dueDate.UpdatedBy,
                            DateCreated = dueDate.LastUpdate,
                            UpdatedBy = dueDate.UpdatedBy,
                            LastUpdate = dueDate.LastUpdate
                        };
                    }
                }
            }

            return null;
        }

        private async Task AddCustomDocFolder(TmkActionDue actionDue)
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
            var actionType = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmkActionType>().QueryableList.Where(at => at.ActionTypeID == criteria.ActionTypeID).AsNoTracking().FirstOrDefaultAsync();
            if (actionType == null) return;

            var actionParam = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmkActionParameter>().QueryableList
                                            .Where(ap => ap.ActionTypeID == actionType.ActionTypeID && ap.ActionDue == criteria.ActionDue)
                                            .FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(criteria.ActionDue) && actionParam == null) return;

            var userName = _user.GetUserName();
            var today = DateTime.Now;


            //Generate new action records if generating from ActionType level
            if (criteria.ActParamId <= 0)
            {
                var trademarks = await _trademarkService.TmkTrademarks
                                    .Where(d => (!criteria.ActiveOnly || (d.TmkTrademarkStatus != null && d.TmkTrademarkStatus.ActiveSwitch))
                                        && (string.IsNullOrEmpty(criteria.Country) || d.Country == criteria.Country)
                                        && (criteria.CaseTypes == null || (d.CaseType != null && criteria.CaseTypes.Contains(d.CaseType)))
                                        && (criteria.FilDateFrom == null || d.FilDate >= criteria.FilDateFrom)
                                        && (criteria.FilDateTo == null || d.FilDate <= criteria.FilDateTo)
                                        && (criteria.PubDateFrom == null || d.PubDate >= criteria.PubDateFrom)
                                        && (criteria.PubDateTo == null || d.PubDate <= criteria.PubDateTo)
                                        && (criteria.RegDateFrom == null || d.RegDate >= criteria.RegDateFrom)
                                        && (criteria.RegDateTo == null || d.RegDate <= criteria.RegDateTo)
                                    )
                                    .ToListAsync();

                var newActionDues = new List<TmkActionDue>();

                foreach (var tmk in trademarks)
                {
                    var dupActionDue = await QueryableList.Where(d => d.TmkId == tmk.TmkId && d.BaseDate.Date == criteria.BaseDate.Date && d.ActionType == actionType.ActionType)
                                            .Include(d => d.DueDates).AsNoTracking().FirstOrDefaultAsync();

                    if (dupActionDue == null)
                    {
                        TmkActionDue actionDue = new TmkActionDue()
                        {
                            TmkId = tmk.TmkId,
                            CaseNumber = tmk.CaseNumber,
                            Country = tmk.Country,
                            SubCase = tmk.SubCase,
                            ActionType = actionType.ActionType,
                            BaseDate = criteria.BaseDate,
                            ResponsibleID = actionType.ResponsibleID,
                            IsOfficeAction = actionType.IsOfficeAction,
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
                        //    actionDue.DueDates = new List<TmkDueDate>()
                        //    {
                        //        new TmkDueDate()
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
                            var actionParams = await _cpiDbContext.GetReadOnlyRepositoryAsync<TmkActionParameter>().QueryableList
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
                                                .Select(d => new TmkDueDate()
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
                                _cpiDbContext.GetRepository<TmkDueDate>().Add(newDueDates);
                                await _cpiDbContext.SaveChangesAsync();
                            }
                        }
                        ////Generate specific ActionDue/ActionParameter that is not in the existing ActionDue record yet
                        //else if (!string.IsNullOrEmpty(criteria.ActionDue) && actionParam != null)
                        //{
                        //    var newDueDate = new TmkDueDate()
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
                        //        _cpiDbContext.GetRepository<TmkDueDate>().Add(newDueDate);
                        //        await _cpiDbContext.SaveChangesAsync();
                        //    }
                        //}
                    }
                }

                if (newActionDues != null && newActionDues.Count > 0)
                {
                    _cpiDbContext.GetRepository<TmkActionDue>().Add(newActionDues);
                    await _cpiDbContext.SaveChangesAsync();
                }
            }
            //Generate new due dates for all existing action records
            else if (criteria.ActParamId > 0 && !string.IsNullOrEmpty(criteria.ActionDue) && actionParam != null)
            {                
                var existingActions = await QueryableList
                    .Where(ad => ad.TmkTrademark != null 
                        && (!criteria.ActiveOnly || (ad.TmkTrademark.TmkTrademarkStatus != null && ad.TmkTrademark.TmkTrademarkStatus.ActiveSwitch))
                        && (string.IsNullOrEmpty(criteria.Country) || ad.TmkTrademark.Country == criteria.Country)
                        && (criteria.CaseTypes == null || (ad.TmkTrademark.CaseType != null && criteria.CaseTypes.Contains(ad.TmkTrademark.CaseType)))
                        && ad.ActionType == actionType.ActionType
                        && (ad.DueDates == null 
                                || !ad.DueDates.Any(dd => dd.ActionDue == actionParam.ActionDue 
                                    && dd.DueDate == ad.BaseDate.AddYears(actionParam.Yr).AddMonths(actionParam.Mo).AddDays((double)actionParam.Dy)) 
                            )
                        && (criteria.DueDateCutOff == null || ad.BaseDate.AddYears(actionParam.Yr).AddMonths(actionParam.Mo).AddDays((double)actionParam.Dy).Date > criteria.DueDateCutOff.Value.Date)
                    )
                    //.Include(d => d.DueDates)
                    .AsNoTracking().ToListAsync();

                var newDueDates = new List<TmkDueDate>();

                foreach (var existingAct in existingActions) {
                    var newDueDate = new TmkDueDate()
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
                    _cpiDbContext.GetRepository<TmkDueDate>().Add(newDueDates);
                    await _cpiDbContext.SaveChangesAsync();
                }
            }
        }

        public async Task UpdateDeDocket(TmkActionDue actionDue)
        {
            var deDocketFields = await _systemSettingManager.GetSystemSetting<DeDocketFields>();
            var updated = await GetByIdAsync(actionDue.ActId);
            Guard.Against.NoRecordPermission(updated != null);

            await ValidatePermission(updated, CPiPermissions.DeDocketer);

            if (updated != null && deDocketFields.TrademarkActionDue != null)
            {
                if (deDocketFields.TrademarkActionDue.Remarks)
                    updated.Remarks = actionDue.Remarks;

                updated.LastUpdate = actionDue.LastUpdate;
                updated.UpdatedBy = actionDue.UpdatedBy;
                updated.tStamp = actionDue.tStamp;

                _cpiDbContext.GetRepository<TmkActionDue>().Update(updated);
                await _cpiDbContext.SaveChangesAsync();
            }
            else
                Guard.Against.UnAuthorizedAccess(false);
        }

        public async Task UpdateCheckDocket(TmkActionDue actionDue)
        {
            var updated = await GetByIdAsync(actionDue.ActId);
            if (updated != null)
            {
                updated.tStamp = actionDue.tStamp;
                _cpiDbContext.GetRepository<TmkActionDue>().Attach(updated);
                updated.CheckDocket = actionDue.CheckDocket;               
                updated.UpdatedBy = actionDue.UpdatedBy;
                updated.LastUpdate = actionDue.LastUpdate;
                await _cpiDbContext.SaveChangesAsync();
                _cpiDbContext.Detach(updated);
            }
        }
    }
}
