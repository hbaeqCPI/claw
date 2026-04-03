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
    public class CountryLawExtController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "patCountryLawExtDetail";

        public CountryLawExtController(IAuthorizationService authService, IApplicationDbContext repository, IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _repository = repository;
            _localizer = localizer;
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

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel { Page = PageType.Search, PageId = "patCountryLawExtSearch", Title = _localizer["Country Law Ext Search"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return Request.IsAjax() ? PartialView("Index", model) : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel { Page = PageType.SearchResults, PageId = "patCountryLawExtSearchResults", Title = _localizer["Country Law Ext Search Results"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search() => RedirectToAction("Index");

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            var entities = _repository.PatCountryLawExts.AsNoTracking().AsQueryable();

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
                    if (filter.Property == "Country" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => EF.Functions.Like(a.Country, filter.Value));
                    else if (filter.Property == "CaseType" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => EF.Functions.Like(a.CaseType, filter.Value));
                    else if (filter.Property == "LabelTaxSched" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => EF.Functions.Like(a.LabelTaxSched, filter.Value));
                }
            }

            var data = await entities.ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string copyCountry = "", string copyCaseType = "", string copySystems = "", string copyLabelTaxSched = "")
        {
            var data = new PatCountryLawExt { IsNewRecord = true };

            if (!string.IsNullOrEmpty(copyCountry))
            {
                data.Country = copyCountry;
                data.CaseType = copyCaseType;
                data.Systems = copySystems ?? "";
                data.LabelTaxSched = copyLabelTaxSched ?? "";
            }

            var model = new PageViewModel
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = _dataContainer,
                Title = _localizer["New Country Law Ext"].ToString(),
                Data = data,
                PagePermission = await GetPermission(),
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        public async Task<IActionResult> Detail(string country, string caseType, string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var detail = await _repository.PatCountryLawExts.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Country == country && c.CaseType == caseType && c.Systems == systems);
            if (detail == null)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }
            var perm = await GetPermission();
            perm.DeleteScreenUrl = perm.CanDeleteRecord ? Url.Action("Delete", new { country = country, caseType = caseType, systems = systems }) : "";
            perm.CopyScreenUrl = perm.CanCopyRecord ? Url.Action("Add", new { fromSearch = true, copyCountry = country, copyCaseType = caseType, copySystems = systems, copyLabelTaxSched = detail.LabelTaxSched }) : "";
            perm.IsCopyScreenPopup = false;
            var model = new PageViewModel
            {
                Page = PageType.Detail,
                PageId = _dataContainer,
                Title = _localizer["Country Law Ext Detail"].ToString(),
                RecordId = 1,
                SingleRecord = singleRecord || !Request.IsAjax(),
                Data = detail,
                PagePermission = perm
            };
            if (Request.IsAjax() && !singleRecord && !fromSearch) model.Page = PageType.DetailContent;
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify), ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatCountryLawExt entity)
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

                // Check for duplicate systems across records with the same (Country, CaseType)
                var allRecords = await _repository.PatCountryLawExts.AsNoTracking()
                    .Where(c => c.Country == entity.Country && c.CaseType == entity.CaseType
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
                    return new JsonBadRequest($"The following systems are already assigned to another Country Law Ext record: {string.Join(", ", duplicates)}");

                await _repository.Database.ExecuteSqlRawAsync(
                    "UPDATE tblPatCountryLaw_Ext SET Country=@p0, CaseType=@p1, LabelTaxSched=@p2, Systems=@p3 WHERE Country=@p4 AND CaseType=@p5 AND Systems=@p6",
                    new object[] {
                        new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.Country ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.CaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.LabelTaxSched ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.Systems),
                        new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.Country ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.CaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p6", originalSystems ?? "")
                    });
            }
            else
            {
                // Insert new record
                // Check for duplicate systems across records with the same (Country, CaseType)
                var allRecords = await _repository.PatCountryLawExts.AsNoTracking()
                    .Where(c => c.Country == entity.Country && c.CaseType == entity.CaseType
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
                    return new JsonBadRequest($"The following systems are already assigned to another Country Law Ext record: {string.Join(", ", duplicates)}");

                await _repository.Database.ExecuteSqlRawAsync(
                    "INSERT INTO tblPatCountryLaw_Ext (Country, CaseType, LabelTaxSched, Systems) VALUES (@p0, @p1, @p2, @p3)",
                    new object[] {
                        new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.Country ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.CaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.LabelTaxSched ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.Systems)
                    });
            }

            return Json(new { id = 0, redirectUrl = Url.Action("Detail", new { country = entity.Country, caseType = entity.CaseType, systems = entity.Systems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string country = "", string caseType = "", string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblPatCountryLaw_Ext WHERE Country=@p0 AND CaseType=@p1 AND Systems=@p2",
                new object[] {
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", country ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", caseType ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p2", systems ?? "")
                });

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Copy(string country = "", string caseType = "", string systems = "")
        {
            var entity = await _repository.PatCountryLawExts.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Country == country && c.CaseType == caseType && c.Systems == systems);
            if (entity == null) return new RecordDoesNotExistResult();

            return RedirectToAction("Add", new { copyCountry = entity.Country, copyCaseType = entity.CaseType, copySystems = entity.Systems, copyLabelTaxSched = entity.LabelTaxSched });
        }

        public IActionResult GetRecordStamps(string country = "", string caseType = "", string systems = "")
        {
            return ViewComponent("RecordStamps", new { createdBy = "", dateCreated = (DateTime?)null, updatedBy = "", lastUpdate = (DateTime?)null });
        }

        public async Task<IActionResult> GetSystemList()
        {
            var systems = (await _repository.AppSystems.AsNoTracking()
                .Select(s => s.SystemName)
                .ToListAsync())
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ThenBy(s => s.Length).ToList();
            return Json(systems);
        }

        public async Task<IActionResult> GetCountryList()
        {
            var countries = await _repository.PatCountries.AsNoTracking()
                .Select(c => new { c.Country, c.CountryName })
                .OrderBy(c => c.Country)
                .ToListAsync();
            return Json(countries);
        }

        public async Task<IActionResult> GetCaseTypeList(string property = "CaseType", string text = "", FilterType filterType = FilterType.Contains)
        {
            var query = _repository.PatCaseTypes.AsNoTracking().Select(c => new { CaseType = c.CaseType, Description = c.Description }).Distinct();
            if (!string.IsNullOrEmpty(text))
                query = query.Where(c => EF.Functions.Like(c.CaseType, $"%{text}%") || EF.Functions.Like(c.Description, $"%{text}%"));
            var caseTypes = await query.OrderBy(c => c.CaseType).ToListAsync();
            return Json(caseTypes);
        }

        [HttpGet]
        public IActionResult DetailLink(string country = "", string caseType = "", string systems = "")
        {
            if (!string.IsNullOrEmpty(country))
                return RedirectToAction(nameof(Detail), new { country = country, caseType = caseType, systems = systems, singleRecord = true, fromSearch = true });
            return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        [HttpPost]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            return File(Convert.FromBase64String(base64), contentType, fileName);
        }
    }
}
