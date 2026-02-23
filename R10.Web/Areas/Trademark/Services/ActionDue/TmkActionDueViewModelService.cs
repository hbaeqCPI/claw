using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.UI;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Areas.Trademark.ViewModels;
using R10.Web.Extensions;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using R10.Core.Entities;
using R10.Core.DTOs;
using Kendo.Mvc.Extensions;
using R10.Web.Services;
using R10.Web.Services.SharePoint;
using Microsoft.Extensions.Options;
using R10.Core;
using R10.Web.Areas.Shared.ViewModels.SharePoint;
using R10.Core.Services.Trademark;
using R10.Core.Entities.Patent;

namespace R10.Web.Areas.Trademark.Services
{
    public class TmkActionDueViewModelService : ITmkActionDueViewModelService
    {
        private readonly IActionDueService<TmkActionDue, TmkDueDate> _actionDueService;
        private readonly ITmkTrademarkService _trademarkService;
        private readonly IMapper _mapper;
        private readonly INotificationSettingManager _userSettingManager;
        private readonly ISystemSettings<TmkSetting> _settings;
        private readonly IEntityService<TmkDueDateDelegation> _dueDateDelegationEntityService;
        private readonly IEntityService<DeDocketInstruction> _auxService;
        private readonly IWorkflowViewModelService _workflowViewModelService;
        private readonly IDocumentService _docService;
        private readonly IParentEntityService<TmkActionType, TmkActionParameter> _actionTypeService;
        private readonly ISharePointService _sharePointService;
        private readonly GraphSettings _graphSettings;

