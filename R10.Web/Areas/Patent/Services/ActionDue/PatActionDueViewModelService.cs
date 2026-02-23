using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using R10.Core;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Documents;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Patent.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Services.SharePoint;
using R10.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using R10.Web.Areas.Shared.ViewModels.SharePoint;
using R10.Core.Entities.Trademark;

namespace R10.Web.Areas.Patent.Services
{
    public class PatActionDueViewModelService : IPatActionDueViewModelService
    {
        private readonly IActionDueService<PatActionDue, PatDueDate> _actionDueService;
        private readonly ICountryApplicationService _applicationService;
        private readonly IMapper _mapper;
        private readonly INotificationSettingManager _userSettingManager;
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly IEntityService<PatDueDateDelegation> _dueDateDelegationEntityService;
        private readonly IEntityService<DeDocketInstruction> _auxService;
        private readonly IWorkflowViewModelService _workflowViewModelService;
        private readonly IDocumentService _docService;
        private readonly IParentEntityService<PatActionType, PatActionParameter> _actionTypeService;
        private readonly ISharePointService _sharePointService;
        private readonly GraphSettings _graphSettings;

        public PatActionDueViewModelService(IActionDueService<PatActionDue, PatDueDate> costTrackingService,
                                            ICountryApplicationService applicationService, IMapper mapper,
                                            INotificationSettingManager userSettingManager,
                                            ISystemSettings<PatSetting> settings,
                                            IEntityService<PatDueDateDelegation> dueDateDelegationEntityService,
                                            IEntityService<DeDocketInstruction> auxService,
                                            IWorkflowViewModelService workflowViewModelService,
                                            IDocumentService docService,
                                            IParentEntityService<PatActionType, PatActionParameter> actionTypeService,
                                            ISharePointService sharePointService, IOptions<GraphSettings> graphSettings)
        {
            _actionDueService = costTrackingService;
            _applicationService = applicationService;
            _mapper = mapper;
            _userSettingManager = userSettingManager;
            _settings = settings;
            _dueDateDelegationEntityService = dueDateDelegationEntityService;
            _auxService = auxService;
            _workflowViewModelService = workflowViewModelService;
            _docService = docService;
            _actionTypeService = actionTypeService;
            _sharePointService = sharePointService;
            _graphSettings = graphSettings.Value;
        }

        public IQueryable<PatActionDueAppInfoViewModel> AppInfo => _applicationService.CountryApplications.ProjectTo<PatActionDueAppInfoViewModel>();

        public IQueryable<PatActionDue> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<PatActionDue> actionsDue)
        {
            //_duedateCriteria = BuildDueDateCriteria(mainSearchFilters);

            if (mainSearchFilters.Count > 0)
            {
                var settings = _settings.GetSetting().GetAwaiter().GetResult();

                var countryOp = mainSearchFilters.GetFilterOperator("CountryOp");
                var country = mainSearchFilters.FirstOrDefault(f => f.Property == "Country");
                if (country != null)
                {
                    country.Operator = countryOp;
                    var countries = country.GetValueList();

                    if (countries.Count > 0)
                    {
                        if (country.Operator == "eq")
                            actionsDue = actionsDue.Where(ad => countries.Contains(ad.Country));
                        else
                            actionsDue = actionsDue.Where(ad => !countries.Contains(ad.Country));

                        mainSearchFilters.Remove(country);
                    }
                }

                var caseTypeOp = mainSearchFilters.GetFilterOperator("CaseTypeOp");
                var caseType = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryApplication.CaseType");
                if (caseType != null)
                {
                    caseType.Operator = caseTypeOp;
                    var caseTypes = caseType.GetValueList();

                    if (caseTypes.Count > 0)
                    {
                        if (caseType.Operator == "eq")
                            actionsDue = actionsDue.Where(ad => caseTypes.Contains(ad.CountryApplication.CaseType));
                        else
                            actionsDue = actionsDue.Where(ad => !caseTypes.Contains(ad.CountryApplication.CaseType));

                        mainSearchFilters.Remove(caseType);
                    }
                }

                var applicationStatusOp = mainSearchFilters.GetFilterOperator("ApplicationStatusOp");
                var applicationStatus = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryApplication.ApplicationStatus");
                if (applicationStatus != null)
                {
                    applicationStatus.Operator = applicationStatusOp;
                    var applicationStatuses = applicationStatus.GetValueList();

                    if (applicationStatuses.Count > 0)
                    {
                        if (applicationStatus.Operator == "eq")
                            actionsDue = actionsDue.Where(ad => applicationStatuses.Contains(ad.CountryApplication.ApplicationStatus));
                        else
                            actionsDue = actionsDue.Where(ad => !applicationStatuses.Contains(ad.CountryApplication.ApplicationStatus));

                        mainSearchFilters.Remove(applicationStatus);
                    }
                }

                var appNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryApplication.AppNumber");
                if (appNumber != null)
                {
                    var appNumberSearch = QueryHelper.ExtractSignificantNumbers(appNumber.Value);
                    actionsDue = actionsDue.Where(ad => (EF.Functions.Like(ad.CountryApplication.AppNumber, appNumber.Value) || EF.Functions.Like(ad.CountryApplication.AppNumberSearch, appNumberSearch)));
                    mainSearchFilters.Remove(appNumber);
                }

                var indicatorOp = mainSearchFilters.GetFilterOperator("IndicatorOp");
                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("DueDates.")) != null)
                {
                    Expression<Func<PatDueDate, bool>> dueDatePredicate = (item) => false;
                    Expression<Func<PatDueDate, bool>> dueDateDummyPredicate = (item) => false;

                    var actionDue = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.ActionDue");
                    if (actionDue != null)
                    {
                        var actionDues = actionDue.GetValueListForLoop();
                        if (actionDues.Count > 0)
                        {
                            var actionDuePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatDueDate>("ActionDue", actionDues, false);
                            if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                                dueDatePredicate = dueDatePredicate.Or(actionDuePredicate);
                            else
                                dueDatePredicate = dueDatePredicate.And(actionDuePredicate);
                        }
                    }

                    var indicator = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.Indicator");
                    if (indicator != null)
                    {
                        indicator.Operator = indicatorOp;
                        var indicators = indicator.GetValueListForLoop();
                        if (indicators.Count > 0)
                        {
                            Expression<Func<PatDueDate, bool>> indicatorPredicate = dd => ((indicator.Operator == "eq" && indicators.Contains(dd.Indicator))
                                                                                                || (indicator.Operator != "eq" && !indicators.Contains(dd.Indicator)));
                            if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                                dueDatePredicate = dueDatePredicate.Or(indicatorPredicate);
                            else
                                dueDatePredicate = dueDatePredicate.And(indicatorPredicate);
                        }
                    }

                    var duedatesAttorney = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.Attorney");
                    if (duedatesAttorney != null)
                    {
                        var ddAttorneys = duedatesAttorney.GetValueListForLoop();
                        if (ddAttorneys.Count > 0)
                        {
                            var ddAttorneyPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatDueDate>("DueDateAttorney.AttorneyCode", ddAttorneys, false);
                            if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                                dueDatePredicate = dueDatePredicate.Or(ddAttorneyPredicate);
                            else
                                dueDatePredicate = dueDatePredicate.And(ddAttorneyPredicate);
                        }
                    }

                    var dueDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.DueDateFrom");
                    var dueDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.DueDateTo");
                    var dateTakenFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.DateTakenFrom");
                    var dateTakenTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.DateTakenTo");
                    var outstandingOnly = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.ShowOutstandingActionsOnly");
                    var showSoftDockets = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.ShowSoftDockets");

                    Expression<Func<PatDueDate, bool>> dueDateCombinedPredicate = d => (
                                                                    (dueDateFrom == null || d.DueDate >= Convert.ToDateTime(dueDateFrom.Value)) &&
                                                                    (dueDateTo == null || d.DueDate <= Convert.ToDateTime(dueDateTo.Value)) &&
                                                                    (dateTakenFrom == null || d.DateTaken >= Convert.ToDateTime(dateTakenFrom.Value)) &&
                                                                    (dateTakenTo == null || d.DateTaken <= Convert.ToDateTime(dateTakenTo.Value)) &&
                                                                    (outstandingOnly == null || d.DateTaken == null) &&
                                                                    (!settings.IsSoftDocketOn || showSoftDockets != null || (d.Indicator != "Soft Docket"))
                                                                );

                    if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                        dueDatePredicate = dueDatePredicate.Or(dueDateCombinedPredicate);
                    else
                        dueDatePredicate = dueDatePredicate.And(dueDateCombinedPredicate);


                    var ddAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<PatActionDue>("DueDates", dueDatePredicate);

                    actionsDue = actionsDue.Where(ddAnyPredicate);

                    //actionsDue = actionsDue.Where(ad => ad.DueDates.Any(d => (actionDue == null || EF.Functions.Like(d.ActionDue, actionDue.Value)) &&
                    //                                                         (indicator == null || EF.Functions.Like(d.Indicator, indicator.Value)) &&
                    //                                                         (dueDateFrom == null || d.DueDate >= Convert.ToDateTime(dueDateFrom.Value)) &&
                    //                                                         (dueDateTo == null || d.DueDate <= Convert.ToDateTime(dueDateTo.Value)) &&
                    //                                                         (dateTakenFrom == null || d.DateTaken >= Convert.ToDateTime(dateTakenFrom.Value)) &&
                    //                                                         (dateTakenTo == null || d.DateTaken <= Convert.ToDateTime(dateTakenTo.Value)) &&
                    //                                                         (outstandingOnly == null || d.DateTaken == null) &&
                    //                                                         (duedatesAttorney == null || d.DueDateAttorney.AttorneyCode== duedatesAttorney.Value) 
                    //                                                         ));

                    mainSearchFilters.Remove(actionDue);
                    mainSearchFilters.Remove(indicator);
                    mainSearchFilters.Remove(dueDateFrom);
                    mainSearchFilters.Remove(dueDateTo);
                    mainSearchFilters.Remove(dateTakenFrom);
                    mainSearchFilters.Remove(dateTakenTo);
                    mainSearchFilters.Remove(outstandingOnly);
                    mainSearchFilters.Remove(duedatesAttorney);
                    mainSearchFilters.Remove(showSoftDockets);
                }
                else if (settings.IsSoftDocketOn) {
                    //dont show by default
                    actionsDue = actionsDue.Where(ad => ad.DueDates.Any(dd => dd.Indicator != "Soft Docket"));
                } 

