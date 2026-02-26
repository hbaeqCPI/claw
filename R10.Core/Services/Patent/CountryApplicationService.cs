using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using R10.Core.Interfaces.Patent;
using R10.Core.Entities.Patent;
using R10.Core.DTOs;
using R10.Core.Identity;
using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using R10.Core.Exceptions;
using System.Security.Claims;
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Core.Helpers;
using static System.Net.Mime.MediaTypeNames;
// using R10.Core.Services.GeneralMatter; // Removed during deep clean
using Microsoft.EntityFrameworkCore.ChangeTracking;
using R10.Core.Entities.ReportScheduler;
using System.Diagnostics.Metrics;


namespace R10.Core.Services
{
    public class CountryApplicationService : ICountryApplicationService
    {
        private readonly ICountryApplicationRepository _countryAppRepository;
        private readonly IApplicationDbContext _repository;
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly ICPiSystemSettingManager _systemSettingManager;
        private readonly ClaimsPrincipal _user;

        public readonly string _terminalDisclaimerAction = "Terminal Disclaimer Expiration";

        public CountryApplicationService(
            ICountryApplicationRepository countryAppRepository,
            IApplicationDbContext repository,
            ISystemSettings<PatSetting> settings,
            ICPiSystemSettingManager systemSettingManager,
            ClaimsPrincipal user)
        {
            _countryAppRepository = countryAppRepository;
            _repository = repository;
            _settings = settings;
            _systemSettingManager = systemSettingManager;
            _user = user;
        }

        public async Task<CountryApplication> GetById(int appId)
        {
            var application = await CountryApplications.FirstOrDefaultAsync(c => c.AppId == appId); //consider entity filter, etc..
            if (application != null)
                return application;
            else throw new NoRecordPermissionException();
        }

        public async Task ValidateRecordFilterPermission(int appId)
        {
            Guard.Against.NoRecordPermission(await CountryApplications.AnyAsync(c => c.AppId == appId)); //consider entity filter, etc..
        }

        public async Task AddCountryApplication(CountryApplication countryApplication, PatIDSRelatedCasesInfo idsInfo, DateTime dateCreated, bool hasRelatedCasesMassCopy, string? sessionKey)
        {
            countryApplication.SubCase = countryApplication.SubCase ?? "";
            await ValidateCountryApplicationInsert(countryApplication);
            await AddDefaults(countryApplication);
            var modifiedFields = SetActionFieldsAsModified();
            await _countryAppRepository.Add(countryApplication, idsInfo, modifiedFields, dateCreated, hasRelatedCasesMassCopy, sessionKey);
        }

        public async Task UpdateCountryApplication(CountryApplication countryApplication, PatIDSRelatedCasesInfo idsInfo, DateTime dateCreated, bool hasRelatedCasesMassCopy, string? sessionKey)
        {
            countryApplication.SubCase = countryApplication.SubCase ?? "";
            await ValidateCountryApplicationUpdate(countryApplication);
            await AddDefaults(countryApplication);
            var modifiedFields = await GetModifiedFields(countryApplication);
            await _countryAppRepository.Update(countryApplication, idsInfo, modifiedFields, dateCreated, hasRelatedCasesMassCopy,sessionKey);
        }

        public async Task DeleteCountryApplication(CountryApplication countryApplication, bool validateRecordFilter = true)
        {
            //check if has access to the record
            if (validateRecordFilter)
                await ValidateRecordFilterPermission(countryApplication.AppId);

            await _countryAppRepository.Delete(countryApplication);
        }

        public async Task CopyCountryApplication(int oldAppId, int newAppId, bool copyImages, bool copyAssignments,
            bool copyInventors, bool copyLicenses, bool copyOwners, bool copyCosts, bool copyIDS, bool copyRelatedCases,
            bool copyRelatedTrademarks, bool copyInventorAward, bool copyProducts, bool copyterminalDisclaimer, string userName)
        {
            await _countryAppRepository.CopyCountryApplication(oldAppId, newAppId, copyImages, copyAssignments,
                copyInventors, copyLicenses, copyOwners, copyCosts, copyIDS, copyRelatedCases, copyRelatedTrademarks, copyInventorAward, copyProducts, copyterminalDisclaimer,userName);
        }

        public async Task GenerateCountryLawFromPriority(int invId, string userName)
        {
            await _countryAppRepository.GenerateCountryLawFromPriority(invId, userName);
        }

        public async Task UpdateExpirationDate(List<PatTerminalDisclaimerChildDTO> children, string updatedBy)
        {
            await _countryAppRepository.UpdateExpirationDate(children, updatedBy);
        }

        public async Task UpdateChild<T>(int appId, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity
        {
            var application = await GetById(appId);
            await _countryAppRepository.UpdateChild(application, userName, updated, added, deleted);
        }

        public async Task SyncChildToDesignatedApplications(int appId, string country, string caseType, string userName, Type childType) {
            await _countryAppRepository.SyncChildToDesignatedApplications(appId, country, caseType, userName, childType);
        }

        public async Task<List<PatActionMultipleBasedOnDTO>> GetActionsWithMultipleBasedOn(int appId, string? sessionKey) {
            return await _countryAppRepository.GetActionsWithMultipleBasedOn(appId, sessionKey);
        }

        public async Task GenerateActionsWithMultipleBasedOn(List<PatActionMultipleBasedOnSelectionDTO> list, string? createdBy)
        {
             await _countryAppRepository.GenerateActionsWithMultipleBasedOn(list, createdBy);
        }

        public IQueryable<CountryApplication> CountryApplications
        {
            get
            {
                var applications = _repository.CountryApplications.AsNoTracking();

                if (_user.HasRespOfficeFilter(SystemType.Patent))
                    applications = applications.Where(RespOfficeFilter());

                if (_user.HasEntityFilter())
                    applications = applications.Where(EntityFilter());

                if (_user.RestrictExportControl())
                    applications = applications.Where(ca => !(ca.ExportControl ?? false));

                if (!_user.CanAccessPatTradeSecret())
                    applications = applications.Where(ca => _repository.Inventions.AsNoTracking().Any(i => !(i.IsTradeSecret ?? false) && i.InvId == ca.InvId));

                return applications;
            }
        }

        public async Task<CountryApplication> GetInventorAwardInfo(int appId)
        {
            var appAwardInfo = await _repository.CountryApplications.Where(ca => ca.AppId == appId)
                                                    .Include(ca => ca.PatApplicationStatus)
                                                    .Include(ca => ca.Inventors)
                                                    .Include(ca => ca.Invention)
                                                    .Include(ca => ca.Awards)
                                                    .AsNoTracking().FirstOrDefaultAsync();
            return appAwardInfo;
        }

        public async Task RefreshCopySetting(List<CountryApplicationCopySetting> added, List<CountryApplicationCopySetting> deleted)
        {
            if (added.Count > 0)
                _repository.CountryApplicationCopySettings.AddRange(added);

            if (deleted.Count > 0) {
                //_repository.CountryApplicationCopySettings.RemoveRange(deleted);
                foreach (var item in deleted) {
                    await _repository.CountryApplicationCopySettings.Where(cs =>  item.CopySettingId == cs.CopySettingId).ExecuteDeleteAsync();
                }
            }
            
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateCopySetting(CountryApplicationCopySetting setting)
        {
            var existing = await _repository.CountryApplicationCopySettings.FirstOrDefaultAsync(s => s.CopySettingId == setting.CopySettingId);
            if (existing != null)
            {
                existing.Copy = setting.Copy;
                _repository.CountryApplicationCopySettings.Update(existing);
                await _repository.SaveChangesAsync();
            }
        }

        public async Task AddCopySettings(List<CountryApplicationCopySetting> settings)
        {
            if (settings.Count > 0)
            {
                _repository.CountryApplicationCopySettings.AddRange(settings);
                await _repository.SaveChangesAsync();
            }
        }

        public async Task<CPiUserSetting> GetMainCopySettings(string userId)
        {
            var setting = await _repository.CPiSettings.Where(d => d.Name == "CountryApplicationCopySetting").AsNoTracking().FirstOrDefaultAsync();
            if (setting == null)
            {
                setting = new CPiSetting { Name = "CountryApplicationCopySetting", Policy = "*" };
                _repository.CPiSettings.Add(setting);
                await _repository.SaveChangesAsync();
            }
            return await _repository.CPiUserSettings.Where(u => u.UserId == userId && u.SettingId == setting.Id).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task UpdateMainCopySettings(CPiUserSetting userSetting)
        {
            if (userSetting.Id > 0)
                _repository.CPiUserSettings.Update(userSetting);
            else
                _repository.CPiUserSettings.Add(userSetting);
            await _repository.SaveChangesAsync();
        }

        public async Task<int> GetMainCopySettingId()
        {
            var setting = await _repository.CPiSettings.Where(d => d.Name == "CountryApplicationCopySetting").AsNoTracking().FirstOrDefaultAsync();
            if (setting != null)
            {
                return setting.Id;
            }
            else
            {
                setting = new CPiSetting { Name = "CountryApplicationCopySetting", Policy = "*" };
                _repository.CPiSettings.Add(setting);
                await _repository.SaveChangesAsync();
                return setting.Id;
            }
        }

        public async Task GenerateWorkflowFromEmailSent(int appId, int qeSetupId)
        {
            var workflowActions = (await CheckWorkflowAction(PatWorkflowTriggerType.EmailSent)).Where(wf => (wf.Workflow.SystemScreen == null || wf.Workflow.SystemScreen.ScreenCode.ToLower() == "ca-workflow") && (wf.Workflow.TriggerValueId==0 || wf.Workflow.TriggerValueId==qeSetupId)).ToList();
            if (workflowActions.Any()) {
                var application = await CountryApplications.Where(c => c.AppId == appId).Include(c => c.Invention).FirstOrDefaultAsync();
                workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || w.Workflow.ClientFilter.Contains("|" + application.Invention.ClientID.ToString() + "|")).ToList();

                if (workflowActions.Any())
                {
                    //client specific will override the base
                    foreach (var item in workflowActions.Where(wf => !string.IsNullOrEmpty(wf.Workflow.ClientFilter)).ToList())
                    {
                        workflowActions.RemoveAll(bf => string.IsNullOrEmpty(bf.Workflow.ClientFilter) && bf.ActionTypeId == item.ActionTypeId);
                    }

                    var createActionWorkflows = workflowActions.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CreateAction).Distinct().ToList();
                    foreach (var item in createActionWorkflows)
                    {
                        await GenerateWorkflowAction(application.AppId, item.ActionValueId, DateTime.Now);
                    }
                }
            }
        }

        public async Task GenerateWorkflowFromActionEmailSent(int actId, int qeSetupId)
        {
            var workflowActions = (await CheckWorkflowAction(PatWorkflowTriggerType.EmailSent)).Where(wf => (wf.Workflow.SystemScreen == null || wf.Workflow.SystemScreen.ScreenCode.ToLower() == "act-workflow") && (wf.Workflow.TriggerValueId == 0 || wf.Workflow.TriggerValueId == qeSetupId)).ToList();
            if (workflowActions.Any())
            {
                var actionDue = await _repository.PatActionDues.Where(a => a.ActId == actId).Include(a => a.CountryApplication).ThenInclude(c => c.Invention).FirstOrDefaultAsync();
                workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || w.Workflow.ClientFilter.Contains("|" + actionDue.CountryApplication.Invention.ClientID.ToString() + "|")).ToList();
                if (workflowActions.Any())
                {
                    //client specific will override the base
                    foreach (var item in workflowActions.Where(wf => !string.IsNullOrEmpty(wf.Workflow.ClientFilter)).ToList())
                    {
                        workflowActions.RemoveAll(bf => string.IsNullOrEmpty(bf.Workflow.ClientFilter) && bf.ActionTypeId == item.ActionTypeId);
                    }

                    var createActionWorkflows = workflowActions.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CreateAction).Distinct().ToList();
                    foreach (var item in createActionWorkflows)
                    {
                        await GenerateWorkflowAction(actionDue.AppId, item.ActionValueId, DateTime.Now);
                    }
                }
            }
        }

