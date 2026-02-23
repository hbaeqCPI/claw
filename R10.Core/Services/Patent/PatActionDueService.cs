using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.VisualBasic;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using static System.Net.Mime.MediaTypeNames;

namespace R10.Core.Services
{
    public class PatActionDueService : EntityService<PatActionDue>, IActionDueDeDocketService<PatActionDue, PatDueDate>
    {
        private readonly ICountryApplicationService _countryAppService;
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly ICPiUserSettingManager _userSettingManager;
        private readonly ICPiSystemSettingManager _systemSettingManager;

        public PatActionDueService(ICPiDbContext cpiDbContext,
            ICountryApplicationService countryAppService,
            ISystemSettings<PatSetting> settings, 
            ClaimsPrincipal user,
            ICPiUserSettingManager userSettingManager,
            ICPiSystemSettingManager systemSettingManager) : base(cpiDbContext, user)
        {
            _countryAppService = countryAppService;
            _settings = settings;
            _userSettingManager = userSettingManager;
            _systemSettingManager = systemSettingManager;
        }

        public override IQueryable<PatActionDue> QueryableList
        {
            get
            {
                var actionsDue = base.QueryableList;

                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Patent) || _user.RestrictExportControl() || !_user.CanAccessPatTradeSecret())
                    actionsDue = actionsDue.Where(a => _countryAppService.CountryApplications.Any(gm => gm.AppId == a.AppId));

