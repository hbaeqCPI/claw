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

using Newtonsoft.Json;
using R10.Web.Areas;

namespace R10.Web.Areas.Trademark.Controllers
{
    [Area("Trademark"), Authorize(Policy = TrademarkAuthorizationPolicy.CanAccessAuxiliary)]
    public class DesCaseTypeController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "tmkDesCaseTypeDetail";

        public DesCaseTypeController(
            IAuthorizationService authService,
            IApplicationDbContext repository,
            IStringLocalizer<SharedResource> localizer)
        {
            _authService = authService;
            _repository = repository;
            _localizer = localizer;
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

        public async Task<IActionResult> Index()
        {
            var model = new PageViewModel()
            {
                Page = PageType.Search,
                PageId = "tmkDesCaseTypeSearch",
                Title = _localizer["Designation Case Type Search"].ToString(),
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
                PageId = "tmkDesCaseTypeSearchResults",
                Title = _localizer["Designation Case Type Search Results"].ToString(),
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
            var entities = _repository.TmkDesCaseTypes.AsNoTracking().AsQueryable();
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
                    if (string.IsNullOrEmpty(filter.Value)) continue;
                    var val = filter.Value;
                    switch (filter.Property)
                    {
                        case "IntlCode": entities = entities.Where(e => EF.Functions.Like(e.IntlCode, val)); break;
                        case "CaseType": entities = entities.Where(e => EF.Functions.Like(e.CaseType, val)); break;
                        case "DesCountry": entities = entities.Where(e => EF.Functions.Like(e.DesCountry, val)); break;
                        case "DesCaseType": entities = entities.Where(e => EF.Functions.Like(e.DesCaseType, val)); break;
                    }
                }
            }
            var data = await entities.ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        public async Task<IActionResult> Detail(string intlCode, string caseType, string desCountry, string desCaseType, string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var detail = await _repository.TmkDesCaseTypes.AsNoTracking()
                .FirstOrDefaultAsync(e => e.IntlCode == intlCode && e.CaseType == caseType && e.DesCountry == desCountry && e.DesCaseType == desCaseType && e.Systems == systems);

            if (detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction("Index");
            }

            var permission = await GetPermission();
            permission.DeleteScreenUrl = permission.CanDeleteRecord
                ? Url.Action("Delete", new { intlCode = detail.IntlCode, caseType = detail.CaseType, desCountry = detail.DesCountry, desCaseType = detail.DesCaseType, systems = detail.Systems })
                : "";
            permission.CopyScreenUrl = permission.CanCopyRecord
                ? Url.Action("Add", new { fromSearch = true, copyIntlCode = detail.IntlCode, copyCaseType = detail.CaseType, copyDesCountry = detail.DesCountry, copyDesCaseType = detail.DesCaseType, copySystems = detail.Systems, copyDefault = detail.Default })
                : "";
            permission.IsCopyScreenPopup = false;

            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = _dataContainer,
                Title = _localizer["Designation Case Type Detail"].ToString(),
                RecordId = 1,
                SingleRecord = singleRecord || !Request.IsAjax(),
                Data = detail,
                PagePermission = permission
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

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string copyIntlCode = "", string copyCaseType = "", string copyDesCountry = "", string copyDesCaseType = "", string copySystems = "", bool copyDefault = false)
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var entity = new TmkDesCaseType { IsNewRecord = true };
            if (!string.IsNullOrEmpty(copyIntlCode))
            {
                entity.IntlCode = copyIntlCode;
                entity.CaseType = copyCaseType;
                entity.DesCountry = copyDesCountry;
                entity.DesCaseType = copyDesCaseType;
                entity.Systems = copySystems ?? "";
                entity.Default = copyDefault;
            }

            var model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = _dataContainer,
                Title = _localizer["New Designation Case Type"].ToString(),
                Data = entity,
                PagePermission = await GetPermission(),
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();

            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkDesCaseType entity)
        {
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

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
            TmkDesCaseType existing = null;
            if (!isNewRecord)
            {
                existing = await _repository.TmkDesCaseTypes.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.IntlCode == entity.IntlCode && c.CaseType == entity.CaseType
                        && c.DesCountry == entity.DesCountry && c.DesCaseType == entity.DesCaseType && c.Systems == originalSystemsValue);
            }

            // Check for duplicate systems across other records with the same key fields
            var allRecords = await _repository.TmkDesCaseTypes.AsNoTracking()
                .Where(c => c.IntlCode == entity.IntlCode && c.CaseType == entity.CaseType
                    && c.DesCountry == entity.DesCountry && c.DesCaseType == entity.DesCaseType
                    && c.Systems != null && c.Systems != "")
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
                return new JsonBadRequest($"The following systems are already assigned to this Designation Case Type record: {string.Join(", ", duplicates)}");

