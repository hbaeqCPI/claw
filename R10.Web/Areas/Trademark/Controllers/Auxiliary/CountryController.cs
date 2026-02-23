using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using R10.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Core.Entities.Patent;

using R10.Web.Areas;

namespace R10.Web.Areas.Trademark.Controllers
{
    [Area("Trademark"), Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessAuxiliary)]
    public class CountryController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<TmkCountry> _viewModelService;
        private readonly IParentEntityService<TmkCountry, TmkAreaCountry> _tmkCountryService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;
        private readonly IReportService _reportService;
        private readonly IEntityService<TmkCountry> _auxService;

        private readonly string _dataContainer = "tmkCountryDetail";

        public CountryController(IAuthorizationService authService, IViewModelService<TmkCountry> viewModelService, IParentEntityService<TmkCountry, TmkAreaCountry> TmkCountryService,
            IStringLocalizer<SharedResource> localizer,
            IReportService reportService,
            IMapper mapper, IEntityService<TmkCountry> auxService)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _tmkCountryService = TmkCountryService;
            _localizer = localizer;
            _reportService = reportService;
            _mapper = mapper;
            _auxService = auxService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "tmkCountrySearch",
                Title = _localizer["Country Search"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded
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
                PageId = "tmkCountrySearchResults",
                Title = _localizer["Country Search Results"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded
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
                var tmkCountries = _tmkCountryService.QueryableList;
                if (mainSearchFilters.Count > 0)
                {
                    var area = mainSearchFilters.FirstOrDefault(f => f.Property == "Area");
                    if (area != null)
                    {
                        tmkCountries = tmkCountries.Where(w => w.TmkCountryAreas.Any(a => EF.Functions.Like(a.Area.Area, area.Value)));
                        mainSearchFilters.Remove(area);
                    }
                    var singleClass = mainSearchFilters.FirstOrDefault(f => f.Property == "SingleClassApplication");
                    if (singleClass != null && singleClass.Value=="false") {
                        tmkCountries = tmkCountries.Where(w => w.SingleClassApplication== null || !(bool)w.SingleClassApplication);
                        mainSearchFilters.Remove(singleClass);
                    }
                }
                tmkCountries = _viewModelService.AddCriteria(tmkCountries, mainSearchFilters);

                var result = await _viewModelService.CreateViewModelForGrid(request, tmkCountries, "Country", "CountryID");
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

        public async Task<IActionResult> Detail(int id, bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            var page = await PrepareEditScreen(id);
            if (page.Detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction("Index");
            }

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = page.Container,
                Title = _localizer["Country Detail"].ToString(),
                RecordId = detail.CountryID,
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
            ViewBag.DownloadName = "Country Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] PrintViewModel tmkCountryPrintModel)
        {
            if (ModelState.IsValid)
            {
                return _reportService.GetReport(tmkCountryPrintModel, ReportType.TmkCountryPrintScreen).Result;
            }

            return BadRequest("Unhandled error.");
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(string id, bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            var detail = page.Detail;

            if (!string.IsNullOrEmpty(id))
                detail.Country = id;

            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Country"].ToString(),
                RecordId = detail.CountryID,
                //SingleRecord = singleRecord || !Request.IsAjax(),
                //ActiveTab = tab,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string tStamp)
        {
            var entity = await _tmkCountryService.QueryableList.FirstOrDefaultAsync(c => c.CountryID == id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            entity.tStamp = Convert.FromBase64String(tStamp);
            await _tmkCountryService.Delete(entity);

            return Ok();
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkCountry TmkCountry)
        {
            if (ModelState.IsValid)
            {
                UpdateEntityStamps(TmkCountry, TmkCountry.CountryID);

                if (TmkCountry.CountryID > 0)
                    await _auxService.Update(TmkCountry);
                    //await _tmkCountryService.Update(TmkCountry); //issue when you update the country code
                else
                    await _tmkCountryService.Add(TmkCountry);

                return Json(TmkCountry.CountryID);
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        public async Task<IActionResult> GetRecordStamps(int id)
        {
            var TmkCountry = await _tmkCountryService.GetByIdAsync(id);
            if (TmkCountry == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = TmkCountry.CreatedBy, dateCreated = TmkCountry.DateCreated, updatedBy = TmkCountry.UpdatedBy, lastUpdate = TmkCountry.LastUpdate, tStamp = TmkCountry.tStamp });
        }

        private async Task<DetailPageViewModel<TmkCountry>> PrepareEditScreen(int id)
        {
            var viewModel = new DetailPageViewModel<TmkCountry>();
            viewModel.Detail = await _tmkCountryService.QueryableList.FirstOrDefaultAsync(c => c.CountryID == id);

            if (viewModel.Detail != null)
            {
                viewModel.AddTrademarkAuxiliarySecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                //hide copy and email buttons
                viewModel.CanCopyRecord = false;
                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";
                viewModel.Container = _dataContainer;
                viewModel.SearchScreenUrl = this.Url.Action("Index");
            }
            return viewModel;
        }

        private async Task<DetailPageViewModel<TmkCountry>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<TmkCountry>();
            viewModel.Detail = new TmkCountry();

            viewModel.AddTrademarkAuxiliarySecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        public async Task<IActionResult> AreasRead([DataSourceRequest] DataSourceRequest request, string country)
        {
            var result = (await _tmkCountryService.ChildService.QueryableList.Where(ca => ca.Country == country).OrderBy(o => o.Area.Area).ProjectTo<CountryAreaViewModel>().ToListAsync()).ToDataSourceResult(request);
            return Json(result);
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> AreasUpdate(string country,
            [Bind(Prefix = "updated")]IEnumerable<CountryAreaViewModel> updated,
            [Bind(Prefix = "new")]IEnumerable<CountryAreaViewModel> added,
            [Bind(Prefix = "deleted")]IEnumerable<CountryAreaViewModel> deleted)
        {
            //no delete validation
            var canDelete = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryCanDelete)).Succeeded;
            if (deleted.Any() && !canDelete)
                return Forbid("Not authorized.");

            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                await _tmkCountryService.ChildService.Update(country, User.GetUserName(),
                    _mapper.Map<List<TmkAreaCountry>>(updated),
                    _mapper.Map<List<TmkAreaCountry>>(added),
                    _mapper.Map<List<TmkAreaCountry>>(deleted)
                    );
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                _localizer["Area has been saved successfully."].ToString() :
                _localizer["Areas have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> AreaDelete([Bind(Prefix = "deleted")] CountryAreaViewModel deleted)
        {
            if (deleted.AreaCtryId > 0)
            {
                await _tmkCountryService.ChildService.Update(deleted.Country, User.GetUserName(), new List<TmkAreaCountry>(), new List<TmkAreaCountry>(), new List<TmkAreaCountry>() { _mapper.Map<TmkAreaCountry>(deleted) });
                return Ok(new { success = _localizer["Area has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_tmkCountryService.QueryableList, request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetCountryList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_tmkCountryService.QueryableList, request, property, text, filterType, new string[] { "Country", "CountryName" }, requiredRelation);
            //requiredRelation won't work if already projected to viewmodel
            //return await GetPicklistData(_tmkCountryService.QueryableList.ProjectTo<CountryLookupViewModel>(), request, property, text, filterType, requiredRelation, false);
        }

        public async Task<IActionResult> GetCountryCurrencyList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_tmkCountryService.QueryableList, request, property, text, filterType, new string[] { "Country", "CountryName", "CurrencyType" }, requiredRelation);
        }

        [HttpGet()]
        public async Task<IActionResult> DetailLink(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var entity = await _viewModelService.GetEntityByCode("Country", id);
                if (entity == null)
                    return RedirectToAction(nameof(Add), new { id = id, fromSearch = true });
                else
                    return RedirectToAction(nameof(Detail), new { id = entity.CountryID, singleRecord = true, fromSearch = true });
            }
            else
                return RedirectToAction(nameof(Add), new { fromSearch = true });
        }
    }
}