        public async Task GenerateWorkflowAction(int appId, int actionTypeId,DateTime baseDate)
        {
            baseDate = baseDate.Date;
            var application = await CountryApplications.Where(c => c.AppId == appId).Include(c => c.Invention).AsNoTracking().FirstOrDefaultAsync();

            var actionType = await _repository.PatActionTypes.Where(at => at.ActionTypeID == actionTypeId).AsNoTracking().FirstOrDefaultAsync();
            if (actionType == null) return;

            var dupActionDue = await _repository.PatActionDues.Where(a => a.AppId == appId && a.BaseDate.Date == baseDate.Date && a.ActionType == actionType.ActionType).AsNoTracking().FirstOrDefaultAsync();
            if (dupActionDue == null)
            {
                var actionDue = await GenerateAction(application, actionType,baseDate);
                _repository.PatActionDues.Add(actionDue);

                var isActionFileIDS = actionType.ActionType.ToUpper().Contains("FILE IDS") && application.Country.ToUpper() == "US" && application.CaseType != "PRO" && application.ApplicationStatus.ToLower() != "issued" && application.ApplicationStatus.ToLower() != "granted";
                if (isActionFileIDS)
                {
                    actionDue.CreatedBy = "Auto-IDS";
                    actionDue.UpdatedBy = actionDue.CreatedBy;
                    actionDue.DueDates.ForEach(d=> { d.CreatedBy = actionDue.CreatedBy; d.UpdatedBy = actionDue.UpdatedBy; });
                    await GenerateFileIDSAction(application, actionType,actionDue);
                }
                await _repository.SaveChangesAsync();
            }
        }