                return actionsDue;
            }
        }

        public override async Task<PatActionDue> GetByIdAsync(int actId)
        {
            return await QueryableList.SingleOrDefaultAsync(a => a.ActId == actId);
        }

        public override async Task Add(PatActionDue actionDue)
        {
            actionDue.ComputerGenerated = false;
            actionDue.IsElectronic = false;

            await ValidatePermission(actionDue, CPiPermissions.FullModify);
            await ValidateResponsibleAttorney(actionDue.ResponsibleID ?? 0);

            var countryApp = await ValidateCountryApplication(actionDue);

            //Web API can add due dates
            if (actionDue.DueDates == null)
                actionDue.DueDates = await GenerateDueDates(actionDue);

            var settings = await _settings.GetSetting();
            if (actionDue.DueDates.Any() && settings.IsWorkflowOn)
            {
                var dueDatesFromWorkflow = await _countryAppService.GenerateDueDateFromActionParameterWorkflow(actionDue, actionDue.DueDates, PatWorkflowTriggerType.Indicator);
                if (dueDatesFromWorkflow != null && dueDatesFromWorkflow.Any())
                {
                    actionDue.DueDates.AddRange(dueDatesFromWorkflow);
                }
            }
            _cpiDbContext.GetRepository<PatActionDue>().Add(actionDue);

            //if (actionDue.ResponseDate != null)
            //    await GenerateFollowUpAction(actionDue);

            if (actionDue.DocFolders != null) {
                 await AddCustomDocFolder(actionDue);
            }
            else
              await _cpiDbContext.SaveChangesAsync();

            //save the main action first before adding a followup
            if (actionDue.ResponseDate != null) {
                await GenerateFollowUpAction(actionDue);
                await _cpiDbContext.SaveChangesAsync();
            }
                

            _cpiDbContext.Detach(actionDue);
            _cpiDbContext.Detach(countryApp);
        }

        public override async Task Update(PatActionDue actionDue)
        {
            await ValidatePermission(actionDue, CPiPermissions.FullModify);
            await ValidateResponsibleAttorney(actionDue.ResponsibleID ?? 0);

            await ValidateComputerGenerated(actionDue);
            var countryApp = await ValidateCountryApplication(actionDue);

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
                    _cpiDbContext.Detach(countryApp);
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

        public override async Task UpdateRemarks(PatActionDue actionDue)
        {
            var updated = await GetByIdAsync(actionDue.ActId);
            Guard.Against.NoRecordPermission(updated != null);

            await ValidatePermission(updated, CPiPermissions.RemarksOnly);

            updated.tStamp = actionDue.tStamp;

            _cpiDbContext.GetRepository<PatActionDue>().Attach(updated);
            updated.Remarks = actionDue.Remarks;
            updated.UpdatedBy = actionDue.UpdatedBy;
            updated.LastUpdate = actionDue.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task UpdateResponseDate(PatActionDue actionDue)
        {
            var updated = await GetByIdAsync(actionDue.ActId);
            Guard.Against.NoRecordPermission(updated != null);

            await ValidatePermission(updated, CPiPermissions.RemarksOnly);

            updated.tStamp = actionDue.tStamp;

            _cpiDbContext.GetRepository<PatActionDue>().Attach(updated);
            updated.ResponseDate = actionDue.ResponseDate;
            updated.UpdatedBy = actionDue.UpdatedBy;
            updated.LastUpdate = actionDue.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();
        }

        public override async Task Delete(PatActionDue actionDue)
        {
            await ValidatePermission(actionDue, CPiPermissions.CanDelete);
            await ValidateResponsibleAttorney(actionDue.ResponsibleID ?? 0);

            await base.Delete(actionDue);
        }

        private async Task ValidatePermission(PatActionDue actionDue, List<string> roles)
        {
            if ((await _userSettingManager.GetUserSetting<UserAccountSettings>(_user.GetUserIdentifier())).RestrictAdhocActions && 
                !string.IsNullOrEmpty(actionDue.ActionType) &&
                (actionDue.ActId == 0 || 
                !actionDue.ActionType.Equals((await GetByIdAsync(actionDue.ActId)).ActionType, StringComparison.InvariantCultureIgnoreCase)))
                Guard.Against.ValueNotAllowed(await _cpiDbContext
                                                        .GetReadOnlyRepositoryAsync<PatActionType>().QueryableList
                                                        .AnyAsync(at => (at.Country == actionDue.Country || (at.Country ?? "") == "") &&
                                                                         at.ActionType == actionDue.ActionType), "Action Type");

            var actId = actionDue.ActId;
            var respOfc = "";
            if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Patent))
            {
                var item = new KeyValuePair<int, string>();
                if (actId > 0)
                {
                    item = (await QueryableList.Where(a => a.ActId == actId)
                                    .Select(a => new { a.AppId, a.CountryApplication.RespOffice })
                                    .ToDictionaryAsync(a => a.AppId, c => c.RespOffice)).FirstOrDefault();
                }
                else
                {
                    actionDue.SubCase = actionDue.SubCase ?? "";
                    item = (await _countryAppService.CountryApplications
                                    .Where(ca => ca.CaseNumber == actionDue.CaseNumber && ca.Country == actionDue.Country && ca.SubCase == actionDue.SubCase)
                                    .Select(c => new { c.AppId, c.RespOffice })
                                    .ToDictionaryAsync(c => c.AppId, c => c.RespOffice)).FirstOrDefault();
                }

                Guard.Against.NoRecordPermission(item.Key > 0);

                respOfc = item.Value;
            }

            var settings = await _settings.GetSetting();
            if (settings.IsSoftDocketOn && actionDue.DueDates != null && actionDue.DueDates.Any(d => !string.IsNullOrEmpty(d.Indicator) && d.Indicator.ToLower() == "soft docket"))
                Guard.Against.NoRecordPermission(_user.IsSoftDocketUser() || _user.IsInRoles(SystemType.Patent, CPiPermissions.SoftDocket));
            else
                Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Patent, roles, respOfc));
        }

        private async Task ValidateResponsibleAttorney(int responsibleId)
        {
            if (responsibleId > 0 && _user.GetEntityFilterType() == CPiEntityType.Attorney)
                Guard.Against.ValueNotAllowed(await base.EntityFilterAllowed(responsibleId), "Responsible Attorney");
        }

        private async Task ValidateComputerGenerated(PatActionDue actionDue)
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

        private async Task<CountryApplication> ValidateCountryApplication(PatActionDue actionDue)
        {
            var settings = await _settings.GetSetting();

            actionDue.SubCase = actionDue.SubCase ?? "";

            var countryApp = await _countryAppService.CountryApplications
                .Where(ca =>
                    ca.CaseNumber == actionDue.CaseNumber &&
                    ca.Country == actionDue.Country &&
                    ca.SubCase == actionDue.SubCase)
                .SingleOrDefaultAsync();

            var caseNumberLabel = settings.LabelCaseNumber;
            Guard.Against.ValueNotAllowed(countryApp?.AppId > 0, $"{caseNumberLabel}/Country/Sub Case");

            if (_user.IsRespOfficeOn(SystemType.Patent))
                Guard.Against.ValueNotAllowed(await ValidateRespOffice(SystemType.Patent, countryApp.RespOffice), $"{caseNumberLabel}/Country/Sub Case");

            actionDue.AppId = countryApp.AppId;

            _cpiDbContext.GetRepository<CountryApplication>().Attach(countryApp);
            countryApp.LastUpdate = actionDue.LastUpdate;
            countryApp.UpdatedBy = actionDue.UpdatedBy;

            return countryApp;
        }

        public async Task<bool> CanModifyAttorney(int responsibleId)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Attorney && responsibleId > 0)
                return await base.EntityFilterAllowed(responsibleId);
            else
                return true;
        }

        private IQueryable<PatDueDate> DueDates => _cpiDbContext.GetRepository<PatDueDate>().QueryableList;

        /// <summary>
        /// Update or generate due dates.
        /// Returns true if follow up actions need to be generated.
        /// </summary>
        /// <param name="actionDue"></param>
        /// <returns></returns>
        private async Task<bool> UpdateDueDates(PatActionDue actionDue)
        {
            var dueDates = new List<PatDueDate>();
            var recurringDueDates = new List<PatDueDate>();

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

                                if (responseDateChanged) {
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
                    _cpiDbContext.GetRepository<PatDueDate>().Delete(oldDueDates);                    
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
                            _cpiDbContext.GetRepository<PatDueDate>().Delete(oldFollowUps);
                            dueDates = dueDates.Where(d => !oldFollowUps.Any(f => f.ActionDue == d.ActionDue && f.DueDate == d.DueDate)).ToList();
                        }
                    }
                    else if (oldFollowUpAction != null)
                    {
                        var oldFollowUpActionDue = await _cpiDbContext.GetRepository<PatActionDue>().QueryableList
                                  .Where(a => a.AppId == oldFollowUpAction.AppId && a.ActionType == oldFollowUpAction.ActionType && a.BaseDate == oldFollowUpAction.BaseDate)
                                  .FirstOrDefaultAsync();

                        if (oldFollowUpActionDue !=null) {
                            var oldFollowUpDueDates = await DueDates.Where(d => d.ActId == oldFollowUpActionDue.ActId).ToListAsync();
                            
                            _cpiDbContext.GetRepository<PatDueDate>().Delete(oldFollowUpDueDates);
                            _cpiDbContext.GetRepository<PatActionDue>().Delete(oldFollowUpActionDue);
                        }
                    }

                    else {
                        var settings = await _settings.GetSetting();
                        var autoFollowUp = settings.IsGenFollowUpOn;
                        if (autoFollowUp)
                        {
                            var autoFollowUpActionDesc = $"{actionDue.ActionType.Substring(0, actionDue.ActionType.Length >= 45 ? 45 : actionDue.ActionType.Length)} Follow Up Date";
                            var existing = dueDates.Where(d => d.ActId == actionDue.ActId && d.ActionDue == autoFollowUpActionDesc).FirstOrDefault();
                            if (existing != null) {
                                _cpiDbContext.GetRepository<PatDueDate>().Delete(existing);
                                dueDates.Remove(existing);
                            }
                        }
                    }
                }

                //update DueDates
                if (dueDates.Any())
                {
                    _cpiDbContext.GetRepository<PatDueDate>().Attach(dueDates);
                }
                foreach (var dueDate in dueDates)
                {
                    var oldDateTaken = dueDate.DateTaken;

                    if (actionDue.CloseDueDates) {
                        dueDate.DateTaken = DateTime.Now.Date;
                    }
                    //update DateTaken with ResponseDate if ResponseDate changed
                    //and when DateTaken is blank or DateTaken is the same as old ResponseDate
                    dueDate.DateTaken = responseDateChanged && (dueDate.DateTaken == null || dueDate.DateTaken == oldActionDue.ResponseDate) ? actionDue.ResponseDate : dueDate.DateTaken;
                    dueDate.IsVerifyDate = actionDue.VerifyDate;
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
                        if (generateRecurring) {
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
            if (dueDates.Any()) {
                actionDue.DueDates = dueDates;
            }
            _cpiDbContext.GetRepository<PatActionDue>().Update(actionDue);

            return generateFollowUp;
        }

        /// <summary>
        /// Generates DueDates based on ActionParameters when actionDue is based on ActionType.
        /// Generates DueDate based on actionDue when actionDue is not based on any ActionType.
        /// </summary>
        /// <param name="actionDue">The action due record that the due dates will be based on.</param>
        /// <returns></returns>
        private async Task<List<PatDueDate>> GenerateDueDates(PatActionDue actionDue)
        {
            var dueDates = new List<PatDueDate>();
            var actionType = await GetActionType(actionDue.ActionType, actionDue.Country);
            var actionParams = new List<PatActionParameter>();

            //actionDue is based on an ActionType
            //get ActionParameters
            if (actionType != null)
                actionParams = await _cpiDbContext.GetReadOnlyRepositoryAsync<PatActionParameter>().QueryableList
                                    .Where(ap => ap.ActionTypeID == actionType.ActionTypeID)
                                    .ToListAsync();

            if (actionParams.Any())
                //generate DueDates based on ActionParameters
                dueDates = actionParams.Select(ap => new PatDueDate()
                {
                    //ActId = actionDue.ActId,
                    ActionDue = ap.ActionDue,
                    //DueDate = actionDue.BaseDate.AddDays((double)ap.Dy).AddMonths(ap.Mo).AddYears(ap.Yr),
                    //DueDate = actionDue.BaseDate.AddMonths(ap.Mo).AddYears(ap.Yr).AddDays((double)ap.Dy),

                    //proper leap year handling
                    DueDate = actionDue.BaseDate.AddYears(ap.Yr).AddMonths(ap.Mo).AddDays((double)ap.Dy),

                    DateTaken = actionDue.ResponseDate,
                    IsVerifyDate = actionDue.VerifyDate,
                    Indicator = ap.Indicator,
                    AttorneyID = actionDue.ResponsibleID,
                    CreatedBy = actionDue.UpdatedBy,
                    DateCreated = actionDue.LastUpdate,
                    UpdatedBy = actionDue.UpdatedBy,
                    LastUpdate = actionDue.LastUpdate
                }).ToList();
            else
                //generate DueDate based on actionDue
                dueDates.Add(new PatDueDate()
                {
                  //  ActId = actionDue.ActId,
                    ActionDue = actionDue.ActionType,
                    DueDate = actionDue.BaseDate,
                    DateTaken = actionDue.ResponseDate,
                    IsVerifyDate = actionDue.VerifyDate,
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
        /// Returns PatActionType based on ActionType and Country.
        /// Retrieves ActionType with matching Country first then ActionType with blank Country.
        /// </summary>
        /// <param name="actionType">The action type to search.</param>
        /// <param name="country">The country to search</param>
        /// <returns></returns>
        private async Task<PatActionType> GetActionType(string actionType, string country)
        {
            //exlude actiontypes used as country law followup
            var actionTypes = _cpiDbContext.GetReadOnlyRepositoryAsync<PatActionType>().QueryableList.Where(a => (a.CDueId ?? 0) == 0);

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
        private async Task GenerateFollowUpAction(PatActionDue actionDue)
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

                        _cpiDbContext.GetRepository<PatDueDate>().Add(followUpDueDate);

                        if (settings.IsWorkflowOn)
                        {
                            var dueDatesFromWorkflow = await _countryAppService.GenerateDueDateFromActionParameterWorkflow(actionDue, new List<PatDueDate> { followUpDueDate}, PatWorkflowTriggerType.Indicator);
                            if (dueDatesFromWorkflow != null && dueDatesFromWorkflow.Any())
                            {
                                dueDatesFromWorkflow.ForEach(dd=> { dd.ActId = actionDue.ActId; dd.DateTaken = null; });
                                _cpiDbContext.GetRepository<PatDueDate>().Add(dueDatesFromWorkflow);
                            }
                        }
                    }
                }
                else
                {
                    //insert new ActionDue with DueDates if ActionDue does not exist
                    var actions = await _cpiDbContext.GetRepository<PatActionDue>().QueryableList
                                    .Where(a => a.AppId == followUpAction.AppId && a.ActionType == followUpAction.ActionType && a.BaseDate == followUpAction.BaseDate)
                                    .ToListAsync();

                    if (!actions.Any() && !(actionDue.ActionType == followUpAction.ActionType && actionDue.BaseDate == followUpAction.BaseDate)) {
                        if (settings.IsWorkflowOn) {
                            var dueDatesFromWorkflow = await _countryAppService.GenerateDueDateFromActionParameterWorkflow(followUpAction, followUpAction.DueDates, PatWorkflowTriggerType.Indicator);
                            if (dueDatesFromWorkflow != null && dueDatesFromWorkflow.Any())
                            {
                                followUpAction.DueDates.AddRange(dueDatesFromWorkflow);
                            }
                        }
                        _cpiDbContext.GetRepository<PatActionDue>().Add(followUpAction);
                        actionDue.FollowUpAction = followUpAction.ActionType;
                    }
                        
                }
            }

            //office action auto followup is handled above
            //Jeff Nichol's email dated 3/20/2024
            else if (actionDue.ResponseDate !=null && actionDue.ComputerGenerated) {
                var autoFollowUp = settings.IsGenFollowUpOn;
                if (autoFollowUp) {
                    var termMonth = settings.FollowUpActionTermMon;
                    var termDay = settings.FollowUpActionTermDay;
                    var indicator = settings.FollowUpActionIndicator;
                    if (string.IsNullOrEmpty(indicator))
                        indicator = "Due Date";

                    var followUpActionDesc = $"{actionDue.ActionType.Substring(0, actionDue.ActionType.Length >= 45 ? 45 : actionDue.ActionType.Length)} Follow Up Date";

                    var existing = await _cpiDbContext.GetRepository<PatDueDate>().QueryableList
                                       .Where(d => d.ActId == actionDue.ActId && d.ActionDue == followUpActionDesc)
                                       .FirstOrDefaultAsync();
                    if (existing != null)
                        _cpiDbContext.GetRepository<PatDueDate>().Delete(existing);

                    var followUpDueDate = new PatDueDate {
                        ActId = actionDue.ActId,
                        ActionDue = followUpActionDesc,
                        Indicator = indicator,
                        DueDate = ((DateTime)actionDue.ResponseDate).AddMonths(termMonth).AddDays(termDay),
                        IsForVerify=false,
                        DateCreated=DateTime.Now,
                        LastUpdate = DateTime.Now,
                        CreatedBy=actionDue.UpdatedBy,
                        UpdatedBy = actionDue.UpdatedBy
                    };
                    //_cpiDbContext.GetRepository<PatDueDate>().Add(followUpDueDate); //foreign key error
                    actionDue.DueDates.Add(followUpDueDate);

                    if (settings.IsWorkflowOn)
                    {
                        var dueDatesFromWorkflow = await _countryAppService.GenerateDueDateFromActionParameterWorkflow(actionDue, new List<PatDueDate> { followUpDueDate }, PatWorkflowTriggerType.Indicator);
                        if (dueDatesFromWorkflow != null && dueDatesFromWorkflow.Any())
                        {
                            dueDatesFromWorkflow.ForEach(dd => { dd.ActId = actionDue.ActId; dd.DateTaken = null; });
                            _cpiDbContext.GetRepository<PatDueDate>().Add(dueDatesFromWorkflow);
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
        private async Task<PatActionDue> GetFollowUpAction(PatActionDue actionDue)
        {
            PatActionDue followUpAction = null;
            PatActionType actionType = null;

            if (actionDue.ComputerGenerated) 
            {
                var countryApp = _cpiDbContext.GetReadOnlyRepositoryAsync<CountryApplication>().QueryableList.Where(ca => ca.AppId == actionDue.AppId);
                var countryDue = _cpiDbContext.GetReadOnlyRepositoryAsync<PatCountryDue>().QueryableList
                                            .Where(cd =>
                                                    countryApp.Any(ca => ca.Country == cd.Country && ca.CaseType == cd.CaseType) &&
                                                    cd.ActionType == actionDue.ActionType 
                                                    //&&  cd.ActionDue == actionDue.ActionType //some are not equal
                                                    );
                //get ActionType using CDueId
                //todo: check effective period?
                actionType = await _cpiDbContext.GetReadOnlyRepositoryAsync<PatActionType>().QueryableList
                                            .Where(at => countryDue.Any(cd => cd.CDueId == at.CDueId))
                                            .FirstOrDefaultAsync();
                if (actionType != null) {
                    var sourceActionType = await _cpiDbContext.GetReadOnlyRepositoryAsync<PatActionType>().QueryableList
                                            .Where(at => (at.CDueId == 0 || at.CDueId == null) && at.ActionType==actionType.ActionType && (at.Country==actionType.Country || string.IsNullOrEmpty(at.Country)))
                                            .OrderByDescending(at=> at.Country).FirstOrDefaultAsync();
                    if (sourceActionType != null) {
                        actionType.IsOfficeAction = sourceActionType.IsOfficeAction;
                        actionType.ResponsibleID = sourceActionType.ResponsibleID;
                    }
                }
            }
            else
                //get ActionType using ActionType and country
                actionType = await GetActionType(actionDue.ActionType, actionDue.Country);

            //follow up action type from computer generated actions does
            if (actionType != null && actionType.FollowUpGen != (short)FollowUpOption.DontGenerate)
            {
                if (string.IsNullOrEmpty(actionType.FollowUpIndicator))
                    actionType.FollowUpIndicator = "Due Date";

                //ActionType exists
                //get follow up ActionType based on FollowUpMsg
                PatActionType followUpActionType = null;

                if (actionDue.ComputerGenerated)
                    followUpActionType = actionType;

                else if (!string.IsNullOrEmpty(actionType.FollowUpMsg))
                    followUpActionType = await GetActionType(actionType.FollowUpMsg, actionDue.Country);

                if (followUpActionType != null)
                {
                    //follow up ActionType exists
                    //create new ActionDue
                    followUpAction = new PatActionDue()
                    {
                        AppId = actionDue.AppId,
                        CaseNumber = actionDue.CaseNumber,
                        Country = actionDue.Country,
                        SubCase = actionDue.SubCase,
                        ActionType = followUpActionType.ActionType,
                        BaseDate = actionType.FollowUpGen == (short)FollowUpOption.BaseDate ? actionDue.BaseDate : (DateTime)actionDue.ResponseDate,
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
                        followUpAction = new PatActionDue()
                        {
                            AppId = actionDue.AppId,
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
                        followUpAction = new PatActionDue()
                        {
                            ActId = actionDue.ActId,
                            ActionType = actionDue.ActionType
                        };
                    }

                    if (followUpAction != null)
                    {
                        //create new follow up DueDate
                        followUpAction.DueDates = new List<PatDueDate>() {
                        new PatDueDate()
                        {
                            ActId = actionDue.ActId,
                            ActionDue = followUpActionDue,
                            DueDate = followUpDueDate.AddMonths(actionType.FollowUpMonth).AddDays(actionType.FollowUpDay),
                            IsVerifyDate = actionDue.VerifyDate,
                            AttorneyID = actionDue.ResponsibleID,
                            Indicator = actionType.FollowUpIndicator,
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

        public async Task<PatDueDate> GetRecurringDueDate(PatActionDue actionDue, PatDueDate dueDate)
        {
            if (actionDue.ComputerGenerated && dueDate.DateTaken != null)
            {
                var countryApp = _cpiDbContext.GetReadOnlyRepositoryAsync<CountryApplication>().QueryableList.Where(ca => ca.AppId == actionDue.AppId);
                var countryDue = await _cpiDbContext.GetReadOnlyRepositoryAsync<PatCountryDue>().QueryableList
                                            .Where(cd =>
                                                    countryApp.Any(ca => ca.Country == cd.Country && ca.CaseType == cd.CaseType) &&
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
                        var effBasedOn = await _cpiDbContext.GetReadOnlyRepositoryAsync<CountryApplication>().QueryableList.Where(ca => ca.AppId == actionDue.AppId)
                                                .Select(ca => countryDue.EffBasedOn == BasedOnOption.Filing ? ca.FilDate :
                                                                countryDue.EffBasedOn == BasedOnOption.Issue ? ca.IssDate :
                                                                countryDue.EffBasedOn == BasedOnOption.ParentFiling ? ca.ParentFilDate :
                                                                countryDue.EffBasedOn == BasedOnOption.ParentIssue ? ca.ParentIssDate :
                                                                countryDue.EffBasedOn == BasedOnOption.PCT ? ca.PCTDate :
                                                                countryDue.EffBasedOn == BasedOnOption.Priority ? ca.Invention.Priorities.Min(pri => pri.FilDate) :
                                                                countryDue.EffBasedOn == BasedOnOption.Publication ? ca.PubDate : null)
                                                .FirstOrDefaultAsync();

                        hasRecurringDueDate = (effBasedOn != null &&
                            (countryDue.EffStartDate == null || effBasedOn >= countryDue.EffStartDate) &&
                            (countryDue.EffEndDate == null || effBasedOn <= countryDue.EffEndDate));
                    }

                    if (hasRecurringDueDate)
                    {
                        var baseDate = countryDue.Recurring == (short)RecurringOption.BasedOnDueDate ? dueDate.DueDate : (DateTime)dueDate.DateTaken;

                        return new PatDueDate()
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
                            LastUpdate = dueDate.LastUpdate,
                            IsVerifyDate = dueDate.IsVerifyDate
                        };
                    }
                }
            }

            return null;
        }

        private async Task AddCustomDocFolder(PatActionDue actionDue) {
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
            var actionType = await _cpiDbContext.GetReadOnlyRepositoryAsync<PatActionType>().QueryableList.Where(at => at.ActionTypeID == criteria.ActionTypeID).AsNoTracking().FirstOrDefaultAsync();
            if (actionType == null) return;

            var actionParam = await _cpiDbContext.GetReadOnlyRepositoryAsync<PatActionParameter>().QueryableList
                                            .Where(ap => ap.ActionTypeID == actionType.ActionTypeID && ap.ActionDue == criteria.ActionDue)
                                            .FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(criteria.ActionDue) && actionParam == null) return;

            var userName = _user.GetUserName();
            var today = DateTime.Now;

            //Generate new action records if generating from ActionType level
            if (criteria.ActParamId <= 0)
            {
                var newActionDues = new List<PatActionDue>();

                //Get appIds
                var applications = await _countryAppService.CountryApplications
                    .Where(d => (!criteria.ActiveOnly || (d.PatApplicationStatus != null && d.PatApplicationStatus.ActiveSwitch))
                        && (string.IsNullOrEmpty(criteria.Country) || d.Country == criteria.Country)
                        && (criteria.CaseTypes == null || (d.CaseType != null && criteria.CaseTypes.Contains(d.CaseType)))                                    
                        && (criteria.FilDateFrom == null || d.FilDate >= criteria.FilDateFrom)
                        && (criteria.FilDateTo == null || d.FilDate <= criteria.FilDateTo)
                        && (criteria.PubDateFrom == null || d.PubDate >= criteria.PubDateFrom)
                        && (criteria.PubDateTo == null || d.PubDate <= criteria.PubDateTo)
                        && (criteria.IssDateFrom == null || d.IssDate >= criteria.IssDateFrom)
                        && (criteria.IssDateTo == null || d.IssDate <= criteria.IssDateTo)
                    )                                
                    .ToListAsync();

                foreach (var app in applications)
                {
                    var dupActionDue = await QueryableList.Where(d => d.AppId == app.AppId && d.BaseDate.Date == criteria.BaseDate.Date && d.ActionType == actionType.ActionType)
                                            .Include(d => d.DueDates).AsNoTracking().FirstOrDefaultAsync();

                    if (dupActionDue == null)
                    {
                        PatActionDue actionDue = new PatActionDue()
                        {
                            AppId = app.AppId,
                            CaseNumber = app.CaseNumber,
                            Country = app.Country,
                            SubCase = app.SubCase,
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
                        //    actionDue.DueDates = new List<PatDueDate>()
                        //    {
                        //        new PatDueDate()
                        //        {
                        //            ActionDue = actionParam.ActionDue,
                        //            //proper leap year handling
                        //            DueDate = actionDue.BaseDate.AddYears(actionParam.Yr).AddMonths(actionParam.Mo).AddDays((double)actionParam.Dy),
                        //            DateTaken = actionDue.ResponseDate,
                        //            IsVerifyDate = actionDue.VerifyDate,
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
                            var actionParams = await _cpiDbContext.GetReadOnlyRepositoryAsync<PatActionParameter>().QueryableList
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
                                                .Select(d => new PatDueDate()
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
                                _cpiDbContext.GetRepository<PatDueDate>().Add(newDueDates);
                                await _cpiDbContext.SaveChangesAsync();
                            }
                        }
                        ////Generate specific ActionDue/ActionParameter that is not in the existing ActionDue record yet
                        //else if (!string.IsNullOrEmpty(criteria.ActionDue) && actionParam != null)
                        //{
                        //    var newDueDate = new PatDueDate()
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
                        //        _cpiDbContext.GetRepository<PatDueDate>().Add(newDueDate);
                        //        await _cpiDbContext.SaveChangesAsync();
                        //    }
                        //}
                    }
                }

                if (newActionDues.Any())
                {
                    _cpiDbContext.GetRepository<PatActionDue>().Add(newActionDues);
                    await _cpiDbContext.SaveChangesAsync();
                }
            }
            //Generate new due dates for all existing action records
            else if (criteria.ActParamId > 0 && !string.IsNullOrEmpty(criteria.ActionDue) && actionParam != null)
            {                
                var existingActions = await QueryableList
                    .Where(ad => ad.CountryApplication != null 
                        && (!criteria.ActiveOnly || (ad.CountryApplication.PatApplicationStatus != null && ad.CountryApplication.PatApplicationStatus.ActiveSwitch))
                        && (string.IsNullOrEmpty(criteria.Country) || ad.CountryApplication.Country == criteria.Country)
                        && (criteria.CaseTypes == null || (ad.CountryApplication.CaseType != null && criteria.CaseTypes.Contains(ad.CountryApplication.CaseType)))
                        && ad.ActionType == actionType.ActionType
                        && (ad.DueDates == null 
                                || !ad.DueDates.Any(dd => dd.ActionDue == actionParam.ActionDue 
                                    && dd.DueDate == ad.BaseDate.AddYears(actionParam.Yr).AddMonths(actionParam.Mo).AddDays((double)actionParam.Dy)) 
                            )
                        && (criteria.DueDateCutOff == null || ad.BaseDate.AddYears(actionParam.Yr).AddMonths(actionParam.Mo).AddDays((double)actionParam.Dy).Date > criteria.DueDateCutOff.Value.Date)
                    )
                    //.Include(d => d.DueDates)
                    .AsNoTracking().ToListAsync();

                var newDueDates = new List<PatDueDate>();

                foreach (var existingAct in existingActions) {
                    var newDueDate = new PatDueDate()
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
                    _cpiDbContext.GetRepository<PatDueDate>().Add(newDueDates);
                    await _cpiDbContext.SaveChangesAsync();
                }
            }
        }

        public async Task UpdateDeDocket(PatActionDue actionDue)
        {
            var deDocketFields = await _systemSettingManager.GetSystemSetting<DeDocketFields>();
            var updated = await GetByIdAsync(actionDue.ActId);
            Guard.Against.NoRecordPermission(updated != null);

            await ValidatePermission(updated, CPiPermissions.DeDocketer);

            if (updated != null && deDocketFields.PatentActionDue != null)
            {
                if (deDocketFields.PatentActionDue.Remarks)
                    updated.Remarks = actionDue.Remarks;

                updated.LastUpdate = actionDue.LastUpdate;
                updated.UpdatedBy = actionDue.UpdatedBy;
                updated.tStamp = actionDue.tStamp;

                _cpiDbContext.GetRepository<PatActionDue>().Update(updated);
                await _cpiDbContext.SaveChangesAsync();
            }
            else
                Guard.Against.UnAuthorizedAccess(false);
        }

        public async Task UpdateCheckDocket(PatActionDue actionDue)
        {
            var updated = await GetByIdAsync(actionDue.ActId);
            if (updated != null)
            {
                updated.tStamp = actionDue.tStamp;
                _cpiDbContext.GetRepository<PatActionDue>().Attach(updated);
                updated.CheckDocket = actionDue.CheckDocket;               
                updated.UpdatedBy = actionDue.UpdatedBy;
                updated.LastUpdate = actionDue.LastUpdate;
                await _cpiDbContext.SaveChangesAsync();
                _cpiDbContext.Detach(updated);
            }
        }
    }
}
