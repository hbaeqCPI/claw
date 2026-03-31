using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
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
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;
using R10.Web.Services;

using Newtonsoft.Json;
using R10.Web.Areas;

namespace R10.Web.Areas.Patent.Controllers
{
    [Area("Patent"), Authorize(Policy = PatentAuthorizationPolicy.CanAccessAuxiliary)]
    public class AreaController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<PatArea> _viewModelService;
        private readonly IEntityService<PatArea> _areaService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _repository;

        private readonly string _dataContainer = "patAreaDetail";

        public AreaController(
            IAuthorizationService authService,
            IViewModelService<PatArea> viewModelService,
            IEntityService<PatArea> areaService,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper,
            IApplicationDbContext repository)
        {
            _authService = authService;
            _viewModelService = viewModelService;
            _areaService = areaService;
            _localizer = localizer;
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "patAreaSearch",
                Title = _localizer["Area Search"].ToString(),
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
                PageId = "patAreaSearchResults",
                Title = _localizer["Area Search Results"].ToString(),
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
                var areas = _areaService.QueryableList;
                if (mainSearchFilters.Count > 0)
                {
                    var country = mainSearchFilters.FirstOrDefault(f => f.Property == "Country");
                    if (country != null)
                    {
                        areas = areas.Where(w => w.PatAreaCountries.Any(a => EF.Functions.Like(a.Country, country.Value)));
                        mainSearchFilters.Remove(country);
                    }

                    var countryName = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryName");
                    if (countryName != null)
                    {
                        mainSearchFilters.Remove(countryName);
                    }

                    var systemName = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemName");
                    if (systemName != null)
                    {
                        areas = areas.Where(a => a.Systems != null && EF.Functions.Like(a.Systems, "%" + systemName.Value.Replace("%", "") + "%"));
                        mainSearchFilters.Remove(systemName);
                    }
                }
                areas = _viewModelService.AddCriteria(areas, mainSearchFilters);

                var result = await _viewModelService.CreateViewModelForGrid(request, areas, "Area", "Area");
                return Json(result);
            }

            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<DetailPageViewModel<PatArea>> PrepareEditScreen(string id, string systems = "")
        {
            var viewModel = new DetailPageViewModel<PatArea>
            {
                Detail = await _areaService.QueryableList.FirstOrDefaultAsync(c => c.Area == id && c.Systems == systems)
            };

            if (viewModel.Detail != null)
            {
                viewModel.AddPatentAuxiliarySecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = _dataContainer;

                viewModel.EditScreenUrl = this.Url.Action("Detail", new { id = id, systems = systems });
                viewModel.DeleteScreenUrl = viewModel.CanDeleteRecord ? Url.Action("Delete", new { areaCode = id, systems = systems }) : "";
                viewModel.CopyScreenUrl = $"{viewModel.CopyScreenUrl}?id={id}&systems={Uri.EscapeDataString(systems)}";
                viewModel.SearchScreenUrl = this.Url.Action("Index");
            }
            return viewModel;
        }

