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
    public class DesCaseTypeFieldsController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "patDesCaseTypeFieldsDetail";

        public DesCaseTypeFieldsController(IAuthorizationService authService, IApplicationDbContext repository, IStringLocalizer<SharedResource> localizer)
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
            var model = new PageViewModel { Page = PageType.Search, PageId = "patDesCaseTypeFieldsSearch", Title = _localizer["Des Case Type Fields Search"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return Request.IsAjax() ? PartialView("Index", model) : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel { Page = PageType.SearchResults, PageId = "patDesCaseTypeFieldsSearchResults", Title = _localizer["Des Case Type Fields Search Results"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search() => RedirectToAction("Index");

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            var entities = _repository.PatDesCaseTypeFields.AsNoTracking().AsQueryable();

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
                    if (filter.Property == "DesCaseType" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => a.DesCaseType == filter.Value);
                    else if (filter.Property == "FromField" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => a.FromField == filter.Value);
                    else if (filter.Property == "ToField" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => a.ToField == filter.Value);
                }
            }

            var data = await entities.ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string copyDesCaseType = "", string copyFromField = "", string copyToField = "", string copySystems = "")
        {
            if (!Request.IsAjax()) return RedirectToAction("Index");
            var data = new PatDesCaseTypeFields { IsNewRecord = true };

            if (!string.IsNullOrEmpty(copyDesCaseType))
            {
                data.DesCaseType = copyDesCaseType;
                data.FromField = copyFromField;
                data.ToField = copyToField;
                data.Systems = copySystems ?? "";
            }

            var model = new PageViewModel
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = _dataContainer,
                Title = _localizer["New Des Case Type Fields"].ToString(),
                Data = data,
                PagePermission = await GetPermission(),
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();
            return PartialView("Index", model);
        }

        public async Task<IActionResult> Detail(string desCaseType, string fromField, string toField, string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var detail = await _repository.PatDesCaseTypeFields.AsNoTracking()
                .FirstOrDefaultAsync(c => c.DesCaseType == desCaseType && c.FromField == fromField && c.ToField == toField && c.Systems == systems);
            if (detail == null)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }
            var perm = await GetPermission();
            perm.DeleteScreenUrl = perm.CanDeleteRecord ? Url.Action("Delete", new { desCaseType = desCaseType, fromField = fromField, toField = toField, systems = systems }) : "";
            perm.CopyScreenUrl = perm.CanCopyRecord ? Url.Action("Add", new { fromSearch = true, copyDesCaseType = desCaseType, copyFromField = fromField, copyToField = toField, copySystems = systems }) : "";
            perm.IsCopyScreenPopup = false;
            var model = new PageViewModel
            {
                Page = PageType.Detail,
                PageId = _dataContainer,
                Title = _localizer["Des Case Type Fields Detail"].ToString(),
                RecordId = 1,
                SingleRecord = singleRecord || !Request.IsAjax(),
                Data = detail,
                PagePermission = perm
            };
            if (Request.IsAjax() && !singleRecord && !fromSearch) model.Page = PageType.DetailContent;
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify), ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatDesCaseTypeFields entity)
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

                // Check for duplicate systems across records with the same (DesCaseType, FromField, ToField)
                var allRecords = await _repository.PatDesCaseTypeFields.AsNoTracking()
                    .Where(c => c.DesCaseType == entity.DesCaseType && c.FromField == entity.FromField && c.ToField == entity.ToField
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
                    return new JsonBadRequest($"The following systems are already assigned to another Des Case Type Fields record: {string.Join(", ", duplicates)}");

                await _repository.Database.ExecuteSqlRawAsync(
                    "UPDATE tblPatDesCaseTypeFields SET DesCaseType=@p0, FromField=@p1, ToField=@p2, Systems=@p3 WHERE DesCaseType=@p4 AND FromField=@p5 AND ToField=@p6 AND Systems=@p7",
                    new object[] {
                        new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.DesCaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.FromField ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.ToField ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.Systems),
                        new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.DesCaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.FromField ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p6", entity.ToField ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p7", originalSystems ?? "")
                    });
            }
            else
            {
                // Insert new record
                // Check for duplicate systems across records with the same (DesCaseType, FromField, ToField)
                var allRecords = await _repository.PatDesCaseTypeFields.AsNoTracking()
                    .Where(c => c.DesCaseType == entity.DesCaseType && c.FromField == entity.FromField && c.ToField == entity.ToField
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
                    return new JsonBadRequest($"The following systems are already assigned to another Des Case Type Fields record: {string.Join(", ", duplicates)}");

                await _repository.Database.ExecuteSqlRawAsync(
                    "INSERT INTO tblPatDesCaseTypeFields (DesCaseType, FromField, ToField, Systems) VALUES (@p0, @p1, @p2, @p3)",
                    new object[] {
                        new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.DesCaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.FromField ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.ToField ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.Systems)
                    });
            }

            return Json(new { id = 0, redirectUrl = Url.Action("Detail", new { desCaseType = entity.DesCaseType, fromField = entity.FromField, toField = entity.ToField, systems = entity.Systems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string desCaseType = "", string fromField = "", string toField = "", string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblPatDesCaseTypeFields WHERE DesCaseType=@p0 AND FromField=@p1 AND ToField=@p2 AND Systems=@p3",
                new object[] {
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", desCaseType ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", fromField ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p2", toField ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p3", systems ?? "")
                });

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Copy(string desCaseType = "", string fromField = "", string toField = "", string systems = "")
        {
            var entity = await _repository.PatDesCaseTypeFields.AsNoTracking()
                .FirstOrDefaultAsync(c => c.DesCaseType == desCaseType && c.FromField == fromField && c.ToField == toField && c.Systems == systems);
            if (entity == null) return new RecordDoesNotExistResult();

            return RedirectToAction("Add", new { copyDesCaseType = entity.DesCaseType, copyFromField = entity.FromField, copyToField = entity.ToField, copySystems = entity.Systems });
        }

        public IActionResult GetRecordStamps(string desCaseType = "", string fromField = "", string toField = "", string systems = "")
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

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_repository.PatDesCaseTypeFields.AsQueryable(), request, property, text, filterType, requiredRelation);
        }
    }
}
