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
using R10.Core.Helpers;
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
    public class DesCaseTypeExtController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "patDesCaseTypeExtDetail";

        public DesCaseTypeExtController(IAuthorizationService authService, IApplicationDbContext repository, IStringLocalizer<SharedResource> localizer)
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
            var model = new PageViewModel { Page = PageType.Search, PageId = "patDesCaseTypeExtSearch", Title = _localizer["Des Case Type Ext Search"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return Request.IsAjax() ? PartialView("Index", model) : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel { Page = PageType.SearchResults, PageId = "patDesCaseTypeExtSearchResults", Title = _localizer["Des Case Type Ext Search Results"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search() => RedirectToAction("Index");

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            var entities = _repository.PatDesCaseTypeExts.AsNoTracking().AsQueryable();

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
                    if (filter.Property == "IntlCode" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => a.IntlCode == filter.Value);
                    else if (filter.Property == "CaseType" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => a.CaseType == filter.Value);
                    else if (filter.Property == "DesCountry" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => a.DesCountry == filter.Value);
                    else if (filter.Property == "DesCaseType" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => a.DesCaseType == filter.Value);
                }
            }

            var data = await entities.ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string copyIntlCode = "", string copyCaseType = "", string copyDesCountry = "", string copyDesCaseType = "", string copySystems = "", bool copyDefault = false, bool copyGenApp = false)
        {
            var data = new PatDesCaseTypeExt { IsNewRecord = true };

            if (!string.IsNullOrEmpty(copyIntlCode))
            {
                data.IntlCode = copyIntlCode;
                data.CaseType = copyCaseType;
                data.DesCountry = copyDesCountry;
                data.DesCaseType = copyDesCaseType;
                data.Systems = copySystems ?? "";
                data.Default = copyDefault;
                data.GenApp = copyGenApp;
            }

            var model = new PageViewModel
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = _dataContainer,
                Title = _localizer["New Des Case Type Ext"].ToString(),
                Data = data,
                PagePermission = await GetPermission(),
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        public async Task<IActionResult> Detail(string intlCode, string caseType, string desCountry, string desCaseType, string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var detail = await _repository.PatDesCaseTypeExts.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IntlCode == intlCode && c.CaseType == caseType && c.DesCountry == desCountry && c.DesCaseType == desCaseType && c.Systems == systems);
            if (detail == null)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }
            var perm = await GetPermission();
            perm.DeleteScreenUrl = perm.CanDeleteRecord ? Url.Action("Delete", new { intlCode = intlCode, caseType = caseType, desCountry = desCountry, desCaseType = desCaseType, systems = systems }) : "";
            perm.CopyScreenUrl = perm.CanCopyRecord ? Url.Action("Add", new { fromSearch = true, copyIntlCode = intlCode, copyCaseType = caseType, copyDesCountry = desCountry, copyDesCaseType = desCaseType, copySystems = systems, copyDefault = detail.Default, copyGenApp = detail.GenApp }) : "";
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

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify), ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatDesCaseTypeExt entity)
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

            if (!isNewRecord)
            {
                // Update existing record
                var originalSystems = entity.OriginalSystems == "__EMPTY__" ? "" : entity.OriginalSystems;

                // Check for duplicate systems across records with the same (IntlCode, CaseType, DesCountry, DesCaseType)
                var allRecords = await _repository.PatDesCaseTypeExts.AsNoTracking()
                    .Where(c => c.IntlCode == entity.IntlCode && c.CaseType == entity.CaseType && c.DesCountry == entity.DesCountry && c.DesCaseType == entity.DesCaseType
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
                    return new JsonBadRequest($"The following systems are already assigned to another Des Case Type Ext record: {string.Join(", ", duplicates)}");

                await _repository.Database.ExecuteSqlRawAsync(
                    "UPDATE tblPatDesCaseType_Ext SET IntlCode=@p0, CaseType=@p1, DesCountry=@p2, DesCaseType=@p3, [Default]=@p4, GenApp=@p5, CreatedBy=@p6, UpdatedBy=@p7, DateCreated=@p8, LastUpdate=@p9, Systems=@p10 WHERE IntlCode=@p11 AND CaseType=@p12 AND DesCountry=@p13 AND DesCaseType=@p14 AND Systems=@p15",
                    new object[] {
                        new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.IntlCode ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.CaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.DesCountry ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.DesCaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.Default),
                        new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.GenApp),
                        new Microsoft.Data.SqlClient.SqlParameter("@p6", entity.CreatedBy ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p7", entity.UpdatedBy ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p8", entity.DateCreated),
                        new Microsoft.Data.SqlClient.SqlParameter("@p9", entity.LastUpdate),
                        new Microsoft.Data.SqlClient.SqlParameter("@p10", entity.Systems),
                        new Microsoft.Data.SqlClient.SqlParameter("@p11", entity.IntlCode ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p12", entity.CaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p13", entity.DesCountry ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p14", entity.DesCaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p15", originalSystems ?? "")
                    });
            }
            else
            {
                // Insert new record
                // Check for duplicate systems across records with the same (IntlCode, CaseType, DesCountry, DesCaseType)
                var allRecords = await _repository.PatDesCaseTypeExts.AsNoTracking()
                    .Where(c => c.IntlCode == entity.IntlCode && c.CaseType == entity.CaseType && c.DesCountry == entity.DesCountry && c.DesCaseType == entity.DesCaseType
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
                    return new JsonBadRequest($"The following systems are already assigned to another Des Case Type Ext record: {string.Join(", ", duplicates)}");

                await _repository.Database.ExecuteSqlRawAsync(
                    "INSERT INTO tblPatDesCaseType_Ext (IntlCode, CaseType, DesCountry, DesCaseType, [Default], GenApp, CreatedBy, UpdatedBy, DateCreated, LastUpdate, Systems) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10)",
                    new object[] {
                        new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.IntlCode ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.CaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.DesCountry ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.DesCaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.Default),
                        new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.GenApp),
                        new Microsoft.Data.SqlClient.SqlParameter("@p6", entity.CreatedBy ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p7", entity.UpdatedBy ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p8", entity.DateCreated),
                        new Microsoft.Data.SqlClient.SqlParameter("@p9", entity.LastUpdate),
                        new Microsoft.Data.SqlClient.SqlParameter("@p10", entity.Systems)
                    });
            }

            return Json(new { id = 0, redirectUrl = Url.Action("Detail", new { intlCode = entity.IntlCode, caseType = entity.CaseType, desCountry = entity.DesCountry, desCaseType = entity.DesCaseType, systems = entity.Systems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string intlCode = "", string caseType = "", string desCountry = "", string desCaseType = "", string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblPatDesCaseType_Ext WHERE IntlCode=@p0 AND CaseType=@p1 AND DesCountry=@p2 AND DesCaseType=@p3 AND Systems=@p4",
                new object[] {
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", intlCode ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", caseType ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p2", desCountry ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p3", desCaseType ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p4", systems ?? "")
                });

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Copy(string intlCode = "", string caseType = "", string desCountry = "", string desCaseType = "", string systems = "")
        {
            var entity = await _repository.PatDesCaseTypeExts.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IntlCode == intlCode && c.CaseType == caseType && c.DesCountry == desCountry && c.DesCaseType == desCaseType && c.Systems == systems);
            if (entity == null) return new RecordDoesNotExistResult();

            return RedirectToAction("Add", new { copyIntlCode = entity.IntlCode, copyCaseType = entity.CaseType, copyDesCountry = entity.DesCountry, copyDesCaseType = entity.DesCaseType, copySystems = entity.Systems, copyDefault = entity.Default, copyGenApp = entity.GenApp });
        }

        public IActionResult GetRecordStamps(string intlCode = "", string caseType = "", string desCountry = "", string desCaseType = "", string systems = "")
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

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_repository.PatDesCaseTypeExts.AsQueryable(), request, property, text, filterType, requiredRelation);
        }
    }
}