        public async Task<IActionResult> Detail(string id, string systems = "", bool singleRecord = false, bool fromSearch = false, string tab = "")
        {
            try
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
                    Title = _localizer["Area Detail"].ToString(),
                    RecordId = 1, // non-zero sentinel for string-keyed entity
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
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'><pre>{ex}</pre></div>", "text/html");
            }
        }

        [HttpPost()]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        private async Task<DetailPageViewModel<PatArea>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<PatArea>
            {
                Detail = new PatArea()
            };

            viewModel.AddPatentAuxiliarySecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false)
        {
            if (!Request.IsAjax() && TempData.Peek("CopyOptions") == null)
                return RedirectToAction("Index");

            var page = await PrepareAddScreen();
            if (page.Detail == null)
                return RedirectToAction("Index");

            if (TempData["CopyOptions"] != null)
                await ExtractCopyParams(page);

            var detail = page.Detail;
            PageViewModel model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = page.Container,
                Title = _localizer["New Area"].ToString(),
                RecordId = 0,
                PagePermission = page,
                Data = detail,
                FromSearch = fromSearch,
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();

            if (Request.IsAjax())
                return PartialView("Index", model);
            return View("Index", model);
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatArea patArea)
        {
            if (ModelState.IsValid)
            {
                var userName = User.GetUserName();
                var now = DateTime.Now;
                patArea.UserID = userName;
                patArea.LastUpdate = now;
                patArea.Description ??= "";
                patArea.Systems ??= "";

                // Require at least one system
                if (string.IsNullOrWhiteSpace(patArea.Systems))
                    return new JsonBadRequest("At least one system must be selected.");

                // Deduplicate and sort systems within this record
                var newSystems = patArea.Systems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
                patArea.Systems = string.Join(",", newSystems);

                var isNewRecord = patArea.IsNewRecord || patArea.OriginalSystems == "__NEW__" || patArea.OriginalSystems == null;
                var originalSystemsValue = patArea.OriginalSystems == "__EMPTY__" ? "" : (patArea.OriginalSystems ?? "");

                // Find existing record on update (match by Area + original systems)
                PatArea existing = null;
                if (!isNewRecord)
                {
                    existing = await _areaService.QueryableList.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Area == patArea.Area && c.Systems == originalSystemsValue);
                }

                // Check for duplicate systems across other records with the same Area name
                var allRecords = await _areaService.QueryableList.AsNoTracking()
                    .Where(c => c.Area == patArea.Area && c.Systems != null && c.Systems != "")
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
                    return new JsonBadRequest($"The following systems are already assigned to {patArea.Area}: {string.Join(", ", duplicates)}");

                if (existing != null)
                {
                    patArea.DateCreated = existing.DateCreated ?? now;

                    // Use raw SQL to avoid EF composite key tracking issues
                    await _repository.Database.ExecuteSqlRawAsync(
                        @"UPDATE tblPatArea SET Area=@p0, Description=@p1, Systems=@p2, UserID=@p3, DateCreated=@p4, LastUpdate=@p5
                          WHERE Area=@p6 AND Systems=@p7",
                        patArea.Area, patArea.Description ?? "", patArea.Systems, patArea.UserID ?? "",
                        patArea.DateCreated, patArea.LastUpdate,
                        existing.Area, existing.Systems ?? "");
                }
                else
                {
                    patArea.DateCreated = now;
                    await _repository.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO tblPatArea (Area, Description, Systems, UserID, DateCreated, LastUpdate)
                          VALUES (@p0, @p1, @p2, @p3, @p4, @p5)",
                        patArea.Area, patArea.Description ?? "", patArea.Systems, patArea.UserID ?? "",
                        patArea.DateCreated, patArea.LastUpdate);
                }

                return Json(new { id = patArea.Area, redirectUrl = Url.Action("Detail", new { id = patArea.Area, systems = patArea.Systems, singleRecord = true }) });
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string areaCode, string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblPatArea WHERE Area=@p0 AND Systems=@p1", areaCode ?? "", systems ?? "");

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        public async Task<IActionResult> GetRecordStamps(string id, string systems = "")
        {
            var patArea = await _areaService.QueryableList.FirstOrDefaultAsync(c => c.Area == id && c.Systems == systems);
            if (patArea == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = patArea.UserID, dateCreated = patArea.DateCreated, updatedBy = patArea.UserID, lastUpdate = patArea.LastUpdate });
        }

        [HttpGet]
        public IActionResult Print()
        {
            ViewBag.Url = Url.Action("Print");
            ViewBag.DownloadName = "Area Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] PrintViewModel patAreaPrintModel)
        {
            // ReportService removed during debloat
            return BadRequest("Report service is not available.");
        }

        [HttpGet()]
        public async Task<IActionResult> Copy(string id, string systems = "")
        {
            var entity = await _areaService.QueryableList.FirstOrDefaultAsync(c => c.Area == id && c.Systems == systems);
            if (entity == null) return new RecordDoesNotExistResult();
            var viewModel = new AreaCopyViewModel
            {
                OriginalArea = entity.Area,
                Area = entity.Area,
                CopyCountries = true
            };
            return PartialView("_Copy", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCopied([FromBody] AreaCopyViewModel copy)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });
            TempData["CopyOptions"] = JsonConvert.SerializeObject(copy);
            return RedirectToAction("Add");
        }

        private async Task ExtractCopyParams(DetailPageViewModel<PatArea> page)
        {
            var copyOptionsString = TempData["CopyOptions"].ToString();
            ViewBag.CopyOptions = copyOptionsString;
            var copyOptions = JsonConvert.DeserializeObject<AreaCopyViewModel>(copyOptionsString);
            if (copyOptions != null)
            {
                var source = await _areaService.QueryableList.AsNoTracking().FirstOrDefaultAsync(c => c.Area == copyOptions.OriginalArea);
                if (source != null)
                {
                    page.Detail.Area = copyOptions.Area;
                    page.Detail.Description = source.Description;
                    page.Detail.Systems = source.Systems;
                }
            }
        }

        #region Countries Child Grid

        public async Task<IActionResult> CountriesRead([DataSourceRequest] DataSourceRequest request, string areaId)
        {
            var result = (await _repository.PatAreasCountries.AsNoTracking().Where(ca => ca.Area == areaId).ProjectTo<CountryAreaViewModel>(_mapper.ConfigurationProvider).ToListAsync()).ToDataSourceResult(request);
            return Json(result);
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> CountriesUpdate(string areaId,
            [Bind(Prefix = "updated")] IEnumerable<CountryAreaViewModel> updated,
            [Bind(Prefix = "new")] IEnumerable<CountryAreaViewModel> added,
            [Bind(Prefix = "deleted")] IEnumerable<CountryAreaViewModel> deleted)
        {
            var canDelete = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryCanDelete)).Succeeded;
            if (deleted.Any() && !canDelete)
                return Forbid("Not authorized.");

            if (updated.Any() || added.Any() || deleted.Any())
            {
                // Set Area on added items before validation (Area comes from parent, not the grid)
                foreach (var item in added)
                    item.Area = areaId;
                foreach (var item in updated)
                    item.Area = areaId;

                ModelState.Clear();
                foreach (var item in added.Concat(updated))
                    TryValidateModel(item);

                if (!ModelState.IsValid)
                    return new JsonBadRequest(new { errors = ModelState.Errors() });

                foreach (var item in _mapper.Map<List<PatAreaCountry>>(added))
                {
                    item.Area = areaId;
                    _repository.PatAreasCountries.Add(item);
                }
                foreach (var item in _mapper.Map<List<PatAreaCountry>>(updated))
                    _repository.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                foreach (var item in _mapper.Map<List<PatAreaCountry>>(deleted))
                    _repository.PatAreasCountries.Remove(item);
                await _repository.SaveChangesAsync();
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                _localizer["Country has been saved successfully."].ToString() :
                _localizer["Countries have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> CountryDelete([Bind(Prefix = "deleted")] CountryAreaViewModel deleted)
        {
            if (!string.IsNullOrEmpty(deleted.Country))
            {
                _repository.PatAreasCountries.Remove(_mapper.Map<PatAreaCountry>(deleted));
                await _repository.SaveChangesAsync();
                return Ok(new { success = _localizer["Country has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        #endregion

        public async Task<IActionResult> GetSystemList()
        {
            var systems = (await _repository.AppSystems.AsNoTracking()
                .Select(s => s.SystemName)
                .ToListAsync())
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ThenBy(s => s.Length).ToList();
            return Json(systems);
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_areaService.QueryableList, request, property, text, filterType, requiredRelation);
        }

        public async Task<IActionResult> GetAreaList([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_areaService.QueryableList, request, property, text, filterType, new string[] { "Area", "Description" }, requiredRelation);
        }

        [HttpGet]
        public async Task<IActionResult> DetailLink(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                // Find first matching record to get its Systems value
                var first = await _areaService.QueryableList.AsNoTracking()
                    .Where(a => a.Area == id)
                    .Select(a => a.Systems)
                    .FirstOrDefaultAsync();
                return RedirectToAction(nameof(Detail), new { id = id, systems = first ?? "", singleRecord = true, fromSearch = true });
            }
            else
            {
                return RedirectToAction(nameof(Add), new { fromSearch = true });
            }
        }
    }
}