                //dedocket
                var instructionOp = mainSearchFilters.GetFilterOperator("InstructionOp");
                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("DeDocket")) != null)
                {
                    Expression<Func<PatDueDate, bool>> dueDatePredicate = (item) => false;
                    Expression<Func<PatDueDate, bool>> dueDateDummyPredicate = (item) => false;

                    var deDocketInstruction = mainSearchFilters.FirstOrDefault(f => f.Property == "DeDocket.Instruction");
                    if (deDocketInstruction != null)
                    {
                        deDocketInstruction.Operator = instructionOp;
                        var ddInstructions = deDocketInstruction.GetValueListForLoop();
                        if (ddInstructions.Count > 0)
                        {
                            Expression<Func<PatDueDate, bool>> predicate = (item) => false;
                            if (deDocketInstruction.Operator == "eq")
                            {
                                foreach (var val in ddInstructions)
                                {
                                    //predicate = predicate.Or(dd => EF.Functions.Like(dd.DeDocketOutstanding.Instruction, val));
                                    predicate = predicate.Or(dd => dd.DueDateDeDockets.Any(ddk => EF.Functions.Like(ddk.Instruction, val)));
                                }
                            }
                            else
                            {
                                foreach (var val in ddInstructions)
                                {
                                    //predicate = predicate.Or(dd => !EF.Functions.Like(dd.DeDocketOutstanding.Instruction, val));
                                    predicate = predicate.Or(dd => dd.DueDateDeDockets.Any(ddk => !EF.Functions.Like(ddk.Instruction, val)));
                                }
                            }

                            if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                                dueDatePredicate = dueDatePredicate.Or(predicate);
                            else
                                dueDatePredicate = dueDatePredicate.And(predicate);
                        }
                    }

                    var deDocketInstructedBy = mainSearchFilters.FirstOrDefault(f => f.Property == "DeDocket.InstructedBy");
                    var deDocketInstructionFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DeDocket.InstructionDateFrom");
                    var deDocketInstructionTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DeDocket.InstructionDateTo");
                    var deDocketInstrCompleted = mainSearchFilters.FirstOrDefault(f => f.Property == "DeDocket.DeDocketInstrCompleted");


                    //Expression<Func<PatDueDate, bool>> ddCombinedFilter = dd => (
                    //                                (deDocketInstructedBy == null || EF.Functions.Like(dd.DeDocketOutstanding.InstructedBy, deDocketInstructedBy.Value)) &&
                    //                                (deDocketInstructionFrom == null || dd.DeDocketOutstanding.InstructionDate >= Convert.ToDateTime(deDocketInstructionFrom.Value)) &&
                    //                                (deDocketInstructionTo == null || dd.DeDocketOutstanding.InstructionDate <= Convert.ToDateTime(deDocketInstructionTo.Value).AddDays(1).AddSeconds(-1))
                    //                            );

                    Expression<Func<PatDueDate, bool>> ddCombinedFilter = dd => (
                                                    (deDocketInstructedBy == null || dd.DueDateDeDockets.Any(ddk => EF.Functions.Like(ddk.InstructedBy, deDocketInstructedBy.Value))) &&
                                                    (deDocketInstructionFrom == null || dd.DueDateDeDockets.Any(ddk => ddk.InstructionDate >= Convert.ToDateTime(deDocketInstructionFrom.Value))) &&
                                                    (deDocketInstructionTo == null || dd.DueDateDeDockets.Any(ddk => ddk.InstructionDate <= Convert.ToDateTime(deDocketInstructionTo.Value).AddDays(1).AddSeconds(-1))) &&
                                                    (deDocketInstrCompleted == null || (deDocketInstrCompleted.Value == "1" && dd.DueDateDeDockets.Any(ddk => ddk.InstructionCompleted)) || (deDocketInstrCompleted.Value == "0" && dd.DueDateDeDockets.Any(ddk => !ddk.InstructionCompleted || ddk.InstructionCompleted == null)))
                                                );

                    if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                        dueDatePredicate = dueDatePredicate.Or(ddCombinedFilter);
                    else
                        dueDatePredicate = dueDatePredicate.And(ddCombinedFilter);

                    var ddAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<PatActionDue>("DueDates", dueDatePredicate);
                    actionsDue = actionsDue.Where(ddAnyPredicate);


                    mainSearchFilters.Remove(deDocketInstruction);
                    mainSearchFilters.Remove(deDocketInstructedBy);
                    mainSearchFilters.Remove(deDocketInstructionFrom);
                    mainSearchFilters.Remove(deDocketInstructionTo);
                    mainSearchFilters.Remove(deDocketInstrCompleted);

                }

                var rtsVerify = mainSearchFilters.FirstOrDefault(f => f.Property == "RTSVerify");
                if (rtsVerify != null)
                {
                    if (rtsVerify.Value != "A")
                    {
                        actionsDue = actionsDue.Where(ad => (rtsVerify.Value == "1" && ad.VerifyDate == null && ad.IsElectronic == true) ||
                                                        (rtsVerify.Value == "0" && (ad.IsElectronic == false || ad.IsElectronic == null)) ||
                                                        (rtsVerify.Value == "2" && ad.VerifyDate != null));
                    }
                    mainSearchFilters.Remove(rtsVerify);
                }

                var poDocketed = mainSearchFilters.FirstOrDefault(f => f.Property == "PODocketed");
                if (poDocketed != null)
                {
                    if (poDocketed.Value == "1")
                    {
                        actionsDue = actionsDue.Where(ad => ad.CreatedBy == "PO" && ad.UpdatedBy == "PO" && ad.IsElectronic.HasValue && (bool)ad.IsElectronic);
                    }
                    else if (poDocketed.Value == "2")
                    {
                        actionsDue = actionsDue.Where(ad => !(ad.CreatedBy == "PO" && ad.UpdatedBy == "PO" && ad.IsElectronic.HasValue && (bool)ad.IsElectronic));
                    }
                    mainSearchFilters.Remove(poDocketed);
                }

                var actionType = mainSearchFilters.FirstOrDefault(f => f.Property == "ActionType");
                if (actionType != null)
                {
                    var actionTypes = actionType.GetValueListForLoop();
                    if (actionTypes.Count > 0)
                    {
                        Expression<Func<PatActionDue, bool>> predicate = (item) => false;
                        foreach (var val in actionTypes)
                        {
                            predicate = predicate.Or(ad => EF.Functions.Like(ad.ActionType, val));
                        }
                        actionsDue = actionsDue.Where(predicate);
                    }
                    mainSearchFilters.Remove(actionType);
                }

                //Verification-start
                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("DocVerify.")) != null)
                {
                    var docName = mainSearchFilters.FirstOrDefault(f => f.Property == "DocVerify.DocName");
                    var verifiedBy = mainSearchFilters.FirstOrDefault(f => f.Property == "DocVerify.VerifiedBy");
                    var verifiedDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DocVerify.VerifiedDateFrom");
                    var verifiedDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DocVerify.VerifiedDateTo");

                    actionsDue = actionsDue.Where(a => (verifiedBy == null || EF.Functions.Like(a.VerifiedBy, verifiedBy.Value))
                                       && (verifiedDateFrom == null || a.DateVerified >= Convert.ToDateTime(verifiedDateFrom.Value))
                                       && (verifiedDateTo == null || a.DateVerified <= Convert.ToDateTime(verifiedDateTo.Value)));
                    if (docName != null)
                    {
                        if (settings.IsSharePointIntegrationOn && settings.IsSharePointListRealTime)
                        {
                            var graphClient = _sharePointService.GetGraphClient();
                            var docs = new List<SharePointGraphDocPicklistViewModel>();

                            if (settings.IsSharePointIntegrationByMetadataOn)
                                docs = graphClient.GetSiteDocumentNamesByMetadata(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Patent, SharePointDocLibraryFolder.Application, docName != null ? docName.Value : "").GetAwaiter().GetResult();
                            else
                                docs = graphClient.GetSiteDocumentNames(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Patent, new List<string> { SharePointDocLibraryFolder.Application }, docName != null ? docName.Value : "").GetAwaiter().GetResult();

                            if (docs.Count > 0)
                            {
                                docs.ForEach(d =>
                                {
                                    var caseNumber = "";
                                    var country = "";
                                    var subCase = "";

                                    if (settings.IsSharePointIntegrationByMetadataOn)
                                    {
                                        var recKeys = d.RecKey.Split(SharePointSeparator.Field);
                                        caseNumber = recKeys[0];
                                        country = recKeys[1];
                                        if (recKeys.Length > 2)
                                        {
                                            subCase = recKeys[2];
                                        }
                                    }
                                    else
                                    {
                                        var folders = d.Folder.Split("/");
                                        caseNumber = folders[1];
                                        var countrySubCase = folders[2].Split(SharePointSeparator.Field);
                                        country = countrySubCase[0];
                                        if (countrySubCase.Length > 1)
                                        {
                                            subCase = countrySubCase[1];
                                        }
                                    }

                                    var application = _applicationService.CountryApplications.Where(a => a.CaseNumber == caseNumber && a.Country == country && a.SubCase == subCase).FirstOrDefaultAsync().GetAwaiter().GetResult();
                                    if (application != null)
                                        d.ParentId = application.AppId;
                                });
                                var driveItemIds = docs.Where(d => d.ParentId > 0).Select(d => d.Id).ToList();
                                var actIds = _docService.DocVerifications.Where(d => driveItemIds.Contains(d.DocDocument.DocFile.DriveItemId) && d.ActId > 0).Select(d => d.ActId).ToListAsync().GetAwaiter().GetResult();

                                actionsDue = actionsDue.Where(a => actIds.Contains(a.ActId));
                            }
                            else
                            {
                                actionsDue = actionsDue.Where(a => false);
                            }
                        }
                        else
                        {
                            actionsDue = actionsDue.Where(a => 
                                                (_docService.DocDocuments.Any(d => d.DocFolder != null 
                                                    && d.DocFolder.SystemType == SystemTypeCode.Patent
                                                    && (d.DocFolder.ScreenCode ?? "").ToLower() == "ca"
                                                    && (d.DocFolder.DataKey ?? "").ToLower() == "appid"
                                                    && d.DocFolder.DataKeyValue == a.AppId
                                                    && d.IsActRequired
                                                    && (docName == null || EF.Functions.Like(d.DocName, docName.Value))
                                                    )
                                                )
                                            );
                        }
                    }

                    mainSearchFilters.Remove(docName);
                    mainSearchFilters.Remove(verifiedBy);
                    mainSearchFilters.Remove(verifiedDateFrom);
                    mainSearchFilters.Remove(verifiedDateTo);
                }

                var filterIndicatorList = new List<string>() { "due date", "final" };
                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("ActVerify.")) != null)
                {
                    var checkDocketSwitch = mainSearchFilters.FirstOrDefault(f => f.Property == "ActVerify.CheckDocketSwitch");

                    //must have at least 1 completed due date with "Due Date" indicator
                    actionsDue = actionsDue.Where(a => a.DueDates != null 
                                && a.DueDates.Any(d => !string.IsNullOrEmpty(d.Indicator) && filterIndicatorList.Contains(d.Indicator.ToLower()) && d.DateTaken != null)
                                && (checkDocketSwitch == null || (Convert.ToBoolean(checkDocketSwitch.Value) == a.CheckDocket))                               
                        );

                    mainSearchFilters.Remove(checkDocketSwitch);
                }
                //Verification-end

                if (mainSearchFilters.Any())
                    actionsDue = QueryHelper.BuildCriteria<PatActionDue>(actionsDue, mainSearchFilters);
            }
            return actionsDue;
        }

        public async Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<PatActionDue> actionsDue, string value)
        {
            var result = await _actionDueService.QueryableList.Where(ad => ad.CaseNumber == value)
                .Select(ad => new CaseNumberLookupViewModel { Id = ad.ActId, CaseNumber = ad.CaseNumber }).FirstOrDefaultAsync();
            return result;
        }

        public PatActionDue ConvertViewModelToActionDue(PatActionDueDetailViewModel viewModel)
        {
            return _mapper.Map<PatActionDue>(viewModel);
        }

        public async Task<PatActionDueDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var viewModel = new PatActionDueDetailViewModel();

            if (id > 0)
            {
                viewModel = await _actionDueService.QueryableList.ProjectTo<PatActionDueDetailViewModel>()
                    .SingleOrDefaultAsync(i => i.ActId == id);

                if (viewModel != null)
                    viewModel.CanModifyAttorney = await _actionDueService.CanModifyAttorney(viewModel.ResponsibleID ?? 0);
            }

            return viewModel;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<PatActionDue> actionsDue,
                                                                      List<QueryFilterViewModel> dueDateFilters)
        {
            //var model = actionsDue.ProjectTo<PatActionDueSearchResultViewModel>();

            IQueryable<PatActionDueSearchResultViewModel> model;
            if (dueDateFilters.Count() == 0)
            {
                model = actionsDue.Select(ad => new PatActionDueSearchResultViewModel
                {
                    ActId = ad.ActId,
                    CaseNumber = ad.CaseNumber,
                    Country = ad.Country,
                    SubCase = ad.SubCase,
                    ActionType = ad.ActionType,
                    BaseDate = ad.BaseDate,
                    ApplicationStatus = ad.CountryApplication.ApplicationStatus,
                    DueDate = ad.DueDates.OrderBy(dd => dd.DueDate).FirstOrDefault().DueDate,
                    ActionDue = ad.DueDates.OrderBy(dd => dd.DueDate).FirstOrDefault().ActionDue,
                    DateTaken = ad.DueDates.OrderBy(dd => dd.DueDate).FirstOrDefault().DateTaken,
                    DueDateExtended = ad.DueDates.Any(dd=> dd.PatDueDateExtensions.Any(e=> e.NewDueDate == ad.DueDates.OrderBy(dd => dd.DueDate).FirstOrDefault().DueDate)),
                    CreatedBy = ad.CreatedBy,
                    UpdatedBy = ad.UpdatedBy,
                    DateCreated = ad.DateCreated,
                    LastUpdate = ad.LastUpdate
                });
            }
            else
            {

                var actionDue = dueDateFilters.FirstOrDefault(f => f.Property == "DueDates.ActionDue");
                var indicatorOp = dueDateFilters.GetFilterOperator("IndicatorOp");
                var indicator = dueDateFilters.FirstOrDefault(f => f.Property == "DueDates.Indicator");
                var dueDateFrom = dueDateFilters.FirstOrDefault(f => f.Property == "DueDates.DueDateFrom");
                var dueDateTo = dueDateFilters.FirstOrDefault(f => f.Property == "DueDates.DueDateTo");
                var dateTakenFrom = dueDateFilters.FirstOrDefault(f => f.Property == "DueDates.DateTakenFrom");
                var dateTakenTo = dueDateFilters.FirstOrDefault(f => f.Property == "DueDates.DateTakenTo");
                var outstandingOnly = dueDateFilters.FirstOrDefault(f => f.Property == "DueDates.ShowOutstandingActionsOnly");
                var duedatesAttorney = dueDateFilters.FirstOrDefault(f => f.Property == "DueDates.Attorney");

                Expression<Func<PatDueDate, bool>> dueDatePredicate = (item) => false;
                Expression<Func<PatDueDate, bool>> dueDateDummyPredicate = (item) => false;

                if (actionDue != null)
                {
                    var actionDues = actionDue.GetValueListForLoop();
                    if (actionDues.Count > 0)
                    {
                        var actionDuePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatDueDate>("ActionDue", actionDues, false);
                        if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                            dueDatePredicate = dueDatePredicate.Or(actionDuePredicate);
                        else
                            dueDatePredicate = dueDatePredicate.And(actionDuePredicate);
                    }
                }
                if (indicator != null)
                {
                    indicator.Operator = indicatorOp;
                    var indicators = indicator.GetValueListForLoop();
                    if (indicators.Count > 0)
                    {
                        Expression<Func<PatDueDate, bool>> indicatorPredicate = dd => ((indicator.Operator == "eq" && indicators.Contains(dd.Indicator))
                                                                                            || (indicator.Operator != "eq" && !indicators.Contains(dd.Indicator)));
                        if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                            dueDatePredicate = dueDatePredicate.Or(indicatorPredicate);
                        else
                            dueDatePredicate = dueDatePredicate.And(indicatorPredicate);
                    }
                }
                if (duedatesAttorney != null)
                {
                    var ddAttorneys = duedatesAttorney.GetValueListForLoop();
                    if (ddAttorneys.Count > 0)
                    {
                        var ddAttorneyPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatDueDate>("DueDateAttorney.AttorneyCode", ddAttorneys, false);
                        if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                            dueDatePredicate = dueDatePredicate.Or(ddAttorneyPredicate);
                        else
                            dueDatePredicate = dueDatePredicate.And(ddAttorneyPredicate);
                    }
                }

                Expression<Func<PatDueDate, bool>> dueDateCombinedPredicate = d => (
                                                                    (dueDateFrom == null || d.DueDate >= Convert.ToDateTime(dueDateFrom.Value)) &&
                                                                    (dueDateTo == null || d.DueDate <= Convert.ToDateTime(dueDateTo.Value)) &&
                                                                    (dateTakenFrom == null || d.DateTaken >= Convert.ToDateTime(dateTakenFrom.Value)) &&
                                                                    (dateTakenTo == null || d.DateTaken <= Convert.ToDateTime(dateTakenTo.Value)) &&
                                                                    (outstandingOnly == null || d.DateTaken == null)
                                                                );

                if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                    dueDatePredicate = dueDatePredicate.Or(dueDateCombinedPredicate);
                else
                    dueDatePredicate = dueDatePredicate.And(dueDateCombinedPredicate);


                model = actionsDue.Select(ad => new PatActionDueSearchResultViewModel
                {
                    ActId = ad.ActId,
                    CaseNumber = ad.CaseNumber,
                    Country = ad.Country,
                    SubCase = ad.SubCase,
                    ActionType = ad.ActionType,
                    BaseDate = ad.BaseDate,
                    ApplicationStatus = ad.CountryApplication.ApplicationStatus,
                    DueDate = ad.DueDates
                                      .AsQueryable().Where(dueDatePredicate)
                                    .OrderBy(dd => dd.DueDate).FirstOrDefault().DueDate,
                    ActionDue = ad.DueDates
                                      .AsQueryable().Where(dueDatePredicate)
                                    .OrderBy(dd => dd.DueDate).FirstOrDefault().ActionDue,
                    DateTaken = ad.DueDates
                                      .AsQueryable().Where(dueDatePredicate)
                                    .OrderBy(dd => dd.DueDate).FirstOrDefault().DateTaken,

                    DueDateExtended = ad.DueDates.AsQueryable().Where(dueDatePredicate)
                                     .Any(dd => dd.PatDueDateExtensions.Any(e => e.NewDueDate == ad.DueDates.AsQueryable().Where(dueDatePredicate).OrderBy(dd => dd.DueDate).FirstOrDefault().DueDate)),

                    CreatedBy = ad.CreatedBy,
                    UpdatedBy = ad.UpdatedBy,
                    DateCreated = ad.DateCreated,
                    LastUpdate = ad.LastUpdate
                });
            }

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(app => app.CaseNumber).ThenBy(app => app.Country).ThenBy(app => app.SubCase);

            var ids = await model.Select(ad => ad.ActId).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };
        }

        public async Task<List<PatActionDueAppInfoViewModel>> GetAppInfoList(string caseNumber, string country, string subCase)
        {
            var appInfo = await _applicationService.CountryApplications
                .Where(c => c.CaseNumber == caseNumber)
                .ProjectTo<PatActionDueAppInfoViewModel>()
                .ToListAsync();

            return appInfo;
        }

        #region Workflow
        public async Task<List<WorkflowEmailViewModel>> DeletedActionDueWorkflow(PatActionDue actionDue, string? emailUrl, string? delegatedEmailUrl, List<LookupIntDTO> openDelegatedDdIds)
        {
            var workFlows = new List<WorkflowViewModel>();
            var emailWorkflows = new List<WorkflowEmailViewModel>();

            var workflowActions = await _workflowViewModelService.GetPatActionDueWorkflowActions(actionDue, PatWorkflowTriggerType.RecordDeleted, false);
            workflowActions = workflowActions.Where(a => (a.Workflow.SystemScreen == null || a.Workflow.SystemScreen.ScreenCode.ToLower() == "act-workflow")).ToList();
            if (workflowActions.Any())
            {
                workflowActions = _workflowViewModelService.ClearPatBaseWorkflowActions(workflowActions);
                foreach (var item in workflowActions)
                {
                    var workFlow = new WorkflowViewModel
                    {
                        ActionTypeId = item.ActionTypeId,
                        ActionValueId = item.ActionValueId,
                        Preview = item.Preview,
                        AttachmentFilter = item.AttachmentFilter
                    };
                    workFlows.Add(workFlow);
                }

                _applicationService.DetachAllEntities();
                var createActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CreateAction).Distinct().ToList();
                foreach (var item in createActionWorkflows)
                {
                    await _applicationService.GenerateWorkflowAction(actionDue.AppId, item.ActionValueId, DateTime.Now);
                }

                var closeActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CloseAction).Distinct().ToList();
                foreach (var item in closeActionWorkflows)
                {
                    var actionDuesToClose = await _applicationService.CloseWorkflowAction(actionDue.AppId, item.ActionValueId);
                    if (actionDuesToClose.Any())
                    {
                        foreach (var actionDueToClose in actionDuesToClose)
                        {
                            await _actionDueService.Update(actionDueToClose);
                        }
                    }

                }

                var wfs = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
                if (wfs.Any())
                {
                    emailWorkflows = wfs.Select(wf => new WorkflowEmailViewModel
                    {
                        isAutoEmail = !wf.Preview,
                        qeSetupId = wf.ActionValueId,
                        autoAttachImages = wf.AutoAttachImages,
                        id = actionDue.ActId,
                        fileNames = new string[] { },
                        emailUrl = emailUrl,
                        attachmentFilter = wf.AttachmentFilter
                    }).ToList();
                }
            }

            //delegated action
            if (openDelegatedDdIds.Any())
            {
                workflowActions = await _workflowViewModelService.GetPatActionDueWorkflowActions(actionDue, PatWorkflowTriggerType.ActionDelegatedDeleted, true);
                if (workflowActions.Any())
                {
                    var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionDeleted, Convert.ToChar(SystemTypeCode.Patent));
                    var wf = workflowActions.Where(w => w.ActionTypeId == (int)PatWorkflowActionType.SendEmail).FirstOrDefault();
                    if (wf != null)
                    {
                        foreach (var d in openDelegatedDdIds)
                        {
                            var emails = await _applicationService.GetDeletedDelegationEmails(d.Value);
                            var emailString = "";
                            foreach (var email in emails)
                            {
                                if (!doNotEmailList.Any(e => e.IsCaseInsensitiveEqual(email.AssignedTo)))
                                {
                                    emailString = emailString + email.AssignedTo + ";";
                                }
                            }

                            if (!string.IsNullOrEmpty(emailString))
                            {
                                emailWorkflows.Add(new WorkflowEmailViewModel
                                {
                                    isAutoEmail = !wf.Preview,
                                    qeSetupId = wf.ActionValueId,
                                    autoAttachImages = wf.IncludeAttachments,
                                    id = d.Value,
                                    fileNames = new string[] { },
                                    emailUrl = delegatedEmailUrl,
                                    emailTo = emailString,
                                    attachmentFilter = wf.AttachmentFilter
                                });
                            }
                        }
                    }
                }
            }
            return emailWorkflows;
        }

        public async Task<List<WorkflowEmailViewModel>> NewOrCompletedActionWorkflow(PatActionDue actionDue, string? emailUrl, bool newAction)
        {
            var application = await _applicationService.CountryApplications.Include(c => c.Invention).Where(c => c.AppId == actionDue.AppId).AsNoTracking().FirstOrDefaultAsync();
            var workFlows = new List<WorkflowViewModel>();
            var triggerType = newAction ? PatWorkflowTriggerType.NewAction : PatWorkflowTriggerType.ActionClosed;
            actionDue.CountryApplication = application;

            var workflowActions = await _workflowViewModelService.GetPatActionDueWorkflowActions(actionDue, triggerType, false);
            if (workflowActions.Any())
            {
                var actionTypes = await _applicationService.PatActionTypes.Where(a => (a.CDueId == 0 || a.CDueId == null) && a.ActionType == actionDue.ActionType).ToListAsync();
                var matchedWorkFlows = workflowActions.Where(a => actionTypes.Any(at => at.ActionTypeID == a.Workflow.TriggerValueId || a.Workflow.TriggerValueId == 0 )).ToList();

                if (!newAction)
                {
                    var matchedWorkFlowsCL = workflowActions.Where(a => a.Workflow.TriggerValueId < 0).ToList();
                    if (matchedWorkFlowsCL.Any())
                    {
                        Expression<Func<PatCountryDue, bool>> predicate = (item) => false;
                        foreach (var item in matchedWorkFlowsCL)
                        {
                            predicate = predicate.Or(cd => cd.CDueId == Math.Abs(item.Workflow.TriggerValueId));
                        }
                        var baseActionTypesCL = await _applicationService.PatCountryDues.Where(predicate).Select(cd => cd.ActionType).ToListAsync();
                        var actionTypesCL = await _applicationService.PatCountryDues.Where(cd => baseActionTypesCL.Any(at => at == cd.ActionType) && cd.ActionType == actionDue.ActionType).ToListAsync();
                        if (actionTypesCL.Any())
                        {
                            matchedWorkFlows.AddRange(matchedWorkFlowsCL);
                        }
                    }
                }
                matchedWorkFlows = _workflowViewModelService.ClearPatBaseWorkflowActions(matchedWorkFlows);

                foreach (var item in matchedWorkFlows)
                {
                    workFlows.Add(new WorkflowViewModel
                    {
                        ActionTypeId = item.ActionTypeId,
                        ActionValueId = item.ActionValueId,
                        Preview = item.Preview,
                        AutoAttachImages = item.IncludeAttachments,
                        EmailUrl = emailUrl,
                        AttachmentFilter = item.AttachmentFilter
                    });
                }
                _applicationService.DetachAllEntities();
            }

            //follow up action may trigger a workflow
            if (!string.IsNullOrEmpty(actionDue.FollowUpAction))
            {
                var followUpWorkflowActions = new List<PatWorkflowAction>();
                if (newAction)
                    followUpWorkflowActions = workflowActions;
                else
                {
                    followUpWorkflowActions = await _workflowViewModelService.GetPatActionDueWorkflowActions(actionDue, PatWorkflowTriggerType.NewAction, false);
                }

                var actionTypes = await _applicationService.PatActionTypes.Where(a => (a.CDueId == 0 || a.CDueId == null) && a.ActionType == actionDue.FollowUpAction).ToListAsync();
                var matchedWorkFlows = followUpWorkflowActions.Where(a => actionTypes.Any(at => at.ActionTypeID == a.Workflow.TriggerValueId || a.Workflow.TriggerValueId == 0)).ToList();
                matchedWorkFlows = _workflowViewModelService.ClearPatBaseWorkflowActions(matchedWorkFlows);

                foreach (var item in matchedWorkFlows)
                {
                    workFlows.Add(new WorkflowViewModel
                    {
                        ActionTypeId = item.ActionTypeId,
                        ActionValueId = item.ActionValueId,
                        Preview = item.Preview,
                        AutoAttachImages = item.IncludeAttachments,
                        EmailUrl = emailUrl,
                        AttachmentFilter = item.AttachmentFilter
                    });
                }
            }

            var createActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CreateAction).Distinct().ToList();
            foreach (var item in createActionWorkflows)
            {
                await _applicationService.GenerateWorkflowAction(application.AppId, item.ActionValueId, DateTime.Now);
            }

            var closeActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CloseAction).Distinct().ToList();
            foreach (var item in closeActionWorkflows)
            {
                var actionDuesToClose = await _applicationService.CloseWorkflowAction(application.AppId, item.ActionValueId);
                if (actionDuesToClose.Any())
                {
                    foreach (var actionDueToClose in actionDuesToClose)
                    {
                        await _actionDueService.Update(actionDueToClose);
                    }
                }
            }

            var emailWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.SendEmail)
                .Select(wf => new WorkflowEmailViewModel
                {
                    isAutoEmail = !wf.Preview,
                    qeSetupId = wf.ActionValueId,
                    autoAttachImages = wf.AutoAttachImages,
                    id = actionDue.ActId,
                    fileNames = new string[] { },
                    emailUrl = emailUrl,
                    attachmentFilter = wf.AttachmentFilter
                }).Distinct().ToList();

            return emailWorkflows;
        }

        public async Task<List<WorkflowEmailViewModel>> NewDedocketInstructionWorkflow(IList<ActionDueViewModel> updated, string? emailUrl)
        {
            var wfs = new List<WorkflowEmailViewModel>();
            var existingRecord = await _actionDueService.QueryableList.Where(a => a.ActId == updated.FirstOrDefault().ActId).Include(a => a.CountryApplication).ThenInclude(c => c.Invention).FirstOrDefaultAsync();
            var workFlows = new List<WorkflowViewModel>();
            var workflowActions = await _workflowViewModelService.GetPatActionDueWorkflowActions(existingRecord, PatWorkflowTriggerType.DedocketInstruction, false);

            if (workflowActions.Any())
            {
                var instructions = (await _auxService.QueryableList.ToListAsync()).Where(i => updated.Any(u => u.Instruction == i.Instruction)).ToList();
                workflowActions = workflowActions.Where(a => instructions.Any(i => i.InstructionId == a.Workflow.TriggerValueId || a.Workflow.TriggerValueId == 0)).ToList();
                workflowActions = _workflowViewModelService.ClearPatBaseWorkflowActions(workflowActions);

                foreach (var item in workflowActions)
                {
                    var workFlow = new WorkflowViewModel
                    {
                        ActionTypeId = item.ActionTypeId,
                        ActionValueId = item.ActionValueId,
                        Preview = item.Preview,
                        AutoAttachImages = item.IncludeAttachments,
                        AttachmentFilter = item.AttachmentFilter
                    };
                    workFlows.Add(workFlow);
                }

                _applicationService.DetachAllEntities();
                var createActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CreateAction).Distinct().ToList();
                foreach (var item in createActionWorkflows)
                {
                    await _applicationService.GenerateWorkflowAction(existingRecord.CountryApplication.AppId, item.ActionValueId, DateTime.Now);
                }

                var closeActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CloseAction).Distinct().ToList();
                foreach (var item in closeActionWorkflows)
                {
                    var actionDuesToClose = await _applicationService.CloseWorkflowAction(existingRecord.CountryApplication.AppId, item.ActionValueId);
                    if (actionDuesToClose.Any())
                    {
                        foreach (var actionDueToClose in actionDuesToClose)
                        {
                            await _actionDueService.Update(actionDueToClose);
                        }
                    }
                }

                var emailWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
                if (emailWorkflows.Any())
                {
                    foreach (var wf in emailWorkflows)
                    {
                        foreach (var dd in updated.Where(u => u.HasNewDeDocketInstruction.HasValue && (bool)u.HasNewDeDocketInstruction).ToList())
                        {
                            if (!string.IsNullOrEmpty(dd.Instruction) && instructions.Any(i => i.Instruction == dd.Instruction))
                            {
                                wfs.Add(new WorkflowEmailViewModel
                                {
                                    isAutoEmail = !wf.Preview,
                                    qeSetupId = wf.ActionValueId,
                                    autoAttachImages = wf.AutoAttachImages,
                                    id = dd.DeDocketId ?? 0,
                                    fileNames = new string[] { },
                                    emailUrl = emailUrl,
                                    attachmentFilter = wf.AttachmentFilter
                                });
                            }
                        }
                    }
                }
            }
            return wfs;
        }

        public async Task<List<WorkflowEmailViewModel>> CompletedDedocketInstructionWorkflow(IList<ActionDueViewModel> updated, string? emailUrl)
        {
            var wfs = new List<WorkflowEmailViewModel>();
            var existingRecord = await _actionDueService.QueryableList.Where(a => a.ActId == updated.FirstOrDefault().ActId).Include(a => a.CountryApplication).ThenInclude(c => c.Invention).FirstOrDefaultAsync();
            var workFlows = new List<WorkflowViewModel>();
            var workflowActions = await _workflowViewModelService.GetPatActionDueWorkflowActions(existingRecord, PatWorkflowTriggerType.DedocketInstructionCompleted, false);

            if (workflowActions.Any())
            {
                var instructions = (await _auxService.QueryableList.ToListAsync()).Where(i => updated.Any(u => u.Instruction == i.Instruction)).ToList();
                workflowActions = workflowActions.Where(a => instructions.Any(i => i.InstructionId == a.Workflow.TriggerValueId || a.Workflow.TriggerValueId == 0)).ToList();
                workflowActions = _workflowViewModelService.ClearPatBaseWorkflowActions(workflowActions);

                foreach (var item in workflowActions)
                {
                    var workFlow = new WorkflowViewModel
                    {
                        ActionTypeId = item.ActionTypeId,
                        ActionValueId = item.ActionValueId,
                        Preview = item.Preview,
                        AutoAttachImages = item.IncludeAttachments,
                        AttachmentFilter = item.AttachmentFilter
                    };
                    workFlows.Add(workFlow);
                }

                var emailWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
                if (emailWorkflows.Any())
                {
                    foreach (var wf in emailWorkflows)
                    {
                        foreach (var dd in updated.Where(u => u.HasNewInstructionCompleted.HasValue && (bool)u.HasNewInstructionCompleted).ToList())
                        {
                            wfs.Add(new WorkflowEmailViewModel
                            {
                                isAutoEmail = !wf.Preview,
                                qeSetupId = wf.ActionValueId,
                                autoAttachImages = wf.AutoAttachImages,
                                id = dd.DeDocketId ?? 0,
                                fileNames = new string[] { },
                                emailUrl = emailUrl,
                                attachmentFilter = wf.AttachmentFilter
                            });
                        }
                    }
                }
            }
            return wfs;
        }

        public async Task<List<WorkflowEmailViewModel>> NewDelegatedTaskWorkflow(string? emailUrl, List<PatDueDateDelegationDetail> newDelegations)
        {
            var emailWorkflows = new List<WorkflowEmailViewModel>();
            var settings = await _settings.GetSetting();

            if (settings.IsWorkflowOn)
            {
                var delegation = newDelegations.FirstOrDefault();
                if (delegation != null)
                {
                    var action = await _actionDueService.QueryableList.Include(a => a.CountryApplication).ThenInclude(c => c.Invention).FirstOrDefaultAsync(a => a.ActId == delegation.ActId || a.DueDates.Any(dd => dd.DDId == delegation.DDId));
                    var workflowActions = await _workflowViewModelService.GetPatActionDueWorkflowActions(action, PatWorkflowTriggerType.ActionDelegated, false);
                    workflowActions = workflowActions.Where(w => w.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
                    workflowActions = _workflowViewModelService.ClearPatBaseWorkflowActions(workflowActions);

                    var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionDelegated, Convert.ToChar(SystemTypeCode.Patent));
                    var wf = workflowActions.FirstOrDefault();
                    if (wf != null)
                    {
                        foreach (var item in newDelegations.ToList())
                        {
                            int? groupId = null;
                            int parseInt;
                            string? userId = null;
                            if (int.TryParse(item.UserId, out parseInt))
                            {
                                groupId = parseInt;
                            }
                            else
                            {
                                userId = item.UserId;
                            }

                            var delegations = new List<PatDueDateDelegation>();
                            if (item.ActId > 0)
                            {
                                delegations = await _dueDateDelegationEntityService.QueryableList.Where(c => c.NotificationSent == 0 && _actionDueService.QueryableList.Any(a => a.ActId == item.ActId && a.DueDates.Any(dd => dd.DDId == c.DDId) && ((c.GroupId == null && groupId == null) || c.GroupId == groupId) && ((c.UserId == null && userId == null) || c.UserId == userId))).ToListAsync();
                            }
                            else if (item.DDId > 0)
                            {
                                delegations = await _dueDateDelegationEntityService.QueryableList.Where(c => c.NotificationSent == 0 && c.DDId == item.DDId && ((c.GroupId == null && groupId == null) || c.GroupId == groupId) && ((c.UserId == null && userId == null) || c.UserId == userId)).ToListAsync();
                            }

                            else if (item.DelegationId > 0)
                            {
                                delegations = await _dueDateDelegationEntityService.QueryableList.Where(c => c.DelegationId == item.DelegationId && ((c.GroupId == null && groupId == null) || c.GroupId == groupId) && ((c.UserId == null && userId == null) || c.UserId == userId)).ToListAsync();
                            }

                            foreach (var d in delegations)
                            {
                                var emails = await _applicationService.GetDelegationEmails(d.DelegationId);
                                var emailString = "";
                                foreach (var email in emails)
                                {
                                    if (!doNotEmailList.Any(e => e.IsCaseInsensitiveEqual(email.AssignedTo)))
                                    {
                                        emailString = emailString + email.AssignedTo + ";";
                                    }
                                }

                                if (!string.IsNullOrEmpty(emailString))
                                {
                                    emailWorkflows.Add(new WorkflowEmailViewModel
                                    {
                                        isAutoEmail = !wf.Preview,
                                        qeSetupId = wf.ActionValueId,
                                        autoAttachImages = wf.IncludeAttachments,
                                        id = d.DelegationId,
                                        fileNames = new string[] { },
                                        emailUrl = emailUrl,
                                        emailTo = emailString,
                                        attachmentFilter = wf.AttachmentFilter
                                    });
                                }
                            }
                        }
                    }
                }
            }
            return emailWorkflows;
        }

        public async Task<List<WorkflowEmailViewModel>> CompletedDelegatedTaskWorkflow(int actId, string? emailUrl, List<LookupIntDTO> dddIds)
        {
            var wfs = new List<WorkflowEmailViewModel>();

            var settings = await _settings.GetSetting();
            if (settings.IsWorkflowOn)
            {
                var existingRecord = await _actionDueService.QueryableList.Where(a => a.ActId == actId).Include(a => a.CountryApplication).ThenInclude(c => c.Invention).FirstOrDefaultAsync();
                var emailWorkflows = await _workflowViewModelService.GetPatActionDueWorkflowActions(existingRecord, PatWorkflowTriggerType.ActionDelegatedCompleted, false);
                emailWorkflows = emailWorkflows.Where(w => w.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
                emailWorkflows = _workflowViewModelService.ClearPatBaseWorkflowActions(emailWorkflows);

                var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionCompleted, Convert.ToChar(SystemTypeCode.Patent));

                if (emailWorkflows.Any())
                {
                    foreach (var wf in emailWorkflows)
                    {
                        foreach (var ddd in dddIds)
                        {
                            var emails = await _applicationService.GetDelegationEmails(ddd.Value);
                            var emailString = "";
                            foreach (var email in emails)
                            {
                                if (!doNotEmailList.Any(e => e.IsCaseInsensitiveEqual(email.AssignedTo)))
                                {
                                    emailString = emailString + email.AssignedTo + ";";
                                }
                            }

                            if (!string.IsNullOrEmpty(emailString))
                            {
                                wfs.Add(new WorkflowEmailViewModel
                                {
                                    isAutoEmail = !wf.Preview,
                                    qeSetupId = wf.ActionValueId,
                                    autoAttachImages = wf.IncludeAttachments,
                                    id = ddd.Value,
                                    fileNames = new string[] { },
                                    emailUrl = emailUrl,
                                    emailTo = emailString,
                                    attachmentFilter = wf.AttachmentFilter
                                });
                            }
                        }
                    }
                }
            }
            return wfs;
        }

        public async Task<List<WorkflowEmailViewModel>> ReassignedDelegatedTaskWorkflow(string? emailUrl, List<PatDueDateDelegationDetail> deletedDelegations)
        {
            var wfs = new List<WorkflowEmailViewModel>();

            var settings = await _settings.GetSetting();
            if (settings.IsWorkflowOn)
            {
                var delegation = deletedDelegations.FirstOrDefault();
                if (delegation != null)
                {
                    var existingRecord = await _actionDueService.QueryableList.Include(a => a.CountryApplication).ThenInclude(c => c.Invention).FirstOrDefaultAsync(a => a.ActId == delegation.ActId || a.DueDates.Any(dd => dd.DDId == delegation.DDId));
                    var emailWorkflows = await _workflowViewModelService.GetPatActionDueWorkflowActions(existingRecord, PatWorkflowTriggerType.ActionDelegatedReAssigned, false);
                    emailWorkflows = emailWorkflows.Where(w => w.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
                    emailWorkflows = _workflowViewModelService.ClearPatBaseWorkflowActions(emailWorkflows);

                    var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionReassigned, Convert.ToChar(SystemTypeCode.Patent));
                    if (emailWorkflows.Any())
                    {
                        foreach (var wf in emailWorkflows)
                        {
                            foreach (var ddd in deletedDelegations.Where(d => d.DelegationId > 0).ToList())
                            {
                                var emails = await _applicationService.GetDeletedDelegationEmails(ddd.DelegationId);
                                var emailString = "";
                                foreach (var email in emails)
                                {
                                    if (!doNotEmailList.Any(e => e.IsCaseInsensitiveEqual(email.AssignedTo)))
                                    {
                                        emailString = emailString + email.AssignedTo + ";";
                                    }
                                }

                                if (!string.IsNullOrEmpty(emailString))
                                {
                                    wfs.Add(new WorkflowEmailViewModel
                                    {
                                        isAutoEmail = !wf.Preview,
                                        qeSetupId = wf.ActionValueId,
                                        autoAttachImages = wf.IncludeAttachments,
                                        id = ddd.DelegationId,
                                        fileNames = new string[] { },
                                        emailUrl = emailUrl,
                                        emailTo = emailString,
                                        attachmentFilter = wf.AttachmentFilter
                                    });
                                }
                            }
                        }
                    }
                }
            }
            return wfs;
        }

        public async Task<List<WorkflowEmailViewModel>> DuedateChangedDelegatedTaskWorkflow(int actId, string? emailUrl, List<LookupIntDTO> dddIds)
        {
            var wfs = new List<WorkflowEmailViewModel>();

            var settings = await _settings.GetSetting();
            if (settings.IsWorkflowOn)
            {
                var existingRecord = await _actionDueService.QueryableList.Where(a => a.ActId == actId).Include(a => a.CountryApplication).ThenInclude(c => c.Invention).FirstOrDefaultAsync();
                var emailWorkflows = await _workflowViewModelService.GetPatActionDueWorkflowActions(existingRecord, PatWorkflowTriggerType.ActionDelegatedDuedateChanged, false);
                emailWorkflows = emailWorkflows.Where(w => w.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
                emailWorkflows = _workflowViewModelService.ClearPatBaseWorkflowActions(emailWorkflows);

                var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionDueDateChanged, Convert.ToChar(SystemTypeCode.Patent));
                if (emailWorkflows.Any())
                {
                    foreach (var wf in emailWorkflows)
                    {
                        foreach (var ddd in dddIds)
                        {
                            var emails = await _applicationService.GetDelegationEmails(ddd.Value);
                            var emailString = "";
                            foreach (var email in emails)
                            {
                                if (!doNotEmailList.Any(e => e.IsCaseInsensitiveEqual(email.AssignedTo)))
                                {
                                    emailString = emailString + email.AssignedTo + ";";
                                }
                            }

                            if (!string.IsNullOrEmpty(emailString))
                            {
                                wfs.Add(new WorkflowEmailViewModel
                                {
                                    isAutoEmail = !wf.Preview,
                                    qeSetupId = wf.ActionValueId,
                                    autoAttachImages = wf.IncludeAttachments,
                                    id = ddd.Value,
                                    fileNames = new string[] { },
                                    emailUrl = emailUrl,
                                    emailTo = emailString,
                                    attachmentFilter = wf.AttachmentFilter
                                });
                            }
                        }
                    }
                }
            }
            return wfs;
        }

        public async Task<List<WorkflowEmailViewModel>> DeletedDelegatedTaskWorkflow(int actId, string? emailUrl, List<DelegationEmailDTO> emails)
        {
            var wfs = new List<WorkflowEmailViewModel>();
            var settings = await _settings.GetSetting();
            if (settings.IsWorkflowOn)
            {
                var existingRecord = await _actionDueService.QueryableList.Where(a => a.ActId == actId).Include(a => a.CountryApplication).ThenInclude(c => c.Invention).FirstOrDefaultAsync();

                var emailWorkflows = await _workflowViewModelService.GetPatActionDueWorkflowActions(existingRecord, PatWorkflowTriggerType.ActionDelegatedDeleted, false);
                emailWorkflows = emailWorkflows.Where(w => w.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
                emailWorkflows = _workflowViewModelService.ClearPatBaseWorkflowActions(emailWorkflows);

                var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionDeleted, Convert.ToChar(SystemTypeCode.Patent));
                if (emailWorkflows.Any())
                {
                    foreach (var wf in emailWorkflows)
                    {
                        var emailString = "";
                        foreach (var email in emails)
                        {
                            if (!doNotEmailList.Any(e => e.IsCaseInsensitiveEqual(email.AssignedTo)))
                            {
                                emailString = emailString + email.AssignedTo + ";";
                            }
                            if (!string.IsNullOrEmpty(emailString))
                            {
                                wfs.Add(new WorkflowEmailViewModel
                                {
                                    isAutoEmail = !wf.Preview,
                                    qeSetupId = wf.ActionValueId,
                                    autoAttachImages = wf.IncludeAttachments,
                                    id = email.DelegationId,
                                    fileNames = new string[] { },
                                    emailUrl = emailUrl,
                                    emailTo = emailString,
                                    attachmentFilter = wf.AttachmentFilter
                                });
                            }

                        }

                    }
                }
            }
            return wfs;
        }

        public async Task<List<WorkflowEmailViewModel>> NewRequestDocketWorkflow(int appId, int reqId, string? emailUrl)
        {
            var wfs = new List<WorkflowEmailViewModel>();
            var application = await _applicationService.CountryApplications.Include(c => c.Invention).Where(c => c.AppId == appId).AsNoTracking().FirstOrDefaultAsync();
            var workFlows = new List<WorkflowViewModel>();
            var workflowActions = await _workflowViewModelService.GetCountryApplicationWorkflowActions(application, PatWorkflowTriggerType.RequestDocket, false);

            if (workflowActions.Any())
            {
                workflowActions = _workflowViewModelService.ClearPatBaseWorkflowActions(workflowActions);

                foreach (var item in workflowActions)
                {
                    var workFlow = new WorkflowViewModel
                    {
                        ActionTypeId = item.ActionTypeId,
                        ActionValueId = item.ActionValueId,
                        Preview = item.Preview,
                        AutoAttachImages = item.IncludeAttachments,
                        AttachmentFilter = item.AttachmentFilter
                    };
                    workFlows.Add(workFlow);
                }

                var emailWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
                if (emailWorkflows.Any())
                {
                    foreach (var wf in emailWorkflows)
                    {
                        wfs.Add(new WorkflowEmailViewModel
                        {
                            isAutoEmail = !wf.Preview,
                            qeSetupId = wf.ActionValueId,
                            autoAttachImages = wf.AutoAttachImages,
                            id = reqId,
                            fileNames = new string[] { },
                            emailUrl = emailUrl,
                            attachmentFilter = wf.AttachmentFilter
                        });

                    }
                }
            }
            return wfs;
        }


        
        #endregion
    }
}
