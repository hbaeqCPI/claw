using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Models;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;

namespace R10.Web.Areas.Trademark.Controllers
{
    [Area("Trademark"), Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessAuxiliary)]
    public class AreaCountryDeleteController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "tmkAreaCountryDeleteDetail";

        public AreaCountryDeleteController(IAuthorizationService authService, IApplicationDbContext repository, IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _repository = repository;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel { Page = PageType.Search, PageId = "tmkAreaCountryDeleteSearch", Title = _localizer["Area Country Delete Search"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return Request.IsAjax() ? PartialView("Index", model) : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel { Page = PageType.SearchResults, PageId = "tmkAreaCountryDeleteSearchResults", Title = _localizer["Area Country Delete Search Results"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search() => RedirectToAction("Index");

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            var data = _repository.TmkAreaCountryDeletes.AsNoTracking().AsQueryable();

            if (mainSearchFilters != null && mainSearchFilters.Count > 0)
            {
                var systemName = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemName");
                if (systemName != null)
                {
                    data = data.Where(a => a.Systems != null && EF.Functions.Like(a.Systems, "%" + systemName.Value.Replace("%", "") + "%"));
                    mainSearchFilters.Remove(systemName);
                }

                foreach (var filter in mainSearchFilters)
                {
                    if (filter.Property == "Area" && !string.IsNullOrEmpty(filter.Value))
                        data = data.Where(a => a.Area == filter.Value);
                    else if (filter.Property == "Country" && !string.IsNullOrEmpty(filter.Value))
                        data = data.Where(a => a.Country == filter.Value);
                    else if (filter.Property == "AreaNew" && !string.IsNullOrEmpty(filter.Value))
                        data = data.Where(a => a.AreaNew == filter.Value);
                    else if (filter.Property == "CountryNew" && !string.IsNullOrEmpty(filter.Value))
                        data = data.Where(a => a.CountryNew == filter.Value);
                }
            }

            var result = await data.ToDataSourceResultAsync(request);
            return Json(result);
        }

        private async Task<DetailPagePermission> GetPermission()
        {
            var p = new DetailPagePermission();
            p.CanEditRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded;
            p.CanDeleteRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryCanDelete)).Succeeded;
            p.CanAddRecord = p.CanEditRecord;
            p.CanCopyRecord = p.CanEditRecord;
            return p;
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string copyArea = "", string copyCountry = "", string copyAreaNew = "", string copyCountryNew = "", string copySystems = "")
        {
            if (!Request.IsAjax()) return RedirectToAction("Index");
            var data = new TmkAreaCountryDelete { IsNewRecord = true };

            if (!string.IsNullOrEmpty(copyArea))
            {
                data.Area = copyArea;
                data.Country = copyCountry;
                data.AreaNew = copyAreaNew;
                data.CountryNew = copyCountryNew;
                data.Systems = copySystems ?? "";
            }

            var model = new PageViewModel
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = _dataContainer,
                Title = _localizer["New Area Country Delete"].ToString(),
                Data = data,
                PagePermission = await GetPermission(),
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();
            return PartialView("Index", model);
        }

        public async Task<IActionResult> Detail(string areaCode, string country, string areaNewCode, string countryNew, string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var detail = await _repository.TmkAreaCountryDeletes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Area == areaCode && c.Country == country && c.AreaNew == areaNewCode && c.CountryNew == countryNew && c.Systems == systems);
            if (detail == null)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }
            var perm = await GetPermission();
            perm.DeleteScreenUrl = perm.CanDeleteRecord ? Url.Action("Delete", new { areaCode = areaCode, country = country, areaNewCode = areaNewCode, countryNew = countryNew, systems = systems }) : "";
            perm.CopyScreenUrl = perm.CanCopyRecord ? Url.Action("Add", new { fromSearch = true, copyArea = areaCode, copyCountry = country, copyAreaNew = areaNewCode, copyCountryNew = countryNew, copySystems = systems }) : "";
            perm.IsCopyScreenPopup = false;
            var model = new PageViewModel
            {
                Page = PageType.Detail,
                PageId = _dataContainer,
                Title = _localizer["Area Country Delete Detail"].ToString(),
                RecordId = 1,
                SingleRecord = singleRecord || !Request.IsAjax(),
                Data = detail,
                PagePermission = perm
            };
            if (Request.IsAjax() && !singleRecord && !fromSearch) model.Page = PageType.DetailContent;
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify), ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkAreaCountryDelete entity)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });

            entity.Systems ??= "";

            // Require at least one system
            if (string.IsNullOrWhiteSpace(entity.Systems))
                return new JsonBadRequest("At least one system must be selected.");

            // Deduplicate and sort systems
            var newSystems = entity.Systems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
            entity.Systems = string.Join(",", newSystems);

            var isNewRecord = entity.IsNewRecord || entity.OriginalSystems == "__NEW__" || entity.OriginalSystems == null;
            var originalSystemsValue = entity.OriginalSystems == "__EMPTY__" ? "" : (entity.OriginalSystems ?? "");

            // Find existing record on update
            TmkAreaCountryDelete existing = null;
            if (!isNewRecord)
            {
                existing = await _repository.TmkAreaCountryDeletes.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Area == entity.Area && c.Country == entity.Country && c.AreaNew == entity.AreaNew && c.CountryNew == entity.CountryNew && c.Systems == originalSystemsValue);
            }

            // Check for duplicate systems across other records with the same (Area, Country, AreaNew, CountryNew)
            var allRecords = await _repository.TmkAreaCountryDeletes.AsNoTracking()
                .Where(c => c.Area == entity.Area && c.Country == entity.Country && c.AreaNew == entity.AreaNew && c.CountryNew == entity.CountryNew && c.Systems != null && c.Systems != "")
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
                return new JsonBadRequest($"The following systems are already assigned to {entity.Area}/{entity.Country}/{entity.AreaNew}/{entity.CountryNew}: {string.Join(", ", duplicates)}");

            if (existing != null)
            {
                // Use raw SQL to avoid EF composite key tracking issues
                await _repository.Database.ExecuteSqlRawAsync(
                    @"UPDATE tblTmkAreaCountryDelete SET Area=@p0, Country=@p1, AreaNew=@p2, CountryNew=@p3, Systems=@p4
                      WHERE Area=@p5 AND Country=@p6 AND AreaNew=@p7 AND CountryNew=@p8 AND Systems=@p9",
                    entity.Area, entity.Country, entity.AreaNew, entity.CountryNew, entity.Systems,
                    existing.Area, existing.Country, existing.AreaNew, existing.CountryNew, existing.Systems ?? "");
            }
            else
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO tblTmkAreaCountryDelete (Area, Country, AreaNew, CountryNew, Systems)
                      VALUES (@p0, @p1, @p2, @p3, @p4)",
                    entity.Area, entity.Country, entity.AreaNew, entity.CountryNew, entity.Systems);
            }

            return Json(new { id = entity.Area, redirectUrl = Url.Action("Detail", new { areaCode = entity.Area, country = entity.Country, areaNewCode = entity.AreaNew, countryNew = entity.CountryNew, systems = entity.Systems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string areaCode = "", string country = "", string areaNewCode = "", string countryNew = "", string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkAreaCountryDelete WHERE Area=@p0 AND Country=@p1 AND AreaNew=@p2 AND CountryNew=@p3 AND Systems=@p4",
                new object[] {
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", areaCode ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", country ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p2", areaNewCode ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p3", countryNew ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p4", systems ?? "")
                });

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Copy(string areaCode = "", string country = "", string areaNewCode = "", string countryNew = "", string systems = "")
        {
            var entity = await _repository.TmkAreaCountryDeletes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Area == areaCode && c.Country == country && c.AreaNew == areaNewCode && c.CountryNew == countryNew && c.Systems == systems);
            if (entity == null) return new RecordDoesNotExistResult();

            return RedirectToAction("Add", new { copyArea = entity.Area, copyCountry = entity.Country, copyAreaNew = entity.AreaNew, copyCountryNew = entity.CountryNew, copySystems = entity.Systems });
        }

        public IActionResult GetRecordStamps(string areaCode = "", string country = "", string areaNewCode = "", string countryNew = "", string systems = "")
        {
            return ViewComponent("RecordStamps", new { createdBy = "", dateCreated = (DateTime?)null, updatedBy = "", lastUpdate = (DateTime?)null });
        }

        [HttpGet]
        public IActionResult DetailLink(string areaCode = "", string country = "", string areaNewCode = "", string countryNew = "", string systems = "")
        {
            if (!string.IsNullOrEmpty(areaCode))
                return RedirectToAction(nameof(Detail), new { areaCode = areaCode, country = country, areaNewCode = areaNewCode, countryNew = countryNew, systems = systems, singleRecord = true, fromSearch = true });
            return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        [HttpPost]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            return File(Convert.FromBase64String(base64), contentType, fileName);
        }

        public async Task<IActionResult> GetSystemList()
        {
            var systems = (await _repository.AppSystems.AsNoTracking()
                .Select(s => s.SystemName)
                .ToListAsync())
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ThenBy(s => s.Length).ToList();
            return Json(systems);
        }

        public async Task<IActionResult> GetAreaList()
        {
            var areas = await _repository.TmkAreas.AsNoTracking()
                .OrderBy(a => a.Area)
                .Select(a => new { a.Area, a.Description })
                .ToListAsync();
            return Json(areas);
        }

        public async Task<IActionResult> GetCountryList()
        {
            var countries = await _repository.TmkCountries.AsNoTracking()
                .OrderBy(c => c.Country)
                .Select(c => new { c.Country, c.CountryName })
                .ToListAsync();
            return Json(countries);
        }
    }
}
