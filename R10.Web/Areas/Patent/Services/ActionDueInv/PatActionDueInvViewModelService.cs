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

namespace R10.Web.Areas.Patent.Services
{
    public class PatActionDueInvViewModelService : IPatActionDueInvViewModelService
    {
        private readonly IActionDueService<PatActionDueInv, PatDueDateInv> _actionDueService;
        private readonly IInventionService _inventionService;
        private readonly IMapper _mapper;
        private readonly INotificationSettingManager _userSettingManager;
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly IEntityService<PatDueDateInvDelegation> _dueDateDelegationEntityService;
        private readonly IEntityService<DeDocketInstruction> _auxService;
        private readonly IWorkflowViewModelService _workflowViewModelService;
        private readonly IDocumentService _docService;
        private readonly IParentEntityService<PatActionType, PatActionParameter> _actionTypeService;
        private readonly ISharePointService _sharePointService;
        private readonly GraphSettings _graphSettings;

        public PatActionDueInvViewModelService(IActionDueService<PatActionDueInv, PatDueDateInv> costTrackingService,
                                            IInventionService inventionService, IMapper mapper,
                                            INotificationSettingManager userSettingManager,
                                            ISystemSettings<PatSetting> settings,
                                            IEntityService<PatDueDateInvDelegation> dueDateDelegationEntityService,
                                            IEntityService<DeDocketInstruction> auxService, 
                                            IWorkflowViewModelService workflowViewModelService,
                                            IDocumentService docService,
                                            IParentEntityService<PatActionType, PatActionParameter> actionTypeService, 
                                            ISharePointService sharePointService, IOptions<GraphSettings> graphSettings)
        {
            _actionDueService = costTrackingService;
            _inventionService = inventionService;
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

        public IQueryable<PatActionDueInvInventionInfoViewModel> InvInfo => _inventionService.Inventions.ProjectTo<PatActionDueInvInventionInfoViewModel>();

        public IQueryable<PatActionDueInv> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<PatActionDueInv> actionsDue)
        {
            //_duedateCriteria = BuildDueDateCriteria(mainSearchFilters);

            if (mainSearchFilters.Count > 0)
            {
                var settings = _settings.GetSetting().GetAwaiter().GetResult();


                var inventionStatusOp = mainSearchFilters.GetFilterOperator("DisclosureStatusOp");
                var inventionStatus = mainSearchFilters.FirstOrDefault(f => f.Property == "Invention.DisclosureStatus");
                if (inventionStatus != null)
                {
                    inventionStatus.Operator = inventionStatusOp;
                    var inventionStatuses = inventionStatus.GetValueList();

                    if (inventionStatuses.Count > 0)
                    {
                        if (inventionStatus.Operator == "eq")
                            actionsDue = actionsDue.Where(ad => inventionStatuses.Contains(ad.Invention.DisclosureStatus));
                        else
                            actionsDue = actionsDue.Where(ad => !inventionStatuses.Contains(ad.Invention.DisclosureStatus));

                        mainSearchFilters.Remove(inventionStatus);
                    }
                }

                var indicatorOp = mainSearchFilters.GetFilterOperator("IndicatorOp");
                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("DueDateInvs.")) != null)
                {
                    Expression<Func<PatDueDateInv, bool>> dueDatePredicate = (item) => false;
                    Expression<Func<PatDueDateInv, bool>> dueDateDummyPredicate = (item) => false;

                    var actionDue = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDateInvs.ActionDue");
                    if (actionDue != null)
                    {
                        var actionDues = actionDue.GetValueListForLoop();
                        if (actionDues.Count > 0)
                        {
                            var actionDuePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatDueDateInv>("ActionDue", actionDues, false);
                            if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                                dueDatePredicate = dueDatePredicate.Or(actionDuePredicate);
                            else
                                dueDatePredicate = dueDatePredicate.And(actionDuePredicate);
                        }
                    }

                    var indicator = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDateInvs.Indicator");                    
                    if (indicator != null)
                    {
                        indicator.Operator = indicatorOp;
                        var indicators = indicator.GetValueListForLoop();
                        if (indicators.Count > 0)
                        {
                            Expression<Func<PatDueDateInv, bool>> indicatorPredicate = dd => ((indicator.Operator == "eq" && indicators.Contains(dd.Indicator))
                                                                                                || (indicator.Operator != "eq" && !indicators.Contains(dd.Indicator)));
                            if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                                dueDatePredicate = dueDatePredicate.Or(indicatorPredicate);
                            else
                                dueDatePredicate = dueDatePredicate.And(indicatorPredicate);
                        }
                    }

                    var duedatesAttorney = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDateInvs.Attorney");
                    if (duedatesAttorney != null)
                    {
                        var ddAttorneys = duedatesAttorney.GetValueListForLoop();
                        if (ddAttorneys.Count > 0)
                        {
                            var ddAttorneyPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatDueDateInv>("DueDateInvAttorney.AttorneyCode", ddAttorneys, false);
                            if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                                dueDatePredicate = dueDatePredicate.Or(ddAttorneyPredicate);
                            else
                                dueDatePredicate = dueDatePredicate.And(ddAttorneyPredicate);
                        }
                    }

                    var dueDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDateInvs.DueDateFrom");
                    var dueDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDateInvs.DueDateTo");
                    var dateTakenFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDateInvs.DateTakenFrom");
                    var dateTakenTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDateInvs.DateTakenTo");
                    var outstandingOnly = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDateInvs.ShowOutstandingActionsOnly");                    

                    Expression<Func<PatDueDateInv, bool>> dueDateCombinedPredicate = d => (
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


                    var ddAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<PatActionDueInv>("DueDateInvs", dueDatePredicate);

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
                }

                //dedocket
                var instructionOp = mainSearchFilters.GetFilterOperator("InstructionOp");
                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("DeDocket")) != null)
                {
                    Expression<Func<PatDueDateInv, bool>> dueDatePredicate = (item) => false;
                    Expression<Func<PatDueDateInv, bool>> dueDateDummyPredicate = (item) => false;

                    var deDocketInstruction = mainSearchFilters.FirstOrDefault(f => f.Property == "DeDocket.Instruction");                    
                    if (deDocketInstruction != null)
                    {
                        deDocketInstruction.Operator = instructionOp;
                        var ddInstructions = deDocketInstruction.GetValueListForLoop();
                        if (ddInstructions.Count > 0)
                        {
                            Expression<Func<PatDueDateInv, bool>> predicate = (item) => false;
                            if (deDocketInstruction.Operator == "eq")
                            {
                                foreach (var val in ddInstructions)
                                {
                                    predicate = predicate.Or(dd => EF.Functions.Like(dd.DeDocketOutstanding.Instruction, val));
                                }
                            }
                            else
                            {
                                foreach (var val in ddInstructions)
                                {
                                    predicate = predicate.Or(dd => !EF.Functions.Like(dd.DeDocketOutstanding.Instruction, val));
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

                    Expression<Func<PatDueDateInv, bool>> ddCombinedFilter = dd => (
                                                    (deDocketInstructedBy == null || EF.Functions.Like(dd.DeDocketOutstanding.InstructedBy, deDocketInstructedBy.Value)) &&
                                                    (deDocketInstructionFrom == null || dd.DeDocketOutstanding.InstructionDate >= Convert.ToDateTime(deDocketInstructionFrom.Value)) &&
                                                    (deDocketInstructionTo == null || dd.DeDocketOutstanding.InstructionDate <= Convert.ToDateTime(deDocketInstructionTo.Value).AddDays(1).AddSeconds(-1))
                                                );

                    if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                        dueDatePredicate = dueDatePredicate.Or(ddCombinedFilter);
                    else
                        dueDatePredicate = dueDatePredicate.And(ddCombinedFilter);

                    var ddAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<PatActionDueInv>("DueDateInvs", dueDatePredicate);
                    actionsDue = actionsDue.Where(ddAnyPredicate);

                    //Expression<Func<PatActionDue, bool>> instructionDateFilter =
                    //    ad => ad.DueDates.Any(dd => (deDocketInstruction == null || EF.Functions.Like(dd.DeDocketOutstanding.Instruction, deDocketInstruction.Value)) &&
                    //                                (deDocketInstructedBy == null || EF.Functions.Like(dd.DeDocketOutstanding.InstructedBy, deDocketInstructedBy.Value)) &&
                    //                                (deDocketInstructionFrom == null || dd.DeDocketOutstanding.InstructionDate >= Convert.ToDateTime(deDocketInstructionFrom.Value)) &&
                    //                                (deDocketInstructionTo == null || dd.DeDocketOutstanding.InstructionDate <= Convert.ToDateTime(deDocketInstructionTo.Value)));

                    //actionsDue = actionsDue.Where(instructionDateFilter);

                    mainSearchFilters.Remove(deDocketInstruction);
                    mainSearchFilters.Remove(deDocketInstructedBy);
                    mainSearchFilters.Remove(deDocketInstructionFrom);
                    mainSearchFilters.Remove(deDocketInstructionTo);
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
                        actionsDue = actionsDue.Where(ad => ad.CreatedBy=="PO" && ad.UpdatedBy == "PO" && ad.IsElectronic.HasValue && (bool)ad.IsElectronic);
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
                        Expression<Func<PatActionDueInv, bool>> predicate = (item) => false;
                        foreach (var val in actionTypes)
                        {
                            predicate = predicate.Or(ad => EF.Functions.Like(ad.ActionType, val));
                        }
                        actionsDue = actionsDue.Where(predicate);                        
                    }
                    mainSearchFilters.Remove(actionType);
                }

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
                        if (settings.IsSharePointIntegrationOn)
                        {
                            var graphClient = _sharePointService.GetGraphClient();
                            var docs = graphClient.GetSiteDocumentNames(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Patent, new List<string> { SharePointDocLibraryFolder.Invention }, docName.Value).GetAwaiter().GetResult();

                            if (docs.Count > 0)
                            {
                                docs.ForEach(d =>
                                {
                                    var folders = d.Folder.Split("/");
                                    var caseNumber = folders[1];
                                    var countrySubCase = folders[2].Split("-");
                                    var country = countrySubCase[0];
                                    var subCase = "";

                                    if (countrySubCase.Length > 1)
                                    {
                                        subCase = countrySubCase[1];
                                    }
                                    var invention = _inventionService.Inventions.Where(a => a.CaseNumber == caseNumber).FirstOrDefaultAsync().GetAwaiter().GetResult();
                                    if (invention != null)
                                        d.ParentId = invention.InvId;
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
                                  _docService.DocDocuments.Any(d => d.DocFolder.SystemType == SystemTypeCode.Patent
                                            && d.DocFolder.ScreenCode.ToLower() == "inv"
                                            && d.DocFolder.DataKey.ToLower() == "invId"
                                            && d.DocFolder.DataKeyValue == a.InvId
                                            && d.IsActRequired
                                            && (docName == null || EF.Functions.Like(d.DocName, docName.Value))));
                        }
                    }                                     

                    mainSearchFilters.Remove(docName);
                    mainSearchFilters.Remove(verifiedBy);
                    mainSearchFilters.Remove(verifiedDateFrom);
                    mainSearchFilters.Remove(verifiedDateTo);
                }

                if (mainSearchFilters.Any())
                    actionsDue = QueryHelper.BuildCriteria<PatActionDueInv>(actionsDue, mainSearchFilters);
            }
            return actionsDue;
        }

        public async Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<PatActionDueInv> actionsDue, string value)
        {
            var result = await _actionDueService.QueryableList.Where(ad => ad.CaseNumber == value)
                .Select(ad => new CaseNumberLookupViewModel { Id = ad.ActId, CaseNumber = ad.CaseNumber }).FirstOrDefaultAsync();
            return result;
        }

