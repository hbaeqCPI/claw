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
    public class AreaCountryController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "tmkAreaCountryDetail";

        public AreaCountryController(IAuthorizationService authService, IApplicationDbContext repository, IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _repository = repository;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel { Page = PageType.Search, PageId = "tmkAreaCountrySearch", Title = _localizer["Area Country Search"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return Request.IsAjax() ? PartialView("Index", model) : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel { Page = PageType.SearchResults, PageId = "tmkAreaCountrySearchResults", Title = _localizer["Area Country Search Results"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search() => RedirectToAction("Index");

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            var data = _repository.TmkAreasCountries.AsNoTracking().AsQueryable();

            if (mainSearchFilters != null && mainSearchFilters.Count > 0)
            {
                {
                    data = Helpers.QueryHelper.ApplySystemsFilter(data, mainSearchFilters, a => a.Systems);
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
        public async Task<IActionResult> Add(bool fromSearch = false, string copyArea = "", string copyCountry = "", string copySystems = "")
        {
            var data = new TmkAreaCountry { IsNewRecord = true };

            if (!string.IsNullOrEmpty(copyArea))
            {
                data.Area = copyArea;
                data.Country = copyCountry;
                data.Systems = copySystems ?? "";
            }

            var model = new PageViewModel
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = _dataContainer,
                Title = _localizer["New Area Country"].ToString(),
                Data = data,
                PagePermission = await GetPermission(),
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        public async Task<IActionResult> Detail(string areaCode, string country, string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var detail = await _repository.TmkAreasCountries.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Area == areaCode && c.Country == country && c.Systems == systems);
            if (detail == null)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }
            var perm = await GetPermission();
            perm.AddScreenUrl = perm.CanAddRecord ? Url.Action("Add", new { fromSearch = true }) : "";
            perm.DeleteScreenUrl = perm.CanDeleteRecord ? Url.Action("Delete", new { areaCode = areaCode, country = country, systems = systems }) : "";
            perm.CopyScreenUrl = perm.CanCopyRecord ? Url.Action("Add", new { fromSearch = true, copyArea = areaCode, copyCountry = country, copySystems = systems }) : "";
            perm.IsCopyScreenPopup = false;
            var model = new PageViewModel
            {
                Page = PageType.Detail,
                PageId = _dataContainer,
                Title = _localizer["Area Country Detail"].ToString(),
                RecordId = 1,
                SingleRecord = singleRecord || !Request.IsAjax(),
                Data = detail,
                PagePermission = perm
            };
            if (Request.IsAjax() && !singleRecord && !fromSearch) model.Page = PageType.DetailContent;
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify), ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkAreaCountry entity)
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
            TmkAreaCountry existing = null;
            if (!isNewRecord)
            {
                existing = await _repository.TmkAreasCountries.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Area == entity.Area && c.Country == entity.Country && c.Systems == originalSystemsValue);
            }

            // Check for duplicate systems across other records with the same (Area, Country)
            var allRecords = await _repository.TmkAreasCountries.AsNoTracking()
                .Where(c => c.Area == entity.Area && c.Country == entity.Country && c.Systems != null && c.Systems != "")
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
                return new JsonBadRequest($"The following systems are already assigned to {entity.Area}/{entity.Country}: {string.Join(", ", duplicates)}");

            if (existing != null)
            {
                // Use raw SQL to avoid EF composite key tracking issues
                await _repository.Database.ExecuteSqlRawAsync(
                    @"UPDATE tblTmkAreaCountry SET Area=@p0, Country=@p1, Systems=@p2
                      WHERE Area=@p3 AND Country=@p4 AND Systems=@p5",
                    entity.Area, entity.Country, entity.Systems,
                    existing.Area, existing.Country, existing.Systems ?? "");
            }
            else
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO tblTmkAreaCountry (Area, Country, Systems)
                      VALUES (@p0, @p1, @p2)",
                    entity.Area, entity.Country, entity.Systems);
            }

            return Json(new { id = entity.Area, redirectUrl = Url.Action("Detail", new { areaCode = entity.Area, country = entity.Country, systems = entity.Systems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string areaCode = "", string country = "", string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkAreaCountry WHERE Area=@p0 AND Country=@p1 AND Systems=@p2",
                new object[] {
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", areaCode ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", country ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p2", systems ?? "")
                });

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Copy(string areaCode = "", string country = "", string systems = "")
        {
            var entity = await _repository.TmkAreasCountries.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Area == areaCode && c.Country == country && c.Systems == systems);
            if (entity == null) return new RecordDoesNotExistResult();

            return RedirectToAction("Add", new { copyArea = entity.Area, copyCountry = entity.Country, copySystems = entity.Systems });
        }

        public IActionResult GetRecordStamps(string areaCode = "", string country = "", string systems = "")
        {
            return ViewComponent("RecordStamps", new { createdBy = "", dateCreated = (DateTime?)null, updatedBy = "", lastUpdate = (DateTime?)null });
        }

        [HttpGet]
        public IActionResult DetailLink(string areaCode = "", string country = "", string systems = "")
        {
            if (!string.IsNullOrEmpty(areaCode))
                return RedirectToAction(nameof(Detail), new { areaCode = areaCode, country = country, systems = systems, singleRecord = true, fromSearch = true });
            else
                return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        [HttpPost]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            return File(Convert.FromBase64String(base64), contentType, fileName);
        }

        public IActionResult GetSystemList()
        {
            return Json(Helpers.SystemsHelper.SystemNames);
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text = "", FilterType filterType = FilterType.StartsWith, string requiredRelation = "")
        {
            return await GetPicklistData(_repository.TmkAreasCountries.AsNoTracking(), request, property, text, filterType, requiredRelation);
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

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> MoveToDelete(string areaCode = "", string countryCode = "", string systems = "")
        {
            var newIndividualSystems = (systems ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (!newIndividualSystems.Any()) newIndividualSystems.Add("");
            var existingDeleteSystems = await _repository.TmkAreaCountryDeletes.AsNoTracking()
                .Where(d => d.Area == areaCode && d.Country == countryCode)
                .Select(d => d.Systems).Distinct().ToListAsync();
            var existingIndividual = existingDeleteSystems
                .SelectMany(s => (s ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var overlap = newIndividualSystems.Where(s => existingIndividual.Contains(s)).ToList();
            if (overlap.Any())
                return BadRequest($"The following systems already exist in the Delete table: {string.Join(", ", overlap)}");

            await _repository.Database.ExecuteSqlRawAsync(
                "INSERT INTO tblTmkAreaCountryDelete (Area, Country, AreaNew, CountryNew, Systems) VALUES (@p0, @p1, @p2, @p3, @p4)",
                areaCode ?? "", countryCode ?? "", areaCode ?? "", countryCode ?? "", systems ?? "");

            await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkAreaCountry WHERE Area=@p0 AND Country=@p1 AND Systems=@p2",
                areaCode ?? "", countryCode ?? "", systems ?? "");

            return Ok(new { success = "Record moved to delete table successfully." });
        }
    }
}
