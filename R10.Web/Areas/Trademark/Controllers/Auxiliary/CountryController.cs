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

using Newtonsoft.Json;
using R10.Web.Areas;

namespace R10.Web.Areas.Trademark.Controllers
{
    [Area("Trademark"), Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessAuxiliary)]
    public class CountryController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<TmkCountry> _viewModelService;
        private readonly IEntityService<TmkCountry> _tmkCountryService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _repository;

        private readonly string _dataContainer = "tmkCountryDetail";

        public CountryController(IAuthorizationService authService, IViewModelService<TmkCountry> viewModelService, IEntityService<TmkCountry> TmkCountryService,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper, IApplicationDbContext repository)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _tmkCountryService = TmkCountryService;
            _localizer = localizer;
            _mapper = mapper;
            _repository = repository;
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
                        tmkCountries = tmkCountries.Where(w => w.TmkCountryAreas.Any(a => EF.Functions.Like(a.Area, area.Value)));
                        mainSearchFilters.Remove(area);
                    }
                    var systemName = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemName");
                    if (systemName != null)
                    {
                        tmkCountries = tmkCountries.Where(a => a.Systems != null && EF.Functions.Like(a.Systems, "%" + systemName.Value.Replace("%", "") + "%"));
                        mainSearchFilters.Remove(systemName);
                    }
                }
                tmkCountries = _viewModelService.AddCriteria(tmkCountries, mainSearchFilters);

                var result = await _viewModelService.CreateViewModelForGrid(request, tmkCountries, "Country", "Country");
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

        public async Task<IActionResult> Detail(string id, string systems = "", bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            var page = await PrepareEditScreen(id, systems);
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
        public IActionResult Print([FromBody] PrintViewModel tmkCountryPrintModel)
        {
            // ReportService removed during debloat
            return BadRequest("Report service is not available.");
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(string id, bool fromSearch = false)
        {
            if (!Request.IsAjax() && TempData.Peek("CopyOptions") == null)
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
                FromSearch = fromSearch,
                AfterCancelledInsert = this.Url.Action("Index")
            };
            ModelState.Clear();

            if (Request.IsAjax())
                return PartialView("Index", model);
            return View("Index", model);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string countryCode, string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkCountry WHERE Country=@p0 AND Systems=@p1", countryCode ?? "", systems ?? "");

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkCountry tmkCountry)
        {
            if (ModelState.IsValid)
            {
                var userName = User.GetUserName();
                var now = DateTime.Now;
                tmkCountry.UserID = userName;
                tmkCountry.LastUpdate = now;
                tmkCountry.Systems ??= "";

                // Require at least one system
                if (string.IsNullOrWhiteSpace(tmkCountry.Systems))
                    return new JsonBadRequest("At least one system must be selected.");

                // Deduplicate and sort systems within this record
                var newSystems = tmkCountry.Systems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
                tmkCountry.Systems = string.Join(",", newSystems);

                var isNewRecord = tmkCountry.IsNewRecord || tmkCountry.OriginalSystems == "__NEW__" || tmkCountry.OriginalSystems == null;
                var originalSystemsValue = tmkCountry.OriginalSystems == "__EMPTY__" ? "" : (tmkCountry.OriginalSystems ?? "");

                // Find existing record on update (match by Country + original systems)
                TmkCountry existing = null;
                if (!isNewRecord)
                {
                    existing = await _tmkCountryService.QueryableList.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Country == tmkCountry.Country && c.Systems == originalSystemsValue);
                }

                // Check for duplicate systems across other records with the same Country
                var allRecords = await _tmkCountryService.QueryableList.AsNoTracking()
                    .Where(c => c.Country == tmkCountry.Country && c.Systems != null && c.Systems != "")
                    .Select(c => c.Systems)
                    .ToListAsync();

                // Exclude existing record's systems from the check
                if (existing != null && !string.IsNullOrEmpty(existing.Systems))
                    allRecords.Remove(existing.Systems);

                var usedSystems = allRecords
                    .Where(s => !string.IsNullOrEmpty(s))
                    .SelectMany(s => s.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(s => s.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var duplicates = newSystems.Where(s => usedSystems.Contains(s)).ToList();
                if (duplicates.Any())
                    return new JsonBadRequest($"The following systems are already assigned to {tmkCountry.Country}: {string.Join(", ", duplicates)}");

                if (existing != null)
                {
                    tmkCountry.DateCreated = existing.DateCreated ?? now;

                    // Use raw SQL to avoid EF composite key tracking issues
                    await _repository.Database.ExecuteSqlRawAsync(
                        @"UPDATE tblTmkCountry SET Country=@p0, CountryName=@p1, CPICode=@p2, Systems=@p3, UserID=@p4, DateCreated=@p5, LastUpdate=@p6
                          WHERE Country=@p7 AND Systems=@p8",
                        tmkCountry.Country, tmkCountry.CountryName ?? "", tmkCountry.CPICode ?? "", tmkCountry.Systems, tmkCountry.UserID ?? "",
                        tmkCountry.DateCreated, tmkCountry.LastUpdate,
                        existing.Country, existing.Systems ?? "");
                }
                else
                {
                    tmkCountry.DateCreated = now;
                    await _repository.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO tblTmkCountry (Country, CountryName, CPICode, Systems, UserID, DateCreated, LastUpdate)
                          VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)",
                        tmkCountry.Country, tmkCountry.CountryName ?? "", tmkCountry.CPICode ?? "", tmkCountry.Systems, tmkCountry.UserID ?? "",
                        tmkCountry.DateCreated, tmkCountry.LastUpdate);
                }

                return Json(new { id = tmkCountry.Country, redirectUrl = Url.Action("Detail", new { id = tmkCountry.Country, systems = tmkCountry.Systems, singleRecord = true }) });
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        public async Task<IActionResult> GetRecordStamps(string id, string systems = "")
        {
            var tmkCountry = await _tmkCountryService.QueryableList.FirstOrDefaultAsync(c => c.Country == id && c.Systems == systems);
            if (tmkCountry == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = tmkCountry.UserID, dateCreated = tmkCountry.DateCreated, updatedBy = tmkCountry.UserID, lastUpdate = tmkCountry.LastUpdate });
        }

        private async Task<DetailPageViewModel<TmkCountry>> PrepareEditScreen(string id, string systems = "")
        {
            var viewModel = new DetailPageViewModel<TmkCountry>();
            viewModel.Detail = await _tmkCountryService.QueryableList.FirstOrDefaultAsync(c => c.Country == id && c.Systems == systems);

            if (viewModel.Detail != null)
            {
                viewModel.AddTrademarkAuxiliarySecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = _dataContainer;

                viewModel.EditScreenUrl = this.Url.Action("Detail", new { id = id, systems = systems });
                viewModel.DeleteScreenUrl = viewModel.CanDeleteRecord ? Url.Action("Delete", new { countryCode = id, systems = systems }) : "";
                viewModel.CopyScreenUrl = $"{viewModel.CopyScreenUrl}?id={id}&systems={Uri.EscapeDataString(systems)}";
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

        [HttpGet()]
        public async Task<IActionResult> Copy(string id, string systems = "")
        {
            var entity = await _tmkCountryService.QueryableList.FirstOrDefaultAsync(c => c.Country == id && c.Systems == systems);
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

        private async Task ExtractCopyParams(DetailPageViewModel<TmkCountry> page)
        {
            var copyOptionsString = TempData["CopyOptions"].ToString();
            ViewBag.CopyOptions = copyOptionsString;
            var copyOptions = JsonConvert.DeserializeObject<CountryCopyViewModel>(copyOptionsString);
            if (copyOptions != null)
            {
                var source = await _tmkCountryService.QueryableList.AsNoTracking().FirstOrDefaultAsync(c => c.Country == copyOptions.OriginalCountry);
                if (source != null)
                {
                    page.Detail = source;
                    page.Detail.Country = copyOptions.Country;
                }
            }
        }

        public async Task<IActionResult> AreasRead([DataSourceRequest] DataSourceRequest request, string country)
        {
            var result = (await _repository.TmkAreasCountries.AsNoTracking().Where(ca => ca.Country == country).ProjectTo<CountryAreaViewModel>(_mapper.ConfigurationProvider).ToListAsync()).ToDataSourceResult(request);
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

                foreach (var item in _mapper.Map<List<TmkAreaCountry>>(added))
                    _repository.TmkAreasCountries.Add(item);
                foreach (var item in _mapper.Map<List<TmkAreaCountry>>(updated))
                    _repository.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                foreach (var item in _mapper.Map<List<TmkAreaCountry>>(deleted))
                    _repository.TmkAreasCountries.Remove(item);
                await _repository.SaveChangesAsync();
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
            if (!string.IsNullOrEmpty(deleted.Area) && !string.IsNullOrEmpty(deleted.Country))
            {
                _repository.TmkAreasCountries.Remove(_mapper.Map<TmkAreaCountry>(deleted));
                await _repository.SaveChangesAsync();
                return Ok(new { success = _localizer["Area has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        public async Task<IActionResult> GetSystemList()
        {
            var systems = await _repository.AppSystems.AsNoTracking()
                .OrderBy(s => s.SystemName)
                .Select(s => s.SystemName)
                .ToListAsync();
            return Json(systems);
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_tmkCountryService.QueryableList, request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetCountryList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_tmkCountryService.QueryableList, request, property, text, filterType, new string[] { "Country", "CountryName" }, requiredRelation);
        }

        public async Task<IActionResult> GetCountryCurrencyList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_tmkCountryService.QueryableList, request, property, text, filterType, new string[] { "Country", "CountryName" }, requiredRelation);
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
