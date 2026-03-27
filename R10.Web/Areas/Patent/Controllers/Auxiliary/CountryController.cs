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
using R10.Core.Entities.Patent;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;

using Newtonsoft.Json;
using R10.Web.Areas;

namespace R10.Web.Areas.Patent.Controllers
{
    [Area("Patent"), Authorize(Policy = PatentAuthorizationPolicy.CanAccessAuxiliary)]
    public class CountryController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<PatCountry> _viewModelService;
        private readonly IEntityService<PatCountry> _patCountryService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _repository;

        private readonly string _dataContainer = "patCountryDetail";

        public CountryController(
            IAuthorizationService authService,
            IViewModelService<PatCountry> viewModelService,
            IEntityService<PatCountry> patCountryService,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper,
            IApplicationDbContext repository)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _patCountryService = patCountryService;
            _localizer = localizer;
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "patCountrySearch",
                Title = _localizer["Country Search"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded
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
                PageId = "patCountrySearchResults",
                Title = _localizer["Country Search Results"].ToString(),
                CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded
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
                var patCountries = _patCountryService.QueryableList;
                if (mainSearchFilters.Count > 0)
                {
                    var area = mainSearchFilters.FirstOrDefault(f => f.Property == "Area");
                    if (area != null)
                    {
                        patCountries = patCountries.Where(w => w.PatCountryAreas.Any(a => EF.Functions.Like(a.Area.Area, area.Value)));
                        mainSearchFilters.Remove(area);
                    }
                }
                patCountries = _viewModelService.AddCriteria(patCountries, mainSearchFilters);

                var result = await _viewModelService.CreateViewModelForGrid(request, patCountries,"Country", "Country");
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

        public async Task<IActionResult> Detail(string id, bool singleRecord = false, bool fromSearch = false, string tab = "")
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
        public IActionResult Print([FromBody] PrintViewModel patCountryPrintModel)
        {
            // ReportService removed during debloat
            return BadRequest("Report service is not available.");
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(string id, bool fromSearch = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            if (TempData["CopyOptions"] != null)
                await ExtractCopyParams(page);

            var detail = page.Detail;

            if (!string.IsNullOrEmpty(id))
                detail.Country = id;

            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Country"].ToString(),
                //SingleRecord = singleRecord || !Request.IsAjax(),
                //ActiveTab = tab,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch
            };
            ModelState.Clear();

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var entity = await _patCountryService.QueryableList.FirstOrDefaultAsync(c => c.Country == id);

            if (entity == null)
                return new RecordDoesNotExistResult();

            await _patCountryService.Delete(entity);

            return Ok();
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatCountry patCountry)
        {
            if (ModelState.IsValid)
            {
                var userName = User.GetUserName();
                var now = DateTime.Now;
                patCountry.UserID = userName;
                patCountry.LastUpdate = now;

                var existing = await _patCountryService.QueryableList.AsNoTracking().FirstOrDefaultAsync(c => c.Country == patCountry.Country);
                if (existing != null)
                    await _patCountryService.Update(patCountry);
                else
                {
                    patCountry.DateCreated = now;
                    await _patCountryService.Add(patCountry);
                }

                return Json(patCountry.Country);
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        public async Task<IActionResult> GetRecordStamps(string id)
        {
            var patCountry = await _patCountryService.QueryableList.FirstOrDefaultAsync(c => c.Country == id);
            if (patCountry == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = patCountry.UserID, dateCreated = patCountry.DateCreated, updatedBy = patCountry.UserID, lastUpdate = patCountry.LastUpdate });
        }

        private async Task<DetailPageViewModel<PatCountry>> PrepareEditScreen(string id)
        {
            var viewModel = new DetailPageViewModel<PatCountry>();
            viewModel.Detail = await _patCountryService.QueryableList.FirstOrDefaultAsync(c => c.Country == id);

            if (viewModel.Detail != null)
            {
                viewModel.AddPatentAuxiliarySecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.EditScreenUrl = $"{viewModel.EditScreenUrl}/{id}";
                viewModel.CopyScreenUrl = $"{viewModel.CopyScreenUrl}/{id}";
                viewModel.Container = _dataContainer;
                viewModel.SearchScreenUrl = this.Url.Action("Index");
            }
            return viewModel;
        }

        private async Task<DetailPageViewModel<PatCountry>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<PatCountry>();
            viewModel.Detail = new PatCountry();

            viewModel.AddPatentAuxiliarySecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        public async Task<IActionResult> AreasRead([DataSourceRequest] DataSourceRequest request, string country)
        {
            var result = (await _repository.PatAreasCountries.AsNoTracking().Where(ca => ca.Country == country).ProjectTo<CountryAreaViewModel>(_mapper.ConfigurationProvider).ToListAsync()).ToDataSourceResult(request);
            return Json(result);
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> AreasUpdate(string country,
            [Bind(Prefix = "updated")]IEnumerable<CountryAreaViewModel> updated,
            [Bind(Prefix = "new")]IEnumerable<CountryAreaViewModel> added,
            [Bind(Prefix = "deleted")]IEnumerable<CountryAreaViewModel> deleted)
        {
            //no delete validation
            var canDelete = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryCanDelete)).Succeeded;
            if (deleted.Any() && !canDelete)
                return Forbid("Not authorized.");

            if (updated.Any() || added.Any() || deleted.Any())
            {
                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                foreach (var item in _mapper.Map<List<PatAreaCountry>>(added))
                    _repository.PatAreasCountries.Add(item);
                foreach (var item in _mapper.Map<List<PatAreaCountry>>(updated))
                    _repository.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                foreach (var item in _mapper.Map<List<PatAreaCountry>>(deleted))
                    _repository.PatAreasCountries.Remove(item);
                await _repository.SaveChangesAsync();
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                _localizer["Area has been saved successfully."].ToString() :
                _localizer["Areas have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> AreaDelete([Bind(Prefix = "deleted")] CountryAreaViewModel deleted)
        {
            if (deleted.AreaCtryId > 0)
            {
                _repository.PatAreasCountries.Remove(_mapper.Map<PatAreaCountry>(deleted));
                await _repository.SaveChangesAsync();
                return Ok(new { success = _localizer["Area has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_patCountryService.QueryableList, request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetCountryList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_patCountryService.QueryableList, request, property, text, filterType, new string[] { "Country", "CountryName" }, requiredRelation);
        }

        public async Task<IActionResult> GetCountryCurrencyList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_patCountryService.QueryableList, request, property, text, filterType, new string[] { "Country", "CountryName" }, requiredRelation);
        }

        [HttpGet()]
        public async Task<IActionResult> Copy(string id)
        {
            var entity = await _patCountryService.QueryableList.FirstOrDefaultAsync(c => c.Country == id);
            if (entity == null) return new RecordDoesNotExistResult();
            var viewModel = new CountryCopyViewModel
            {
                OriginalCountry = entity.Country,
                Country = entity.Country
            };
            return PartialView("_Copy", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCopied([FromBody] CountryCopyViewModel copy)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });
            TempData["CopyOptions"] = JsonConvert.SerializeObject(copy);
            return RedirectToAction("Add");
        }

        private async Task ExtractCopyParams(DetailPageViewModel<PatCountry> page)
        {
            var copyOptionsString = TempData["CopyOptions"].ToString();
            ViewBag.CopyOptions = copyOptionsString;
            var copyOptions = JsonConvert.DeserializeObject<CountryCopyViewModel>(copyOptionsString);
            if (copyOptions != null)
            {
                var source = await _patCountryService.QueryableList.AsNoTracking().FirstOrDefaultAsync(c => c.Country == copyOptions.OriginalCountry);
                if (source != null)
                {
                    page.Detail = source;
                    page.Detail.Country = copyOptions.Country;
                }
            }
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
                    return RedirectToAction(nameof(Detail), new { id = entity.Country, singleRecord = true, fromSearch = true });
            }
            else
                return RedirectToAction(nameof(Add), new { fromSearch = true });
        }
    }
}
