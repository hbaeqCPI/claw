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
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Models;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Models.PageViewModels;
using R10.Web.Security;

namespace R10.Web.Areas.Patent.Controllers
{
    [Area("Patent"), Authorize(Policy = PatentAuthorizationPolicy.CanAccessAuxiliary)]
    public class AreaCountryController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "patAreaCountryDetail";

        public AreaCountryController(IAuthorizationService authService, IApplicationDbContext repository, IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _repository = repository;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel { Page = PageType.Search, PageId = "patAreaCountrySearch", Title = _localizer["Area Country Search"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return Request.IsAjax() ? PartialView("Index", model) : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel { Page = PageType.SearchResults, PageId = "patAreaCountrySearchResults", Title = _localizer["Area Country Search Results"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search() => RedirectToAction("Index");

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            var entities = _repository.PatAreasCountries.AsNoTracking().AsQueryable();

            if (mainSearchFilters != null && mainSearchFilters.Count > 0)
            {
                var systemName = mainSearchFilters.FirstOrDefault(f => f.Property == "SystemName");
                if (systemName != null)
                {
                    entities = entities.Where(a => a.Systems != null && EF.Functions.Like(a.Systems, "%" + systemName.Value.Replace("%", "") + "%"));
                    mainSearchFilters.Remove(systemName);
                }

                foreach (var filter in mainSearchFilters)
                {
                    if (filter.Property == "Area" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => a.Area == filter.Value);
                    else if (filter.Property == "Country" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => a.Country == filter.Value);
                }
            }

            var data = await entities.ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        private async Task<DetailPagePermission> GetPermission()
        {
            var p = new DetailPagePermission();
            p.CanEditRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded;
            p.CanDeleteRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryCanDelete)).Succeeded;
            p.CanAddRecord = p.CanEditRecord;
            p.CanCopyRecord = p.CanEditRecord;
            return p;
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string copyArea = "", string copyCountry = "", string copySystems = "")
        {
            if (!Request.IsAjax()) return RedirectToAction("Index");
            var data = new PatAreaCountry { IsNewRecord = true };

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
            return PartialView("Index", model);
        }

        public async Task<IActionResult> Detail(string areaCode, string country, string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var detail = await _repository.PatAreasCountries.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Area == areaCode && c.Country == country && c.Systems == systems);
            if (detail == null)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }
            var perm = await GetPermission();
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

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify), ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatAreaCountry entity)
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

            if (!isNewRecord)
            {
                // Update existing record
                var originalSystems = entity.OriginalSystems == "__EMPTY__" ? "" : entity.OriginalSystems;

                // Check for duplicate systems across records with the same (Area, Country)
                var allRecords = await _repository.PatAreasCountries.AsNoTracking()
                    .Where(c => c.Area == entity.Area && c.Country == entity.Country
                        && c.Systems != originalSystems
                        && c.Systems != null && c.Systems != "")
                    .Select(c => c.Systems)
                    .ToListAsync();

                var usedSystems = allRecords
                    .Where(s => !string.IsNullOrEmpty(s))
                    .SelectMany(s => s.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(s => s.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var duplicates = newSystems.Where(s => usedSystems.Contains(s)).ToList();
                if (duplicates.Any())
                    return new JsonBadRequest($"The following systems are already assigned to another Area Country record: {string.Join(", ", duplicates)}");

                await _repository.Database.ExecuteSqlRawAsync(
                    "UPDATE tblPatAreaCountry SET Area=@p0, Country=@p1, Systems=@p2 WHERE Area=@p3 AND Country=@p4 AND Systems=@p5",
                    new object[] {
                        new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.Area ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.Country ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.Systems),
                        new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.Area ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.Country ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p5", originalSystems ?? "")
                    });
            }
            else
            {
                // Insert new record
                // Check for duplicate systems across records with the same (Area, Country)
                var allRecords = await _repository.PatAreasCountries.AsNoTracking()
                    .Where(c => c.Area == entity.Area && c.Country == entity.Country
                        && c.Systems != null && c.Systems != "")
                    .Select(c => c.Systems)
                    .ToListAsync();

                var usedSystems = allRecords
                    .Where(s => !string.IsNullOrEmpty(s))
                    .SelectMany(s => s.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(s => s.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var duplicates = newSystems.Where(s => usedSystems.Contains(s)).ToList();
                if (duplicates.Any())
                    return new JsonBadRequest($"The following systems are already assigned to another Area Country record: {string.Join(", ", duplicates)}");

                await _repository.Database.ExecuteSqlRawAsync(
                    "INSERT INTO tblPatAreaCountry (Area, Country, Systems) VALUES (@p0, @p1, @p2)",
                    new object[] {
                        new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.Area ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.Country ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.Systems)
                    });
            }

            return Json(new { id = 0, redirectUrl = Url.Action("Detail", new { areaCode = entity.Area, country = entity.Country, systems = entity.Systems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string areaCode = "", string country = "", string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblPatAreaCountry WHERE Area=@p0 AND Country=@p1 AND Systems=@p2",
                new object[] {
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", areaCode ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", country ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p2", systems ?? "")
                });

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Copy(string areaCode = "", string country = "", string systems = "")
        {
            var entity = await _repository.PatAreasCountries.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Area == areaCode && c.Country == country && c.Systems == systems);
            if (entity == null) return new RecordDoesNotExistResult();

            return RedirectToAction("Add", new { copyArea = entity.Area, copyCountry = entity.Country, copySystems = entity.Systems });
        }

        public IActionResult GetRecordStamps(string areaCode = "", string country = "", string systems = "")
        {
            return ViewComponent("RecordStamps", new { createdBy = "", dateCreated = (DateTime?)null, updatedBy = "", lastUpdate = (DateTime?)null });
        }

        public async Task<IActionResult> GetSystemList()
        {
            var systems = await _repository.AppSystems.AsNoTracking()
                .OrderBy(s => s.SystemName)
                .Select(s => s.SystemName)
                .ToListAsync();
            return Json(systems);
        }

        public async Task<IActionResult> GetAreaList(string property = "Area", string text = "", FilterType filterType = FilterType.Contains)
        {
            var query = _repository.PatAreas.AsNoTracking().Select(a => new { Area = a.Area }).Distinct();
            if (!string.IsNullOrEmpty(text))
                query = query.Where(a => EF.Functions.Like(a.Area, $"%{text}%"));
            var areas = await query.OrderBy(a => a.Area).ToListAsync();
            return Json(areas);
        }

        public async Task<IActionResult> GetCountryList()
        {
            var countries = await _repository.PatCountries.AsNoTracking()
                .Select(c => new { c.Country, c.CountryName })
                .OrderBy(c => c.Country)
                .ToListAsync();
            return Json(countries);
        }

        [HttpGet]
        public IActionResult DetailLink(string areaCode = "", string country = "", string systems = "")
        {
            if (!string.IsNullOrEmpty(areaCode))
                return RedirectToAction(nameof(Detail), new { areaCode = areaCode, country = country, systems = systems, singleRecord = true, fromSearch = true });
            return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        [HttpPost]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            return File(Convert.FromBase64String(base64), contentType, fileName);
        }
    }
}
