using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Shared;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Helpers;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Interfaces;
using R10.Web.Security;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using AutoMapper.QueryableExtensions;
using R10.Core;
using R10.Core.DTOs;
using R10.Core.Entities.Trademark;
using R10.Web.Areas.Trademark.ViewModels;
using R10.Web.Areas.Trademark.ViewModels.CountryLaw;
using R10.Core.Helpers;
using R10.Web.Services;

using R10.Web.Areas;

namespace R10.Web.Areas.Trademark.Controllers
{
    [Area("Trademark"), Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessCountryLaw)]
    public class CountryLawController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<TmkCountryLaw> _countryLawViewModelService;
        private readonly ITmkCountryLawService _countryLawService;
        private readonly IParentEntityService<TmkCountry, TmkAreaCountry> _TmkCountryService;
     private readonly IStringLocalizer<CountryLawResource> _localizer;
        private readonly IReportService _reportService;
        private readonly IWebLinksService _webLinksService;

        private readonly string _dataContainer = "countryLawDetailsView";

        public CountryLawController(
            IAuthorizationService authService,
            ITmkCountryLawService countryLawService,
            IViewModelService<TmkCountryLaw> countryLawViewModelService,
            IParentEntityService<TmkCountry, TmkAreaCountry> TmkCountryService,
            IReportService reportService,
            IStringLocalizer<CountryLawResource> localizer,
            IWebLinksService webLinksService
            )
        {
            _authService = authService;
            _countryLawService = countryLawService;
            _countryLawViewModelService = countryLawViewModelService;
            _TmkCountryService = TmkCountryService;
          _localizer = localizer;
            _reportService = reportService;
            _webLinksService = webLinksService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "countryLawSearch",
                Title = _localizer["Country Law Search"].ToString(),
                CanAddRecord = await CanAddRecord()
            };

            if (Request.IsAjax())
                return PartialView("Index", model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {

            var model = new PageViewModel()
            {
                Page = PageType.SearchResults,
                PageId = "countryLawSearchResults",
                Title = _localizer["Country Law Search Results"].ToString(),
                CanAddRecord = await CanAddRecord()
            };

            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search()
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            if (ModelState.IsValid)
            {
                var countryLaws = this.TmkCountryLaws;
                if (mainSearchFilters.Count > 0)
                {
                    countryLaws = AddRelatedDataCriteria(countryLaws, mainSearchFilters);
                    countryLaws = _countryLawViewModelService.AddCriteria(countryLaws, mainSearchFilters);
                }

                var result = await CreateViewModelForGrid(request, countryLaws);
                return Json(result);
            }
            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        [HttpPost()]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        [HttpGet()]
        public IActionResult DetailLink(int? id)
        {
            if (id > 0)
            {
                return RedirectToAction(nameof(Detail), new { id = id, singleRecord = true, fromSearch = true });
            }
            else
            {
                return RedirectToAction(nameof(Add), new { fromSearch = true });
            }
        }

        public async Task<IActionResult> DetailLinkCountryCaseType(string country, string caseType)
        {
            var id = 0;
            var countryLaw = await _countryLawService.TmkCountryLaws.Where(c => c.Country == country && c.CaseType == caseType).FirstOrDefaultAsync();
            if (countryLaw != null)
                id = countryLaw.CountryLawID;
            return RedirectToAction(nameof(DetailLink), new { id = id });
        }

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                return RedirectToAction("Index");
            }

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Country Law Detail"].ToString(),
                RecordId = detail.CountryLawID,
                SingleRecord = singleRecord || !Request.IsAjax(),
                ActiveTab = tab,
                PagePermission = page,
                Data = detail
            };

            if (Request.IsAjax())
            {
                if (!singleRecord && !fromSearch)
                    model.Page = PageType.DetailContent;

                return PartialView("Index", model);
            }

            return View("Index", model);
        }

        [HttpGet]
        public IActionResult Print()
        {
            ViewBag.Url = Url.Action("Print");
            ViewBag.DownloadName = "Country Law Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] TmkCountryLawPrintViewModel tmkCountryLawPrintModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(tmkCountryLawPrintModel, ReportType.TmkCountryLawPrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.CountryLawModify)]
        public async Task<IActionResult> Add(bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer[$"New Country Law"].ToString(),
                RecordId = detail.CountryLawID,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };

            return PartialView("Index", model);
        }

        private async Task<DetailPageViewModel<TmkCountryLawDetailViewModel>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<TmkCountryLawDetailViewModel>
            {
                Detail = new TmkCountryLawDetailViewModel()
            };

            viewModel.AddTrademarkCountryLawSecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }
        
        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.CountryLawCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string tStamp)
        {
            var countryLaw = _countryLawService.TmkCountryLaws.FirstOrDefault(c => c.CountryLawID == id && c.tStamp == Convert.FromBase64String(tStamp));
            if (countryLaw != null)
            {
                await _countryLawService.DeleteCountryLaw(countryLaw);
                return Ok();
            }
            else
                return BadRequest(_localizer["Unable to perform operation because the record is no longer on file or has been modified by another user."]);

        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.CountryLawModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkCountryLaw countryLaw)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(countryLaw, countryLaw.CountryLawID);

                if (countryLaw.CountryLawID > 0)
                    await _countryLawService.UpdateCountryLaw(countryLaw);
                else
                    await _countryLawService.AddCountryLaw(countryLaw);

                return Json(countryLaw.CountryLawID);
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.CountryLawRemarksOnly)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRemarks([FromBody] TmkCountryLaw countryLaw)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(countryLaw, countryLaw.CountryLawID);
                await _countryLawService.UpdateCountryLawRemarks(countryLaw);
                return Json(countryLaw.CountryLawID);
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }


        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var countryLaw = await GetById(id);
            if (countryLaw == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = countryLaw.CreatedBy, dateCreated = countryLaw.DateCreated, updatedBy = countryLaw.UpdatedBy, lastUpdate = countryLaw.LastUpdate, tStamp = countryLaw.tStamp });
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_countryLawService.TmkCountryLaws, request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetCountryList(string property, string text, FilterType filterType)
        {
            var countries = _TmkCountryService.QueryableList;
            countries = countries.Where(c => c.TmkCountryLaws.Any());
            countries = QueryHelper.BuildCriteria(countries, property, text, filterType);
            var list = await countries.Select(c => new { Country = c.Country, CountryName = c.CountryName }).OrderBy(c => c.Country).ToListAsync();
            return Json(list);
        }

        public async Task<IActionResult> GetCaseTypeList(string property, string text, FilterType filterType)
        {
            var caseTypes = _countryLawService.TmkCaseTypes.Where(c => c.CaseTypeCountryLaws.Any());
            caseTypes = QueryHelper.BuildCriteria(caseTypes, property, text, filterType);
            var list = await caseTypes.Select(c => new { CaseType = c.CaseType, Description = c.Description }).OrderBy(c => c.CaseType).ToListAsync();
            return Json(list);
        }

        public IActionResult GetBasedOnList(string property, string text, FilterType filterType)
        {
            return Json(_countryLawService.GetBasedOnList());
        }
        

        #region CountryDue

        public async Task<IActionResult> CountryDueRead([DataSourceRequest] DataSourceRequest request, int parentId)
        {
            var result = (await _countryLawService.GetCountryDues(parentId)).ToDataSourceResult(request);
            return Json(result);
        }

        public IActionResult CountryDueAdd(int countryLawId,string country, string caseType)
        {
            return PartialView("_CountryDueEntry", new TmkCountryDue  { CountryLawID = countryLawId,Country=country,CaseType=caseType, Calculate = true });
        }

        public async Task<IActionResult> CountryDueCopy(int cDueId)
        {
            var countryDue = await _countryLawService.GetCountryDue(cDueId);
            countryDue.CDueId = 0;
            countryDue.CPIAction = false;
            countryDue.CPIPermanentID = 0;

            ViewBag.CopyLawAction = true;
            return PartialView("_CountryDueEntry", countryDue);
        }

        public async Task<IActionResult> CountryDueEdit(int cDueId)
        {
            var countryDue = await _countryLawService.GetCountryDue(cDueId);
            return PartialView("_CountryDueEntry", countryDue);
        }

        public async Task<IActionResult> CountryDueCompute(int cDueId)
        {
            var vm = await _countryLawService.TmkCountryDues.ProjectTo<CountryLawRetroParam>()
                .FirstOrDefaultAsync(d => d.CDueId == cDueId);
            return PartialView("_CountryDueCompute", vm);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.CountryLawModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateCountryLawActions([FromBody]CountryLawRetroParam criteria)
        {
            criteria.UserName = User.Identity.Name;
            criteria.HasEntityFilterOn = User.HasEntityFilter();
            criteria.HasRespOfficeOn = User.HasRespOfficeFilter();
            await _countryLawService.GenerateCountryLawActions(criteria);
            return Ok();
        }

        public async Task<IActionResult> GetActionDues()
        {
            var result = await _countryLawService.GetActionDues();
            return Json(result);
        }

        public async Task<IActionResult> GetActionTypes()
        {
            var result = await _countryLawService.GetActionTypes();
            return Json(result);
        }

        public IActionResult GetRecurringOptions()
        {
            var result =  _countryLawService.GetRecurringOptions();
            return Json(result);
        }

        public IActionResult GetFollowupList(string country)
        {
            var result = _countryLawService.GetFollowupList(country);
            return Json(result);
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.CountryLawModify)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CountryDueSave([FromBody]TmkCountryDue countryDue)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(countryDue.ActionType))
                    countryDue.ActionType = countryDue.ActionDue;

                if (countryDue.CPIAction)
                {
                    var origCountryDue = await _countryLawService.TmkCountryDues.AsNoTracking().FirstOrDefaultAsync(c => c.CDueId == countryDue.CDueId);
                    origCountryDue.Calculate = countryDue.Calculate;
                    origCountryDue.FollowupAction = countryDue.FollowupAction;
                    origCountryDue.OldFollowupAction = countryDue.OldFollowupAction;
                    origCountryDue.ParentTStamp = countryDue.ParentTStamp;
                    UpdateEntityStamps(origCountryDue, origCountryDue.CDueId);
                    await _countryLawService.CountryDueUpdate(origCountryDue);
                }
                else {
                    UpdateEntityStamps(countryDue, countryDue.CDueId);
                    await _countryLawService.CountryDueUpdate(countryDue);
                }
                return Ok();
            }
            return BadRequest(ModelState);
        }


        [Authorize(Policy = TrademarkAuthorizationPolicy.CountryLawModify)]
        public async Task<IActionResult> CountryDuesUpdate([DataSourceRequest] DataSourceRequest request, int parentId, string country, string caseType, byte[] tStamp, [Bind(Prefix = "updated")]IList<TmkCountryDue> updated,
                   [Bind(Prefix = "new")]IList<TmkCountryDue> added, [Bind(Prefix = "deleted")]IList<TmkCountryDue> deleted)
        {
            //no delete validation
            var canDelete = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CountryLawCanDelete)).Succeeded;
            if (deleted.Any() && !canDelete)
                return Forbid("Not authorized.");

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            foreach (var item in updated)
            {
                item.TmkCountryLaw = null;
                UpdateEntityStamps(item, item.CDueId);
            }
            foreach (var item in added)
            {
                item.TmkCountryLaw = null;
                UpdateEntityStamps(item, item.CDueId);
            }
            await _countryLawService.UpdateChild(parentId, country, caseType, User.GetUserName(), tStamp, updated, added, deleted);
            return Ok();
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.CountryLawCanDelete)]
        public async Task<IActionResult> CountryDueDelete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "deleted")] TmkCountryDue deleted)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (deleted.CDueId > 0)
            {
                UpdateEntityStamps(deleted, deleted.CDueId);
                await _countryLawService.DeleteCountryDue(deleted.CountryLawID, deleted.Country, deleted.CaseType, User.GetUserName(), deleted.ParentTStamp, new List<TmkCountryDue>() { deleted });
            }
            return Ok();
        }
        #endregion

        #region DesCaseType

        public IActionResult DesCaseTypeRead([DataSourceRequest] DataSourceRequest request, string country, string caseType)
        {
            var result = _countryLawService.TmkDesCaseTypes.ProjectTo<TmkDesCaseTypeViewModel>().Where(d => d.IntlCode == country && d.CaseType == caseType).ToDataSourceResult(request);
            return Json(result);
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.CountryLawModify)]
        public async Task<IActionResult> DesCaseTypeUpdate([DataSourceRequest] DataSourceRequest request, int parentId, string country, string caseType, byte[] tStamp, [Bind(Prefix = "updated")]IList<TmkDesCaseType> updated,
                   [Bind(Prefix = "new")]IList<TmkDesCaseType> added, [Bind(Prefix = "deleted")]IList<TmkDesCaseType> deleted)
        {
            //no delete validation
            var canDelete = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CountryLawCanDelete)).Succeeded;
            if (deleted.Any() && !canDelete)
                return Forbid("Not authorized.");

            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            foreach (var item in updated)
            {
                item.ChildCountry = null;
                item.ChildCaseType = null;
                item.ParentCaseType = null;
                item.ParentCountry = null;

                UpdateEntityStamps(item, item.DesCaseTypeID);
            }
            foreach (var item in added)
            {
                item.IntlCode = country;
                item.CaseType = caseType;
                item.GenApp = item.GenApp ?? false;
                item.DesCtryFieldID = 0;
                item.ChildCountry = null;
                UpdateEntityStamps(item, item.DesCaseTypeID);
            }
            await _countryLawService.UpdateChild(parentId, country, caseType, User.GetUserName(), tStamp, updated, added, deleted);
            return Ok();
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.CountryLawCanDelete)]
        public async Task<IActionResult> DesCaseTypeDelete([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "deleted")] TmkDesCaseType deleted)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            if (deleted.DesCaseTypeID > 0)
            {
                UpdateEntityStamps(deleted, deleted.DesCaseTypeID); 
                await _countryLawService.UpdateChild(deleted.CountryLawID, deleted.IntlCode, deleted.CaseType, User.GetUserName(), deleted.ParentTStamp, new List<TmkDesCaseType>(), new List<TmkDesCaseType>(), new List<TmkDesCaseType>() { deleted });
            }
            return Ok();
        }
        #endregion

        protected IQueryable<TmkCountryLaw> TmkCountryLaws => _countryLawService.TmkCountryLaws;

        private async Task<DetailPageViewModel<TmkCountryLawDetailViewModel>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<TmkCountryLawDetailViewModel>
            {
                Detail = await GetById(id)
            };

            if (viewModel.Detail != null)
            {
                viewModel.AddTrademarkCountryLawSecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                //hide copy and email buttons
                viewModel.CanCopyRecord = false;
                viewModel.CanEmail = false;
                viewModel.Detail.CanDeleteChild = viewModel.CanDeleteRecord;
                viewModel.CanDeleteRecord = viewModel.CanDeleteRecord && !viewModel.Detail.IsCPiAction;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = _dataContainer;

                viewModel.EditScreenUrl = Url.Action("Detail", new { id = id });
                viewModel.SearchScreenUrl = Url.Action("Index");
            }
            return viewModel;
        }

        private async Task<TmkCountryLawDetailViewModel> GetById(int id)
        {
            var detail = await _countryLawService.TmkCountryLaws.ProjectTo<TmkCountryLawDetailViewModel>().FirstOrDefaultAsync(c => c.CountryLawID == id);
            detail.HasDesignatedCountries = await _countryLawService.HasDesignatedCountries(detail.Country, detail.CaseType);
            return detail;
        }

        public async Task<IActionResult> WebLinksRead([DataSourceRequest] DataSourceRequest request, int id, int displayChoice = 2)
        {
            var webLinks = await _webLinksService.GetWebLinks(id, "tmkcountrylaw", "FormLink", "");
            if (webLinks != null && displayChoice == 0)
                webLinks = webLinks.Where(w => w.RecordLink).ToList();

            var result = webLinks != null ? webLinks.ToDataSourceResult(request) : new Kendo.Mvc.UI.DataSourceResult();
            return Json(result);
        }

        public async Task<IActionResult> WebLinksUrl(int mainId, int id)
        {
            try
            {
                var url = await _webLinksService.GetWebLinksUrl(mainId, id, "tmkcountrylaw", "FormLink", "");
                return Json(new { url });
            }
            catch (ArgumentException ex)
            {
                return Json(new { url = "", error = ex.Message });
            }
        }

        private async Task<bool> CanAddRecord()
        {
            return (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.CountryLawModify)).Succeeded;
        }

        private IQueryable<TmkCountryLaw> AddRelatedDataCriteria(IQueryable<TmkCountryLaw> countryLaws, List<QueryFilterViewModel> mainSearchFilters)
        {
            var countryName = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryName");
            if (countryName != null)
            {
                countryLaws = countryLaws.Where(c => EF.Functions.Like(c.TmkCountry.CountryName, countryName.Value));
                mainSearchFilters.Remove(countryName);
            }

            var systemName = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemName");
            if (systemName != null)
            {
                countryLaws = countryLaws.Where(c => c.Systems != null && EF.Functions.Like(c.Systems, "%" + systemName.Value + "%"));
                mainSearchFilters.Remove(systemName);
            }

            return countryLaws;
        }

        private async Task<CPiDataSourceResult> CreateViewModelForGrid(DataSourceRequest request, IQueryable<TmkCountryLaw> countryLaws)
        {
            var model = countryLaws.ProjectTo<TmkCountryLawSearchViewModel>();

            if (request.Sorts != null && request.Sorts.Any())
                model = model.ApplySorting(request.Sorts);
            else
                model = model.OrderBy(c => c.CountryName).ThenBy(c => c.CaseType);

            var ids = await model.Select(c => c.CountryLawID).ToArrayAsync();

            return new CPiDataSourceResult()
            {
                Data = await model.ApplyPaging(request.Page, request.PageSize).ToListAsync(),
                Total = ids.Length,
                Ids = ids
            };
        }
    }
}