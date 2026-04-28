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
using LawPortal.Core.Entities.Trademark;
using LawPortal.Core.Helpers;
using LawPortal.Core.Interfaces;
using LawPortal.Web.Areas.Shared.ViewModels;
using LawPortal.Web.Extensions;
using LawPortal.Web.Extensions.ActionResults;
using LawPortal.Web.Helpers;
using LawPortal.Web.Interfaces;
using LawPortal.Web.Models;
using LawPortal.Web.Models.PageViewModels;
using LawPortal.Web.Security;
using LawPortal.Web.Services;

using Newtonsoft.Json;
using LawPortal.Web.Areas;

namespace LawPortal.Web.Areas.Trademark.Controllers
{
    [Area("Trademark"), Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessAuxiliary)]
    public class AreaController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IViewModelService<TmkArea> _viewModelService;
        private readonly IEntityService<TmkArea> _areaService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _repository;

        private readonly string _dataContainer = "tmkAreaDetail";

        public AreaController(
            IAuthorizationService authService,
            IViewModelService<TmkArea> viewModelService,
            IEntityService<TmkArea> areaService,
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
                PageId = "tmkAreaSearch",
                Title = _localizer["Area Search"].ToString(),
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
                PageId = "tmkAreaSearchResults",
                Title = _localizer["Area Search Results"].ToString(),
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
                var areas = _areaService.QueryableList;
                if (mainSearchFilters != null && mainSearchFilters.Count > 0)
                {
                    var country = mainSearchFilters.FirstOrDefault(f => f.Property == "Country");
                    if (country != null && !string.IsNullOrEmpty(country.Value))
                    {
                        var cVals = country.Value.StartsWith("[")
                            ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(country.Value) ?? new List<string> { country.Value }
                            : new List<string> { country.Value };
                        areas = areas.Where(w => w.TmkAreaCountries.Any(a => cVals.Any(v => EF.Functions.Like(a.Country, v))));
                    }
                    if (country != null) mainSearchFilters.Remove(country);

                    var countryName = mainSearchFilters.FirstOrDefault(f => f.Property == "CountryName");
                    if (countryName != null)
                    {
                        mainSearchFilters.Remove(countryName);
                    }

                    {
                    areas = Helpers.QueryHelper.ApplySystemsFilter(areas, mainSearchFilters, a => a.Systems);
                    }
                }
                areas = _viewModelService.AddCriteria(areas, mainSearchFilters);

                var result = await _viewModelService.CreateViewModelForGrid(request, areas, "Area", "Area");
                return Json(result);
            }

            return new JsonBadRequest(new { errors = ModelState.Errors() });
        }

        private async Task<DetailPageViewModel<TmkArea>> PrepareEditScreen(string id, string systems = "")
        {
            var viewModel = new DetailPageViewModel<TmkArea>
            {
                Detail = await _areaService.QueryableList.FirstOrDefaultAsync(c => c.Area == id && c.Systems == systems)
            };

            if (viewModel.Detail != null)
            {
                viewModel.AddTrademarkAuxiliarySecurityPolicies();
                await viewModel.ApplyDetailPagePermission(User, _authService);

                viewModel.CanEmail = false;

                this.AddDefaultNavigationUrls(viewModel);

                viewModel.Container = _dataContainer;
                viewModel.AddScreenUrl = viewModel.CanAddRecord ? Url.Action("Add", new { fromSearch = true }) : "";

                viewModel.EditScreenUrl = this.Url.Action("Detail", new { id = id, systems = systems });
                viewModel.DeleteScreenUrl = viewModel.CanDeleteRecord ? Url.Action("Delete", new { areaCode = id, systems = systems }) : "";
                viewModel.CopyScreenUrl = $"{viewModel.CopyScreenUrl}?id={id}&systems={Uri.EscapeDataString(systems)}";
                viewModel.SearchScreenUrl = this.Url.Action("Index");
            }
            return viewModel;
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
                Title = _localizer["Area Detail"].ToString(),
                RecordId = 1,
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

        [HttpPost()]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }

        private async Task<DetailPageViewModel<TmkArea>> PrepareAddScreen()
        {
            var viewModel = new DetailPageViewModel<TmkArea>
            {
                Detail = new TmkArea()
            };

            viewModel.AddTrademarkAuxiliarySecurityPolicies();
            await viewModel.ApplyDetailPagePermission(User, _authService);

            this.AddDefaultNavigationUrls(viewModel);
            viewModel.Container = _dataContainer;
            return viewModel;
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
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

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkArea tmkArea)
        {
            if (ModelState.IsValid)
            {
                var userName = User.GetUserName();
                var now = DateTime.Now;
                tmkArea.UserID = userName;
                tmkArea.LastUpdate = now;
                tmkArea.Description ??= "";
                tmkArea.Systems ??= "";

                // Require at least one system
                if (string.IsNullOrWhiteSpace(tmkArea.Systems))
                    return new JsonBadRequest("At least one system must be selected.");

                // Deduplicate and sort systems within this record
                var newSystems = tmkArea.Systems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
                tmkArea.Systems = string.Join(",", newSystems);

                // Normalize OriginalSystems for comparison
                var isNewRecord = tmkArea.IsNewRecord || tmkArea.OriginalSystems == "__NEW__" || tmkArea.OriginalSystems == null;
                var originalSystemsValue = tmkArea.OriginalSystems == "__EMPTY__" ? "" : (tmkArea.OriginalSystems ?? "");

                // Find existing record on update (match by Area + original systems)
                TmkArea existing = null;
                if (!isNewRecord)
                {
                    existing = await _areaService.QueryableList.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Area == tmkArea.Area && c.Systems == originalSystemsValue);
                }

                // Check for duplicate systems across other records with the same Area name
                var allRecords = await _areaService.QueryableList.AsNoTracking()
                    .Where(c => c.Area == tmkArea.Area && c.Systems != null && c.Systems != "")
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
                    return new JsonBadRequest($"The following systems are already assigned to {tmkArea.Area}: {string.Join(", ", duplicates)}");

                if (existing != null)
                {
                    tmkArea.DateCreated = existing.DateCreated ?? now;

                    // Use raw SQL to avoid EF composite key tracking issues
                    await _repository.Database.ExecuteSqlRawAsync(
                        @"UPDATE tblTmkArea SET Area=@p0, Description=@p1, Systems=@p2, UserID=@p3, DateCreated=@p4, LastUpdate=@p5
                          WHERE Area=@p6 AND Systems=@p7",
                        tmkArea.Area, tmkArea.Description ?? "", tmkArea.Systems, tmkArea.UserID ?? "",
                        tmkArea.DateCreated, tmkArea.LastUpdate,
                        existing.Area, existing.Systems ?? "");

                    // Cascade Systems change to child AreaCountry records
                    if (tmkArea.Systems != originalSystemsValue)
                    {
                        await _repository.Database.ExecuteSqlRawAsync(
                            @"UPDATE tblTmkAreaCountry SET Systems=@p0 WHERE Area=@p1 AND Systems=@p2",
                            tmkArea.Systems, tmkArea.Area, originalSystemsValue);
                    }
                }
                else
                {
                    tmkArea.DateCreated = now;
                    await _repository.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO tblTmkArea (Area, Description, Systems, UserID, DateCreated, LastUpdate)
                          VALUES (@p0, @p1, @p2, @p3, @p4, @p5)",
                        tmkArea.Area, tmkArea.Description ?? "", tmkArea.Systems, tmkArea.UserID ?? "",
                        tmkArea.DateCreated, tmkArea.LastUpdate);

                    // Copy child AreaCountry records from source if this is a copy
                    if (!string.IsNullOrEmpty(tmkArea.CopyFromSystems))
                    {
                        var sourceArea = tmkArea.CopyFromArea ?? tmkArea.Area;
                        var sourceSystems = tmkArea.CopyFromSystems;
                        await _repository.Database.ExecuteSqlRawAsync(
                            @"INSERT INTO tblTmkAreaCountry (Area, Country, Systems)
                              SELECT @p0, Country, @p1
                              FROM tblTmkAreaCountry
                              WHERE Area=@p2 AND Systems=@p3
                              AND NOT EXISTS (
                                  SELECT 1 FROM tblTmkAreaCountry ac2
                                  WHERE ac2.Area=@p0 AND ac2.Country=tblTmkAreaCountry.Country AND ac2.Systems=@p1
                              )",
                            tmkArea.Area ?? "", tmkArea.Systems, sourceArea ?? "", sourceSystems ?? "");
                    }
                }

                return Json(new { id = tmkArea.Area, redirectUrl = Url.Action("Detail", new { id = tmkArea.Area, systems = tmkArea.Systems, singleRecord = true }) });
            }
            else
            {
                return new JsonBadRequest(new { errors = ModelState.Errors() });
            }
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string areaCode, string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkArea WHERE Area=@p0 AND Systems=@p1", areaCode ?? "", systems ?? "");

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
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

        private async Task ExtractCopyParams(DetailPageViewModel<TmkArea> page)
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
                    if (copyOptions.CopyCountries)
                    {
                        page.Detail.CopyFromSystems = source.Systems;
                        page.Detail.CopyFromArea = source.Area;
                    }
                }
            }
        }

        public async Task<IActionResult> GetRecordStamps(string id, string systems = "")
        {
            var tmkArea = await _areaService.QueryableList.FirstOrDefaultAsync(c => c.Area == id && c.Systems == systems);
            if (tmkArea == null)
                return new NoRecordFoundResult();

            return ViewComponent("RecordStamps", new { createdBy = tmkArea.UserID, dateCreated = tmkArea.DateCreated, updatedBy = tmkArea.UserID, lastUpdate = tmkArea.LastUpdate });
        }

        [HttpGet]
        public IActionResult Print()
        {
            ViewBag.Url = Url.Action("Print");
            ViewBag.DownloadName = "Area Print Screen";
            return View();
        }

        [HttpPost]
        public IActionResult Print([FromBody] PrintViewModel tmkAreaPrintModel)
        {
            // ReportService removed during debloat
            return BadRequest("Report service is not available.");
        }

        #region Countries Child Grid

        public async Task<IActionResult> CountriesRead([DataSourceRequest] DataSourceRequest request, string areaId, string systems = "")
        {
            // Show records where ANY of the parent's systems appear in the AreaCountry's Systems
            var parentIndividualSystems = (systems ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            var allAreaCountries = await _repository.TmkAreasCountries.AsNoTracking().Where(ca => ca.Area == areaId).ToListAsync();
            var filtered = allAreaCountries.Where(ac =>
            {
                var acSystems = (ac.Systems ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                return parentIndividualSystems.Any(ps => acSystems.Contains(ps));
            }).ToList();
            var countryNames = await _repository.TmkCountries.AsNoTracking()
                .ToDictionaryAsync(c => c.Country ?? "", c => c.CountryName ?? "");
            var result = filtered.Select(ac => new CountryAreaViewModel
            {
                Area = ac.Area,
                Country = ac.Country,
                Systems = ac.Systems,
                CountryName = countryNames.GetValueOrDefault(ac.Country ?? "", "")
            }).ToList().ToDataSourceResult(request);
            return Json(result);
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> CountriesUpdate(string areaId,
            [Bind(Prefix = "updated")] IEnumerable<CountryAreaViewModel> updated,
            [Bind(Prefix = "new")] IEnumerable<CountryAreaViewModel> added,
            [Bind(Prefix = "deleted")] IEnumerable<CountryAreaViewModel> deleted,
            string systems = "")
        {
            var canDelete = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryCanDelete)).Succeeded;
            if (deleted.Any() && !canDelete)
                return Forbid("Not authorized.");

            if (updated.Any() || added.Any() || deleted.Any())
            {
                var parentSystems = systems;

                var parentIndividualSystems = (parentSystems ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var item in added)
                {
                    // Check if any existing record for this (Area, Country) has overlapping systems
                    var existingRecords = await _repository.TmkAreasCountries.AsNoTracking()
                        .Where(ac => ac.Area == areaId && ac.Country == item.Country)
                        .Select(ac => ac.Systems).ToListAsync();
                    var existingSystems = existingRecords
                        .SelectMany(s => (s ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                        .Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var overlap = parentIndividualSystems.Where(s => existingSystems.Contains(s)).ToList();
                    if (overlap.Any())
                        return BadRequest($"Country '{item.Country}' already exists for system(s): {string.Join(", ", overlap)}");

                    await _repository.Database.ExecuteSqlRawAsync(
                        "INSERT INTO tblTmkAreaCountry (Area, Country, Systems) VALUES (@p0, @p1, @p2)",
                        areaId, item.Country ?? "", parentSystems);
                }
                foreach (var item in updated)
                {
                    await _repository.Database.ExecuteSqlRawAsync(
                        "UPDATE tblTmkAreaCountry SET Country=@p0 WHERE Area=@p1 AND Country=@p2 AND Systems=@p3",
                        item.Country ?? "", areaId, item.Country ?? "", parentSystems);
                }
                foreach (var item in deleted)
                {
                    await _repository.Database.ExecuteSqlRawAsync(
                        "DELETE FROM tblTmkAreaCountry WHERE Area=@p0 AND Country=@p1 AND Systems=@p2",
                        areaId, item.Country ?? "", parentSystems);
                }
                var success = deleted.Count() + updated.Count() + added.Count() == 1 ?
                _localizer["Country has been saved successfully."].ToString() :
                _localizer["Countries have been saved successfully"].ToString();
                return Ok(new { success = success });
            }
            return Ok();
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> CountryDelete([Bind(Prefix = "deleted")] CountryAreaViewModel deleted)
        {
            if (!string.IsNullOrEmpty(deleted.Country))
            {
                var parentSystems = (await _repository.TmkAreas.AsNoTracking()
                    .Where(a => a.Area == deleted.Area)
                    .Select(a => a.Systems).FirstOrDefaultAsync()) ?? "";

                await _repository.Database.ExecuteSqlRawAsync(
                    "DELETE FROM tblTmkAreaCountry WHERE Area=@p0 AND Country=@p1 AND Systems=@p2",
                    deleted.Area ?? "", deleted.Country ?? "", parentSystems);
                return Ok(new { success = _localizer["Country has been deleted successfully."].ToString() });
            }
            return Ok();
        }

        #endregion

        public IActionResult GetSystemList()
        {
            return Json(Helpers.SystemsHelper.SystemNames);
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
