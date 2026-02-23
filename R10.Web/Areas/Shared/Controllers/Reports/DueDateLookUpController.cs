using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using R10.Core.Interfaces.Patent;
using R10.Web.Interfaces;
using R10.Core.Entities.Patent;
using System.Linq;
using R10.Core.Interfaces.Patent;
using Kendo.Mvc.Extensions;
using R10.Web.Helpers;
using Microsoft.EntityFrameworkCore;
using R10.Web.Security;
using Microsoft.AspNetCore.Authorization;
using R10.Core.Interfaces;
using R10.Web.Interfaces.Shared;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces.DMS;
using R10.Core.Interfaces.AMS;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.DMS;
using System.Globalization;
using R10.Core.Services;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace R10.Web.Areas.Shared.Controllers.Reports
{
    [Area("Shared"), Authorize(Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    public class DueDateLookUpController : SharedReportBaseLookUpController
    {
        protected readonly IEntityService<TmkStandardGood> _tmkStandardGoodService;
        protected readonly IQuickDocketService _quickDocketService;
        protected readonly IDueDateService<PatActionDue, PatDueDate> _patDueDateService;
        protected readonly IDueDateService<TmkActionDue, TmkDueDate> _tmkDueDateService;
        protected readonly IDueDateService<GMActionDue, GMDueDate> _gmDueDateService;
        protected readonly IDueDateService<DMSActionDue, DMSDueDate> _dmsDueDateService;

        protected readonly IChildEntityService<Invention, PatKeyword> _patKeywordService;
        protected readonly IChildEntityService<TmkTrademark, TmkKeyword> _tmkKeywordService;
        protected readonly IChildEntityService<Disclosure, DMSKeyword> _dmsKeywordService;

        private readonly IEntityService<DMSDueDateDelegation> _DMSDueDateDelegationEntityService;
        private readonly IEntityService<PatDueDateDelegation> _PatDueDateDelegationEntityService;
        private readonly IEntityService<TmkDueDateDelegation> _TmkDueDateDelegationEntityService;
        private readonly IEntityService<GMDueDateDelegation> _GMDueDateDelegationEntityService;
        
        private readonly IDelegationService _delegationService;
        private readonly IProductService _productService;

        public DueDateLookUpController(IInventionService inventionService
            , ICountryApplicationService applicationService
            , ISharedReportViewModelService sharedReportViewModelService
            , ITmkTrademarkService trademarkService
            , IGMMatterService gmMatterService
            , IEntityService<TmkStandardGood> tmkStandardGoodService
            , IDisclosureService disclosureService
            , IAMSDueService amsDueService
            , ISystemSettings<PatSetting> patSettings
            , IMultipleEntityService<Invention, PatOwnerInv> patOwnerInvService
            , IMultipleEntityService<PatOwnerApp> patOwnerAppService
            , IEntityService<TmkOwner> tmkOwnerService
            , IMultipleEntityService<GMMatter, GMMatterAttorney> matterAttorneyService
            , IGMMatterCountryService matterCountryService
            , IQuickDocketService quickDocketService
            , IDueDateService<PatActionDue, PatDueDate> patDueDateService
            , IDueDateService<TmkActionDue, TmkDueDate> tmkDueDateService
            , IDueDateService<GMActionDue, GMDueDate> gmDueDateService
            , IDueDateService<DMSActionDue, DMSDueDate> dmsDueDateService
            , IChildEntityService<Invention, PatKeyword> patKeywordService
            , IChildEntityService<TmkTrademark, TmkKeyword> tmkKeywordService
            , IChildEntityService<Disclosure, DMSKeyword> dmsKeywordService
            , IDelegationService delegationService
            , IEntityService<DMSDueDateDelegation> DMSDueDateDelegationEntityService
            , IEntityService<PatDueDateDelegation> PatDueDateDelegationEntityService
            , IEntityService<TmkDueDateDelegation> TmkDueDateDelegationEntityService
            , IEntityService<GMDueDateDelegation> GMDueDateDelegationEntityService
            , IProductService productService
) : base(inventionService, applicationService, sharedReportViewModelService, trademarkService, gmMatterService, disclosureService, amsDueService, patSettings, patOwnerInvService, patOwnerAppService, tmkOwnerService, matterAttorneyService, matterCountryService
    ,patDueDateService, tmkDueDateService, gmDueDateService, dmsDueDateService)
        {
            _tmkStandardGoodService = tmkStandardGoodService;
            _quickDocketService = quickDocketService;

            _patDueDateService = patDueDateService;
            _tmkDueDateService = tmkDueDateService;
            _gmDueDateService = gmDueDateService;
            _dmsDueDateService = dmsDueDateService;

            _patKeywordService = patKeywordService;
            _tmkKeywordService = tmkKeywordService;
            _dmsKeywordService = dmsKeywordService;

            _delegationService = delegationService;

            _DMSDueDateDelegationEntityService = DMSDueDateDelegationEntityService;
            _PatDueDateDelegationEntityService = PatDueDateDelegationEntityService;
            _TmkDueDateDelegationEntityService = TmkDueDateDelegationEntityService;
            _GMDueDateDelegationEntityService = GMDueDateDelegationEntityService;
            _productService = productService;
        }

        public async Task<IActionResult> GetActionTypeList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var actionTypes = _sharedReportViewModelService.GetCombinedActionTypes;
            IList<SharedEntity> actionTypesList = new List<SharedEntity>();
            if (systemType != null)
            {
                if (systemType.Contains("P") || systemType.Contains("L"))
                    actionTypesList.AddRange(_patDueDateService.ActionsDue.Where(c => _patDueDateService.QueryableList.Any(d => d.ActId == c.ActId && d.DateTaken == null)).Select(c => new SharedEntity { Id = null, Code = c.ActionType, Name = null }).Distinct().ToList());
                if (systemType.Contains("T") || systemType.Contains("M"))
                    actionTypesList.AddRange(_tmkDueDateService.ActionsDue.Where(c => _tmkDueDateService.QueryableList.Any(d => d.ActId == c.ActId && d.DateTaken == null)).Select(c => new SharedEntity { Id = null, Code = c.ActionType, Name = null }).Distinct().ToList());
                if (systemType.Contains("G"))
                    actionTypesList.AddRange(_gmDueDateService.ActionsDue.Where(c => _gmDueDateService.QueryableList.Any(d => d.ActId == c.ActId && d.DateTaken == null)).Select(c => new SharedEntity { Id = null, Code = c.ActionType, Name = null }).Distinct().ToList());
                if (systemType.Contains("D"))
                    actionTypesList.AddRange(_dmsDueDateService.ActionsDue.Where(c => _dmsDueDateService.QueryableList.Any(d => d.ActId == c.ActId && d.DateTaken == null)).Select(c => new SharedEntity { Id = null, Code = c.ActionType, Name = null }).Distinct().ToList());
                if (systemType.Contains("A"))
                    actionTypesList.Add(new SharedEntity { Id = null, Code = "AMS Annuity Due", Name = null });
            }

            //return Json(await QueryHelper.GetPicklistDataAsync(actionTypes.Where(c => actionTypesList.Any(d => d.Code.Equals(c.ActionType))), property, text, filterType, requiredRelation));
            return Json(actionTypesList.Select(c => new { ActionType = c.Code }).OrderBy(c => c.ActionType).ToList());
        }

        public async Task<IActionResult> GetActionDueList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var actionDues = _sharedReportViewModelService.GetCombinedActionDues;
            IList<SharedEntity> actionDuesList = new List<SharedEntity>();
            if (systemType != null)
            {
                if (systemType.Contains("P") || systemType.Contains("L"))
                    actionDuesList.AddRange(_patDueDateService.QueryableList.Where(c => c.DateTaken == null).Select(c => new SharedEntity { Id = null, Code = c.ActionDue, Name = null }).Distinct().ToList());
                if (systemType.Contains("T") || systemType.Contains("M"))
                    actionDuesList.AddRange(_tmkDueDateService.QueryableList.Where(c => c.DateTaken == null).Select(c => new SharedEntity { Id = null, Code = c.ActionDue, Name = null }).Distinct().ToList());
                if (systemType.Contains("G"))
                    actionDuesList.AddRange(_gmDueDateService.QueryableList.Where(c => c.DateTaken == null).Select(c => new SharedEntity { Id = null, Code = c.ActionDue, Name = null }).Distinct().ToList());
                if (systemType.Contains("D"))
                    actionDuesList.AddRange(_dmsDueDateService.QueryableList.Where(c => c.DateTaken == null).Select(c => new SharedEntity { Id = null, Code = c.ActionDue, Name = null }).Distinct().ToList());
                if (systemType.Contains("A"))
                    actionDuesList.Add(new SharedEntity { Id = null, Code = "AMS Annuity Due", Name = null });
            }

            //return Json(await QueryHelper.GetPicklistDataAsync(actionDues.Where(c => actionDuesList.Any(d => d.Code.Equals(c.ActionDue))), property, text, filterType, requiredRelation));
            return Json(actionDuesList.Select(c => new { ActionDue = c.Code }).OrderBy(c => c.ActionDue).ToList());
        }

        public async Task<IActionResult> GetIndicatorList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            var indicators = _sharedReportViewModelService.GetCombinedIndicators;
            return Json(await QueryHelper.GetPicklistDataAsync(indicators, property, text, filterType, requiredRelation));
        }

        public async Task<IActionResult> GetClassList(string property, string text, FilterType filterType)
        {
            return Json(await _tmkStandardGoodService.QueryableList.Select(c => new { ClassAndType = c.Class + " " + c.ClassType }).OrderBy(c => c.ClassAndType).ToListAsync());
        }

        public async Task<IActionResult> GetStatusList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "", string activeSwitch = "2")
        {
            IList<SharedEntity> statuses = new List<SharedEntity>();
            statuses.AddRange(CountryApplications.Where(c=> activeSwitch == "2" || (activeSwitch == "1" && c.PatApplicationStatus.ActiveSwitch) || (activeSwitch == "0" && !c.PatApplicationStatus.ActiveSwitch)).Select(c => new SharedEntity { Id = null, Code = c.ApplicationStatus, Name = null }).Distinct().ToList());
            statuses.AddRange(TmkTrademarks.Where(c => activeSwitch == "2" || (activeSwitch == "1" && c.TmkTrademarkStatus.ActiveSwitch) || (activeSwitch == "0" && !c.TmkTrademarkStatus.ActiveSwitch)).Select(c => new SharedEntity { Id = null, Code = c.TrademarkStatus, Name = null }).Distinct().ToList());
            statuses.AddRange(GMMatters.Where(c => activeSwitch == "2" || (activeSwitch == "1" && c.GMMatterStatus.ActiveSwitch) || (activeSwitch == "0" && !c.GMMatterStatus.ActiveSwitch)).Select(c => new SharedEntity { Id = null, Code = c.MatterStatus, Name = null }).Distinct().ToList());
            statuses.AddRange(Disclosures.Where(c => activeSwitch == "2").Select(c => new SharedEntity { Id = null, Code = c.DMSDisclosureStatus.DisclosureStatus, Name = null }).Distinct().ToList());
            //statuses.AddRange(AMSProjections.Select(c => new SharedEntity { Id = null, Code = c.AMSMain.CountryApplication == null ? c.AMSMain.CPIStatus : c.AMSMain.CountryApplication.ApplicationStatus, Name = null }).Distinct().ToList());

            //var result = _sharedReportViewModelService.GetCombinedStatuses;
            //result = result.Where(c => statuses.Any(e => e.Code == c.Status));
            //if (activeSwitch == "1")
            //{
            //    result = result.Where(s => s.ActiveSwitch == true);
            //}
            //else if (activeSwitch == "0")
            //{
            //    result = result.Where(s => s.ActiveSwitch == false);
            //}

            //return Json(await QueryHelper.GetPicklistDataAsync(statuses.AsQueryable(), property, text, filterType, requiredRelation));
            return Json(statuses.Select(c => new { Status = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(c.Code.ToLower()) }).Distinct().OrderBy(c => c.Status).ToList());
        }

        public IActionResult GetInstructedByList(string property, string text, string systemType, FilterType filterType, string requiredRelation = "")
        {
            IList<SharedEntity> instructedBys = new List<SharedEntity>();
            instructedBys.AddRange(_quickDocketService.PatDueDateDeDockets.Select(c => new SharedEntity { Id = null, Code = c.InstructedBy, Name = null }).Distinct().ToList());
            instructedBys.AddRange(_quickDocketService.TmkDueDateDeDockets.Select(c => new SharedEntity { Id = null, Code = c.InstructedBy, Name = null }).Distinct().ToList());
            instructedBys.AddRange(_quickDocketService.GMDueDateDeDockets.Select(c => new SharedEntity { Id = null, Code = c.InstructedBy, Name = null }).Distinct().ToList());

            var result = instructedBys.Select(c=> new { InstructedBy = c.Code }).Distinct();

            return Json(result.ToList());
        }

        public IActionResult GetKeywordList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType)
        {
            IList<SharedEntity> keywords = new List<SharedEntity>();
            //var patKeywords = _patKeywordService.QueryableList.Where(k => Inventions.Any(i => k.InvId == i.InvId)).Select(c => new SharedEntity { Id = null, Code = c.Keyword, Name = null }).Distinct().ToList();
            //var tmkKeywords = _tmkKeywordService.QueryableList.Where(k => TmkTrademarks.Any(i => k.TmkId == i.TmkId)).Select(c => new SharedEntity { Id = null, Code = c.Keyword, Name = null }).Distinct().ToList();
            //var dmsKeywords = _dmsKeywordService.QueryableList.Where(k => Inventions.Any(i => k.DMSId == i.DMSId)).Select(c => new SharedEntity { Id = null, Code = c.Keyword, Name = null }).Distinct().ToList();

            if (systemType.Contains("P") || systemType.Contains("L") || systemType.Contains("A"))
                keywords.AddRange(_patKeywordService.QueryableList.Where(k => Inventions.Any(i => k.InvId == i.InvId)).Select(c => new SharedEntity { Id = null, Code = c.Keyword, Name = null }).Distinct().ToList());
            if (systemType.Contains("T") || systemType.Contains("M"))
                keywords.AddRange(_tmkKeywordService.QueryableList.Where(k => TmkTrademarks.Any(i => k.TmkId == i.TmkId)).Select(c => new SharedEntity { Id = null, Code = c.Keyword, Name = null }).Distinct().ToList());
            if (systemType.Contains("D"))
                keywords.AddRange(_dmsKeywordService.QueryableList.Where(k => Disclosures.Any(i => k.DMSId == i.DMSId)).Select(c => new SharedEntity { Id = null, Code = c.Keyword, Name = null }).Distinct().ToList());

            return Json(keywords.Select(c => new { Keyword = c.Code }).Distinct().OrderBy(c => c.Keyword).ToList());
        }

        public IActionResult GetUserGroupList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType)
        {
            var userGrouplist = _delegationService.GetAvaliableGroupAndUser();
            var DMSuserGrouplist = userGrouplist.Where(c => _DMSDueDateDelegationEntityService.QueryableList.Any(d => (d.UserId ==null ? d.GroupId.ToString() : d.UserId) == c.Id));
            var PatuserGrouplist = userGrouplist.Where(c => _PatDueDateDelegationEntityService.QueryableList.Any(d => (d.UserId ==null ? d.GroupId.ToString() : d.UserId) == c.Id));
            var TmkuserGrouplist = userGrouplist.Where(c => _TmkDueDateDelegationEntityService.QueryableList.Any(d => (d.UserId ==null ? d.GroupId.ToString() : d.UserId) == c.Id));
            var GMuserGrouplist = userGrouplist.Where(c => _GMDueDateDelegationEntityService.QueryableList.Any(d => (d.UserId ==null ? d.GroupId.ToString() : d.UserId) == c.Id));
            var result = DMSuserGrouplist.Union(PatuserGrouplist).Union(TmkuserGrouplist).Union(GMuserGrouplist).Distinct();
            return Json(result);
        }

        public async Task<IActionResult> GetFamilyNumberList(string property, string text, FilterType filterType)
        {
            var inventions = Inventions.Where(c => c.FamilyNumber != null);

            return Json(await inventions.Select(c => new { FamilyNumber = c.FamilyNumber }).Distinct().OrderBy(c => c.FamilyNumber).ToListAsync());
        }

        public IActionResult GetProductList([DataSourceRequest] DataSourceRequest request, string property, string text, string systemType, FilterType filterType)
        {
            IList<SharedEntity> products = new List<SharedEntity>();
                products.AddRange(_productService.QueryableList.Where(p => _applicationService.QueryableChildList<PatProduct>().Any(c => CountryApplications.Any(a => c.AppId == a.AppId) && p.ProductId == c.ProductId))
                    .Select(c => new SharedEntity { Id = null, Code = c.ProductCode, Name = c.ProductName }).Distinct().ToList());
                products.AddRange(_productService.QueryableList.Where(p => _applicationService.QueryableChildList<PatProductInv>().Any(c => Inventions.Any(i => c.InvId == i.InvId) && p.ProductId == c.ProductId))
                    .Select(c => new SharedEntity { Id = null, Code = c.ProductCode, Name = c.ProductName }).Distinct().ToList());
                products.AddRange(_productService.QueryableList.Where(p => _trademarkService.QueryableChildList<TmkProduct>().Any(c => TmkTrademarks.Any(t => c.TmkId == t.TmkId) && p.ProductId == c.ProductId))
                    .Select(c => new SharedEntity { Id = null, Code = c.ProductCode, Name = c.ProductName }).Distinct().ToList());
                products.AddRange(_productService.QueryableList.Where(p => _gmMatterService.QueryableChildList<GMProduct>().Any(c => GMMatters.Any(g => c.MatId == g.MatId) && p.ProductId == c.ProductId))
                    .Select(c => new SharedEntity { Id = null, Code = c.ProductCode, Name = c.ProductName }).Distinct().ToList());
               
            return Json(products.Select(c => new { ProductCode = c.Code, Product = c.Name }).Distinct().OrderBy(c => c.Product).ToList());
        }
    }
}
