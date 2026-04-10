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
    public class DesCaseTypeExtController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "tmkDesCaseTypeExtDetail";

        public DesCaseTypeExtController(IAuthorizationService authService, IApplicationDbContext repository, IStringLocalizer<SharedResource> localizer)
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
            var model = new PageViewModel { Page = PageType.Search, PageId = "tmkDesCaseTypeExtSearch", Title = _localizer["Des Case Type Ext Search"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return Request.IsAjax() ? PartialView("Index", model) : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel { Page = PageType.SearchResults, PageId = "tmkDesCaseTypeExtSearchResults", Title = _localizer["Des Case Type Ext Search Results"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search() => RedirectToAction("Index");

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            var entities = _repository.TmkDesCaseTypeExts.AsNoTracking().AsQueryable();

            if (mainSearchFilters != null && mainSearchFilters.Count > 0)
            {
                {
                    entities = Helpers.QueryHelper.ApplySystemsFilter(entities, mainSearchFilters, a => a.Systems);
                }
            }
            entities = entities.BuildCriteria(mainSearchFilters);
            var data = await entities.ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string copyIntlCode = "", string copyCaseType = "", string copyDesCountry = "", string copyDesCaseType = "", string copySystems = "", bool copyDefault = false, bool copyGenApp = false)
        {
            var entity = new TmkDesCaseTypeExt { IsNewRecord = true };

            if (!string.IsNullOrEmpty(copyIntlCode))
            {
                entity.IntlCode = copyIntlCode;
                entity.CaseType = copyCaseType;
                entity.DesCountry = copyDesCountry;
                entity.DesCaseType = copyDesCaseType;
                entity.Systems = copySystems ?? "";
                entity.Default = copyDefault;
                entity.GenApp = copyGenApp;
            }

            var model = new PageViewModel
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = _dataContainer,
                Title = _localizer["New Des Case Type Ext"].ToString(),
                Data = entity,
                PagePermission = await GetPermission(),
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        public async Task<IActionResult> Detail(string intlCode, string caseType, string desCountry, string desCaseType, string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var detail = await _repository.TmkDesCaseTypeExts.AsNoTracking()
                .FirstOrDefaultAsync(e => e.IntlCode == intlCode && e.CaseType == caseType && e.DesCountry == desCountry && e.DesCaseType == desCaseType && e.Systems == systems);

            if (detail == null)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }

            var perm = await GetPermission();
            perm.DeleteScreenUrl = perm.CanDeleteRecord
                ? Url.Action("Delete", new { intlCode = detail.IntlCode, caseType = detail.CaseType, desCountry = detail.DesCountry, desCaseType = detail.DesCaseType, systems = detail.Systems })
                : "";
            perm.CopyScreenUrl = perm.CanCopyRecord
                ? Url.Action("Add", new { fromSearch = true, copyIntlCode = detail.IntlCode, copyCaseType = detail.CaseType, copyDesCountry = detail.DesCountry, copyDesCaseType = detail.DesCaseType, copySystems = detail.Systems, copyDefault = detail.Default, copyGenApp = detail.GenApp })
                : "";
            perm.IsCopyScreenPopup = false;

            var model = new PageViewModel
            {
                Page = PageType.Detail,
                PageId = _dataContainer,
                Title = _localizer["Des Case Type Ext Detail"].ToString(),
                RecordId = 1,
                SingleRecord = singleRecord || !Request.IsAjax(),
                Data = detail,
                PagePermission = perm
            };
            if (Request.IsAjax() && !singleRecord && !fromSearch) model.Page = PageType.DetailContent;
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify), ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkDesCaseTypeExt entity)
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
            TmkDesCaseTypeExt existing = null;
            if (!isNewRecord)
            {
                existing = await _repository.TmkDesCaseTypeExts.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.IntlCode == entity.IntlCode && c.CaseType == entity.CaseType
                        && c.DesCountry == entity.DesCountry && c.DesCaseType == entity.DesCaseType && c.Systems == originalSystemsValue);
            }

            // Check for duplicate systems across other records with the same key fields
            var allRecords = await _repository.TmkDesCaseTypeExts.AsNoTracking()
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
                return new JsonBadRequest($"The following systems are already assigned to this Des Case Type Ext record: {string.Join(", ", duplicates)}");

            if (existing != null)
            {
                // Use raw SQL to avoid EF composite key tracking issues
                await _repository.Database.ExecuteSqlRawAsync(
                    @"UPDATE tblTmkDesCaseType_Ext SET IntlCode=@p0, CaseType=@p1, DesCountry=@p2, DesCaseType=@p3, [Default]=@p4, GenApp=@p5, Systems=@p6, CreatedBy=@p7, UpdatedBy=@p8, DateCreated=@p9, LastUpdate=@p10
                      WHERE IntlCode=@p11 AND CaseType=@p12 AND DesCountry=@p13 AND DesCaseType=@p14 AND Systems=@p15",
                    entity.IntlCode ?? "", entity.CaseType ?? "", entity.DesCountry ?? "", entity.DesCaseType ?? "", entity.Default, entity.GenApp, entity.Systems,
                    entity.CreatedBy ?? "", entity.UpdatedBy ?? "", entity.DateCreated, entity.LastUpdate,
                    existing.IntlCode ?? "", existing.CaseType ?? "", existing.DesCountry ?? "", existing.DesCaseType ?? "", existing.Systems ?? "");
            }
            else
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO tblTmkDesCaseType_Ext (IntlCode, CaseType, DesCountry, DesCaseType, [Default], GenApp, Systems, CreatedBy, UpdatedBy, DateCreated, LastUpdate)
                      VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10)",
                    entity.IntlCode ?? "", entity.CaseType ?? "", entity.DesCountry ?? "", entity.DesCaseType ?? "", entity.Default, entity.GenApp, entity.Systems,
                    entity.CreatedBy ?? "", entity.UpdatedBy ?? "", entity.DateCreated, entity.LastUpdate);
            }

            return Json(new { redirectUrl = Url.Action("Detail", new { intlCode = entity.IntlCode, caseType = entity.CaseType, desCountry = entity.DesCountry, desCaseType = entity.DesCaseType, systems = entity.Systems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string intlCode = "", string caseType = "", string desCountry = "", string desCaseType = "", string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkDesCaseType_Ext WHERE IntlCode=@p0 AND CaseType=@p1 AND DesCountry=@p2 AND DesCaseType=@p3 AND Systems=@p4",
                intlCode ?? "", caseType ?? "", desCountry ?? "", desCaseType ?? "", systems ?? "");

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Copy(string intlCode = "", string caseType = "", string desCountry = "", string desCaseType = "", string systems = "")
        {
            var entity = await _repository.TmkDesCaseTypeExts.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IntlCode == intlCode && c.CaseType == caseType && c.DesCountry == desCountry && c.DesCaseType == desCaseType && c.Systems == systems);
            if (entity == null) return new RecordDoesNotExistResult();

            return RedirectToAction("Add", new { fromSearch = true, copyIntlCode = entity.IntlCode, copyCaseType = entity.CaseType, copyDesCountry = entity.DesCountry, copyDesCaseType = entity.DesCaseType, copySystems = entity.Systems, copyDefault = entity.Default, copyGenApp = entity.GenApp });
        }

        public IActionResult GetSystemList()
        {
            return Json(Helpers.SystemsHelper.SystemNames);
        }

        public async Task<IActionResult> GetRecordStamps(string intlCode = "", string caseType = "", string desCountry = "", string desCaseType = "", string systems = "")
        {
            return ViewComponent("RecordStamps", new { createdBy = "", dateCreated = (DateTime?)null, updatedBy = "", lastUpdate = (DateTime?)null });
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_repository.TmkDesCaseTypeExts.AsQueryable(), request, property, text, filterType, requiredRelation);
        }

        [HttpGet]
        public IActionResult DetailLink(string intlCode = "", string caseType = "", string desCountry = "", string desCaseType = "", string systems = "")
        {
            if (!string.IsNullOrEmpty(intlCode))
                return RedirectToAction(nameof(Detail), new { intlCode = intlCode, caseType = caseType, desCountry = desCountry, desCaseType = desCaseType, systems = systems, singleRecord = true, fromSearch = true });
            return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        [HttpPost]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            return File(Convert.FromBase64String(base64), contentType, fileName);
        }
    }
}