        public TmkActionDueViewModelService(IActionDueService<TmkActionDue, TmkDueDate> actionDueService, 
                                            ITmkTrademarkService trademarkService, IMapper mapper,
                                            INotificationSettingManager userSettingManager,
                                            ISystemSettings<TmkSetting> settings,
                                            IEntityService<TmkDueDateDelegation> dueDateDelegationEntityService,
                                            IEntityService<DeDocketInstruction> auxService, IWorkflowViewModelService workflowViewModelService,
                                            IDocumentService docService,
                                            IParentEntityService<TmkActionType, TmkActionParameter> actionTypeService,
                                            ISharePointService sharePointService, IOptions<GraphSettings> graphSettings)
        {
            _actionDueService = actionDueService;
            _trademarkService = trademarkService;
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

        public IQueryable<TmkActionDueTmkInfoViewModel> TmkInfo => _trademarkService.TmkTrademarks.ProjectTo<TmkActionDueTmkInfoViewModel>();

        public IQueryable<TmkActionDue> AddCriteria(List<QueryFilterViewModel> mainSearchFilters, IQueryable<TmkActionDue> actionsDue)
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
                var caseType = mainSearchFilters.FirstOrDefault(f => f.Property == "TmkTrademark.CaseType");
                if (caseType != null)
                {
                    caseType.Operator = caseTypeOp;
                    var caseTypes = caseType.GetValueList();

                    if (caseTypes.Count > 0)
                    {
                        if (caseType.Operator == "eq")
                            actionsDue = actionsDue.Where(ad => caseTypes.Contains(ad.TmkTrademark.CaseType));
                        else
                            actionsDue = actionsDue.Where(ad => !caseTypes.Contains(ad.TmkTrademark.CaseType));

                        mainSearchFilters.Remove(caseType);
                    }
                }

                var trademarkStatusOp = mainSearchFilters.GetFilterOperator("TrademarkStatusOp");
                var trademarkStatus = mainSearchFilters.FirstOrDefault(f => f.Property == "TmkTrademark.TrademarkStatus");
                if (trademarkStatus != null)
                {
                    trademarkStatus.Operator = trademarkStatusOp;
                    var trademarkStatuses = trademarkStatus.GetValueList();

                    if (trademarkStatuses.Count > 0)
                    {
                        if (trademarkStatus.Operator == "eq")
                            actionsDue = actionsDue.Where(ad => trademarkStatuses.Contains(ad.TmkTrademark.TrademarkStatus));
                        else
                            actionsDue = actionsDue.Where(ad => !trademarkStatuses.Contains(ad.TmkTrademark.TrademarkStatus));

                        mainSearchFilters.Remove(trademarkStatus);
                    }
                }

                var appNumber = mainSearchFilters.FirstOrDefault(f => f.Property == "TmkTrademark.AppNumber");
                if (appNumber != null)
                {
                    var appNumberSearch = QueryHelper.ExtractSignificantNumbers(appNumber.Value);
                    actionsDue = actionsDue.Where(ad => (EF.Functions.Like(ad.TmkTrademark.AppNumber, appNumber.Value) || EF.Functions.Like(ad.TmkTrademark.AppNumberSearch, appNumberSearch)));
                    mainSearchFilters.Remove(appNumber);
                }

                var indicatorOp = mainSearchFilters.GetFilterOperator("IndicatorOp");
                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("DueDates.")) != null)
                {
                    var actionDue = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.ActionDue");
                    var indicator = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.Indicator");
                    var dueDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.DueDateFrom");
                    var dueDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.DueDateTo");
                    var dateTakenFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.DateTakenFrom");
                    var dateTakenTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.DateTakenTo");
                    var outstandingOnly = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.ShowOutstandingActionsOnly");
                    var duedatesAttorney = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.Attorney");
                    var showSoftDockets = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.ShowSoftDockets");

                    Expression<Func<TmkDueDate, bool>> dueDatePredicate = (item) => false;
                    Expression<Func<TmkDueDate, bool>> dueDateDummyPredicate = (item) => false;
                                        
                    if (actionDue != null)
                    {
                        var actionDues = actionDue.GetValueListForLoop();
                        if (actionDues.Count > 0)
                        {
                            var actionDuePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<TmkDueDate>("ActionDue", actionDues, false);
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
                            Expression<Func<TmkDueDate, bool>> indicatorPredicate = dd => ((indicator.Operator == "eq" && indicators.Contains(dd.Indicator))
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
                            var ddAttorneyPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<TmkDueDate>("DueDateAttorney.AttorneyCode", ddAttorneys, false);
                            if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                                dueDatePredicate = dueDatePredicate.Or(ddAttorneyPredicate);
                            else
                                dueDatePredicate = dueDatePredicate.And(ddAttorneyPredicate);
                        }
                    }

                    Expression<Func<TmkDueDate, bool>> dueDateCombinedPredicate = d => (
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


                    var ddAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<TmkActionDue>("DueDates", dueDatePredicate);

                    actionsDue = actionsDue.Where(ddAnyPredicate);

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
                else if (settings.IsSoftDocketOn)
                {
                    //dont show by default
                    actionsDue = actionsDue.Where(ad => ad.DueDates.Any(dd => dd.Indicator != "Soft Docket"));
                }

                //dedocket
                var instructionOp = mainSearchFilters.GetFilterOperator("InstructionOp");
                if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("DeDocket")) != null) {
                    
                    var deDocketInstruction = mainSearchFilters.FirstOrDefault(f => f.Property == "DeDocket.Instruction");
                    var deDocketInstructedBy = mainSearchFilters.FirstOrDefault(f => f.Property == "DeDocket.InstructedBy");
                    var deDocketInstructionFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DeDocket.InstructionDateFrom");
                    var deDocketInstructionTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DeDocket.InstructionDateTo");
                    var deDocketInstrCompleted = mainSearchFilters.FirstOrDefault(f => f.Property == "DeDocket.DeDocketInstrCompleted");

                    Expression<Func<TmkDueDate, bool>> dueDatePredicate = (item) => false;
                    Expression<Func<TmkDueDate, bool>> dueDateDummyPredicate = (item) => false;
                    
                    if (deDocketInstruction != null)
                    {
                        deDocketInstruction.Operator = instructionOp;
                        var ddInstructions = deDocketInstruction.GetValueListForLoop();
                        if (ddInstructions.Count > 0)
                        {
                            Expression<Func<TmkDueDate, bool>> predicate = (item) => false;
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

                    Expression<Func<TmkDueDate, bool>> ddCombinedFilter = dd => (
                                                  (deDocketInstructedBy == null || dd.DueDateDeDockets.Any(ddk => EF.Functions.Like(ddk.InstructedBy, deDocketInstructedBy.Value))) &&
                                                  (deDocketInstructionFrom == null || dd.DueDateDeDockets.Any(ddk => ddk.InstructionDate >= Convert.ToDateTime(deDocketInstructionFrom.Value))) &&
                                                  (deDocketInstructionTo == null || dd.DueDateDeDockets.Any(ddk => ddk.InstructionDate <= Convert.ToDateTime(deDocketInstructionTo.Value).AddDays(1).AddSeconds(-1))) &&
                                                  (deDocketInstrCompleted == null || (deDocketInstrCompleted.Value == "1" && dd.DueDateDeDockets.Any(ddk => ddk.InstructionCompleted)) || (deDocketInstrCompleted.Value == "0" && dd.DueDateDeDockets.Any(ddk => !ddk.InstructionCompleted || ddk.InstructionCompleted == null)))
                                              );

                    if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                        dueDatePredicate = dueDatePredicate.Or(ddCombinedFilter);
                    else
                        dueDatePredicate = dueDatePredicate.And(ddCombinedFilter);

                    var ddAnyPredicate = R10.Core.ExpressionHelper.BuildAnyPredicate<TmkActionDue>("DueDates", dueDatePredicate);
                    actionsDue = actionsDue.Where(ddAnyPredicate);

                    mainSearchFilters.Remove(deDocketInstruction);
                    mainSearchFilters.Remove(deDocketInstructedBy);
                    mainSearchFilters.Remove(deDocketInstructionFrom);
                    mainSearchFilters.Remove(deDocketInstructionTo);
                    mainSearchFilters.Remove(deDocketInstrCompleted);
                }

                var tlVerify = mainSearchFilters.FirstOrDefault(f => f.Property == "TLVerify");
                if (tlVerify != null)
                {
                    if (tlVerify.Value != "A") {
                        actionsDue = actionsDue.Where(ad => (tlVerify.Value == "1" && ad.VerifyDate == null && ad.IsElectronic == true) ||
                                                        (tlVerify.Value == "0" && (ad.IsElectronic == false || ad.IsElectronic == null)) ||
                                                        (tlVerify.Value == "2" && ad.VerifyDate != null)
                                                 );
                    }
                    mainSearchFilters.Remove(tlVerify);
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
                                docs = graphClient.GetSiteDocumentNamesByMetadata(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Trademark, SharePointDocLibraryFolder.Trademark, docName != null ? docName.Value : "").GetAwaiter().GetResult();
                            else
                                docs = graphClient.GetSiteDocumentNames(_graphSettings.Site.RelativePath, _graphSettings.Site.HostName, SharePointDocLibrary.Trademark, new List<string> { SharePointDocLibraryFolder.Trademark }, docName != null ? docName.Value : "").GetAwaiter().GetResult();

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

                                    var tmkTrademark = _trademarkService.TmkTrademarks.Where(a => a.CaseNumber == caseNumber && a.Country == country && a.SubCase == subCase).FirstOrDefaultAsync().GetAwaiter().GetResult();
                                    if (tmkTrademark != null)
                                        d.ParentId = tmkTrademark.TmkId;
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
                                                    _docService.DocDocuments.Any(d => d.DocFolder != null 
                                                            && d.DocFolder.SystemType == SystemTypeCode.Trademark
                                                            && (d.DocFolder.ScreenCode ?? "").ToLower() == ScreenCode.Trademark.ToLower()
                                                            && (d.DocFolder.DataKey ?? "").ToLower() == "tmkid"
                                                            && d.DocFolder.DataKeyValue == a.TmkId
                                                            && d.IsActRequired
                                                            && (docName == null || EF.Functions.Like(d.DocName, docName.Value))
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

                var actionType = mainSearchFilters.FirstOrDefault(f => f.Property == "ActionType");
                if (actionType != null)
                {
                    var actionTypes = actionType.GetValueListForLoop();
                    if (actionTypes.Count > 0)
                    {
                        Expression<Func<TmkActionDue, bool>> predicate = (item) => false;
                        foreach (var val in actionTypes)
                        {
                            predicate = predicate.Or(ad => EF.Functions.Like(ad.ActionType, val));
                        }
                        actionsDue = actionsDue.Where(predicate);
                    }
                    mainSearchFilters.Remove(actionType);
                }

                if (mainSearchFilters.Any())
                    actionsDue = QueryHelper.BuildCriteria<TmkActionDue>(actionsDue, mainSearchFilters);
            }

            return actionsDue;
        }

        public async Task<CaseNumberLookupViewModel> CaseNumberSearchValueMapper(IQueryable<TmkActionDue> actionsDue, string value)
        {
            var result = await _actionDueService.QueryableList.Where(ad => ad.CaseNumber == value)
                .Select(ad => new CaseNumberLookupViewModel { Id = ad.ActId, CaseNumber = ad.CaseNumber }).FirstOrDefaultAsync();
            return result;
        }

        public TmkActionDue ConvertViewModelToActionDue(TmkActionDueDetailViewModel viewModel)
        {
            return _mapper.Map<TmkActionDue>(viewModel);
        }

        public async Task<TmkActionDueDetailViewModel> CreateViewModelForDetailScreen(int id)
        {
            var viewModel = new TmkActionDueDetailViewModel();

            if (id > 0)
            {
                viewModel = await _actionDueService.QueryableList.ProjectTo<TmkActionDueDetailViewModel>()
                    .SingleOrDefaultAsync(i => i.ActId == id);

                if (viewModel != null)
                    viewModel.CanModifyAttorney = await _actionDueService.CanModifyAttorney(viewModel.ResponsibleID ?? 0);
            }

            return viewModel;
        }

        public async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<TmkActionDue> actionsDue, List<QueryFilterViewModel> dueDateFilters)
        {
            //var model = actionsDue.ProjectTo<TmkActionDueSearchResultViewModel>();

            IQueryable<TmkActionDueSearchResultViewModel> model;
            if (dueDateFilters.Count() == 0)
            {
                model = actionsDue.Select(ad => new TmkActionDueSearchResultViewModel
                {
                    ActId = ad.ActId,
                    CaseNumber = ad.CaseNumber,
                    Country = ad.Country,
                    SubCase = ad.SubCase,
                    ActionType = ad.ActionType,
                    BaseDate = ad.BaseDate,
                    TrademarkStatus = ad.TmkTrademark.TrademarkStatus,
                    DueDate = ad.DueDates.OrderBy(dd => dd.DueDate).FirstOrDefault().DueDate,
                    ActionDue = ad.DueDates.OrderBy(dd => dd.DueDate).FirstOrDefault().ActionDue,
                    DateTaken = ad.DueDates.OrderBy(dd => dd.DueDate).FirstOrDefault().DateTaken,
                    DueDateExtended = ad.DueDates.Any(dd => dd.TmkDueDateExtensions.Any(e => e.NewDueDate == ad.DueDates.OrderBy(dd => dd.DueDate).FirstOrDefault().DueDate)),
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

                Expression<Func<TmkDueDate, bool>> dueDatePredicate = (item) => false;
                Expression<Func<TmkDueDate, bool>> dueDateDummyPredicate = (item) => false;

                if (actionDue != null)
                {
                    var actionDues = actionDue.GetValueListForLoop();
                    if (actionDues.Count > 0)
                    {
                        var actionDuePredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<TmkDueDate>("ActionDue", actionDues, false);
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
                        Expression<Func<TmkDueDate, bool>> indicatorPredicate = dd => ((indicator.Operator == "eq" && indicators.Contains(dd.Indicator))
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
                        var ddAttorneyPredicate = R10.Web.Helpers.ExpressionHelper.BuildLoopNestedPredicate<TmkDueDate>("DueDateAttorney.AttorneyCode", ddAttorneys, false);
                        if (dueDatePredicate.ToString() == dueDateDummyPredicate.ToString())
                            dueDatePredicate = dueDatePredicate.Or(ddAttorneyPredicate);
                        else
                            dueDatePredicate = dueDatePredicate.And(ddAttorneyPredicate);
                    }
                }

                Expression<Func<TmkDueDate, bool>> dueDateCombinedPredicate = d => (
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


                model = actionsDue.Select(ad => new TmkActionDueSearchResultViewModel
                {
                    ActId = ad.ActId,
                    CaseNumber = ad.CaseNumber,
                    Country = ad.Country,
                    SubCase = ad.SubCase,
                    ActionType = ad.ActionType,
                    BaseDate = ad.BaseDate,
                    TrademarkStatus = ad.TmkTrademark.TrademarkStatus,
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
                                     .Any(dd => dd.TmkDueDateExtensions.Any(e => e.NewDueDate == ad.DueDates.AsQueryable().Where(dueDatePredicate).OrderBy(dd => dd.DueDate).FirstOrDefault().DueDate)),
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

            var result = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync();
            return new CPiDataSourceResult()
            {
                Data = result, // await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };
        }

        public async Task<List<TmkActionDueTmkInfoViewModel>> GetTmkInfoList(string caseNumber, string country, string subCase)
        {
            var tmkInfo = await _trademarkService.TmkTrademarks
                .Where(c => c.CaseNumber == caseNumber)
                .ProjectTo<TmkActionDueTmkInfoViewModel>()
                .ToListAsync();

            return tmkInfo;
        }

        private Expression<Func<TmkDueDate, bool>> BuildDueDateCriteria(List<QueryFilterViewModel> mainSearchFilters)
        {
            if (mainSearchFilters.FirstOrDefault(f => f.Property.StartsWith("DueDates.")) != null)
            {
                var actionDue = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.ActionDue");
                var indicator = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.Indicator");
                var dueDateFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.DueDateFrom");
                var dueDateTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.DueDateTo");
                var dateTakenFrom = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.DateTakenFrom");
                var dateTakenTo = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.DateTakenTo");
                var outstandingOnly = mainSearchFilters.FirstOrDefault(f => f.Property == "DueDates.ShowOutstandingActionsOnly");

                Expression<Func<TmkDueDate, bool>> criteria = d => (actionDue == null || EF.Functions.Like(d.ActionDue, actionDue.Value)) &&
                                                                        (indicator == null || EF.Functions.Like(d.Indicator, indicator.Value)) &&
                                                                        (dueDateFrom == null || d.DueDate >= Convert.ToDateTime(dueDateFrom.Value)) &&
                                                                        (dueDateTo == null || d.DueDate <= Convert.ToDateTime(dueDateTo.Value)) &&
                                                                        (dateTakenFrom == null || d.DateTaken >= Convert.ToDateTime(dateTakenFrom.Value)) &&
                                                                        (dateTakenTo == null || d.DateTaken <= Convert.ToDateTime(dateTakenTo.Value)) &&
                                                                        (outstandingOnly == null || d.DateTaken == null);
                return criteria;

            }
            return d => true;
        }

        #region Workflow
        public async Task<List<WorkflowEmailViewModel>> DeletedActionDueWorkflow(TmkActionDue actionDue, string? emailUrl, string? delegatedEmailUrl, List<LookupIntDTO> openDelegatedDdIds)
        {
            var workFlows = new List<WorkflowViewModel>();
            var emailWorkflows = new List<WorkflowEmailViewModel>();

            var workflowActions = await _workflowViewModelService.GetTmkActionDueWorkflowActions(actionDue, TmkWorkflowTriggerType.RecordDeleted, false);
            workflowActions = workflowActions.Where(a => (a.Workflow.SystemScreen == null || a.Workflow.SystemScreen.ScreenCode.ToLower() == "act-workflow")).ToList();
            workflowActions = _workflowViewModelService.ClearTmkBaseWorkflowActions(workflowActions);
            if (workflowActions.Any())
            {
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

                _trademarkService.DetachAllEntities();
                var createActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.CreateAction).Distinct().ToList();
                foreach (var item in createActionWorkflows)
                {
                    await _trademarkService.GenerateWorkflowAction(actionDue.TmkId, item.ActionValueId);
                }

                var closeActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.CloseAction).Distinct().ToList();
                foreach (var item in closeActionWorkflows)
                {
                    var actionDuesToClose = await _trademarkService.CloseWorkflowAction(actionDue.TmkId, item.ActionValueId);
                    if (actionDuesToClose.Any())
                    {
                        foreach (var actionDueToClose in actionDuesToClose)
                        {
                            await _actionDueService.Update(actionDueToClose);
                        }
                    }
                    
                }

                var wfs = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.SendEmail).ToList();
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
                workflowActions = await _workflowViewModelService.GetTmkActionDueWorkflowActions(actionDue, TmkWorkflowTriggerType.ActionDelegatedDeleted, true);
                if (workflowActions.Any())
                {
                    var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionDeleted, Convert.ToChar(SystemTypeCode.Trademark));
                    var wf = workflowActions.Where(w => w.ActionTypeId == (int)TmkWorkflowActionType.SendEmail).FirstOrDefault();
                    if (wf != null)
                    {
                        foreach (var d in openDelegatedDdIds)
                        {
                            var emails = await _trademarkService.GetDeletedDelegationEmails(d.Value);
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


        public async Task<List<WorkflowEmailViewModel>> NewOrCompletedActionWorkflow(TmkActionDue actionDue, string? emailUrl, bool newAction)
        {
            var trademark = await _trademarkService.TmkTrademarks.Where(t => t.TmkId == actionDue.TmkId).AsNoTracking().FirstOrDefaultAsync();
            var workFlows = new List<WorkflowViewModel>();
            var triggerType = newAction ? TmkWorkflowTriggerType.NewAction : TmkWorkflowTriggerType.ActionClosed;
            actionDue.TmkTrademark = trademark;

            var workflowActions = await _workflowViewModelService.GetTmkActionDueWorkflowActions(actionDue, triggerType, false);
            if (workflowActions.Any())
            {
                var actionTypes = await _trademarkService.TmkActionTypes.Where(a => (a.CDueId == 0 || a.CDueId == null) && a.ActionType == actionDue.ActionType).ToListAsync();
                var matchedWorkFlows = workflowActions.Where(a => actionTypes.Any(at => at.ActionTypeID == a.Workflow.TriggerValueId || a.Workflow.TriggerValueId == 0)).ToList();
                matchedWorkFlows = _workflowViewModelService.ClearTmkBaseWorkflowActions(matchedWorkFlows);

                if (!newAction)
                {
                    var matchedWorkFlowsCL = workflowActions.Where(a => a.Workflow.TriggerValueId < 0).ToList();
                    if (matchedWorkFlowsCL.Any())
                    {
                        Expression<Func<TmkCountryDue, bool>> predicate = (item) => false;
                        foreach (var item in matchedWorkFlowsCL)
                        {
                            predicate = predicate.Or(cd => cd.CDueId == Math.Abs(item.Workflow.TriggerValueId));
                        }
                        var baseActionTypesCL = await _trademarkService.TmkCountryDues.Where(predicate).Select(cd => cd.ActionType).ToListAsync();
                        var actionTypesCL = await _trademarkService.TmkCountryDues.Where(cd => baseActionTypesCL.Any(at => at == cd.ActionType) && cd.ActionType == actionDue.ActionType).ToListAsync();
                        if (actionTypesCL.Any())
                        {
                            matchedWorkFlows.AddRange(matchedWorkFlowsCL);
                        }
                    }
                }

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
                _trademarkService.DetachAllEntities();
            }

            //follow up action may trigger a workflow
            if (!string.IsNullOrEmpty(actionDue.FollowUpAction))
            {
                var followUpWorkflowActions = new List<TmkWorkflowAction>();
                if (newAction)
                    followUpWorkflowActions = workflowActions;
                else
                {
                    followUpWorkflowActions = await _workflowViewModelService.GetTmkActionDueWorkflowActions(actionDue, TmkWorkflowTriggerType.NewAction, true);
                }
                var actionTypes = await _trademarkService.TmkActionTypes.Where(a => (a.CDueId == 0 || a.CDueId == null) && a.ActionType == actionDue.FollowUpAction).ToListAsync();
                var matchedWorkFlows = followUpWorkflowActions.Where(a => actionTypes.Any(at => at.ActionTypeID == a.Workflow.TriggerValueId || a.Workflow.TriggerValueId == 0)).ToList();
                matchedWorkFlows = _workflowViewModelService.ClearTmkBaseWorkflowActions(matchedWorkFlows);

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

            var createActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.CreateAction).Distinct().ToList();
            foreach (var item in createActionWorkflows)
            {
                await _trademarkService.GenerateWorkflowAction(trademark.TmkId, item.ActionValueId);
            }

            var closeActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.CloseAction).Distinct().ToList();
            foreach (var item in closeActionWorkflows)
            {
                var actionDuesToClose = await _trademarkService.CloseWorkflowAction(trademark.TmkId, item.ActionValueId);
                if (actionDuesToClose.Any())
                {
                    foreach (var actionDueToClose in actionDuesToClose)
                    {
                        await _actionDueService.Update(actionDueToClose);
                    }
                }
            }

            var emailWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.SendEmail)
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
            var existingRecord = await _actionDueService.QueryableList.Where(a => a.ActId == updated.FirstOrDefault().ActId).Include(a => a.TmkTrademark).FirstOrDefaultAsync();
            var workFlows = new List<WorkflowViewModel>();
            var workflowActions = await _workflowViewModelService.GetTmkActionDueWorkflowActions(existingRecord, TmkWorkflowTriggerType.DedocketInstruction, false);

            if (workflowActions.Any())
            {
                var instructions = (await _auxService.QueryableList.ToListAsync()).Where(i => updated.Any(u => u.Instruction == i.Instruction)).ToList();
                workflowActions = workflowActions.Where(a => instructions.Any(i => i.InstructionId == a.Workflow.TriggerValueId || a.Workflow.TriggerValueId == 0)).ToList();
                workflowActions = _workflowViewModelService.ClearTmkBaseWorkflowActions(workflowActions);

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

                _trademarkService.DetachAllEntities();
                var createActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.CreateAction).Distinct().ToList();
                foreach (var item in createActionWorkflows)
                {
                    await _trademarkService.GenerateWorkflowAction(existingRecord.TmkTrademark.TmkId, item.ActionValueId);
                }

                var closeActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.CloseAction).Distinct().ToList();
                foreach (var item in closeActionWorkflows)
                {
                    var actionDuesToClose = await _trademarkService.CloseWorkflowAction(existingRecord.TmkTrademark.TmkId, item.ActionValueId);
                    if (actionDuesToClose.Any())
                    {
                        foreach (var actionDueToClose in actionDuesToClose)
                        {
                            await _actionDueService.Update(actionDueToClose);
                        }
                    }
                }

                var emailWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.SendEmail).ToList();
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
            var existingRecord = await _actionDueService.QueryableList.Where(a => a.ActId == updated.FirstOrDefault().ActId).Include(a => a.TmkTrademark).FirstOrDefaultAsync();
            var workFlows = new List<WorkflowViewModel>();
            var workflowActions = await _workflowViewModelService.GetTmkActionDueWorkflowActions(existingRecord, TmkWorkflowTriggerType.DedocketInstructionCompleted, false);

            if (workflowActions.Any())
            {
                var instructions = (await _auxService.QueryableList.ToListAsync()).Where(i => updated.Any(u => u.Instruction == i.Instruction)).ToList();
                workflowActions = workflowActions.Where(a => instructions.Any(i => i.InstructionId == a.Workflow.TriggerValueId || a.Workflow.TriggerValueId == 0)).ToList();
                workflowActions = _workflowViewModelService.ClearTmkBaseWorkflowActions(workflowActions);

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

                var createActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.CreateAction).Distinct().ToList();
                foreach (var item in createActionWorkflows)
                {
                    await _trademarkService.GenerateWorkflowAction(existingRecord.TmkTrademark.TmkId, item.ActionValueId);
                }

                var closeActionWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.CloseAction).Distinct().ToList();
                foreach (var item in closeActionWorkflows)
                {
                    await _trademarkService.CloseWorkflowAction(existingRecord.TmkTrademark.TmkId, item.ActionValueId);
                }

                var emailWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.SendEmail).ToList();
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

        public async Task<List<WorkflowEmailViewModel>> NewDelegatedTaskWorkflow(string? emailUrl, List<TmkDueDateDelegationDetail> newDelegations)
        {
            var emailWorkflows = new List<WorkflowEmailViewModel>();
            var settings = await _settings.GetSetting();

            if (settings.IsWorkflowOn)
            {

                var delegation = newDelegations.FirstOrDefault();
                if (delegation != null)
                {
                    var action = await _actionDueService.QueryableList.Include(a => a.TmkTrademark).FirstOrDefaultAsync(a => a.ActId == delegation.ActId || a.DueDates.Any(dd => dd.DDId == delegation.DDId));
                    var workflowActions = await _workflowViewModelService.GetTmkActionDueWorkflowActions(action, TmkWorkflowTriggerType.ActionDelegated, true);
                    workflowActions = workflowActions.Where(w => w.ActionTypeId == (int)TmkWorkflowActionType.SendEmail).ToList();
                    workflowActions = _workflowViewModelService.ClearTmkBaseWorkflowActions(workflowActions);

                    var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionDelegated, Convert.ToChar(SystemTypeCode.Trademark));
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

                            var delegations = new List<TmkDueDateDelegation>();
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
                                var emails = await _trademarkService.GetDelegationEmails(d.DelegationId);
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
                var existingRecord = await _actionDueService.QueryableList.Where(a => a.ActId == actId).Include(a => a.TmkTrademark).FirstOrDefaultAsync();
                var emailWorkflows = await _workflowViewModelService.GetTmkActionDueWorkflowActions(existingRecord, TmkWorkflowTriggerType.ActionDelegatedCompleted, false);
                emailWorkflows = emailWorkflows.Where(w => w.ActionTypeId == (int)TmkWorkflowActionType.SendEmail).ToList();
                emailWorkflows = _workflowViewModelService.ClearTmkBaseWorkflowActions(emailWorkflows);

                var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionCompleted, Convert.ToChar(SystemTypeCode.Trademark));
                if (emailWorkflows.Any())
                {
                    foreach (var wf in emailWorkflows)
                    {
                        foreach (var ddd in dddIds)
                        {
                            var emails = await _trademarkService.GetDelegationEmails(ddd.Value);
                            var emailString = "";
                            foreach (var email in emails)
                            {
                                if (!doNotEmailList.Any(e => e.IsCaseInsensitiveEqual(email.AssignedTo)))
                                {
                                    emailString = emailString + email.AssignedTo + ";";
                                }
                            }

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
            return wfs;
        }

        public async Task<List<WorkflowEmailViewModel>> ReassignedDelegatedTaskWorkflow(string? emailUrl, List<TmkDueDateDelegationDetail> deletedDelegations)
        {
            var wfs = new List<WorkflowEmailViewModel>();

            var settings = await _settings.GetSetting();
            if (settings.IsWorkflowOn)
            {
                var delegation = deletedDelegations.FirstOrDefault();
                if (delegation != null)
                {
                    var existingRecord = await _actionDueService.QueryableList.Include(a => a.TmkTrademark).FirstOrDefaultAsync(a => a.ActId == delegation.ActId || a.DueDates.Any(dd => dd.DDId == delegation.DDId));
                    var emailWorkflows = await _workflowViewModelService.GetTmkActionDueWorkflowActions(existingRecord, TmkWorkflowTriggerType.ActionDelegatedReAssigned, false);
                    emailWorkflows = emailWorkflows.Where(w => w.ActionTypeId == (int)TmkWorkflowActionType.SendEmail).ToList();
                    emailWorkflows = _workflowViewModelService.ClearTmkBaseWorkflowActions(emailWorkflows);

                    var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionReassigned, Convert.ToChar(SystemTypeCode.Trademark));
                    if (emailWorkflows.Any())
                    {
                        foreach (var wf in emailWorkflows)
                        {
                            foreach (var ddd in deletedDelegations.Where(d => d.DelegationId > 0).ToList())
                            {
                                var emails = await _trademarkService.GetDeletedDelegationEmails(ddd.DelegationId);
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
                var existingRecord = await _actionDueService.QueryableList.Where(a => a.ActId == actId).Include(a => a.TmkTrademark).FirstOrDefaultAsync();
                var emailWorkflows = await _workflowViewModelService.GetTmkActionDueWorkflowActions(existingRecord, TmkWorkflowTriggerType.ActionDelegatedDuedateChanged, false);
                emailWorkflows = emailWorkflows.Where(w => w.ActionTypeId == (int)TmkWorkflowActionType.SendEmail).ToList();
                emailWorkflows = _workflowViewModelService.ClearTmkBaseWorkflowActions(emailWorkflows);

                var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionDueDateChanged, Convert.ToChar(SystemTypeCode.Trademark));
                if (emailWorkflows.Any())
                {
                    foreach (var wf in emailWorkflows)
                    {
                        foreach (var ddd in dddIds)
                        {
                            var emails = await _trademarkService.GetDelegationEmails(ddd.Value);
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
                var existingRecord = await _actionDueService.QueryableList.Where(a => a.ActId == actId).Include(a => a.TmkTrademark).FirstOrDefaultAsync();
                var emailWorkflows = await _workflowViewModelService.GetTmkActionDueWorkflowActions(existingRecord, TmkWorkflowTriggerType.ActionDelegatedDeleted, false);
                emailWorkflows = emailWorkflows.Where(w => w.ActionTypeId == (int)TmkWorkflowActionType.SendEmail).ToList();
                emailWorkflows = _workflowViewModelService.ClearTmkBaseWorkflowActions(emailWorkflows);

                var doNotEmailList = await _userSettingManager.GetDoNotSendQuickEmailList(QuickEmailOptOutSetting.ActionDeleted, Convert.ToChar(SystemTypeCode.Trademark));
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

        public async Task<List<WorkflowEmailViewModel>> NewRequestDocketWorkflow(int tmkId, int reqId, string? emailUrl)
        {
            var wfs = new List<WorkflowEmailViewModel>();
            var trademark = await _trademarkService.TmkTrademarks.Where(c => c.TmkId == tmkId).AsNoTracking().FirstOrDefaultAsync();
            var workFlows = new List<WorkflowViewModel>();
            var workflowActions = await _workflowViewModelService.GetTrademarkWorkflowActions(trademark, TmkWorkflowTriggerType.RequestDocket, false);

            if (workflowActions.Any())
            {
                workflowActions = _workflowViewModelService.ClearTmkBaseWorkflowActions(workflowActions);

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

                var emailWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.SendEmail).ToList();
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