        public PatActionDueInv ConvertViewModelToActionDue(PatActionDueInvDetailViewModel viewModel)
        {
            return _mapper.Map<PatActionDueInv>(viewModel);
        }

        public async Task<PatActionDueInvDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var viewModel = new PatActionDueInvDetailViewModel();

            if (id > 0)
            {
                viewModel = await _actionDueService.QueryableList.ProjectTo<PatActionDueInvDetailViewModel>()
                    .SingleOrDefaultAsync(i => i.ActId == id);

                if (viewModel != null)
                    viewModel.CanModifyAttorney = await _actionDueService.CanModifyAttorney(viewModel.ResponsibleID ?? 0);
            }

            return viewModel;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<PatActionDueInv> actionsDue,
                                                                      List<QueryFilterViewModel> dueDateFilters)
        {
            //var model = actionsDue.ProjectTo<PatActionDueSearchResultViewModel>();

            IQueryable<PatActionDueInvSearchResultViewModel> model;
            if (dueDateFilters.Count() == 0)
            {
                model = actionsDue.Select(ad => new PatActionDueInvSearchResultViewModel
                {
                    ActId = ad.ActId,
                    CaseNumber = ad.CaseNumber,
                    ActionType = ad.ActionType,
                    BaseDate = ad.BaseDate,
                    DisclosureStatus = ad.Invention.DisclosureStatus,
                    DueDate = ad.DueDateInvs.OrderBy(dd => dd.DueDate).FirstOrDefault().DueDate,
                    ActionDue = ad.DueDateInvs.OrderBy(dd => dd.DueDate).FirstOrDefault().ActionDue,
                    DateTaken = ad.DueDateInvs.OrderBy(dd => dd.DueDate).FirstOrDefault().DateTaken,
                    DueDateExtended = ad.DueDateInvs.Any(dd => dd.PatDueDateInvExtensions.Any(e => e.NewDueDate == ad.DueDateInvs.OrderBy(dd => dd.DueDate).FirstOrDefault().DueDate)),
                    CreatedBy = ad.CreatedBy,
                    UpdatedBy = ad.UpdatedBy,
                    DateCreated = ad.DateCreated,
                    LastUpdate = ad.LastUpdate
                });
            }
            else
            {

                var actionDue = dueDateFilters.FirstOrDefault(f => f.Property == "DueDateInvs.ActionDue");
                var indicatorOp = dueDateFilters.GetFilterOperator("IndicatorOp");
                var indicator = dueDateFilters.FirstOrDefault(f => f.Property == "DueDateInvs.Indicator");
                var dueDateFrom = dueDateFilters.FirstOrDefault(f => f.Property == "DueDateInvs.DueDateFrom");
                var dueDateTo = dueDateFilters.FirstOrDefault(f => f.Property == "DueDateInvs.DueDateTo");
                var dateTakenFrom = dueDateFilters.FirstOrDefault(f => f.Property == "DueDateInvs.DateTakenFrom");
                var dateTakenTo = dueDateFilters.FirstOrDefault(f => f.Property == "DueDateInvs.DateTakenTo");
                var outstandingOnly = dueDateFilters.FirstOrDefault(f => f.Property == "DueDateInvs.ShowOutstandingActionsOnly");
                var duedatesAttorney = dueDateFilters.FirstOrDefault(f => f.Property == "DueDateInvs.Attorney");

                Expression<Func<PatDueDateInv, bool>> dueDatePredicate = (item) => false;
                Expression<Func<PatDueDateInv, bool>> dueDateDummyPredicate = (item) => false;

                if (actionDue != null)
                {
                    var actionDues = actionDue.GetValueListForLoop();
                    if (actionDues.Count > 0)
                    {
                        var actionDuePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatDueDateInv>("ActionDue", actionDues, false);
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
                        Expression<Func<PatDueDateInv, bool>> indicatorPredicate = dd => ((indicator.Operator == "eq" && indicators.Contains(dd.Indicator))
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
                        var ddAttorneyPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<PatDueDateInv>("DueDateInvAttorney.AttorneyCode", ddAttorneys, false);
                        if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                            dueDatePredicate = dueDatePredicate.Or(ddAttorneyPredicate);
                        else
                            dueDatePredicate = dueDatePredicate.And(ddAttorneyPredicate);
                    }
                }

                Expression<Func<PatDueDateInv, bool>> dueDateCombinedPredicate = d => (
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


                model = actionsDue.Select(ad => new PatActionDueInvSearchResultViewModel
                {
                    ActId = ad.ActId,
                    CaseNumber = ad.CaseNumber,
                    ActionType = ad.ActionType,
                    BaseDate = ad.BaseDate,
                    DisclosureStatus = ad.Invention.DisclosureStatus,
                    DueDate = ad.DueDateInvs
                                      .AsQueryable().Where(dueDatePredicate)
                                    .OrderBy(dd => dd.DueDate).FirstOrDefault().DueDate,
                    ActionDue = ad.DueDateInvs
                                      .AsQueryable().Where(dueDatePredicate)
                                    .OrderBy(dd => dd.DueDate).FirstOrDefault().ActionDue,
                    DateTaken = ad.DueDateInvs
                                      .AsQueryable().Where(dueDatePredicate)
                                    .OrderBy(dd => dd.DueDate).FirstOrDefault().DateTaken,
                    DueDateExtended = ad.DueDateInvs.AsQueryable().Where(dueDatePredicate)
                                     .Any(dd => dd.PatDueDateInvExtensions.Any(e => e.NewDueDate == ad.DueDateInvs.AsQueryable().Where(dueDatePredicate).OrderBy(dd => dd.DueDate).FirstOrDefault().DueDate)),

                    CreatedBy = ad.CreatedBy,
                    UpdatedBy = ad.UpdatedBy,
                    DateCreated = ad.DateCreated,
                    LastUpdate = ad.LastUpdate
                });
            }

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(inv => inv.CaseNumber);

            var ids = await model.Select(ad => ad.ActId).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };
        }

        public async Task<List<PatActionDueInvInventionInfoViewModel>> GetInvInfoList(string caseNumber)
        {
            var invInfo = await _inventionService.Inventions
                .Where(c => c.CaseNumber == caseNumber)
                .ProjectTo<PatActionDueInvInventionInfoViewModel>()
                .ToListAsync();

            return invInfo;
        }

        #region Workflow
        //public async Task<List<WorkflowEmailViewModel>> DeletedActionDueWorkflow(PatActionDueInv actionDue, string? emailUrl, string? delegatedEmailUrl, List<LookupIntDTO> openDelegatedDdIds)
        //{
        //    var workFlows = new List<WorkflowViewModel>();
        //    var emailWorkflows = new List<WorkflowEmailViewModel>();

        //    var workflowActions = await _workflowViewModelService.GetPatActionDueWorkflowActions(actionDue, PatWorkflowTriggerType.RecordDeleted, false);
        //    workflowActions = workflowActions.Where(a => (a.Workflow.SystemScreen == null || a.Workflow.SystemScreen.ScreenCode.ToLower() == "actinv-workflow")).ToList();
        //    if (workflowActions.Any())
        //    {
        //        workflowActions = _workflowViewModelService.ClearPatBaseWorkflowActions(workflowActions);
        //        foreach (var item in workflowActions)
        //        {
        //            var workFlow = new WorkflowViewModel
        //            {
        //                ActionTypeId = item.ActionTypeId,
        //                ActionValueId = item.ActionValueId,
        //                Preview = item.Preview
        //            };
        //            workFlows.Add(workFlow);
        //        }

        //        var createActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CreateAction).Distinct().ToList();
        //        foreach (var item in createActionWorkflows)
        //        {
        //            await _inventionService.GenerateWorkflowAction(actionDue.InvId, item.ActionValueId, DateTime.Now);
        //        }

        //        var closeActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CloseAction).Distinct().ToList();
        //        foreach (var item in closeActionWorkflows)
        //        {
        //            await _inventionService.CloseWorkflowAction(actionDue.InvId, item.ActionValueId);
        //        }

        //        var wfs = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
        //        if (wfs.Any())
        //        {
        //            emailWorkflows = wfs.Select(wf => new WorkflowEmailViewModel
        //            {
        //                isAutoEmail = !wf.Preview,
        //                qeSetupId = wf.ActionValueId,
        //                autoAttachImages = wf.AutoAttachImages,
        //                id = actionDue.ActId,
        //                fileNames = new string[] { },
        //                emailUrl = emailUrl
        //            }).ToList();
        //        }
        //    }

        //    //delegated action
        //    if (openDelegatedDdIds.Any())
        //    {
        //        workflowActions = await _workflowViewModelService.GetPatActionDueWorkflowActions(actionDue, PatWorkflowTriggerType.ActionDelegatedDeleted, true);
        //        if (workflowActions.Any())
        //        {
        //            var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionDeleted, Convert.ToChar(SystemTypeCode.Patent));
        //            var wf = workflowActions.Where(w => w.ActionTypeId == (int)PatWorkflowActionType.SendEmail).FirstOrDefault();
        //            if (wf != null)
        //            {
        //                foreach (var d in openDelegatedDdIds)
        //                {
        //                    var emails = await _inventionService.GetDeletedDelegationEmails(d.Value);
        //                    var emailString = "";
        //                    foreach (var email in emails)
        //                    {
        //                        if (!doNotEmailList.Any(e => e.IsCaseInsensitiveEqual(email.AssignedTo)))
        //                        {
        //                            emailString = emailString + email.AssignedTo + ";";
        //                        }
        //                    }

        //                    if (!string.IsNullOrEmpty(emailString))
        //                    {
        //                        emailWorkflows.Add(new WorkflowEmailViewModel
        //                        {
        //                            isAutoEmail = !wf.Preview,
        //                            qeSetupId = wf.ActionValueId,
        //                            autoAttachImages = wf.IncludeAttachments,
        //                            id = d.Value,
        //                            fileNames = new string[] { },
        //                            emailUrl = delegatedEmailUrl,
        //                            emailTo = emailString
        //                        });
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return emailWorkflows;
        //}

        //public async Task<List<WorkflowEmailViewModel>> NewOrCompletedActionWorkflow(PatActionDueInv actionDue, string? emailUrl, bool newAction)
        //{
        //    var invention = await _inventionService.Inventions.Include(c => c.Invention).Where(c => c.InvId == actionDue.InvId).FirstOrDefaultAsync();
        //    var workFlows = new List<WorkflowViewModel>();
        //    var triggerType = newAction ? PatWorkflowTriggerType.NewAction : PatWorkflowTriggerType.ActionClosed;
        //    actionDue.Invention = invention;

        //    var workflowActions = await _workflowViewModelService.GetPatActionDueWorkflowActions(actionDue, triggerType, false);
        //    if (workflowActions.Any())
        //    {
        //        var actionTypes = await _inventionService.PatActionTypes.Where(a => (a.CDueId == 0 || a.CDueId == null) && a.ActionType == actionDue.ActionType).ToListAsync();
        //        var matchedWorkFlows = workflowActions.Where(a => actionTypes.Any(at => at.ActionTypeID == a.Workflow.TriggerValueId || a.Workflow.TriggerValueId == 0)).ToList();
        //        matchedWorkFlows = _workflowViewModelService.ClearPatBaseWorkflowActions(matchedWorkFlows);

        //        foreach (var item in matchedWorkFlows)
        //        {
        //            workFlows.Add(new WorkflowViewModel
        //            {
        //                ActionTypeId = item.ActionTypeId,
        //                ActionValueId = item.ActionValueId,
        //                Preview = item.Preview,
        //                AutoAttachImages = item.IncludeAttachments,
        //                EmailUrl = emailUrl
        //            });
        //        }
        //    }

        //    //follow up action may trigger a workflow
        //    if (!string.IsNullOrEmpty(actionDue.FollowUpAction))
        //    {
        //        var followUpWorkflowActions = new List<PatWorkflowAction>();
        //        if (newAction)
        //            followUpWorkflowActions = workflowActions;
        //        else
        //        {
        //            followUpWorkflowActions = await _workflowViewModelService.GetPatActionDueWorkflowActions(actionDue, PatWorkflowTriggerType.NewAction, false);
        //        }

        //        var actionTypes = await _inventionService.PatActionTypes.Where(a => (a.CDueId == 0 || a.CDueId == null) && a.ActionType == actionDue.FollowUpAction).ToListAsync();
        //        var matchedWorkFlows = followUpWorkflowActions.Where(a => actionTypes.Any(at => at.ActionTypeID == a.Workflow.TriggerValueId || a.Workflow.TriggerValueId == 0)).ToList();
        //        matchedWorkFlows = _workflowViewModelService.ClearPatBaseWorkflowActions(matchedWorkFlows);

        //        foreach (var item in matchedWorkFlows)
        //        {
        //            workFlows.Add(new WorkflowViewModel
        //            {
        //                ActionTypeId = item.ActionTypeId,
        //                ActionValueId = item.ActionValueId,
        //                Preview = item.Preview,
        //                AutoAttachImages = item.IncludeAttachments,
        //                EmailUrl = emailUrl
        //            });
        //        }
        //    }

        //    var createActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CreateAction).Distinct().ToList();
        //    foreach (var item in createActionWorkflows)
        //    {
        //        await _inventionService.GenerateWorkflowAction(invention.InvId, item.ActionValueId, DateTime.Now);
        //    }

        //    var closeActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CloseAction).Distinct().ToList();
        //    foreach (var item in closeActionWorkflows)
        //    {
        //        await _inventionService.CloseWorkflowAction(invention.InvId, item.ActionValueId);
        //    }

        //    var emailWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.SendEmail)
        //        .Select(wf => new WorkflowEmailViewModel
        //        {
        //            isAutoEmail = !wf.Preview,
        //            qeSetupId = wf.ActionValueId,
        //            autoAttachImages = wf.AutoAttachImages,
        //            id = actionDue.ActId,
        //            fileNames = new string[] { },
        //            emailUrl = emailUrl
        //        }).Distinct().ToList();

        //    return emailWorkflows;
        //}

        //public async Task<List<WorkflowEmailViewModel>> NewDedocketInstructionWorkflow(IList<ActionDueInvViewModel> updated, string? emailUrl)
        //{
        //    var wfs = new List<WorkflowEmailViewModel>();
        //    var existingRecord = await _actionDueService.QueryableList.Where(a => a.ActId == updated.FirstOrDefault().ActId).Include(a => a.Invention).ThenInclude(c => c.Invention).FirstOrDefaultAsync();
        //    var workFlows = new List<WorkflowViewModel>();
        //    var workflowActions = await _workflowViewModelService.GetPatActionDueWorkflowActions(existingRecord, PatWorkflowTriggerType.DedocketInstruction, false);

        //    if (workflowActions.Any())
        //    {
        //        var instructions = (await _auxService.QueryableList.ToListAsync()).Where(i => updated.Any(u => u.Instruction == i.Instruction)).ToList();
        //        workflowActions = workflowActions.Where(a => instructions.Any(i => i.InstructionId == a.Workflow.TriggerValueId || a.Workflow.TriggerValueId == 0)).ToList();
        //        workflowActions = _workflowViewModelService.ClearPatBaseWorkflowActions(workflowActions);

        //        foreach (var item in workflowActions)
        //        {
        //            var workFlow = new WorkflowViewModel
        //            {
        //                ActionTypeId = item.ActionTypeId,
        //                ActionValueId = item.ActionValueId,
        //                Preview = item.Preview,
        //                AutoAttachImages = item.IncludeAttachments
        //            };
        //            workFlows.Add(workFlow);
        //        }

        //        var createActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CreateAction).Distinct().ToList();
        //        foreach (var item in createActionWorkflows)
        //        {
        //            await _inventionService.GenerateWorkflowAction(existingRecord.Invention.InvId, item.ActionValueId, DateTime.Now);
        //        }

        //        var closeActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.CloseAction).Distinct().ToList();
        //        foreach (var item in closeActionWorkflows)
        //        {
        //            await _inventionService.CloseWorkflowAction(existingRecord.Invention.InvId, item.ActionValueId);
        //        }

        //        var emailWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
        //        if (emailWorkflows.Any())
        //        {
        //            foreach (var wf in emailWorkflows)
        //            {
        //                foreach (var dd in updated)
        //                {
        //                    if (!string.IsNullOrEmpty(dd.Instruction) && instructions.Any(i => i.Instruction == dd.Instruction))
        //                    {
        //                        wfs.Add(new WorkflowEmailViewModel
        //                        {
        //                            isAutoEmail = !wf.Preview,
        //                            qeSetupId = wf.ActionValueId,
        //                            autoAttachImages = wf.AutoAttachImages,
        //                            id = dd.DeDocketId ?? 0,
        //                            fileNames = new string[] { },
        //                            emailUrl = emailUrl
        //                        });

        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return wfs;
        //}


        public async Task<List<WorkflowEmailViewModel>> NewDelegatedTaskWorkflow(string? emailUrl, List<PatDueDateDelegationDetail> newDelegations)
        {
            var emailWorkflows = new List<WorkflowEmailViewModel>();
            var settings = await _settings.GetSetting();

            if (settings.IsWorkflowOn)
            {
                var delegation = newDelegations.FirstOrDefault();
                if (delegation != null)
                {
                    var action = await _actionDueService.QueryableList.Include(a => a.Invention).FirstOrDefaultAsync(a => a.ActId == delegation.ActId || a.DueDateInvs.Any(dd => dd.DDId == delegation.DDId));
                    var workflowActions = await _workflowViewModelService.GetPatActionDueInvWorkflowActions(action, PatWorkflowTriggerType.ActionDelegated, false);
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

                            var delegations = new List<PatDueDateInvDelegation>();
                            if (item.ActId > 0)
                            {
                                delegations = await _dueDateDelegationEntityService.QueryableList.Where(c => c.NotificationSent == 0 && _actionDueService.QueryableList.Any(a => a.ActId == item.ActId && a.DueDateInvs.Any(dd => dd.DDId == c.DDId) && ((c.GroupId == null && groupId == null) || c.GroupId == groupId) && ((c.UserId == null && userId == null) || c.UserId == userId))).ToListAsync();
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
                                var emails = await _inventionService.GetDelegationEmails(d.DelegationId);
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

        //public async Task<List<WorkflowEmailViewModel>> CompletedDelegatedTaskWorkflow(int actId, string? emailUrl, List<LookupIntDTO> dddIds)
        //{
        //    var wfs = new List<WorkflowEmailViewModel>();

        //    var settings = await _settings.GetSetting();
        //    if (settings.IsWorkflowOn)
        //    {
        //        var existingRecord = await _actionDueService.QueryableList.Where(a => a.ActId == actId).Include(a => a.Invention).ThenInclude(c => c.Invention).FirstOrDefaultAsync();
        //        var emailWorkflows = await _workflowViewModelService.GetPatActionDueWorkflowActions(existingRecord, PatWorkflowTriggerType.ActionDelegatedCompleted, false);
        //        emailWorkflows = emailWorkflows.Where(w => w.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
        //        emailWorkflows = _workflowViewModelService.ClearPatBaseWorkflowActions(emailWorkflows);

        //        var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionCompleted, Convert.ToChar(SystemTypeCode.Patent));

        //        if (emailWorkflows.Any())
        //        {
        //            foreach (var wf in emailWorkflows)
        //            {
        //                foreach (var ddd in dddIds)
        //                {
        //                    var emails = await _inventionService.GetDelegationEmails(ddd.Value);
        //                    var emailString = "";
        //                    foreach (var email in emails)
        //                    {
        //                        if (!doNotEmailList.Any(e => e.IsCaseInsensitiveEqual(email.AssignedTo)))
        //                        {
        //                            emailString = emailString + email.AssignedTo + ";";
        //                        }
        //                    }

        //                    if (!string.IsNullOrEmpty(emailString))
        //                    {
        //                        wfs.Add(new WorkflowEmailViewModel
        //                        {
        //                            isAutoEmail = !wf.Preview,
        //                            qeSetupId = wf.ActionValueId,
        //                            autoAttachImages = wf.IncludeAttachments,
        //                            id = ddd.Value,
        //                            fileNames = new string[] { },
        //                            emailUrl = emailUrl,
        //                            emailTo = emailString
        //                        });
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return wfs;
        //}

        public async Task<List<WorkflowEmailViewModel>> ReassignedDelegatedTaskWorkflow(string? emailUrl, List<PatDueDateDelegationDetail> deletedDelegations)
        {
            var wfs = new List<WorkflowEmailViewModel>();

            var settings = await _settings.GetSetting();
            if (settings.IsWorkflowOn)
            {
                var delegation = deletedDelegations.FirstOrDefault();
                if (delegation != null)
                {
                    var existingRecord = await _actionDueService.QueryableList.Include(a => a.Invention).FirstOrDefaultAsync(a => a.ActId == delegation.ActId || a.DueDateInvs.Any(dd => dd.DDId == delegation.DDId));
                    var emailWorkflows = await _workflowViewModelService.GetPatActionDueInvWorkflowActions(existingRecord, PatWorkflowTriggerType.ActionDelegatedReAssigned, false);
                    emailWorkflows = emailWorkflows.Where(w => w.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
                    emailWorkflows = _workflowViewModelService.ClearPatBaseWorkflowActions(emailWorkflows);

                    var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionReassigned, Convert.ToChar(SystemTypeCode.Patent));
                    if (emailWorkflows.Any())
                    {
                        foreach (var wf in emailWorkflows)
                        {
                            foreach (var ddd in deletedDelegations.Where(d => d.DelegationId > 0).ToList())
                            {
                                var emails = await _inventionService.GetDeletedDelegationEmails(ddd.DelegationId);
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

        //public async Task<List<WorkflowEmailViewModel>> DuedateChangedDelegatedTaskWorkflow(int actId, string? emailUrl, List<LookupIntDTO> dddIds)
        //{
        //    var wfs = new List<WorkflowEmailViewModel>();

        //    var settings = await _settings.GetSetting();
        //    if (settings.IsWorkflowOn)
        //    {
        //        var existingRecord = await _actionDueService.QueryableList.Where(a => a.ActId == actId).Include(a => a.Invention).ThenInclude(c => c.Invention).FirstOrDefaultAsync();
        //        var emailWorkflows = await _workflowViewModelService.GetPatActionDueWorkflowActions(existingRecord, PatWorkflowTriggerType.ActionDelegatedDuedateChanged, false);
        //        emailWorkflows = emailWorkflows.Where(w => w.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
        //        emailWorkflows = _workflowViewModelService.ClearPatBaseWorkflowActions(emailWorkflows);

        //        var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionDueDateChanged, Convert.ToChar(SystemTypeCode.Patent));
        //        if (emailWorkflows.Any())
        //        {
        //            foreach (var wf in emailWorkflows)
        //            {
        //                foreach (var ddd in dddIds)
        //                {
        //                    var emails = await _inventionService.GetDelegationEmails(ddd.Value);
        //                    var emailString = "";
        //                    foreach (var email in emails)
        //                    {
        //                        if (!doNotEmailList.Any(e => e.IsCaseInsensitiveEqual(email.AssignedTo)))
        //                        {
        //                            emailString = emailString + email.AssignedTo + ";";
        //                        }
        //                    }

        //                    if (!string.IsNullOrEmpty(emailString))
        //                    {
        //                        wfs.Add(new WorkflowEmailViewModel
        //                        {
        //                            isAutoEmail = !wf.Preview,
        //                            qeSetupId = wf.ActionValueId,
        //                            autoAttachImages = wf.IncludeAttachments,
        //                            id = ddd.Value,
        //                            fileNames = new string[] { },
        //                            emailUrl = emailUrl,
        //                            emailTo = emailString
        //                        });
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return wfs;
        //}

        //public async Task<List<WorkflowEmailViewModel>> DeletedDelegatedTaskWorkflow(int actId, string? emailUrl, List<DelegationEmailDTO> emails)
        //{
        //    var wfs = new List<WorkflowEmailViewModel>();
        //    var settings = await _settings.GetSetting();
        //    if (settings.IsWorkflowOn)
        //    {
        //        var existingRecord = await _actionDueService.QueryableList.Where(a => a.ActId == actId).Include(a => a.Invention).ThenInclude(c => c.Invention).FirstOrDefaultAsync();

        //        var emailWorkflows = await _workflowViewModelService.GetPatActionDueWorkflowActions(existingRecord, PatWorkflowTriggerType.ActionDelegatedDeleted, false);
        //        emailWorkflows = emailWorkflows.Where(w => w.ActionTypeId == (int)PatWorkflowActionType.SendEmail).ToList();
        //        emailWorkflows = _workflowViewModelService.ClearPatBaseWorkflowActions(emailWorkflows);

        //        var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionDeleted, Convert.ToChar(SystemTypeCode.Patent));
        //        if (emailWorkflows.Any())
        //        {
        //            foreach (var wf in emailWorkflows)
        //            {
        //                var emailString = "";
        //                foreach (var email in emails)
        //                {
        //                    if (!doNotEmailList.Any(e => e.IsCaseInsensitiveEqual(email.AssignedTo)))
        //                    {
        //                        emailString = emailString + email.AssignedTo + ";";
        //                    }
        //                    if (!string.IsNullOrEmpty(emailString))
        //                    {
        //                        wfs.Add(new WorkflowEmailViewModel
        //                        {
        //                            isAutoEmail = !wf.Preview,
        //                            qeSetupId = wf.ActionValueId,
        //                            autoAttachImages = wf.IncludeAttachments,
        //                            id = email.DelegationId,
        //                            fileNames = new string[] { },
        //                            emailUrl = emailUrl,
        //                            emailTo = emailString
        //                        });
        //                    }

        //                }

        //            }
        //        }
        //    }
        //    return wfs;
        //}

        #endregion
    }
}