        public async Task<List<PatDueDate>> GenerateDueDateFromActionParameterWorkflow(PatActionDue? newActionDue, List<PatDueDate> dueDates, PatWorkflowTriggerType triggerType, bool clearBase = true)
        {
            var workflowActionParameters = await CheckWorkflowActionParameters(triggerType);
            if (!workflowActionParameters.Any())
                return null;

            var dueDateindicators = dueDates.Select(d => d.Indicator).ToList();
            var indicators = await PatIndicators.ToListAsync();
            workflowActionParameters = workflowActionParameters.Where(w => indicators.Any(i => i.IndicatorId == w.Workflow.TriggerValueId && dueDateindicators.Any(s => s.ToLower() == i.Indicator.ToLower()))).ToList();

            if (!workflowActionParameters.Any())
                return null;

            var application = new CountryApplication();
            
            //from duedate grid insert
            if (newActionDue.ActId > 0 && string.IsNullOrEmpty(newActionDue.CaseNumber))
            {
                newActionDue = await _repository.PatActionDues.Where(ad => ad.ActId == newActionDue.ActId).Include(ad => ad.CountryApplication).ThenInclude(ca => ca.Invention).AsNoTracking().FirstOrDefaultAsync();
                if (newActionDue != null)
                    application = newActionDue.CountryApplication;
                else
                    application = null;
            }
            else if (!string.IsNullOrEmpty(newActionDue.CaseNumber)) {
                application = await _repository.CountryApplications.Where(ca => ca.CaseNumber == newActionDue.CaseNumber && ca.Country == newActionDue.Country && ca.SubCase == newActionDue.SubCase).Include(ca => ca.Invention).AsNoTracking().FirstOrDefaultAsync();
            }

            if (application != null)
            {
                workflowActionParameters = workflowActionParameters.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || (w.Workflow.ClientFilter != null && w.Workflow.ClientFilter.Contains("|" + application.Invention.ClientID.ToString() + "|"))).ToList();
                workflowActionParameters = workflowActionParameters.Where(w => string.IsNullOrEmpty(w.Workflow.CountryFilter) || (w.Workflow.CountryFilter != null && w.Workflow.CountryFilter.Contains("|" + application.Country + "|"))).ToList();
                workflowActionParameters = workflowActionParameters.Where(w => string.IsNullOrEmpty(w.Workflow.CaseTypeFilter) || (w.Workflow.CaseTypeFilter != null && w.Workflow.CaseTypeFilter.Contains("|" + application.CaseType + "|"))).ToList();
                workflowActionParameters = workflowActionParameters.Where(w => string.IsNullOrEmpty(w.Workflow.RespOfficeFilter) || (w.Workflow.RespOfficeFilter != null && w.Workflow.RespOfficeFilter.Contains("|" + application.RespOffice + "|"))).ToList();

                workflowActionParameters = workflowActionParameters.Where(w => string.IsNullOrEmpty(w.Workflow.AttorneyFilter) || (w.Workflow.AttorneyFilter != null &&
                                   (w.Workflow.AttorneyFilter.Contains("|" + application.Invention.Attorney1ID.ToString() + "|") ||
                                    w.Workflow.AttorneyFilter.Contains("|" + application.Invention.Attorney2ID.ToString() + "|") ||
                                    w.Workflow.AttorneyFilter.Contains("|" + application.Invention.Attorney3ID.ToString() + "|") ||
                                    w.Workflow.AttorneyFilter.Contains("|" + application.Invention.Attorney4ID.ToString() + "|") ||
                                    w.Workflow.AttorneyFilter.Contains("|" + application.Invention.Attorney5ID.ToString() + "|")
                                   ))).ToList();
                if (clearBase)
                {
                    workflowActionParameters = ClearPatBaseWorkflowActionParameters(workflowActionParameters);
                }

                if (!workflowActionParameters.Any())
                    return null;

                var newDueDates = new List<PatDueDate>();
                var basedOns = dueDates.Where(dd => indicators.Any(i => i.Indicator.ToLower() == dd.Indicator.ToLower() && workflowActionParameters.Any(wf => wf.Workflow.TriggerValueId == i.IndicatorId))).ToList();

                foreach (var dd in basedOns)
                {
                    foreach (var item in workflowActionParameters.Where(w => indicators.Any(i => i.IndicatorId == w.Workflow.TriggerValueId && i.Indicator.ToLower() == dd.Indicator.ToLower())).ToList())
                    {
                        //based on DueDate 
                        var computedDueDate = dd.DueDate.AddYears(item.Yr).AddMonths(item.Mo).AddDays((double)item.Dy);

                        //proper leap year handling
                        //var computedDueDate = actionDue.BaseDate.AddYears(ap.Yr).AddMonths(ap.Mo).AddDays((double)ap.Dy),

                        //make sure it is non existing
                        if (!dueDates.Any(edd => edd.ActionDue.ToLower() == item.ActionDue.ToLower() && edd.DueDate == computedDueDate) &&
                            !newDueDates.Any(ndd => ndd.ActionDue.ToLower() == item.ActionDue.ToLower() && ndd.DueDate == computedDueDate)) {
                            var dueDate = new PatDueDate()
                            {
                                ActId = 0,
                                ActionDue = item.ActionDue,
                                DueDate = computedDueDate,
                                DateTaken = newActionDue.ResponseDate,
                                IsVerifyDate = newActionDue.VerifyDate,
                                Indicator = item.Indicator,
                                AttorneyID = newActionDue.ResponsibleID,
                                CreatedBy = newActionDue.UpdatedBy,
                                DateCreated = newActionDue.LastUpdate,
                                UpdatedBy = newActionDue.UpdatedBy,
                                LastUpdate = newActionDue.LastUpdate
                            };
                            newDueDates.Add(dueDate);
                        }
                    }
                }
                return newDueDates;
            }
            return null;
        }

        public async Task<List<PatDueDate>> GetUpdatedDueDateIndicator(int actId, List<PatDueDate> dueDates) {
            var existingDueDates = await _repository.PatDueDates.Where(dd => dd.ActId == actId).AsNoTracking().ToListAsync();
            if (existingDueDates.Any()) {

                var updatedDueDates = dueDates.Where(udd => existingDueDates.Any(dd => udd.DDId == dd.DDId && udd.Indicator.ToLower() != dd.Indicator.ToLower())).ToList();
                return updatedDueDates;
            }
            return null;
        }


        private List<PatWorkflowActionParameter> ClearPatBaseWorkflowActionParameters(List<PatWorkflowActionParameter> workflowActions)
        {

            //with filter will override the record with no filter at all
            foreach (var item in workflowActions.Where(wf => !(string.IsNullOrEmpty(wf.Workflow.ClientFilter) && string.IsNullOrEmpty(wf.Workflow.CountryFilter) && string.IsNullOrEmpty(wf.Workflow.CaseTypeFilter)
                                                              && string.IsNullOrEmpty(wf.Workflow.RespOfficeFilter) && string.IsNullOrEmpty(wf.Workflow.AttorneyFilter))).ToList())
            {
                workflowActions.RemoveAll(bf => string.IsNullOrEmpty(bf.Workflow.ClientFilter) && string.IsNullOrEmpty(bf.Workflow.CountryFilter) && string.IsNullOrEmpty(bf.Workflow.CaseTypeFilter)
                                                              && string.IsNullOrEmpty(bf.Workflow.RespOfficeFilter) && string.IsNullOrEmpty(bf.Workflow.AttorneyFilter) && bf.ActionDue == item.ActionDue && bf.Workflow.TriggerValueId == item.Workflow.TriggerValueId && (bf.Workflow.TriggerValueName ?? "") == (item.Workflow.TriggerValueName ?? ""));
            }
            return workflowActions;
        }


        public async Task<List<PatWorkflowAction>> CheckWorkflowAction(PatWorkflowTriggerType triggerType)
        {
            var actions = await _repository.PatWorkflowActions.Where(w => w.Workflow.TriggerTypeId == (int)triggerType && w.Workflow.ActiveSwitch).Include(w => w.Workflow).ThenInclude(w => w.SystemScreen).OrderBy(w => w.OrderOfEntry).ToListAsync();
            return actions;
        }

        public async Task<List<PatWorkflowActionParameter>> CheckWorkflowActionParameters(PatWorkflowTriggerType triggerType)
        {
            var actionParameters = await _repository.PatWorkflowActionParameters.Where(w => w.Workflow.TriggerTypeId == (int)triggerType && w.Workflow.ActiveSwitch).Include(w => w.Workflow).ToListAsync();
            return actionParameters;
        }

        public async Task<bool> HasWorkflowEnabled(PatWorkflowTriggerType triggerType)
        {
            return await _repository.PatWorkflows.AnyAsync(wf => wf.TriggerTypeId == (int)triggerType && wf.ActiveSwitch);
        }
        public async Task AddCustomFieldsAsCopyFields() { 
             await _countryAppRepository.AddCustomFieldsAsCopyFields();
        }

        private async Task<PatActionDue> GenerateAction(CountryApplication application,PatActionType actionType, DateTime baseDate)
        {
            PatActionDue actionDue = new PatActionDue() { AppId = application.AppId, CaseNumber = application.CaseNumber, Country = application.Country, SubCase = application.SubCase, ActionType = actionType.ActionType, BaseDate = baseDate.Date, ResponsibleID = null, IsOfficeAction = true, CreatedBy = _user.GetUserName(), UpdatedBy = _user.GetUserName(), DateCreated = DateTime.Now, LastUpdate = DateTime.Now };

            var dueDates = new List<PatDueDate>();
            var actionParams = await _repository.PatActionParameters.Where(ap => ap.ActionTypeID == actionType.ActionTypeID).AsNoTracking().ToListAsync();

            if (actionParams.Any())
                dueDates = actionParams.Select(ap => new PatDueDate()
                {
                    ActId = actionDue.ActId,
                    ActionDue = ap.ActionDue,
                    DueDate = actionDue.BaseDate.AddDays((double)ap.Dy).AddMonths(ap.Mo).AddYears(ap.Yr),
                    DateTaken = actionDue.ResponseDate,
                    Indicator = ap.Indicator,
                    CreatedBy = actionDue.UpdatedBy,
                    DateCreated = actionDue.LastUpdate,
                    UpdatedBy = actionDue.UpdatedBy,
                    LastUpdate = actionDue.LastUpdate
                }).ToList();
            else
                dueDates.Add(new PatDueDate()
                {
                    ActId = actionDue.ActId,
                    ActionDue = actionDue.ActionType,
                    DueDate = actionDue.BaseDate,
                    DateTaken = actionDue.ResponseDate,
                    Indicator = "Due Date",
                    CreatedBy = actionDue.UpdatedBy,
                    DateCreated = actionDue.LastUpdate,
                    UpdatedBy = actionDue.UpdatedBy,
                    LastUpdate = actionDue.LastUpdate
                });

            actionDue.DueDates = dueDates;

            var dueDatesFromIndicatorWorkflow = await GenerateDueDateFromActionParameterWorkflow(actionDue, actionDue.DueDates, PatWorkflowTriggerType.Indicator);
            if (dueDatesFromIndicatorWorkflow != null && dueDatesFromIndicatorWorkflow.Any())
            {
                actionDue.DueDates.AddRange(dueDatesFromIndicatorWorkflow);
            }
            return actionDue;
        }

        
        private async Task GenerateFileIDSAction(CountryApplication application, PatActionType actionType,PatActionDue parentActionDue)
        {
            var familyMembers = await _repository.CountryApplications.Where(ca => ca.AppId != application.AppId && ca.Country == "US" && ca.CaseType != "PRO" && ca.PatApplicationStatus.ActiveSwitch &&
                                ca.ApplicationStatus.ToLower() != "issued" && ca.ApplicationStatus.ToLower() != "granted" &&
                                (ca.CaseNumber == application.CaseNumber || (!string.IsNullOrEmpty(application.Invention.FamilyNumber) && ca.Invention.FamilyNumber == application.Invention.FamilyNumber) ||
                                _repository.PatRelatedCases.Any(rc => rc.AppId == application.AppId && rc.RelatedAppId == ca.AppId && (rc.RelationshipType == "Family" || rc.RelationshipType == "Subject Matter"))
                                )).ToListAsync();

            foreach (var app in familyMembers)
            {
                var dupActionDue = await _repository.PatActionDues.Where(a => a.AppId == app.AppId && a.BaseDate.Date == DateTime.Now.Date && a.ActionType == actionType.ActionType).AsNoTracking().FirstOrDefaultAsync();
                if (dupActionDue == null) {
                    var actionDue = await GenerateAction(app, actionType,DateTime.Now);
                    actionDue.Remarks = $"Regarding references cited in office action dated {parentActionDue.BaseDate.ToString("dd-MMM-yyyy")} in related case {application.CaseNumber} {application.Country} {application.SubCase}";
                    actionDue.CreatedBy = parentActionDue.CreatedBy;
                    actionDue.UpdatedBy = parentActionDue.UpdatedBy;
                    actionDue.DueDates.ForEach(d => { d.CreatedBy = actionDue.CreatedBy; d.UpdatedBy = actionDue.UpdatedBy; });

                    _repository.PatActionDues.Add(actionDue);
                }
            }
        }


        public async Task<List<PatActionDue>> CloseWorkflowAction(int appId, int actionTypeId)
        {
            var application = await GetById(appId);
            var actionDues = new List<PatActionDue>();  

            if (actionTypeId != 0)
            {
                var actionType = await _repository.PatActionTypes.Where(at => at.ActionTypeID == actionTypeId).AsNoTracking().FirstOrDefaultAsync();
                if (actionType != null)
                {
                    //var actionDue = await _repository.PatActionDues.Where(a => a.AppId == appId && a.ActionType == actionType.ActionType && a.ResponseDate == null).FirstOrDefaultAsync();
                    //if (actionDue != null)
                    //{
                    //    actionDue.ResponseDate = DateTime.Now.Date;
                    //    actionDue.UpdatedBy = _user.GetUserName();
                    //    actionDue.LastUpdate = DateTime.Now;

                    //    var dueDates = await _repository.PatDueDates.Where(d => d.ActId == actionDue.ActId && d.DateTaken == null)
                    //                        .ToListAsync();
                    //    foreach (var dueDate in dueDates)
                    //    {
                    //        dueDate.DateTaken = DateTime.Now.Date;
                    //        dueDate.LastUpdate = actionDue.LastUpdate;
                    //        dueDate.UpdatedBy = actionDue.UpdatedBy;
                    //    }
                    //    await _repository.SaveChangesAsync();
                    //}
                    actionDues = await _repository.PatActionDues.Where(a => a.AppId == appId && a.ActionType == actionType.ActionType && (a.ResponseDate == null || a.DueDates.Any(dd=> dd.DateTaken==null))).Include(a => a.DueDates).AsNoTracking().ToListAsync();
                }

            }
            //all outstanding actions
            if (actionTypeId == 0)
            {
                actionDues = await _repository.PatActionDues.Where(a => a.AppId == appId && (a.ResponseDate == null || a.DueDates.Any(dd => dd.DateTaken == null))).Include(a => a.DueDates).AsNoTracking().ToListAsync();
            }

            if (actionDues.Any())
            {
                foreach (var actionDue in actionDues)
                {
                    //for all outstanding actions, we want to close everything and avoid followup
                    if (actionTypeId != 0)
                    {
                        if (actionDue.ResponseDate == null)
                        {
                            actionDue.ResponseDate = DateTime.Now.Date;
                            actionDue.UpdatedBy = _user.GetUserName();
                            actionDue.LastUpdate = DateTime.Now;
                        }
                    }
                    actionDue.CloseDueDates = true;

                }
            }
            return actionDues;
        }

        public IQueryable<T> QueryableChildList<T>() where T : BaseEntity
        {
            var queryableList = _repository.Set<T>() as IQueryable<T>;

            if (_user.HasRespOfficeFilter(SystemType.Patent) || _user.HasEntityFilter())
                queryableList = queryableList.Where(a => this.CountryApplications.Any(ca => ca.AppId == EF.Property<int>(a, "AppId")));

            return queryableList;
        }

        public IQueryable<PatParentCaseDTO> ParentApplications
        {
            get
            {
                var parentCases = _repository.PatParentCaseDTO.AsNoTracking();
                if (_user.HasRespOfficeFilter(SystemType.Patent) || _user.HasEntityFilter())
                {
                    parentCases =
                        parentCases.Where(p => this.CountryApplications.Any(a => a.ParentAppId == p.ParentId));
                }

                return parentCases;
            }
        }

        public IQueryable<PatParentCaseTDDTO> TerminalDisclaimerParents
        {
            get
            {
                var parentCases = _repository.PatParentCaseTDDTO.AsNoTracking();
                if (_user.HasRespOfficeFilter(SystemType.Patent) || _user.HasEntityFilter())
                {
                    parentCases =
                        parentCases.Where(p => this.CountryApplications.Any(a => a.ParentAppId == p.ParentId));
                }

                return parentCases;
            }
        }

        public IQueryable<PatCountry> PatCountries => _repository.PatCountries.AsNoTracking();
        public IQueryable<Agent> Agents => _repository.Agents.AsNoTracking();
        public IQueryable<Attorney> Attorneys => _repository.Attorneys.AsNoTracking();
        public IQueryable<Client> Clients => _repository.Clients.AsNoTracking();
        public IQueryable<Owner> Owners => _repository.Owners.AsNoTracking();
        public IQueryable<Product> Products => _repository.Products.AsNoTracking();
        // Removed during deep clean
        // public IQueryable<GMMatterPatent> GMMatterPatents => _repository.GMMatterPatents.AsNoTracking();
        public IQueryable<PatCountryLaw> PatCountryLaws => _repository.PatCountryLaws.AsNoTracking();
        public IQueryable<PatCountryDue> PatCountryDues => _repository.PatCountryDues.AsNoTracking();
        public IQueryable<PatActionType> PatActionTypes => _repository.PatActionTypes.AsNoTracking();
        public IQueryable<PatIndicator> PatIndicators => _repository.PatIndicators.AsNoTracking();
        public IQueryable<PatActionDue> PatActionDues => _repository.PatActionDues.AsNoTracking();
        public IQueryable<PatDueDate> PatDueDates => _repository.PatDueDates.AsNoTracking();

        public IQueryable<PatActionParameter> PatActionParameters => _repository.PatActionParameters.AsNoTracking();
        public IQueryable<PatAssignmentHistory> PatAssignmentsHistory => _repository.PatAssignmentsHistory.AsNoTracking();
        public IQueryable<CountryApplicationCopySetting> CountryApplicationCopySettings => _repository.CountryApplicationCopySettings.AsNoTracking();
        public IQueryable<PatRelatedCaseDTO> PatRelatedCaseDTO => _repository.PatRelatedCaseDTO.AsNoTracking();
        public IQueryable<PatApplicationStatus> ApplicationStatuses => _repository.ApplicationStatuses.AsNoTracking();

        public List<CountryApplicationCopySettingChild> CountryApplicationCopySettingsChild => GetCountryApplicationCopySettingChild();

        private bool IsMultipleOwners => _settings.GetSetting().Result.IsMultipleOwnerOn;

        //owner and inventor are auto added from the invention level
        public bool IsOwnerRequired => false;
        public bool IsInventorRequired => false;

        protected Expression<Func<CountryApplication, bool>> RespOfficeFilter()
        {
            return a => _repository.CPiUserSystemRoles.AsNoTracking().Any(r => r.UserId == _user.GetUserIdentifier() && r.SystemId == SystemType.Patent && a.RespOffice == r.RespOffice && !string.IsNullOrEmpty(r.RespOffice));
        }

        public Expression<Func<CountryApplication, bool>> EntityFilter()
        {
            string userIdentifier = _user.GetUserIdentifier();
            var userEntityFilters = _repository.CPiUserEntityFilters.AsNoTracking();

            switch (_user.GetEntityFilterType())
            {
                case CPiEntityType.Client:
                    return a => userEntityFilters.Any(f =>
                        f.UserId == userIdentifier && f.EntityId == a.Invention.ClientID);

                case CPiEntityType.Agent:
                    return a => userEntityFilters.Any(f => f.UserId == userIdentifier && f.EntityId == a.AgentID);

                case CPiEntityType.Owner:
                    return a => userEntityFilters.Any(f => f.UserId == userIdentifier && a.Owners.Any(o => o.OwnerID == f.EntityId));

                //if (IsMultipleOwners)
                //  return a => userEntityFilters.Any(f => f.UserId == userIdentifier && a.Owners.Any(o => o.OwnerID == f.EntityId));
                //else
                //return a => userEntityFilters.Any(f => f.UserId == userIdentifier && f.EntityId == a.OwnerID);

                case CPiEntityType.Attorney:
                    return a => userEntityFilters.Any(f =>
                        f.UserId == userIdentifier && (f.EntityId == a.Invention.Attorney1ID ||
                                                       f.EntityId == a.Invention.Attorney2ID ||
                                                       f.EntityId == a.Invention.Attorney3ID ||
                                                       f.EntityId == a.Invention.Attorney4ID ||
                                                       f.EntityId == a.Invention.Attorney5ID));
                case CPiEntityType.Inventor:
                    return a => userEntityFilters.Any(f =>
                        f.UserId == userIdentifier && a.Inventors.Any(i => i.InventorID == f.EntityId));

                case CPiEntityType.ContactPerson:
                    return a => userEntityFilters.Any(f => f.UserId == userIdentifier && (a.Invention.Client.ClientContacts.Any(d => d.ContactID == f.EntityId)));
            }
            return null;
        }

        public async Task<List<LookupDTO>> GetAllowedRespOffices()
        {
            return await GetAllowedRespOffices(_user.GetUserIdentifier(),
                SystemType.Patent);
        }

        public async Task<List<CPiUserEntityFilter>> GetUserEntityFilters()
        {
            return await _repository.CPiUserEntityFilters.AsNoTracking().Where(f => f.UserId == _user.GetUserIdentifier()).ToListAsync();
        }

        public async Task<string> GetTaxScheduleLabel(string country, string caseType)
        {
            var countryLaw = await _repository.PatCountryLaws.FirstOrDefaultAsync(c => c.Country == country && c.CaseType == caseType);
            if (countryLaw != null)
                return countryLaw.LabelTaxSched;

            return "Tax Schedule";
        }

        public async Task<bool> ShouldLockRecord(int appId)
        {
            var app = await _repository.CountryApplications.Where(c => c.AppId == appId).Include(c => c.PatCaseType).FirstOrDefaultAsync();
            if (app != null)
                return app.PatCaseType.LockPatRecord ?? false;
            return false;
        }

        public async Task TerminalDisclaimerAddAction(PatActionDue actionDue, DateTime expirationDate)
        {
            actionDue.ActionType = _terminalDisclaimerAction;

            actionDue.DueDates = new List<PatDueDate>();
            actionDue.DueDates.Add(new PatDueDate
            {
                ActionDue = actionDue.ActionType,
                DueDate = expirationDate,
                Indicator = "Due Date",
                CreatedBy = actionDue.CreatedBy,
                UpdatedBy = actionDue.CreatedBy,
                DateCreated = DateTime.Now,
                LastUpdate = DateTime.Now
            });
            _repository.PatActionDues.Add(actionDue);
            await _repository.SaveChangesAsync();
        }

        public async Task<bool> HasTerminalDisclaimerAction(int appId)
        {
            var hasTDAction = await _repository.PatActionDues.AnyAsync(a => a.AppId == appId && a.ActionType == _terminalDisclaimerAction);
            return hasTDAction;
        }

        protected async Task AddDefaults(CountryApplication countryApplication)
        {
            countryApplication.ApplicationStatus = string.IsNullOrEmpty(countryApplication.ApplicationStatus) ? "Unfiled" : countryApplication.ApplicationStatus;
            countryApplication.InvId = await _repository.Inventions.AsNoTracking().Where(i => i.CaseNumber == countryApplication.CaseNumber)
                    .Select(i => i.InvId).FirstOrDefaultAsync();

            if (!(countryApplication.InvId > 0))
            {
                var settings = await _settings.GetSetting();
                var caseNumberLabel = settings.LabelCaseNumber;
                throw new NoRecordPermissionException($"{caseNumberLabel} is not on file or you don't have permission to this record.");
            }
        }

        //protected async Task ComputeStatus(CountryApplication countryApplication)
        //{
        //    var newStatus = countryApplication.ApplicationStatus;

        //    if (string.IsNullOrEmpty(countryApplication.ApplicationStatus) ||
        //        await _repository.ApplicationStatuses.AsNoTracking().AnyAsync(s =>
        //            s.ApplicationStatus == countryApplication.ApplicationStatus && s.CPIAppStatus))
        //    {
        //        if (!(countryApplication.IssDate == null && string.IsNullOrEmpty(countryApplication.PatNumber)))
        //        {
        //            newStatus = "Granted";
        //        }
        //        else if (!(countryApplication.PubDate == null && string.IsNullOrEmpty(countryApplication.PubNumber)))
        //        {
        //            newStatus = "Published";
        //        }
        //        else if (!(countryApplication.FilDate == null && string.IsNullOrEmpty(countryApplication.AppNumber)))
        //        {
        //            newStatus = "Pending";
        //        }
        //        else
        //        {
        //            newStatus = "Unfiled";
        //        }
        //    }

        //    if (newStatus != countryApplication.ApplicationStatus)
        //    {
        //        if (newStatus.ToLower() == "granted" && (countryApplication.IssDate != null))
        //        {
        //            countryApplication.ApplicationStatusDate = countryApplication.IssDate.Value.Date;
        //        }
        //        else if (newStatus.ToLower() == "published" && (countryApplication.PubDate != null))
        //        {
        //            countryApplication.ApplicationStatusDate = countryApplication.PubDate.Value.Date;
        //        }
        //        else if (newStatus.ToLower() == "pending" && (countryApplication.FilDate != null))
        //        {
        //            countryApplication.ApplicationStatusDate = countryApplication.FilDate.Value.Date;
        //        }
        //        else
        //        {
        //            countryApplication.ApplicationStatusDate = DateTime.Today;
        //        }
        //    }

        //    countryApplication.ApplicationStatus = newStatus;
        //    countryApplication.SubCase = countryApplication.SubCase ?? "";
        //    countryApplication.InvId = await _repository.Inventions.AsNoTracking().Where(i => i.CaseNumber == countryApplication.CaseNumber)
        //            .Select(i => i.InvId).FirstOrDefaultAsync();

        //}

        public async Task<ApplicationModifiedFields> GetModifiedFields(CountryApplication modified)
        {
            var existing = await this.CountryApplications.Where(c => c.AppId == modified.AppId)
                .FirstOrDefaultAsync();
            var modifiedFields = new ApplicationModifiedFields
            {
                KeyModified = (existing.CaseNumber != modified.CaseNumber || existing.Country != modified.Country ||
                               existing.SubCase != modified.SubCase),
                IsChgCaseType = existing.CaseType != modified.CaseType,
                IsChgFilDate = existing.FilDate != modified.FilDate,
                IsChgPubDate = existing.PubDate != modified.PubDate,
                IsChgIssDate = existing.IssDate != modified.IssDate,
                IsChgParentFilDate = existing.ParentFilDate != modified.ParentFilDate,
                IsChgParentIssDate = existing.ParentIssDate != modified.ParentIssDate,
                IsChgPCTDate = existing.PCTDate != modified.PCTDate,
                IsChgPatNumber = existing.PatNumber != modified.PatNumber,
                IsChgPriorityDate = false
            };
            return modifiedFields;
        }

        protected ApplicationModifiedFields SetActionFieldsAsModified()
        {

            var modifiedFields = new ApplicationModifiedFields
            {
                IsChgCaseType = true,
                IsChgFilDate = true,
                IsChgPubDate = true,
                IsChgIssDate = true,
                IsChgParentFilDate = true,
                IsChgParentIssDate = true,
                IsChgPCTDate = true,
                IsChgPriorityDate = true
            };
            return modifiedFields;
        }

        protected List<CountryApplicationCopySettingChild> GetCountryApplicationCopySettingChild()
        {
            var list = new List<CountryApplicationCopySettingChild>();
            list.Add(new CountryApplicationCopySettingChild { FromField = "FilDate", ToField = "ParentFilDate" });
            list.Add(new CountryApplicationCopySettingChild { FromField = "AppNumber", ToField = "ParentAppNumber" });
            list.Add(new CountryApplicationCopySettingChild { FromField = "IssDate", ToField = "ParentIssDate" });
            list.Add(new CountryApplicationCopySettingChild { FromField = "PatNumber", ToField = "ParentPatNumber" });
            list.Add(new CountryApplicationCopySettingChild { FromField = "Country", ToField = "ParentFilCountry" });
            list.Add(new CountryApplicationCopySettingChild { FromField = "AppTitle", ToField = "AppTitle" });
            list.Add(new CountryApplicationCopySettingChild { FromField = "PCTNumber", ToField = "PCTNumber" });
            list.Add(new CountryApplicationCopySettingChild { FromField = "PCTDate", ToField = "PCTDate" });
            list.Add(new CountryApplicationCopySettingChild { FromField = "RespOffice", ToField = "RespOffice" });
            return list;
        }

        #region Validation

        protected async Task ValidateCountryApplicationInsert(CountryApplication countryApplication)
        {
            await ValidateCountryApplication(countryApplication);
        }

        protected async Task ValidateCountryApplicationUpdate(CountryApplication countryApplication)
        {
            //check if has access to the record
            var existing = await CountryApplications.FirstOrDefaultAsync(c => c.AppId == countryApplication.AppId);
            if (existing == null)
                throw new NoRecordPermissionException();

            //if status is not part of the auto compute
            if (existing.ApplicationStatus != countryApplication.ApplicationStatus)
            {
                if (countryApplication.ApplicationStatusDate == null)
                    countryApplication.ApplicationStatusDate = DateTime.Now.Date;
            }


            await ValidateCountryApplication(countryApplication);
        }

        protected async Task ValidateCountryApplication(CountryApplication countryApplication)
        {
            var userIdentifier = _user.GetUserIdentifier();

            //check if value entered is allowed
            if (_user.HasRespOfficeFilter(SystemType.Patent))
            {
                var allowed = await RespOfficeAllowed(userIdentifier, countryApplication.RespOffice);
                Guard.Against.ValueNotAllowed(allowed, "Responsible Office");
            }

            var settings = await _settings.GetSetting();

            if (_user.HasEntityFilter())
            {
                var entityFilterType = _user.GetEntityFilterType();

                string label = "";
                if (entityFilterType == CPiEntityType.Agent)
                {
                    label = settings.LabelAgent;
                    Guard.Against.Null(countryApplication.AgentID, label);
                }

                //owner and inventor are auto added from the invention level
                //else if (entityFilterType == CPiEntityType.Owner)
                //{
                //    label = settings.LabelOwner;
                //    Guard.Against.Null(countryApplication.OwnerID, label);
                //}
                var allowed = await EntityFilterAllowed(userIdentifier, entityFilterType, countryApplication);
                if (!allowed)
                {
                    Guard.Against.ValueNotAllowed(false, label);
                }

            }

            var hasBillingEnabled = settings.IsBillingNoOn;
            if (hasBillingEnabled)
            {
                Guard.Against.NullOrEmpty(countryApplication.BillingNumber, "Billing Number");
            }

        }

        //owner and inventor are auto added from the invention level
        public async Task<bool> EntityFilterAllowed(string userIdentifier, CPiEntityType entityFilterType,
            CountryApplication application)
        {
            if (entityFilterType != CPiEntityType.Agent)
            {
                return true;
            }
            return await EntityFilterAllowed(userIdentifier, application.AgentID);
        }

        #endregion

        #region Designation

        public async Task<bool> CanHaveDesignatedCountry(string country, string caseType)
        {
            return await _countryAppRepository.CanHaveDesignatedCountry(country, caseType);
        }

        public async Task<object[]> GetSelectableDesignatedCountries(string country, string caseType, int appId)
        {
            return await _countryAppRepository.GetSelectableDesignatedCountries(country, caseType, appId);
        }

        public async Task<string[]> GetSelectableDesignatedCaseTypes(string country, string caseType, string desCountry)
        {
            return await _countryAppRepository.GetSelectableDesignatedCaseTypes(country, caseType, desCountry);
        }

        public async Task<List<PatParentCaseDTO>> GetPossibleFamilyReferences(int appId, string caseNumber)
        {
            return await _countryAppRepository.GetPossibleFamilyReferences(appId, caseNumber);
        }

        public async Task<List<FamilyTreeParentCaseDTO>> GetPossibleFamilyTreeReferences(int appId, string caseNumber)
        {
            return await _countryAppRepository.GetPossibleFamilyTreeReferences(appId, caseNumber);
        }

        public async Task<List<PatParentCaseDTO>> GetAllPossibleTerminalDisclaimer(int appId) {
            return await _countryAppRepository.GetAllPossibleTerminalDisclaimer(appId);
        }

        public async Task<int> GetActiveTerminalDisclaimerAppId(int appId) {
            return await _countryAppRepository.GetActiveTerminalDisclaimerAppId(appId);
        }

        public async Task<List<PatTerminalDisclaimerChildDTO>> GetTerminalDisclaimerChildren(int appId) {
            return await _countryAppRepository.GetTerminalDisclaimerChildren(appId);
        }
        public async Task DesignateCountries(int appId, bool fromCountryLaw, string createdBy)
        {
            await _countryAppRepository.DesignateCountries(appId, fromCountryLaw, createdBy);
        }

        public async Task<List<PatDesignatedCountry>> GetSelectableCountries(int appId)
        {
            return await _countryAppRepository.GetSelectableCountries(appId);
        }

        public async Task GenerateApplications(int parentAppId, string desCountries, string updatedBy)
        {
            await _countryAppRepository.GenerateApplications(parentAppId, desCountries, updatedBy);
        }

        public async Task MarkDesCountriesWithExistingApps(int appId)
        {
            await _countryAppRepository.MarkDesCountriesWithExistingApps(appId);
        }

        #endregion


        #region Related Cases

        public async Task<List<PatRelatedCaseDTO>> GetRelatedCases(int appId)
        {
            return await _countryAppRepository.GetRelatedCases(appId);
        }

        public async Task<bool> HasRelatedCases(int appId)
        {
            return await _repository.PatRelatedCases.AnyAsync(p => p.AppId == appId || p.RelatedAppId == appId);
        }
        public async Task<bool> HasOutstandingDedocket(int appId)
        {
            return await _repository.PatActionDues.AnyAsync(ad=> ad.AppId==appId && ad.DueDates.Any(dd=> dd.DeDocketOutstanding !=null));
        }

        #endregion

        #region Family Tree View

        public async Task<IEnumerable<FamilyTreeDTO>> GetFamilyTree(string paramType, string paramValue, string paramParent)
        {
            return await _countryAppRepository.GetFamilyTree(paramType, paramValue, paramParent);
        }

        public FamilyTreePatDTO GetNodeDetails(string paramType, string paramValue)
        {
            return _countryAppRepository.GetNodeDetails(paramType, paramValue);
        }

        public void UpdateParent(int childAppId, int newParentId, string parentInfo, string userName)
        {
            _countryAppRepository.UpdateParent(childAppId, newParentId, parentInfo, userName);
        }

        //public  string GetExpandedNodes(string paramType, string paramValue)
        //{
        //    return _countryAppRepository.GetExpandedNodes(paramType, paramValue);
        //}
        #endregion

        #region Licensees
        public async Task<bool> HasLicensees(int appId)
        {
            return await _repository.PatLicensees.AnyAsync(l => l.AppId == appId);
        }
        #endregion

        #region Products
        public async Task<bool> HasProducts(int appId)
        {
            return await _repository.PatProducts.AnyAsync(p => p.AppId == appId);
        }
        #endregion

        #region Filters 
        //todo: put in a common service?
        protected async Task<bool> RespOfficeAllowed(string userIdentifier, string respOffice)
        {
            return await _repository.CPiUserSystemRoles.AsNoTracking().AnyAsync(r => r.UserId == userIdentifier && r.SystemId == SystemType.Patent && r.RespOffice == respOffice);
        }

        protected async Task<List<LookupDTO>> GetAllowedRespOffices(string userIdentifier, string systemType)
        {
            if (_user.HasRespOfficeFilter(systemType))
                return await _repository.CPiRespOffices.Where(ro => ro.UserSystemRoles.Any(r => r.UserId == userIdentifier && r.SystemId == systemType && r.RespOffice == ro.RespOffice))
                                        .Select(r => new LookupDTO() { Value = r.RespOffice, Text = r.RespOffice }).ToListAsync();
            return await _repository.CPiRespOffices.AsNoTracking().Select(r => new LookupDTO() { Value = r.RespOffice, Text = r.RespOffice }).ToListAsync();
        }

        protected async Task<bool> EntityFilterAllowed(string userIdentifier, int? entityId)
        {
            return await _repository.CPiUserEntityFilters.AsNoTracking().AnyAsync(f => f.UserId == userIdentifier && f.EntityId == entityId);
        }
        #endregion

        #region Action
        public async Task<List<DelegationEmailDTO>> GetDelegationEmails(int delegationId) {
            return await _countryAppRepository.GetDelegationEmails(delegationId);
        }
        public async Task MarkDelegationasEmailed(int delegationId) {
            await _countryAppRepository.MarkDelegationasEmailed(delegationId);
        }
        public async Task<List<LookupIntDTO>> GetDelegatedDdIds(int action, int[] recIds) {
            return await _countryAppRepository.GetDelegatedDdIds(action, recIds);
        }
        public async Task<List<DelegationEmailDTO>> GetDeletedDelegationEmails(int delegationId) {
            return await _countryAppRepository.GetDeletedDelegationEmails(delegationId);
        }
        public async Task<List<LookupIntDTO>> GetDuedateChangedDelegationIds(int action, List<PatDueDate> updated) {
            return await _countryAppRepository.GetDuedateChangedDelegationIds(action,updated);
        }

        public async Task<DelegationDetailDTO> GetDeletedDelegation(int delegationId) {
            return await _countryAppRepository.GetDeletedDelegation(delegationId);
        }

        public IQueryable<DeDocketInstruction> DeDocketInstructions
        {
            get
            {
                return _repository.DeDocketInstructions.AsNoTracking();
            }
        }
        #endregion
        #region Unitary Patent
        public async Task<bool> ShouldShowUnitaryPatentFields(int action, string country, string caseType, int appId)
        {
            var result = await _countryAppRepository.ShouldShowUnitaryPatentFields(action, country, caseType, appId);
            return result == 1;
        }
        public async Task<int> GetUnitaryPatentDesignatedCount(int action,string country, string caseType, int appId) {
            var result = await _countryAppRepository.ShouldShowUnitaryPatentFields(action, country, caseType, appId);
            return result;
        }
        public async Task<List<PatDesignationDTO>> GetDesignatedCountries(int appId) {
            return  await _countryAppRepository.GetDesignatedCountries(appId);
        }
        #endregion

        public async Task UpdateDeDocket(CountryApplication countryApplication)
        {
            var deDocketFields = await _systemSettingManager.GetSystemSetting<DeDocketFields>();
            var updated = await GetById(countryApplication.AppId);

            Guard.Against.NoRecordPermission(updated != null);

            await ValidateCountryApplication(updated);

            if (updated != null && deDocketFields.CountryApplication != null)
            {
                if (deDocketFields.CountryApplication.Title)
                    updated.AppTitle = countryApplication.AppTitle;

                if (deDocketFields.CountryApplication.ClientReference)
                    updated.AppClientRef = countryApplication.AppClientRef;

                if (deDocketFields.CountryApplication.OtherReferenceNumber)
                    updated.OtherReferenceNumber = countryApplication.OtherReferenceNumber;

                if (deDocketFields.CountryApplication.AgentReference)
                    updated.AgentRef = countryApplication.AgentRef;

                if (deDocketFields.CountryApplication.TaxSchedule)
                    updated.TaxSchedule = countryApplication.TaxSchedule;

                if (deDocketFields.CountryApplication.Remarks)
                    updated.Remarks = countryApplication.Remarks;

                updated.LastUpdate = countryApplication.LastUpdate;
                updated.UpdatedBy = countryApplication.UpdatedBy;
                updated.tStamp = countryApplication.tStamp;

                //await _countryAppRepository.Update(countryApplication, null, new ApplicationModifiedFields(), DateTime.Now);

                _repository.CountryApplications.Update(updated);
                await _repository.SaveChangesAsync();
            }
            else
                Guard.Against.UnAuthorizedAccess(false);
        }

        public void DetachAllEntities() {
            _repository.DetachAllEntities();
        }
        public List<EntityEntry> GetAllTrackedEntities() { 
            return _repository.GetAllTrackedEntities();
        }

        public async Task<List<int>> GenerateEPODocMappedAction(int appId, string documentCode, DateTime baseDate)
        {
            var userName = _user.GetUserName();
            var today = DateTime.Now;
            var newActIds = new List<int>();

            baseDate = baseDate.Date;
            var application = await CountryApplications.Where(c => c.AppId == appId).Include(c => c.Invention).AsNoTracking().FirstOrDefaultAsync();
            if (application == null) return newActIds;

            var mappedActionTypes = await _repository.PatEPODocumentMapActs.Where(at => at.DocumentCode == documentCode).AsNoTracking().ToListAsync();
            if (mappedActionTypes == null) return newActIds;

            var newActionDues = new List<PatActionDue>();
            var newDueDates = new List<PatDueDate>();
            var uniqueActionTypes = mappedActionTypes.Select(d => d.ActionType).Distinct().ToList();
            foreach (var mappedActionType in uniqueActionTypes)
            {
                var dupActionDue = await _repository.PatActionDues.Where(a => a.AppId == appId && a.BaseDate.Date == baseDate.Date && a.ActionType == mappedActionType).AsNoTracking().FirstOrDefaultAsync();
                if (dupActionDue == null)
                {
                    PatActionDue actionDue = new PatActionDue()
                    {
                        AppId = application.AppId,
                        CaseNumber = application.CaseNumber,
                        Country = application.Country,
                        SubCase = application.SubCase,
                        ActionType = mappedActionType,
                        BaseDate = baseDate.Date,
                        ResponsibleID = null,
                        IsOfficeAction = true,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = today,
                        LastUpdate = today
                    };

                    var dueDates = mappedActionTypes.Where(d => d.ActionType == mappedActionType).Select(ap => new PatDueDate()
                    {
                        ActId = actionDue.ActId,
                        ActionDue = ap.ActionDue,
                        DueDate = actionDue.BaseDate.AddDays((double)ap.Dy).AddMonths(ap.Mo).AddYears(ap.Yr),
                        DateTaken = actionDue.ResponseDate,
                        Indicator = ap.Indicator,
                        CreatedBy = actionDue.UpdatedBy,
                        DateCreated = actionDue.LastUpdate,
                        UpdatedBy = actionDue.UpdatedBy,
                        LastUpdate = actionDue.LastUpdate
                    }).ToList();

                    actionDue.DueDates = dueDates;

                    var dueDatesFromIndicatorWorkflow = await GenerateDueDateFromActionParameterWorkflow(actionDue, actionDue.DueDates, PatWorkflowTriggerType.Indicator);
                    if (dueDatesFromIndicatorWorkflow != null && dueDatesFromIndicatorWorkflow.Any())
                    {
                        actionDue.DueDates.AddRange(dueDatesFromIndicatorWorkflow);
                    }

                    newActionDues.Add(actionDue);                    
                }
                 //If ActionDue record already exists, check and insert DueDate records
                else
                {
                    var dueDates = mappedActionTypes.Where(d => d.ActionType == mappedActionType).Select(ap => new PatDueDate()
                    {
                        ActId = dupActionDue.ActId,
                        ActionDue = ap.ActionDue,
                        DueDate = dupActionDue.BaseDate.AddDays((double)ap.Dy).AddMonths(ap.Mo).AddYears(ap.Yr),                        
                        Indicator = ap.Indicator,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = today,                        
                        LastUpdate = today
                    }).ToList();

                    var tempNewDueDates = new List<PatDueDate>();
                    foreach (var dueDate in dueDates)
                    {
                        if (!(await _repository.PatDueDates.AsNoTracking().AnyAsync(d => d.PatActionDue != null 
                                                && d.PatActionDue.ActId == dupActionDue.ActId
                                                && d.PatActionDue.AppId == appId                                                 
                                                && d.ActionDue == dueDate.ActionDue 
                                                && d.DueDate.Date == dueDate.DueDate.Date 
                                                && d.Indicator == dueDate.Indicator)))
                        {
                            tempNewDueDates.Add(dueDate);
                        }
                    }

                    if (tempNewDueDates != null && tempNewDueDates.Count > 0)
                    {
                        var dueDatesFromIndicatorWorkflow = await GenerateDueDateFromActionParameterWorkflow(dupActionDue, tempNewDueDates, PatWorkflowTriggerType.Indicator);
                        if (dueDatesFromIndicatorWorkflow != null && dueDatesFromIndicatorWorkflow.Any())
                        {
                            tempNewDueDates.AddRange(dueDatesFromIndicatorWorkflow);
                        }

                        newDueDates.AddRange(tempNewDueDates);
                    }                    
                }
            }

            if (newActionDues != null && newActionDues.Count > 0)
                _repository.PatActionDues.AddRange(newActionDues);
            if (newDueDates != null && newDueDates.Count > 0)
                _repository.PatDueDates.AddRange(newDueDates);

            if ((newActionDues != null && newActionDues.Count > 0) || (newDueDates != null && newDueDates.Count > 0))
            {
                await _repository.SaveChangesAsync();

                if (newActionDues != null && newActionDues.Count > 0)
                    newActIds.AddRange(newActionDues.Select(d => d.ActId).Distinct().ToList());

                if (newDueDates != null && newDueDates.Count > 0)
                    newActIds.AddRange(newDueDates.Select(d => d.ActId).Distinct().ToList());
            }                

            return newActIds;
        }

        public async Task<List<int>> GenerateEPOActMappedAction(int appId, string termKey, DateTime epoDueDate)
        {
            ///EPO Due Date mapping is 1 to 1
            ///Mapping is saved in tblPatEPOActionMapAct: TermId, ActionType, ActionDue, Indicator
            ///Use DueDate from EPO Due Date data as the actual due date for the actions and also as BaseDate
            ///Group due dates (tblPatDueDate) with same ActionType into one Action Due record (tblPatActionDue)
            ///Check if already exists to avoid duplicate from actions generated when download EPO Communications
            var userName = _user.GetUserName();
            var today = DateTime.Now;
            var newActIds = new List<int>();

            epoDueDate = epoDueDate.Date;
            var application = await CountryApplications.Where(c => c.AppId == appId).Include(c => c.Invention).AsNoTracking().FirstOrDefaultAsync();
            if (application == null) return newActIds;

            var mappedActionDues = await _repository.PatEPOActionMapActs.AsNoTracking()
                                    .Where(at => at.EPODueDateTerm != null && !string.IsNullOrEmpty(at.EPODueDateTerm.TermKey) && at.EPODueDateTerm.TermKey.ToLower() == termKey.ToLower())
                                    .ToListAsync();

            if (mappedActionDues == null) return newActIds;
            
            var newActionDues = new List<PatActionDue>();
            var newDueDates = new List<PatDueDate>();
            var uniqueActionTypes = mappedActionDues.Select(d => d.ActionType).Distinct().ToList();
            foreach (var mappedActionType in uniqueActionTypes)
            {                
                var dupActionDue = await _repository.PatActionDues.Where(a => a.AppId == appId && a.BaseDate.Date == epoDueDate.Date && a.ActionType == mappedActionType).AsNoTracking().FirstOrDefaultAsync();
                if (dupActionDue == null)
                {
                    PatActionDue actionDue = new PatActionDue()
                    {
                        AppId = application.AppId,
                        CaseNumber = application.CaseNumber,
                        Country = application.Country,
                        SubCase = application.SubCase,
                        ActionType = mappedActionType,
                        BaseDate = epoDueDate.Date,
                        ResponsibleID = null,
                        IsOfficeAction = true,
                        CreatedBy = userName,
                        UpdatedBy = userName,
                        DateCreated = today,
                        LastUpdate = today
                    };

                    var dueDates = mappedActionDues.Where(d => d.ActionType == mappedActionType).Select(ap => new PatDueDate()
                    {
                        ActId = actionDue.ActId,
                        ActionDue = ap.ActionDue,                        
                        DueDate = actionDue.BaseDate.Date,                        
                        Indicator = ap.Indicator,
                        CreatedBy = actionDue.UpdatedBy,
                        DateCreated = actionDue.LastUpdate,
                        UpdatedBy = actionDue.UpdatedBy,
                        LastUpdate = actionDue.LastUpdate
                    }).ToList();

                    //Check for duplicates on tblPatDueDate
                    var filteredDueDates = new List<PatDueDate>();
                    foreach (var dueDate in dueDates)
                    {
                        if (!(await _repository.PatDueDates.AsNoTracking().AnyAsync(d => d.PatActionDue != null 
                                                && d.PatActionDue.AppId == appId 
                                                && d.PatActionDue.ActionType == actionDue.ActionType 
                                                && d.ActionDue == dueDate.ActionDue 
                                                && d.DueDate.Date == dueDate.DueDate.Date 
                                                && d.Indicator == dueDate.Indicator)))
                        {
                            filteredDueDates.Add(dueDate);
                        }
                    }

                    if (filteredDueDates != null && filteredDueDates.Count > 0)
                    {
                        actionDue.DueDates = filteredDueDates;

                        var dueDatesFromIndicatorWorkflow = await GenerateDueDateFromActionParameterWorkflow(actionDue, actionDue.DueDates, PatWorkflowTriggerType.Indicator);
                        if (dueDatesFromIndicatorWorkflow != null && dueDatesFromIndicatorWorkflow.Any())
                        {
                            actionDue.DueDates.AddRange(dueDatesFromIndicatorWorkflow);
                        }

                        newActionDues.Add(actionDue);
                    }                    
                }
                //If ActionDue record already exists, check and insert DueDate records
                else
                {
                    var dueDates = mappedActionDues.Where(d => d.ActionType == mappedActionType).Select(ap => new PatDueDate()
                    {
                        ActId = dupActionDue.ActId,
                        ActionDue = ap.ActionDue,
                        DueDate = epoDueDate.Date,
                        Indicator = ap.Indicator,
                        CreatedBy = userName,
                        DateCreated = today,
                        UpdatedBy = userName,
                        LastUpdate = today
                    }).ToList();

                    var tempNewDueDates = new List<PatDueDate>();

                    foreach (var dueDate in dueDates)
                    {
                        if (!(await _repository.PatDueDates.AsNoTracking().AnyAsync(d => d.PatActionDue != null 
                                                && d.PatActionDue.ActId == dupActionDue.ActId
                                                && d.PatActionDue.AppId == appId                                                 
                                                && d.ActionDue == dueDate.ActionDue 
                                                && d.DueDate.Date == dueDate.DueDate.Date 
                                                && d.Indicator == dueDate.Indicator)))
                        {
                            tempNewDueDates.Add(dueDate);
                        }
                    }

                    if (tempNewDueDates != null && tempNewDueDates.Count > 0)
                    {
                        var dueDatesFromIndicatorWorkflow = await GenerateDueDateFromActionParameterWorkflow(dupActionDue, tempNewDueDates, PatWorkflowTriggerType.Indicator);
                        if (dueDatesFromIndicatorWorkflow != null && dueDatesFromIndicatorWorkflow.Any())
                        {
                            tempNewDueDates.AddRange(dueDatesFromIndicatorWorkflow);
                        }

                        newDueDates.AddRange(tempNewDueDates);
                    } 
                }
            }

            if (newActionDues != null && newActionDues.Count > 0)
                _repository.PatActionDues.AddRange(newActionDues);

            if (newDueDates != null && newDueDates.Count > 0)
                _repository.PatDueDates.AddRange(newDueDates);

            if ((newActionDues != null && newActionDues.Count > 0) || (newDueDates != null && newDueDates.Count > 0))
            {
                await _repository.SaveChangesAsync();

                if (newActionDues != null && newActionDues.Count > 0)
                    newActIds.AddRange(newActionDues.Select(d => d.ActId).Distinct().ToList());

                if (newDueDates != null && newDueDates.Count > 0)
                    newActIds.AddRange(newDueDates.Select(d => d.ActId).Distinct().ToList());
            }

            return newActIds;
        }

        public async Task<int> GetRequestDocketPendingCount(int appId) {
            return await _repository.PatDocketRequests.Where(r => r.AppId == appId && r.CompletedDate == null).CountAsync();
        }

        public async Task<List<PatDocketRequest>> GetRequestDockets(int appId, bool outstandingOnly) {
            return await _repository.PatDocketRequests.Where(r => r.AppId == appId && (!outstandingOnly || (outstandingOnly && r.CompletedDate ==null))).ToListAsync();
        }

    }
}
