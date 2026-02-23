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
    public class PatActionDueInvService : EntityService<PatActionDueInv>, IActionDueDeDocketService<PatActionDueInv, PatDueDateInv>
    {
        private readonly IInventionService _inventionService;
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly ICPiUserSettingManager _userSettingManager;
        private readonly ICPiSystemSettingManager _systemSettingManager;

        public PatActionDueInvService(ICPiDbContext cpiDbContext,
            IInventionService inventionService,
            ISystemSettings<PatSetting> settings,
            ClaimsPrincipal user,
            ICPiUserSettingManager userSettingManager,
            ICPiSystemSettingManager systemSettingManager) : base(cpiDbContext, user)
        {
            _inventionService = inventionService;
            _settings = settings;
            _userSettingManager = userSettingManager;
            _systemSettingManager = systemSettingManager;
        }

        public override IQueryable<PatActionDueInv> QueryableList
        {
            get
            {
                var actionsDue = base.QueryableList;

                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Patent) || _user.RestrictExportControl())
                    actionsDue = actionsDue.Where(a => _inventionService.Inventions.Any(gm => gm.InvId == a.InvId));

                return actionsDue;
            }
        }

        public override async Task<PatActionDueInv> GetByIdAsync(int actId)
        {
            return await QueryableList.SingleOrDefaultAsync(a => a.ActId == actId);
        }

        public override async Task Add(PatActionDueInv actionDue)
        {
            actionDue.ComputerGenerated = false;

            await ValidatePermission(actionDue, CPiPermissions.FullModify);
            await ValidateResponsibleAttorney(actionDue.ResponsibleID ?? 0);

            await ValidateInvention(actionDue);

            //Web API can add due dates
            if (actionDue.DueDateInvs == null)
                actionDue.DueDateInvs = await GenerateDueDates(actionDue);

            _cpiDbContext.GetRepository<PatActionDueInv>().Add(actionDue);

            if (actionDue.ResponseDate != null)
                await GenerateFollowUpAction(actionDue);

            if (actionDue.DocFolders != null)
            {
                await AddCustomDocFolder(actionDue);
            }
            else
                await _cpiDbContext.SaveChangesAsync();

            _cpiDbContext.Detach(actionDue);
        }

        public override async Task Update(PatActionDueInv actionDue)
        {
            await ValidatePermission(actionDue, CPiPermissions.FullModify);
            await ValidateResponsibleAttorney(actionDue.ResponsibleID ?? 0);

            await ValidateComputerGenerated(actionDue);
            var invention = await ValidateInvention(actionDue);

            var generateFollowUp = await UpdateDueDates(actionDue);
            if (generateFollowUp)
                await GenerateFollowUpAction(actionDue);

            bool concurrencyFailure;
            do
            {
                concurrencyFailure = false;
                try
                {
                    await _cpiDbContext.SaveChangesAsync();
                    _cpiDbContext.Detach(actionDue);
                    _cpiDbContext.Detach(invention);
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

        public override async Task UpdateRemarks(PatActionDueInv actionDue)
        {
            var updated = await GetByIdAsync(actionDue.ActId);
            Guard.Against.NoRecordPermission(updated != null);

            await ValidatePermission(updated, CPiPermissions.RemarksOnly);

            updated.tStamp = actionDue.tStamp;

            _cpiDbContext.GetRepository<PatActionDueInv>().Attach(updated);
            updated.Remarks = actionDue.Remarks;
            updated.UpdatedBy = actionDue.UpdatedBy;
            updated.LastUpdate = actionDue.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task UpdateResponseDate(PatActionDueInv actionDue)
        {
            var updated = await GetByIdAsync(actionDue.ActId);
            Guard.Against.NoRecordPermission(updated != null);

            await ValidatePermission(updated, CPiPermissions.RemarksOnly);

            updated.tStamp = actionDue.tStamp;

            _cpiDbContext.GetRepository<PatActionDueInv>().Attach(updated);
            updated.ResponseDate = actionDue.ResponseDate;
            updated.UpdatedBy = actionDue.UpdatedBy;
            updated.LastUpdate = actionDue.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();
        }

        public override async Task Delete(PatActionDueInv actionDue)
        {
            await ValidatePermission(actionDue, CPiPermissions.CanDelete);
            await ValidateResponsibleAttorney(actionDue.ResponsibleID ?? 0);

            await base.Delete(actionDue);
        }

        private async Task ValidatePermission(PatActionDueInv actionDue, List<string> roles)
        {
            if ((await _userSettingManager.GetUserSetting<UserAccountSettings>(_user.GetUserIdentifier())).RestrictAdhocActions &&
                !string.IsNullOrEmpty(actionDue.ActionType) &&
                (actionDue.ActId == 0 ||
                !actionDue.ActionType.Equals((await GetByIdAsync(actionDue.ActId)).ActionType, StringComparison.InvariantCultureIgnoreCase)))
                Guard.Against.ValueNotAllowed(await _cpiDbContext
                                                        .GetReadOnlyRepositoryAsync<PatActionType>().QueryableList
                                                        .AnyAsync(at => at.ActionType == actionDue.ActionType), "Action Type");

            var actId = actionDue.ActId;
            var respOfc = "";
            if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Patent))
            {
                var item = new KeyValuePair<int, string>();
                if (actId > 0)
                {
                    item = (await QueryableList.Where(a => a.ActId == actId)
                                    .Select(a => new { a.InvId, a.Invention.RespOffice })
                                    .ToDictionaryAsync(a => a.InvId, c => c.RespOffice)).FirstOrDefault();
                }
                else
                {
                    item = (await _inventionService.Inventions
                                    .Where(inv => inv.CaseNumber == actionDue.CaseNumber)
                                    .Select(c => new { c.InvId, c.RespOffice })
                                    .ToDictionaryAsync(c => c.InvId, c => c.RespOffice)).FirstOrDefault();
                }

                Guard.Against.NoRecordPermission(item.Key > 0);

                respOfc = item.Value;
            }

            var settings = await _settings.GetSetting();
            if (settings.IsSoftDocketOn && actionDue.DueDateInvs != null && actionDue.DueDateInvs.Any(d => !string.IsNullOrEmpty(d.Indicator) && d.Indicator.ToLower() == "soft docket"))
                Guard.Against.NoRecordPermission(_user.IsSoftDocketUser() || _user.IsInRoles(SystemType.Patent, CPiPermissions.SoftDocket));
            else
                Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Patent, roles, respOfc));
        }

        private async Task ValidateResponsibleAttorney(int responsibleId)
        {
            if (responsibleId > 0 && _user.GetEntityFilterType() == CPiEntityType.Attorney)
                Guard.Against.ValueNotAllowed(await base.EntityFilterAllowed(responsibleId), "Responsible Attorney");
        }

        private async Task ValidateComputerGenerated(PatActionDueInv actionDue)
        {
            if (actionDue.ComputerGenerated)
            {
                var notAllowed = await QueryableList.AnyAsync(a => a.ActId == actionDue.ActId &&
                                            (
                                                a.CaseNumber != actionDue.CaseNumber ||
                                                a.ActionType != actionDue.ActionType ||
                                                a.BaseDate != actionDue.BaseDate
                                            ));
                Guard.Against.NoRecordPermission(!notAllowed);
            }
        }

        private async Task<Invention> ValidateInvention(PatActionDueInv actionDue)
        {
            var settings = await _settings.GetSetting();

            var invention = await _inventionService.Inventions
                .Where(inv =>
                    inv.CaseNumber == actionDue.CaseNumber)
                .SingleOrDefaultAsync();

            var caseNumberLabel = settings.LabelCaseNumber;
            Guard.Against.ValueNotAllowed(invention?.InvId > 0, $"{caseNumberLabel}/Country/Sub Case");

            if (_user.IsRespOfficeOn(SystemType.Patent))
                Guard.Against.ValueNotAllowed(await ValidateRespOffice(SystemType.Patent, invention.RespOffice), $"{caseNumberLabel}/Country/Sub Case");

            actionDue.InvId = invention.InvId;

            _cpiDbContext.GetRepository<Invention>().Attach(invention);
            invention.LastUpdate = actionDue.LastUpdate;
            invention.UpdatedBy = actionDue.UpdatedBy;

            return invention;
        }

        public async Task<bool> CanModifyAttorney(int responsibleId)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Attorney && responsibleId > 0)
                return await base.EntityFilterAllowed(responsibleId);
            else
                return true;
        }

        private IQueryable<PatDueDateInv> DueDateInvs => _cpiDbContext.GetRepository<PatDueDateInv>().QueryableList;

        /// <summary>
        /// Update or generate due dates.
        /// Returns true if follow up actions need to be generated.
        /// </summary>
        /// <param name="actionDue"></param>
        /// <returns></returns>
        private async Task<bool> UpdateDueDates(PatActionDueInv actionDue)
        {
            var dueDates = new List<PatDueDateInv>();
            var recurringDueDates = new List<PatDueDateInv>();

            var oldActionDue = await QueryableList.SingleOrDefaultAsync(a => a.ActId == actionDue.ActId);
            var responseDateChanged = oldActionDue.ResponseDate != actionDue.ResponseDate;
            var generateDueDates = oldActionDue.ActionType != actionDue.ActionType || oldActionDue.BaseDate != actionDue.BaseDate;
            var generateFollowUp = (responseDateChanged && actionDue.ResponseDate != null);

            if (generateDueDates)
            {
                //actionType or baseDate changed
                //regenerate due dates
                generateFollowUp = actionDue.ResponseDate != null;
                dueDates = await GenerateDueDates(actionDue);

                var oldDueDates = await DueDateInvs.Where(d => d.ActId == actionDue.ActId).ToListAsync();
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

                                if (generateRecurring)
                                {
                                    var recurringDueDate = await GetRecurringDueDate(actionDue, dueDate);
                                    if (recurringDueDate != null)
                                        recurringDueDates.Add(recurringDueDate);
                                }
                            }
                        }
                    }
                    //delete old DueDates
                    _cpiDbContext.GetRepository<PatDueDateInv>().Delete(oldDueDates);
                }
            }
            else
            {
                //update DueDates when VerifyDate or ResponseDate changed
                var verifyDateChanged = oldActionDue.VerifyDate != actionDue.VerifyDate;
                dueDates = await DueDateInvs.Where(d => d.ActId == actionDue.ActId &&
                                                        ((responseDateChanged && (d.DateTaken == null || d.DateTaken == oldActionDue.ResponseDate)) ||
                                                            verifyDateChanged
                                                        )).ToListAsync();

                if (responseDateChanged && actionDue.ResponseDate == null)
                {
                    //ResponseDate is updated to blank
                    //remove old follow up due date
                    var oldFollowUpAction = await GetFollowUpAction(oldActionDue);
                    if (oldFollowUpAction != null && oldFollowUpAction.ActId == oldActionDue.ActId)
                    {
                        var oldFollowUpDueDate = oldFollowUpAction.DueDateInvs.FirstOrDefault();
                        var oldFollowUps = await DueDateInvs.Where(d => d.ActId == actionDue.ActId && d.ActionDue == oldFollowUpDueDate.ActionDue).ToListAsync();

                        if (oldFollowUps.Any())
                        {
                            _cpiDbContext.GetRepository<PatDueDateInv>().Delete(oldFollowUps);
                            dueDates = dueDates.Where(d => !oldFollowUps.Any(f => f.ActionDue == d.ActionDue && f.DueDate == d.DueDate)).ToList();
                        }
                    }
                    else if (oldFollowUpAction != null)
                    {
                        var oldFollowUpActionDue = await _cpiDbContext.GetRepository<PatActionDueInv>().QueryableList
                                  .Where(a => a.InvId == oldFollowUpAction.InvId && a.ActionType == oldFollowUpAction.ActionType && a.BaseDate == oldFollowUpAction.BaseDate)
                                  .FirstOrDefaultAsync();

                        if (oldFollowUpActionDue != null)
                        {
                            var oldFollowUpDueDates = await DueDateInvs.Where(d => d.ActId == oldFollowUpActionDue.ActId).ToListAsync();

                            _cpiDbContext.GetRepository<PatDueDateInv>().Delete(oldFollowUpDueDates);
                            _cpiDbContext.GetRepository<PatActionDueInv>().Delete(oldFollowUpActionDue);
                        }
                    }

                    else
                    {
                        var settings = await _settings.GetSetting();
                        var autoFollowUp = settings.IsGenFollowUpOn;
                        if (autoFollowUp)
                        {
                            var autoFollowUpActionDesc = $"{actionDue.ActionType.Substring(0, actionDue.ActionType.Length >= 45 ? 45 : actionDue.ActionType.Length)}  Follow Up Date";
                            var existing = dueDates.Where(d => d.ActId == actionDue.ActId && d.ActionDue == autoFollowUpActionDesc).FirstOrDefault();
                            if (existing != null)
                            {
                                _cpiDbContext.GetRepository<PatDueDateInv>().Delete(existing);
                                dueDates.Remove(existing);
                            }
                        }
                    }
                }

                //update DueDates
                _cpiDbContext.GetRepository<PatDueDateInv>().Attach(dueDates);
                foreach (var dueDate in dueDates)
                {
                    var oldDateTaken = dueDate.DateTaken;

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
            actionDue.DueDateInvs = dueDates;
            _cpiDbContext.GetRepository<PatActionDueInv>().Update(actionDue);

            return generateFollowUp;
        }

        /// <summary>
        /// Generates DueDates based on ActionParameters when actionDue is based on ActionType.
        /// Generates DueDate based on actionDue when actionDue is not based on any ActionType.
        /// </summary>
        /// <param name="actionDue">The action due record that the due dates will be based on.</param>
        /// <returns></returns>
        private async Task<List<PatDueDateInv>> GenerateDueDates(PatActionDueInv actionDue)
        {
            var dueDates = new List<PatDueDateInv>();
            var actionType = await GetActionType(actionDue.ActionType, "");
            var actionParams = new List<PatActionParameter>();

            //actionDue is based on an ActionType
            //get ActionParameters
            if (actionType != null)
                actionParams = await _cpiDbContext.GetReadOnlyRepositoryAsync<PatActionParameter>().QueryableList
                                    .Where(ap => ap.ActionTypeID == actionType.ActionTypeID)
                                    .ToListAsync();

            if (actionParams.Any())
                //generate DueDates based on ActionParameters
                dueDates = actionParams.Select(ap => new PatDueDateInv()
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
                dueDates.Add(new PatDueDateInv()
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
        private async Task GenerateFollowUpAction(PatActionDueInv actionDue)
        {
            var followUpAction = await GetFollowUpAction(actionDue);
            if (followUpAction != null)
            {
                //if (followUpAction.ActId == actionDue.ActId) //foreign key error
                if (followUpAction.ActionType == actionDue.ActionType)
                {
                    var followUpDueDate = followUpAction.DueDateInvs.FirstOrDefault();

                    //insert one DueDate if DueDate does not exist
                    if (!actionDue.DueDateInvs.Any(d => d.ActionDue == followUpDueDate.ActionDue && d.DueDate == followUpDueDate.DueDate))
                        _cpiDbContext.GetRepository<PatDueDateInv>().Add(followUpDueDate);
                }
                else
                {
                    //insert new ActionDue with DueDates if ActionDue does not exist
                    var actions = await _cpiDbContext.GetRepository<PatActionDueInv>().QueryableList
                                    .Where(a => a.InvId == followUpAction.InvId && a.ActionType == followUpAction.ActionType && a.BaseDate == followUpAction.BaseDate)
                                    .ToListAsync();

                    if (!actions.Any() && !(actionDue.ActionType == followUpAction.ActionType && actionDue.BaseDate == followUpAction.BaseDate))
                    {
                        _cpiDbContext.GetRepository<PatActionDueInv>().Add(followUpAction);
                        actionDue.FollowUpAction = followUpAction.ActionType;
                    }

                }
            }
            else if (actionDue.ResponseDate != null)
            {
                var settings = await _settings.GetSetting();
                var autoFollowUp = settings.IsGenFollowUpOn;
                if (autoFollowUp)
                {
                    var termMonth = settings.FollowUpActionTermMon;
                    var termDay = settings.FollowUpActionTermDay;
                    var indicator = settings.FollowUpActionIndicator;
                    if (string.IsNullOrEmpty(indicator))
                        indicator = "Due Date";

                    var followUpActionDesc = $"{actionDue.ActionType.Substring(0, actionDue.ActionType.Length >= 45 ? 45 : actionDue.ActionType.Length)}  Follow Up Date";

                    var existing = await _cpiDbContext.GetRepository<PatDueDateInv>().QueryableList
                                       .Where(d => d.ActId == actionDue.ActId && d.ActionDue == followUpActionDesc)
                                       .FirstOrDefaultAsync();
                    if (existing != null)
                        _cpiDbContext.GetRepository<PatDueDateInv>().Delete(existing);

                    var followUpDueDate = new PatDueDateInv
                    {
                        ActId = actionDue.ActId,
                        ActionDue = followUpActionDesc,
                        Indicator = indicator,
                        DueDate = ((DateTime)actionDue.ResponseDate).AddMonths(termMonth).AddDays(termDay),
                        IsForVerify = false,
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now,
                        CreatedBy = actionDue.UpdatedBy,
                        UpdatedBy = actionDue.UpdatedBy
                    };
                    //_cpiDbContext.GetRepository<PatDueDate>().Add(followUpDueDate); //foreign key error
                    actionDue.DueDateInvs.Add(followUpDueDate);
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
        private async Task<PatActionDueInv> GetFollowUpAction(PatActionDueInv actionDue)
        {
            PatActionDueInv followUpAction = null;
            PatActionType actionType = null;

            if (actionDue.ComputerGenerated)
            {
                var invention = _cpiDbContext.GetReadOnlyRepositoryAsync<Invention>().QueryableList.Where(inv => inv.InvId == actionDue.InvId);
                var countryDue = _cpiDbContext.GetReadOnlyRepositoryAsync<PatCountryDue>().QueryableList
                                            .Where(cd =>
                                                    //invention.Any(inv => inv.Country == cd.Country && inv.CaseType == cd.CaseType) &&
                                                    cd.ActionType == actionDue.ActionType
                                                    //&&  cd.ActionDue == actionDue.ActionType //some are not equal
                                                    );
                //get ActionType using CDueId
                //todo: check effective period?
                actionType = await _cpiDbContext.GetReadOnlyRepositoryAsync<PatActionType>().QueryableList
                                            .Where(at => countryDue.Any(cd => cd.CDueId == at.CDueId))
                                            .FirstOrDefaultAsync();
                if (actionType != null)
                {
                    var sourceActionType = await _cpiDbContext.GetReadOnlyRepositoryAsync<PatActionType>().QueryableList
                                            .Where(at => (at.CDueId == 0 || at.CDueId == null) && at.ActionType == actionType.ActionType && (at.Country == actionType.Country || string.IsNullOrEmpty(at.Country)))
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
                actionType = await GetActionType(actionDue.ActionType, "");

            //follow up action type from computer generated actions does
            if (actionType != null && !string.IsNullOrEmpty(actionType.FollowUpMsg) && actionType.FollowUpGen != (short)FollowUpOption.DontGenerate)
            {
                if (string.IsNullOrEmpty(actionType.FollowUpIndicator))
                    actionType.FollowUpIndicator = "Due Date";

                //ActionType exists
                //get follow up ActionType based on FollowUpMsg
                PatActionType followUpActionType = null;

                if (actionDue.ComputerGenerated)
                    followUpActionType = actionType;

                else if (!string.IsNullOrEmpty(actionType.FollowUpMsg))
                    followUpActionType = await GetActionType(actionType.FollowUpMsg, "");

                if (followUpActionType != null)
                {
                    //follow up ActionType exists
                    //create new ActionDue
                    followUpAction = new PatActionDueInv()
                    {
                        InvId = actionDue.InvId,
                        CaseNumber = actionDue.CaseNumber,
                        ActionType = followUpActionType.ActionType,
                        BaseDate = actionType.FollowUpGen == (short)FollowUpOption.BaseDate ? actionDue.BaseDate : (DateTime)actionDue.ResponseDate,
                        ResponsibleID = followUpActionType.ResponsibleID > 0 ? followUpActionType.ResponsibleID : actionDue.ResponsibleID,
                        DateCreated = DateTime.Now,
                        LastUpdate = DateTime.Now,
                        CreatedBy = actionDue.UpdatedBy,
                        UpdatedBy = actionDue.UpdatedBy
                    };
                    //create new DueDates
                    followUpAction.DueDateInvs = await GenerateDueDates(followUpAction);
                }
                else
                {
                    //FollowUpMsg is blank or
                    //follow up ActionType does not exist
                    var followUpDueDate = actionType.FollowUpGen == (short)FollowUpOption.BaseDate ? actionDue.BaseDate : (DateTime)actionDue.ResponseDate;
                    var followUpActionDue = actionType.FollowUpMsg;

                    //generate follow up ActionDue description
                    if (string.IsNullOrEmpty(followUpActionDue))
                    {
                        followUpActionDue = actionDue.ActionType.Length > 45 ? actionDue.ActionType.Substring(0, 45) : actionDue.ActionType;
                        followUpActionDue = $"{followUpActionDue} Follow Up Date";
                    }

                    //do not create new ActionDue
                    //use same actId
                    followUpAction = new PatActionDueInv()
                    {
                        ActId = actionDue.ActId
                    };

                    //create new follow up DueDate
                    followUpAction.DueDateInvs = new List<PatDueDateInv>() {
                        new PatDueDateInv()
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
            //todo: more follow up

            return followUpAction;
        }

        public async Task<PatDueDateInv> GetRecurringDueDate(PatActionDueInv actionDue, PatDueDateInv dueDate)
        {
            if (actionDue.ComputerGenerated && dueDate.DateTaken != null)
            {
                var invention = _cpiDbContext.GetReadOnlyRepositoryAsync<Invention>().QueryableList.Where(inv => inv.InvId == actionDue.InvId);
                var countryDue = await _cpiDbContext.GetReadOnlyRepositoryAsync<PatCountryDue>().QueryableList
                                            .Where(cd =>
                                                    //invention.Any(inv => inv.Country == cd.Country && inv.CaseType == cd.CaseType) &&
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
                        var effBasedOn = await _cpiDbContext.GetReadOnlyRepositoryAsync<Invention>().QueryableList.Where(inv => inv.InvId == actionDue.InvId)
                                                .Select(inv => //countryDue.EffBasedOn == BasedOnOption.Filing ? inv.FilDate :
                                                //                countryDue.EffBasedOn == BasedOnOption.Issue ? inv.IssDate :
                                                //                countryDue.EffBasedOn == BasedOnOption.ParentFiling ? inv.ParentFilDate :
                                                //                countryDue.EffBasedOn == BasedOnOption.ParentIssue ? inv.ParentIssDate :
                                                //                countryDue.EffBasedOn == BasedOnOption.PCT ? inv.PCTDate :
                                                                countryDue.EffBasedOn == BasedOnOption.Priority ? inv.Priorities.Min(pri => pri.FilDate) :
                                                //                countryDue.EffBasedOn == BasedOnOption.Publication ? inv.PubDate : 
                                                                null)
                                                .FirstOrDefaultAsync();

                        hasRecurringDueDate = (effBasedOn != null &&
                            (countryDue.EffStartDate == null || effBasedOn >= countryDue.EffStartDate) &&
                            (countryDue.EffEndDate == null || effBasedOn <= countryDue.EffEndDate));
                    }

                    if (hasRecurringDueDate)
                    {
                        var baseDate = countryDue.Recurring == (short)RecurringOption.BasedOnDueDate ? dueDate.DueDate : (DateTime)dueDate.DateTaken;

                        return new PatDueDateInv()
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

        private async Task AddCustomDocFolder(PatActionDueInv actionDue)
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
            var actionType = await _cpiDbContext.GetReadOnlyRepositoryAsync<PatActionType>().QueryableList.Where(at => at.ActionTypeID == criteria.ActionTypeID).AsNoTracking().FirstOrDefaultAsync();
            if (actionType == null) return;

            var actionParam = await _cpiDbContext.GetReadOnlyRepositoryAsync<PatActionParameter>().QueryableList
                                            .Where(ap => ap.ActionTypeID == actionType.ActionTypeID && ap.ActionDue == criteria.ActionDue)
                                            .FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(criteria.ActionDue) && actionParam == null) return;

            var userName = _user.GetUserName();
            var today = DateTime.Now;


            if (criteria.ActParamId <= 0)
            {
                //TO DO: Get invIds
                var inventions = await _inventionService.Inventions
                                    //.Where(d => (!criteria.ActiveOnly || d.DisclosureStatus.ActiveSwitch)
                                    //    //&& (string.IsNullOrEmpty(criteria.Country) || d.Country == criteria.Country)
                                    //    //&& (criteria.CaseTypes == null || criteria.CaseTypes.Contains(d.CaseType))
                                    //    //&& (criteria.FilDateFrom == null || d.FilDate >= criteria.FilDateFrom)
                                    //    //&& (criteria.FilDateTo == null || d.FilDate <= criteria.FilDateTo)
                                    //    //&& (criteria.PubDateFrom == null || d.PubDate >= criteria.PubDateFrom)
                                    //    //&& (criteria.PubDateTo == null || d.PubDate <= criteria.PubDateTo)
                                    //    //&& (criteria.IssDateFrom == null || d.IssDate >= criteria.IssDateFrom)
                                    //    //&& (criteria.IssDateTo == null || d.IssDate <= criteria.IssDateTo)
                                    //)
                                    .ToListAsync();

                var actionDues = new List<PatActionDueInv>();

                foreach (var inv in inventions)
                {
                    var dupActionDue = await QueryableList.Where(d => d.InvId == inv.InvId && d.BaseDate.Date == criteria.BaseDate.Date && d.ActionType == actionType.ActionType)
                                            .Include(d => d.DueDateInvs).AsNoTracking().FirstOrDefaultAsync();

                    if (dupActionDue == null)
                    {
                        PatActionDueInv actionDue = new PatActionDueInv()
                        {
                            InvId = inv.InvId,
                            CaseNumber = inv.CaseNumber,
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
                            actionDue.DueDateInvs = await GenerateDueDates(actionDue);
                        }
                        ////Generate specific ActionDue/ActionParameter
                        //else if (!string.IsNullOrEmpty(criteria.ActionDue) && actionParam != null)
                        //{
                        //    actionDue.DueDateInvs = new List<PatDueDateInv>()
                        //    {
                        //        new PatDueDateInv()
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

                        if (criteria.DueDateCutOff != null && actionDue.DueDateInvs != null) actionDue.DueDateInvs.RemoveAll(d => d.DueDate.Date <= criteria.DueDateCutOff.Value.Date);

                        if (actionDue != null && actionDue.DueDateInvs != null) actionDues.Add(actionDue);
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
                            var newDueDates = actionParams.Where(d => dupActionDue.DueDateInvs == null || !dupActionDue.DueDateInvs.Any()
                                                || !dupActionDue.DueDateInvs.Any(a => a.ActionDue == d.ActionDue && a.DueDate == d.DueDate))
                                                .Select(d => new PatDueDateInv()
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
                                _cpiDbContext.GetRepository<PatDueDateInv>().Add(newDueDates);
                                await _cpiDbContext.SaveChangesAsync();
                            }
                        }
                        ////Generate specific ActionDue/ActionParameter that is not in the existing ActionDue record yet
                        //else if (!string.IsNullOrEmpty(criteria.ActionDue) && actionParam != null)
                        //{
                        //    var newDueDate = new PatDueDateInv()
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

                        //    if ((dupActionDue.DueDateInvs == null || !dupActionDue.DueDateInvs.Any(d => d.ActionDue == newDueDate.ActionDue && d.DueDate == newDueDate.DueDate)) && (criteria.DueDateCutOff == null || newDueDate.DueDate.Date > criteria.DueDateCutOff.Value.Date))
                        //    {
                        //        _cpiDbContext.GetRepository<PatDueDateInv>().Add(newDueDate);
                        //        await _cpiDbContext.SaveChangesAsync();
                        //    }
                        //}
                    }
                }

                if (actionDues.Any())
                {
                    _cpiDbContext.GetRepository<PatActionDueInv>().Add(actionDues);
                    await _cpiDbContext.SaveChangesAsync();
                }
            }
            //Generate new due dates for all existing inv action records
            else if (criteria.ActParamId > 0 && !string.IsNullOrEmpty(criteria.ActionDue) && actionParam != null)
            {                
                var existingActions = await QueryableList
                    .Where(ad => ad.Invention != null 
                        //&& (!criteria.ActiveOnly || (ad.Invention.PatApplicationStatus != null && ad.Invention.PatApplicationStatus.ActiveSwitch))
                        //&& (string.IsNullOrEmpty(criteria.Country) || ad.Invention.Country == criteria.Country)
                        //&& (criteria.CaseTypes == null || (ad.Invention.CaseType != null && criteria.CaseTypes.Contains(ad.Invention.CaseType)))
                        && ad.ActionType == actionType.ActionType
                        && (ad.DueDateInvs == null 
                                || !ad.DueDateInvs.Any(dd => dd.ActionDue == actionParam.ActionDue 
                                    && dd.DueDate == ad.BaseDate.AddYears(actionParam.Yr).AddMonths(actionParam.Mo).AddDays((double)actionParam.Dy)) 
                            )
                        && (criteria.DueDateCutOff == null || ad.BaseDate.AddYears(actionParam.Yr).AddMonths(actionParam.Mo).AddDays((double)actionParam.Dy).Date > criteria.DueDateCutOff.Value.Date)
                    )
                    //.Include(d => d.DueDates)
                    .AsNoTracking().ToListAsync();

                var newDueDateInvs = new List<PatDueDateInv>();

                foreach (var existingAct in existingActions) {
                    var newDueDate = new PatDueDateInv()
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

                    newDueDateInvs.Add(newDueDate);

                    //if ((existingAct.DueDates == null || !existingAct.DueDates.Any(d => d.ActionDue == newDueDate.ActionDue && d.DueDate == newDueDate.DueDate)) && (criteria.DueDateCutOff == null || newDueDate.DueDate.Date > criteria.DueDateCutOff.Value.Date))
                    //{
                    //    newDueDates.Add(newDueDate);
                    //}
                }

                if (newDueDateInvs != null && newDueDateInvs.Count > 0)
                {
                    _cpiDbContext.GetRepository<PatDueDateInv>().Add(newDueDateInvs);
                    await _cpiDbContext.SaveChangesAsync();
                }
            }
            
        }

        public async Task UpdateDeDocket(PatActionDueInv actionDue)
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

                _cpiDbContext.GetRepository<PatActionDueInv>().Update(updated);
                await _cpiDbContext.SaveChangesAsync();
            }
            else
                Guard.Against.UnAuthorizedAccess(false);
        }

        public Task UpdateCheckDocket(PatActionDueInv actionDue)
        {
            throw new NotImplementedException();
        }
    }
}
