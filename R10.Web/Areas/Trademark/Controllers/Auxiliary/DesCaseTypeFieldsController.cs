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
    public class DesCaseTypeFieldsController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "tmkDesCaseTypeFieldsDetail";

        public DesCaseTypeFieldsController(
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
                PageId = "tmkDesCaseTypeFieldsSearch",
                Title = _localizer["Des Case Type Fields Search"].ToString(),
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
                PageId = "tmkDesCaseTypeFieldsSearchResults",
                Title = _localizer["Des Case Type Fields Search Results"].ToString(),
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
            var entities = _repository.TmkDesCaseTypeFields.AsNoTracking().AsQueryable();
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
                        case "DesCaseType": entities = entities.Where(e => EF.Functions.Like(e.DesCaseType, val)); break;
                        case "FromField": entities = entities.Where(e => EF.Functions.Like(e.FromField, val)); break;
                        case "ToField": entities = entities.Where(e => EF.Functions.Like(e.ToField, val)); break;
                    }
                }
            }
            var data = await entities.ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        public async Task<IActionResult> Detail(string desCaseType, string fromField, string toField, string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var detail = await _repository.TmkDesCaseTypeFields.AsNoTracking()
                .FirstOrDefaultAsync(e => e.DesCaseType == desCaseType && e.FromField == fromField && e.ToField == toField && e.Systems == systems);

            if (detail == null)
            {
                if (Request.IsAjax())
                    return new RecordDoesNotExistResult();
                else
                    return RedirectToAction("Index");
            }

            var permission = await GetPermission();
            permission.DeleteScreenUrl = permission.CanDeleteRecord
                ? Url.Action("Delete", new { desCaseType = detail.DesCaseType, fromField = detail.FromField, toField = detail.ToField, systems = detail.Systems })
                : "";
            permission.CopyScreenUrl = permission.CanCopyRecord
                ? Url.Action("Add", new { fromSearch = true, copyDesCaseType = detail.DesCaseType, copyFromField = detail.FromField, copyToField = detail.ToField, copySystems = detail.Systems })
                : "";
            permission.IsCopyScreenPopup = false;

            PageViewModel model = new PageViewModel()
            {
                Page = PageType.Detail,
                PageId = _dataContainer,
                Title = _localizer["Des Case Type Fields Detail"].ToString(),
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
        public async Task<IActionResult> Add(bool fromSearch = false, string copyDesCaseType = "", string copyFromField = "", string copyToField = "", string copySystems = "")
        {
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var entity = new TmkDesCaseTypeFields { IsNewRecord = true };
            if (!string.IsNullOrEmpty(copyDesCaseType))
            {
                entity.DesCaseType = copyDesCaseType;
                entity.FromField = copyFromField;
                entity.ToField = copyToField;
                entity.Systems = copySystems ?? "";
            }

            var model = new PageViewModel()
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = _dataContainer,
                Title = _localizer["New Des Case Type Fields"].ToString(),
                Data = entity,
                PagePermission = await GetPermission(),
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();

            return PartialView("Index", model);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkDesCaseTypeFields entity)
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
            TmkDesCaseTypeFields existing = null;
            if (!isNewRecord)
            {
                existing = await _repository.TmkDesCaseTypeFields.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.DesCaseType == entity.DesCaseType && c.FromField == entity.FromField
                        && c.ToField == entity.ToField && c.Systems == originalSystemsValue);
            }

            // Check for duplicate systems across other records with the same key fields
            var allRecords = await _repository.TmkDesCaseTypeFields.AsNoTracking()
                .Where(c => c.DesCaseType == entity.DesCaseType && c.FromField == entity.FromField
                    && c.ToField == entity.ToField
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
                return new JsonBadRequest($"The following systems are already assigned to this Des Case Type Fields record: {string.Join(", ", duplicates)}");

            if (existing != null)
            {
                // Use raw SQL to avoid EF composite key tracking issues
                await _repository.Database.ExecuteSqlRawAsync(
                    @"UPDATE tblTmkDesCaseTypeFields SET DesCaseType=@p0, FromField=@p1, ToField=@p2, Systems=@p3
                      WHERE DesCaseType=@p4 AND FromField=@p5 AND ToField=@p6 AND Systems=@p7",
                    entity.DesCaseType ?? "", entity.FromField ?? "", entity.ToField ?? "", entity.Systems,
                    existing.DesCaseType ?? "", existing.FromField ?? "", existing.ToField ?? "", existing.Systems ?? "");
            }
            else
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO tblTmkDesCaseTypeFields (DesCaseType, FromField, ToField, Systems)
                      VALUES (@p0, @p1, @p2, @p3)",
                    entity.DesCaseType ?? "", entity.FromField ?? "", entity.ToField ?? "", entity.Systems);
            }

            return Json(new { redirectUrl = Url.Action("Detail", new { desCaseType = entity.DesCaseType, fromField = entity.FromField, toField = entity.ToField, systems = entity.Systems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string desCaseType, string fromField, string toField, string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkDesCaseTypeFields WHERE DesCaseType=@p0 AND FromField=@p1 AND ToField=@p2 AND Systems=@p3",
                desCaseType ?? "", fromField ?? "", toField ?? "", systems ?? "");

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        [HttpGet()]
        public async Task<IActionResult> Copy(string desCaseType, string fromField, string toField, string systems = "")
        {
            var entity = await _repository.TmkDesCaseTypeFields.AsNoTracking()
                .FirstOrDefaultAsync(c => c.DesCaseType == desCaseType && c.FromField == fromField && c.ToField == toField && c.Systems == systems);
            if (entity == null) return new RecordDoesNotExistResult();

            return RedirectToAction("Add", new { fromSearch = true, copyDesCaseType = entity.DesCaseType, copyFromField = entity.FromField, copyToField = entity.ToField, copySystems = entity.Systems });
        }

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
            return await GetPicklistData(_repository.TmkDesCaseTypeFields.AsQueryable(), request, property, text, filterType, requiredRelation);
        }
    }
}
