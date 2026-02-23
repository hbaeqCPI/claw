using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.DMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace R10.Core.Services
{
    public class DMSActionDueService : EntityService<DMSActionDue>, IActionDueService<DMSActionDue, DMSDueDate>
    {
        private readonly IDisclosureService _disclosureService;
        private readonly ISystemSettings<DMSSetting> _settings;

        public DMSActionDueService(ICPiDbContext cpiDbContext,
            IDisclosureService countryAppService,
            ISystemSettings<DMSSetting> settings,
            ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _disclosureService = countryAppService;
            _settings = settings;
        }

        public override IQueryable<DMSActionDue> QueryableList
        {
            get
            {
                var actionsDue = base.QueryableList;

                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.DMS) || !_user.CanAccessDMSTradeSecret())
                    actionsDue = actionsDue.Where(a => _disclosureService.QueryableList.Any(d => d.DMSId== a.DMSId));

                return actionsDue;
            }
        }

        private IQueryable<DMSDueDate> DueDates => _cpiDbContext.GetRepository<DMSDueDate>().QueryableList;

        public override async Task<DMSActionDue> GetByIdAsync(int actId)
        {
            return await QueryableList.SingleOrDefaultAsync(a => a.ActId == actId);
        }

        public override async Task Add(DMSActionDue actionDue)
        {

            await ValidateResponsibleAttorney(actionDue.ResponsibleID ?? 0);
            await ValidateDisclosure(actionDue);

            actionDue.DueDates = await GenerateDueDates(actionDue);
            _cpiDbContext.GetRepository<DMSActionDue>().Add(actionDue);

            //if (actionDue.ResponseDate != null)
            //{
            //    await GenerateFollowUpAction(actionDue);
            //}
            await _cpiDbContext.SaveChangesAsync();
            
            //save the main action first before adding a followup
            if (actionDue.ResponseDate != null)
            {
                await GenerateFollowUpAction(actionDue);
                await _cpiDbContext.SaveChangesAsync();
            }
        }


        public override async Task Update(DMSActionDue actionDue)
        {
            await ValidatePermission(actionDue.ActId);
            await ValidateResponsibleAttorney(actionDue.ResponsibleID ?? 0);
            await ValidateDisclosure(actionDue);

            var dueDates = new List<DMSDueDate>();

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
                    _cpiDbContext.GetRepository<DMSDueDate>().Delete(oldDueDates);
                }
            }
            else
            {
                //update DueDates when VerifyDate or ResponseDate changed
                dueDates = await DueDates.Where(d => d.ActId == actionDue.ActId &&
                                                        ((responseDateChanged && (d.DateTaken == null || d.DateTaken == oldActionDue.ResponseDate)) 
                                                            || (actionDue.CloseDueDates && d.DateTaken == null)
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
                            _cpiDbContext.GetRepository<DMSDueDate>().Delete(oldFollowUps);
                            dueDates = dueDates.Where(d => !oldFollowUps.Any(f => f.ActionDue == d.ActionDue && f.DueDate == d.DueDate)).ToList();
                        }
                    }

                    else if (oldFollowUpAction != null)
                    {
                        var oldFollowUpActionDue = await _cpiDbContext.GetRepository<DMSActionDue>().QueryableList
                                  .Where(a => a.DMSId == oldFollowUpAction.DMSId && a.ActionType == oldFollowUpAction.ActionType && a.BaseDate == oldFollowUpAction.BaseDate)
                                  .FirstOrDefaultAsync();

                        if (oldFollowUpActionDue != null)
                        {
                            var oldFollowUpDueDates = await DueDates.Where(d => d.ActId == oldFollowUpActionDue.ActId).ToListAsync();

                            _cpiDbContext.GetRepository<DMSDueDate>().Delete(oldFollowUpDueDates);
                            _cpiDbContext.GetRepository<DMSActionDue>().Delete(oldFollowUpActionDue);
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
                                _cpiDbContext.GetRepository<DMSDueDate>().Delete(existing);
                                dueDates.Remove(existing);
                            }
                        }
                    }
                }

                //update DueDates
                if (dueDates.Any())
                    _cpiDbContext.GetRepository<DMSDueDate>().Attach(dueDates);

                foreach (var dueDate in dueDates)
                {
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

            //attach new or updated DueDates to actionDue
            actionDue.DueDates = dueDates;
            _cpiDbContext.GetRepository<DMSActionDue>().Update(actionDue);

            if (generateFollowUp)
            {
                //ResponseDate is updated and not blank
                await GenerateFollowUpAction(actionDue);
                //todo: generate recurring actions
            }

            await _cpiDbContext.SaveChangesAsync();
        }

        public override async Task UpdateRemarks(DMSActionDue actionDue)
        {
            var updated = await GetByIdAsync(actionDue.ActId);

            Guard.Against.NoRecordPermission(updated != null);
            //todo: pass permission to validate --> await ValidatePermission(updated, CPiPermissions.RemarksOnly);
            await ValidateDisclosure(actionDue);

            updated.tStamp = actionDue.tStamp;

            _cpiDbContext.GetRepository<DMSActionDue>().Attach(updated);
            updated.Remarks = actionDue.Remarks;
            updated.UpdatedBy = actionDue.UpdatedBy;
            updated.LastUpdate = actionDue.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task UpdateResponseDate(DMSActionDue actionDue)
        {
            await ValidatePermission(actionDue.ActId);

            var dueDates = new List<DMSDueDate>();

            var oldActionDue = await QueryableList.SingleOrDefaultAsync(a => a.ActId == actionDue.ActId);
            var responseDateChanged = oldActionDue.ResponseDate != actionDue.ResponseDate;
            var generateFollowUp = (responseDateChanged && actionDue.ResponseDate != null);

            //update DueDates when VerifyDate or ResponseDate changed
            dueDates = await DueDates.Where(d => d.ActId == actionDue.ActId &&
                                                    (responseDateChanged && (d.DateTaken == null || d.DateTaken == oldActionDue.ResponseDate)
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
                        _cpiDbContext.GetRepository<DMSDueDate>().Delete(oldFollowUps);
                        dueDates = dueDates.Where(d => !oldFollowUps.Any(f => f.ActionDue == d.ActionDue && f.DueDate == d.DueDate)).ToList();
                    }
                }
            }

            //update DueDates
            _cpiDbContext.GetRepository<DMSDueDate>().Attach(dueDates);
            foreach (var dueDate in dueDates)
            {
                //update DateTaken with ResponseDate if ResponseDate changed
                //and when DateTaken is blank or DateTaken is the same as old ResponseDate
                dueDate.DateTaken = responseDateChanged && (dueDate.DateTaken == null || dueDate.DateTaken == oldActionDue.ResponseDate) ? actionDue.ResponseDate : dueDate.DateTaken;
                dueDate.LastUpdate = actionDue.LastUpdate;
                dueDate.UpdatedBy = actionDue.UpdatedBy;
            }


            //attach new or updated DueDates to actionDue
            oldActionDue.DueDates = dueDates;
            _cpiDbContext.GetRepository<DMSActionDue>().Update(oldActionDue);

            if (generateFollowUp)
            {
                //ResponseDate is updated and not blank
                await GenerateFollowUpAction(oldActionDue);
            //todo: generate recurring actions
            }

            oldActionDue.tStamp = actionDue.tStamp;
            _cpiDbContext.GetRepository<DMSActionDue>().Attach(oldActionDue);
            oldActionDue.ResponseDate = actionDue.ResponseDate;
            oldActionDue.UpdatedBy = actionDue.UpdatedBy;
            oldActionDue.LastUpdate = actionDue.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();
        }

        public override async Task Delete(DMSActionDue actionDue)
        {
            await ValidatePermission(actionDue.ActId);
            await ValidateDisclosure(actionDue);

            //SQL performs cascade delete on DueDates
            //var dueDates = await DueDates.Where(d => d.ActId == actionDue.ActId).ToListAsync();
            //_cpiDbContext.GetRepository<DMSDueDate>().Delete(dueDates);

            await base.Delete(actionDue);
        }

        private async Task ValidatePermission(int actId)
        {
            if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.DMS))
                Guard.Against.NoRecordPermission(await QueryableList.AnyAsync(a => a.ActId == actId));
        }

        private async Task ValidateResponsibleAttorney(int responsibleId)
        {
            if (responsibleId > 0 && _user.GetEntityFilterType() == CPiEntityType.Attorney)
                Guard.Against.ValueNotAllowed(await base.EntityFilterAllowed(responsibleId), "Responsible Attorney");
        }

        /// <summary>
        /// Validates parent disclosure record.
        /// Returns ValueNotAllowedException if DisclosureNumber does not exists.
        /// Returns ValueNotAllowedException if user has no record permission.
        /// Updates LastUpdate and UpdateBy fields.
        /// </summary>
        /// <param name="actionDue">The action due record to validate.</param>
        /// <returns></returns>
        private async Task ValidateDisclosure(DMSActionDue actionDue)
        {
            var settings = await _settings.GetSetting();

            var disclosure = await _disclosureService.QueryableList
                .Where(d => d.DisclosureNumber == actionDue.DisclosureNumber || d.ActionDues.Any(a => a.ActId == actionDue.ActId))
                .SingleOrDefaultAsync();

            var disclosureNumberLabel = settings.LabelDisclosureNumber;
            Guard.Against.ValueNotAllowed(disclosure?.DMSId > 0, $"{disclosureNumberLabel}");

            actionDue.DMSId = disclosure.DMSId;

            _cpiDbContext.GetRepository<Disclosure>().Attach(disclosure);
            disclosure.LastUpdate = actionDue.LastUpdate;
            disclosure.UpdatedBy = actionDue.UpdatedBy;
        }

        public async Task<bool> CanModifyAttorney(int responsibleId)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Attorney && responsibleId > 0)
                return await base.EntityFilterAllowed(responsibleId);
            else
                return true;
        }

        /// <summary>
        /// Generates DueDates based on ActionParameters when actionDue is based on ActionType.
        /// Generates DueDate based on actionDue when actionDue is not based on any ActionType.
        /// </summary>
        /// <param name="actionDue">The action due record that the due dates will be based on.</param>
        /// <returns></returns>
        private async Task<List<DMSDueDate>> GenerateDueDates(DMSActionDue actionDue)
        {
            var dueDates = new List<DMSDueDate>();
            var actionType = await GetActionType(actionDue.ActionType);
            var actionParams = new List<DMSActionParameter>();

            //actionDue is based on an ActionType
            //get ActionParameters
            if (actionType != null)
                actionParams = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSActionParameter>().QueryableList
                                    .Where(ap => ap.ActionTypeID == actionType.ActionTypeID)
                                    .ToListAsync();

            if (actionParams.Any())
                //generate DueDates based on ActionParameters
                dueDates = actionParams.Select(ap => new DMSDueDate()
                {
                    ActId = actionDue.ActId,
                    ActionDue = ap.ActionDue,
                    //DueDate = actionDue.BaseDate.AddDays((double)ap.Dy).AddMonths(ap.Mo).AddYears(ap.Yr),
                    //DueDate = actionDue.BaseDate.AddMonths(ap.Mo).AddYears(ap.Yr).AddDays((double)ap.Dy),
                    
                    //proper leap year handling
                    DueDate = actionDue.BaseDate.AddYears(ap.Yr).AddMonths(ap.Mo).AddDays((double)ap.Dy),
                    DateTaken = actionDue.ResponseDate,
                    Indicator = ap.Indicator,
                    //AttorneyID =actionDue.ResponsibleID,
                    CreatedBy = actionDue.UpdatedBy,
                    DateCreated = actionDue.LastUpdate,
                    UpdatedBy = actionDue.UpdatedBy,
                    LastUpdate = actionDue.LastUpdate
                }).ToList();
            else
                //generate DueDate based on actionDue
                dueDates.Add(new DMSDueDate()
                {
                    ActId = actionDue.ActId,
                    ActionDue = actionDue.ActionType,
                    DueDate = actionDue.BaseDate,
                    DateTaken = actionDue.ResponseDate,
                    Indicator = "Due Date",
                    //AttorneyID = actionDue.ResponsibleID,
                    CreatedBy = actionDue.UpdatedBy,
                    DateCreated = actionDue.LastUpdate,
                    UpdatedBy = actionDue.UpdatedBy,
                    LastUpdate = actionDue.LastUpdate
                });

            return dueDates;
        }

        //todo: move to DMSActionType service
        /// <summary>
        /// Returns DMSActionType based on ActionType and Country.
        /// Retrieves ActionType with matching Country first then ActionType with blank Country.
        /// </summary>
        /// <param name="actionType">The action type to search.</param>
        /// <returns></returns>
        private async Task<DMSActionType> GetActionType(string actionType)
        {
            var actionTypes = _cpiDbContext.GetReadOnlyRepositoryAsync<DMSActionType>().QueryableList;

            actionTypes = actionTypes.Where(a => a.ActionType == actionType);

            return await actionTypes.FirstOrDefaultAsync();
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
        private async Task GenerateFollowUpAction(DMSActionDue actionDue)
        {
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
                        _cpiDbContext.GetRepository<DMSDueDate>().Add(followUpDueDate);
                    }
                        
                }
                else
                {
                    //insert new ActionDue with DueDates if ActionDue does not exist
                    var actions = await _cpiDbContext.GetRepository<DMSActionDue>().QueryableList
                                    .Where(a => a.DMSId == followUpAction.DMSId && a.ActionType == followUpAction.ActionType && a.BaseDate == followUpAction.BaseDate)
                                    .ToListAsync();

                    if (!actions.Any() && !(actionDue.ActionType == followUpAction.ActionType && actionDue.BaseDate == followUpAction.BaseDate))
                        _cpiDbContext.GetRepository<DMSActionDue>().Add(followUpAction);
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

                    var existing = await _cpiDbContext.GetRepository<DMSDueDate>().QueryableList
                                       .Where(d => d.ActId == actionDue.ActId && d.ActionDue == followUpActionDesc)
                                       .FirstOrDefaultAsync();
                    if (existing != null)
                        _cpiDbContext.GetRepository<DMSDueDate>().Delete(existing);

                    var followUpDueDate = new DMSDueDate
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
                    //_cpiDbContext.GetRepository<DMSDueDate>().Add(followUpDueDate); //foreign key error
                    actionDue.DueDates.Add(followUpDueDate);
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
        private async Task<DMSActionDue> GetFollowUpAction(DMSActionDue actionDue)
        {
            DMSActionDue followUpAction = null;
            DMSActionType actionType = null;

            
            //get ActionType using ActionType and country
            actionType = await GetActionType(actionDue.ActionType);

            if (actionType != null && actionType.FollowUpGen != (short)FollowUpOption.DontGenerate)
            {
                if (string.IsNullOrEmpty(actionType.FollowUpIndicator))
                    actionType.FollowUpIndicator = "Due Date";

                //ActionType exists
                //get follow up ActionType based on FollowUpMsg
                DMSActionType followUpActionType = null;

                if (!string.IsNullOrEmpty(actionType.FollowUpMsg))
                    followUpActionType = await GetActionType(actionType.FollowUpMsg);

                if (followUpActionType != null)
                {
                    //follow up ActionType exists
                    //create new ActionDue
                    followUpAction = new DMSActionDue()
                    {
                        DMSId = actionDue.DMSId,
                        DisclosureNumber = actionDue.DisclosureNumber,
                        ActionType = followUpActionType.ActionType,
                        BaseDate = actionType.FollowUpGen == (short)FollowUpOption.BaseDate ? actionDue.BaseDate : (DateTime)actionDue.ResponseDate,
                        ResponsibleID = actionDue.ResponsibleID,
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
                        followUpAction = new DMSActionDue()
                        {
                            DMSId = actionDue.DMSId,
                            DisclosureNumber = actionDue.DisclosureNumber,
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
                        followUpActionDue = actionDue.ActionType.Length > 45 ? actionDue.ActionType.Substring(0, 45) : actionDue.ActionType;
                        followUpActionDue = $"{followUpActionDue} Follow Up Date";
                    }

                    //do not create new ActionDue
                    //use same actId
                    followUpAction = new DMSActionDue()
                    {
                        ActId = actionDue.ActId,
                        ActionType = actionDue.ActionType
                    };

                    //create new follow up DueDate
                    followUpAction.DueDates = new List<DMSDueDate>() {
                        new DMSDueDate()
                        {
                            ActId = actionDue.ActId,
                            ActionDue = followUpActionDue,
                            DueDate = followUpDueDate.AddMonths(actionType.FollowUpMonth).AddDays(actionType.FollowUpDay),
                            Indicator = actionType.FollowUpIndicator,
                            //AttorneyID = actionDue.ResponsibleID,
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

        public Task<DMSDueDate> GetRecurringDueDate(DMSActionDue actionDue, DMSDueDate dueDate)
        {
            throw new NotImplementedException();
        }

        public async Task RetroGenerateActionDues(ActionDueRetroParam criteria)
        {
            var actionType = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSActionType>().QueryableList.Where(at => at.ActionTypeID == criteria.ActionTypeID).AsNoTracking().FirstOrDefaultAsync();
            if (actionType == null) return;

            var actionParam = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSActionParameter>().QueryableList
                                            .Where(ap => ap.ActionTypeID == actionType.ActionTypeID && ap.ActionDue == criteria.ActionDue)
                                            .FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(criteria.ActionDue) && actionParam == null) return;

            var userName = _user.GetUserName();
            var today = DateTime.Now;

            //Generate new action records if generating from ActionType level
            if (criteria.ActParamId <= 0)
            {
                var disclosures = await _disclosureService.QueryableList
                                    .Where(d => (criteria.Statuses == null || (d.DisclosureStatus != null && criteria.Statuses.Contains(d.DisclosureStatus)))
                                        && (criteria.StatusDateFrom == null || d.DisclosureStatusDate >= criteria.StatusDateFrom)
                                        && (criteria.StatusDateTo == null || d.DisclosureStatusDate <= criteria.StatusDateTo)
                                        && (criteria.DisclosureDateFrom == null || d.DisclosureDate >= criteria.DisclosureDateFrom)
                                        && (criteria.DisclosureDateTo == null || d.DisclosureDate <= criteria.DisclosureDateTo)                                   
                                    )
                                    .ToListAsync();

                var newActionDues = new List<DMSActionDue>();

                foreach (var disc in disclosures)
                {
                    var dupActionDue = await QueryableList.Where(d => d.DMSId == disc.DMSId && d.BaseDate.Date == criteria.BaseDate.Date && d.ActionType == actionType.ActionType)
                                            .Include(d => d.DueDates).AsNoTracking().FirstOrDefaultAsync();

                    if (dupActionDue == null)
                    {
                        DMSActionDue actionDue = new DMSActionDue()
                        {
                            DMSId = disc.DMSId,
                            DisclosureNumber = disc.DisclosureNumber,                                                
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
                        //    actionDue.DueDates = new List<DMSDueDate>()
                        //    {
                        //        new DMSDueDate()
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

                        if (criteria.DueDateCutOff != null && actionDue.DueDates != null) actionDue.DueDates.RemoveAll(d => d.DueDate <= criteria.DueDateCutOff);

                        if (actionDue != null && actionDue.DueDates != null) newActionDues.Add(actionDue);
                    }
                    else
                    {
                        //Generate all ActionDues/ActionParameters that are not in the existing ActionDue record yet
                        if (string.IsNullOrEmpty(criteria.ActionDue))
                        {
                            var actionParams = await _cpiDbContext.GetReadOnlyRepositoryAsync<DMSActionParameter>().QueryableList
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
                                                .Select(d => new DMSDueDate()
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
                                _cpiDbContext.GetRepository<DMSDueDate>().Add(newDueDates);
                                await _cpiDbContext.SaveChangesAsync();
                            }
                        }
                        ////Generate specific ActionDue/ActionParameter that is not in the existing ActionDue record yet
                        //else if (!string.IsNullOrEmpty(criteria.ActionDue) && actionParam != null)
                        //{
                        //    var newDueDate = new DMSDueDate()
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
                        //        _cpiDbContext.GetRepository<DMSDueDate>().Add(newDueDate);
                        //        await _cpiDbContext.SaveChangesAsync();
                        //    }
                        //}
                    }
                }

                if (newActionDues.Any())
                {
                    _cpiDbContext.GetRepository<DMSActionDue>().Add(newActionDues);
                    await _cpiDbContext.SaveChangesAsync();
                }
            }
            //Generate new due dates for all existing action records
            else if (criteria.ActParamId > 0 && !string.IsNullOrEmpty(criteria.ActionDue) && actionParam != null)
            {                
                var existingActions = await QueryableList
                    .Where(ad => ad.Disclosure != null 
                        && (criteria.Statuses == null || (ad.Disclosure.DisclosureStatus != null && criteria.Statuses.Contains(ad.Disclosure.DisclosureStatus)))
                        && ad.ActionType == actionType.ActionType
                        && (ad.DueDates == null 
                                || !ad.DueDates.Any(dd => dd.ActionDue == actionParam.ActionDue 
                                    && dd.DueDate == ad.BaseDate.AddYears(actionParam.Yr).AddMonths(actionParam.Mo).AddDays((double)actionParam.Dy)) 
                            )
                        && (criteria.DueDateCutOff == null || ad.BaseDate.AddYears(actionParam.Yr).AddMonths(actionParam.Mo).AddDays((double)actionParam.Dy).Date > criteria.DueDateCutOff.Value.Date)
                    )
                    //.Include(d => d.DueDates)
                    .AsNoTracking().ToListAsync();

                var newDueDates = new List<DMSDueDate>();

                foreach (var existingAct in existingActions) {
                    var newDueDate = new DMSDueDate()
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
                    _cpiDbContext.GetRepository<DMSDueDate>().Add(newDueDates);
                    await _cpiDbContext.SaveChangesAsync();
                }
            }


        }

        public Task UpdateCheckDocket(DMSActionDue actionDue)
        {
            throw new NotImplementedException();
        }
    }
    
}