            if (existing != null)
            {
                // Use raw SQL to avoid EF composite key tracking issues
                await _repository.Database.ExecuteSqlRawAsync(
                    @"UPDATE tblTmkDesCaseType SET IntlCode=@p0, CaseType=@p1, DesCountry=@p2, DesCaseType=@p3, [Default]=@p4, Systems=@p5
                      WHERE IntlCode=@p6 AND CaseType=@p7 AND DesCountry=@p8 AND DesCaseType=@p9 AND Systems=@p10",
                    entity.IntlCode ?? "", entity.CaseType ?? "", entity.DesCountry ?? "", entity.DesCaseType ?? "", entity.Default, entity.Systems,
                    existing.IntlCode ?? "", existing.CaseType ?? "", existing.DesCountry ?? "", existing.DesCaseType ?? "", existing.Systems ?? "");
            }
            else
            {
                // Check for duplicate systems across records with the same key fields
                var insertRecords = await _repository.TmkDesCaseTypes.AsNoTracking()
                    .Where(c => c.IntlCode == entity.IntlCode && c.CaseType == entity.CaseType && c.DesCountry == entity.DesCountry && c.DesCaseType == entity.DesCaseType
                        && c.Systems != null && c.Systems != "")
                    .Select(c => c.Systems)
                    .ToListAsync();

                var insertUsedSystems = insertRecords
                    .SelectMany(s => (s ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(s => s.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var insertDuplicates = newSystems.Where(s => insertUsedSystems.Contains(s)).ToList();
                if (insertDuplicates.Any())
                    return new JsonBadRequest($"The following systems are already assigned: {string.Join(", ", insertDuplicates)}");

                await _repository.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO tblTmkDesCaseType (IntlCode, CaseType, DesCountry, DesCaseType, [Default], Systems)
                      VALUES (@p0, @p1, @p2, @p3, @p4, @p5)",
                    entity.IntlCode ?? "", entity.CaseType ?? "", entity.DesCountry ?? "", entity.DesCaseType ?? "", entity.Default, entity.Systems);
            }

            return Json(new { redirectUrl = Url.Action("Detail", new { intlCode = entity.IntlCode, caseType = entity.CaseType, desCountry = entity.DesCountry, desCaseType = entity.DesCaseType, systems = entity.Systems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string intlCode, string caseType, string desCountry, string desCaseType, string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkDesCaseType WHERE IntlCode=@p0 AND CaseType=@p1 AND DesCountry=@p2 AND DesCaseType=@p3 AND Systems=@p4",
                intlCode ?? "", caseType ?? "", desCountry ?? "", desCaseType ?? "", systems ?? "");

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        [HttpGet()]
        public async Task<IActionResult> Copy(string intlCode, string caseType, string desCountry, string desCaseType, string systems = "")
        {
            var entity = await _repository.TmkDesCaseTypes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IntlCode == intlCode && c.CaseType == caseType && c.DesCountry == desCountry && c.DesCaseType == desCaseType && c.Systems == systems);
            if (entity == null) return new RecordDoesNotExistResult();

            return RedirectToAction("Add", new { fromSearch = true, copyIntlCode = entity.IntlCode, copyCaseType = entity.CaseType, copyDesCountry = entity.DesCountry, copyDesCaseType = entity.DesCaseType, copySystems = entity.Systems, copyDefault = entity.Default });
        }

        public async Task<IActionResult> GetSystemList()
        {
            var systems = (await _repository.AppSystems.AsNoTracking()
                .Select(s => s.SystemName)
                .ToListAsync())
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ThenBy(s => s.Length).ToList();
            return Json(systems);
        }

        public async Task<IActionResult> GetRecordStamps(string intlCode, string caseType, string desCountry, string desCaseType, string systems = "")
        {
            // This entity does not have audit fields, return empty
            return ViewComponent("RecordStamps", new { createdBy = "", dateCreated = (DateTime?)null, updatedBy = "", lastUpdate = (DateTime?)null });
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_repository.TmkDesCaseTypes.AsQueryable(), request, property, text, filterType, requiredRelation);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> MoveToDelete(string intlCode = "", string caseType = "", string desCountry = "", string desCaseType = "", bool defaultVal = false, string systems = "")
        {
            var existingDelete = await _repository.TmkDesCaseTypeDeletes.AsNoTracking()
                .FirstOrDefaultAsync(d => d.IntlCode == intlCode && d.CaseType == caseType && d.DesCountry == desCountry && d.DesCaseType == desCaseType);

            var newIndividualSystems = (systems ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (existingDelete != null)
            {
                // Merge systems into existing delete record
                var existingSystems = (existingDelete.Systems ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var overlap = newIndividualSystems.Where(s => existingSystems.Contains(s)).ToList();
                if (overlap.Any())
                    return BadRequest($"The following systems already exist in the Delete table: {string.Join(", ", overlap)}");

                foreach (var s in newIndividualSystems) existingSystems.Add(s);
                var mergedSystems = string.Join(",", existingSystems.OrderBy(s => s, StringComparer.OrdinalIgnoreCase));

                await _repository.Database.ExecuteSqlRawAsync(
                    "UPDATE tblTmkDesCaseTypeDelete SET Systems=@p0 WHERE IntlCode=@p1 AND CaseType=@p2 AND DesCountry=@p3 AND DesCaseType=@p4 AND Systems=@p5",
                    mergedSystems, existingDelete.IntlCode ?? "", existingDelete.CaseType ?? "", existingDelete.DesCountry ?? "", existingDelete.DesCaseType ?? "", existingDelete.Systems ?? "");
            }
            else
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    "INSERT INTO tblTmkDesCaseTypeDelete (IntlCode, CaseType, DesCountry, DesCaseType, [Default], IntlCodeNew, CaseTypeNew, DesCountryNew, DesCaseTypeNew, Systems) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9)",
                    intlCode ?? "", caseType ?? "", desCountry ?? "", desCaseType ?? "", defaultVal, intlCode ?? "", caseType ?? "", desCountry ?? "", desCaseType ?? "", systems ?? "");
            }

            // Delete from source table
            await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkDesCaseType WHERE IntlCode=@p0 AND CaseType=@p1 AND DesCountry=@p2 AND DesCaseType=@p3 AND Systems=@p4",
                intlCode ?? "", caseType ?? "", desCountry ?? "", desCaseType ?? "", systems ?? "");

            return Ok(new { success = "Record moved to delete table successfully." });
        }
    }
}
