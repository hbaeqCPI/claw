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
using R10.Core.Helpers;
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
    public class DesCaseTypeFieldsExtController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "tmkDesCaseTypeFieldsExtDetail";

        public DesCaseTypeFieldsExtController(IAuthorizationService authService, IApplicationDbContext repository, IStringLocalizer<SharedResource> localizer)
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
            var model = new PageViewModel { Page = PageType.Search, PageId = "tmkDesCaseTypeFieldsExtSearch", Title = _localizer["Des Case Type Fields Ext Search"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return Request.IsAjax() ? PartialView("Index", model) : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel { Page = PageType.SearchResults, PageId = "tmkDesCaseTypeFieldsExtSearchResults", Title = _localizer["Des Case Type Fields Ext Search Results"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search() => RedirectToAction("Index");

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            var entities = _repository.TmkDesCaseTypeFieldsExts.AsNoTracking().AsQueryable();

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

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string copyDesCaseType = "", string copyFromField = "", string copyToField = "", string copySystems = "", bool copyInUse = false)
        {
            if (!Request.IsAjax()) return RedirectToAction("Index");
            var entity = new TmkDesCaseTypeFieldsExt { IsNewRecord = true };

            if (!string.IsNullOrEmpty(copyDesCaseType))
            {
                entity.DesCaseType = copyDesCaseType;
                entity.FromField = copyFromField;
                entity.ToField = copyToField;
                entity.InUse = copyInUse;
                entity.Systems = copySystems ?? "";
            }

            var model = new PageViewModel
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = _dataContainer,
                Title = _localizer["New Des Case Type Fields Ext"].ToString(),
                Data = entity,
                PagePermission = await GetPermission(),
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();
            return PartialView("Index", model);
        }

        public async Task<IActionResult> Detail(string desCaseType, string fromField, string toField, string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var detail = await _repository.TmkDesCaseTypeFieldsExts.AsNoTracking()
                .FirstOrDefaultAsync(e => e.DesCaseType == desCaseType && e.FromField == fromField && e.ToField == toField && e.Systems == systems);

            if (detail == null)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }

            var perm = await GetPermission();
            perm.DeleteScreenUrl = perm.CanDeleteRecord
                ? Url.Action("Delete", new { desCaseType = detail.DesCaseType, fromField = detail.FromField, toField = detail.ToField, systems = detail.Systems })
                : "";
            perm.CopyScreenUrl = perm.CanCopyRecord
                ? Url.Action("Add", new { fromSearch = true, copyDesCaseType = detail.DesCaseType, copyFromField = detail.FromField, copyToField = detail.ToField, copySystems = detail.Systems, copyInUse = detail.InUse })
                : "";
            perm.IsCopyScreenPopup = false;

            var model = new PageViewModel
            {
                Page = PageType.Detail,
                PageId = _dataContainer,
                Title = _localizer["Des Case Type Fields Ext Detail"].ToString(),
                RecordId = 1,
                SingleRecord = singleRecord || !Request.IsAjax(),
                Data = detail,
                PagePermission = perm
            };
            if (Request.IsAjax() && !singleRecord && !fromSearch) model.Page = PageType.DetailContent;
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify), ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkDesCaseTypeFieldsExt entity)
        {
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });

            entity.Systems ??= "";

            var userName = User.GetUserName();
            var now = DateTime.Now;
            entity.UpdatedBy = userName;
            entity.LastUpdate = now;
            if (entity.CreatedBy == null) entity.CreatedBy = userName;
            if (entity.DateCreated == default) entity.DateCreated = now;

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
            TmkDesCaseTypeFieldsExt existing = null;
            if (!isNewRecord)
            {
                existing = await _repository.TmkDesCaseTypeFieldsExts.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.DesCaseType == entity.DesCaseType && c.FromField == entity.FromField
                        && c.ToField == entity.ToField && c.Systems == originalSystemsValue);
            }

            // Check for duplicate systems across other records with the same key fields
            var allRecords = await _repository.TmkDesCaseTypeFieldsExts.AsNoTracking()
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
                return new JsonBadRequest($"The following systems are already assigned to this Des Case Type Fields Ext record: {string.Join(", ", duplicates)}");

            if (existing != null)
            {
                // Use raw SQL to avoid EF composite key tracking issues
                await _repository.Database.ExecuteSqlRawAsync(
                    @"UPDATE tblTmkDesCaseTypeFields_Ext SET DesCaseType=@p0, FromField=@p1, ToField=@p2, InUse=@p3, Systems=@p4, CreatedBy=@p5, UpdatedBy=@p6, DateCreated=@p7, LastUpdate=@p8
                      WHERE DesCaseType=@p9 AND FromField=@p10 AND ToField=@p11 AND Systems=@p12",
                    entity.DesCaseType ?? "", entity.FromField ?? "", entity.ToField ?? "", entity.InUse, entity.Systems,
                    entity.CreatedBy ?? "", entity.UpdatedBy ?? "", entity.DateCreated, entity.LastUpdate,
                    existing.DesCaseType ?? "", existing.FromField ?? "", existing.ToField ?? "", existing.Systems ?? "");
            }
            else
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO tblTmkDesCaseTypeFields_Ext (DesCaseType, FromField, ToField, InUse, Systems, CreatedBy, UpdatedBy, DateCreated, LastUpdate)
                      VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8)",
                    entity.DesCaseType ?? "", entity.FromField ?? "", entity.ToField ?? "", entity.InUse, entity.Systems,
                    entity.CreatedBy ?? "", entity.UpdatedBy ?? "", entity.DateCreated, entity.LastUpdate);
            }

            return Json(new { redirectUrl = Url.Action("Detail", new { desCaseType = entity.DesCaseType, fromField = entity.FromField, toField = entity.ToField, systems = entity.Systems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string desCaseType = "", string fromField = "", string toField = "", string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkDesCaseTypeFields_Ext WHERE DesCaseType=@p0 AND FromField=@p1 AND ToField=@p2 AND Systems=@p3",
                desCaseType ?? "", fromField ?? "", toField ?? "", systems ?? "");

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Copy(string desCaseType = "", string fromField = "", string toField = "", string systems = "")
        {
            var entity = await _repository.TmkDesCaseTypeFieldsExts.AsNoTracking()
                .FirstOrDefaultAsync(c => c.DesCaseType == desCaseType && c.FromField == fromField && c.ToField == toField && c.Systems == systems);
            if (entity == null) return new RecordDoesNotExistResult();

            return RedirectToAction("Add", new { fromSearch = true, copyDesCaseType = entity.DesCaseType, copyFromField = entity.FromField, copyToField = entity.ToField, copySystems = entity.Systems, copyInUse = entity.InUse });
        }

        public async Task<IActionResult> GetSystemList()
        {
            var systems = await _repository.AppSystems.AsNoTracking()
                .OrderBy(s => s.SystemName)
                .Select(s => s.SystemName)
                .ToListAsync();
            return Json(systems);
        }

        public async Task<IActionResult> GetRecordStamps(string desCaseType = "", string fromField = "", string toField = "", string systems = "")
        {
            return ViewComponent("RecordStamps", new { createdBy = "", dateCreated = (DateTime?)null, updatedBy = "", lastUpdate = (DateTime?)null });
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_repository.TmkDesCaseTypeFieldsExts.AsQueryable(), request, property, text, filterType, requiredRelation);
        }

        [HttpGet]
        public IActionResult DetailLink(string desCaseType = "", string fromField = "", string toField = "", string systems = "")
        {
            if (!string.IsNullOrEmpty(desCaseType))
                return RedirectToAction(nameof(Detail), new { desCaseType = desCaseType, fromField = fromField, toField = toField, systems = systems, singleRecord = true, fromSearch = true });
            return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        [HttpPost]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            return File(Convert.FromBase64String(base64), contentType, fileName);
        }
    }
}
