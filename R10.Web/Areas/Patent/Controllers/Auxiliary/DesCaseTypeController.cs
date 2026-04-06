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
    public class DesCaseTypeController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "patDesCaseTypeDetail";

        public DesCaseTypeController(IAuthorizationService authService, IApplicationDbContext repository, IStringLocalizer<SharedResource> localizer)
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
            var model = new PageViewModel { Page = PageType.Search, PageId = "patDesCaseTypeSearch", Title = _localizer["Designation Case Type Search"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return Request.IsAjax() ? PartialView("Index", model) : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel { Page = PageType.SearchResults, PageId = "patDesCaseTypeSearchResults", Title = _localizer["Designation Case Type Search Results"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, PatentAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search() => RedirectToAction("Index");

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            var entities = _repository.PatDesCaseTypes.AsNoTracking().AsQueryable();

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
                        entities = entities.Where(a => EF.Functions.Like(a.IntlCode, filter.Value));
                    else if (filter.Property == "CaseType" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => EF.Functions.Like(a.CaseType, filter.Value));
                    else if (filter.Property == "DesCountry" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => EF.Functions.Like(a.DesCountry, filter.Value));
                    else if (filter.Property == "DesCaseType" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => EF.Functions.Like(a.DesCaseType, filter.Value));
                }
            }

            var data = await entities.ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string copyIntlCode = "", string copyCaseType = "", string copyDesCountry = "", string copyDesCaseType = "", string copySystems = "", bool copyDefault = false)
        {
            var data = new PatDesCaseType { IsNewRecord = true };

            if (!string.IsNullOrEmpty(copyIntlCode))
            {
                data.IntlCode = copyIntlCode;
                data.CaseType = copyCaseType;
                data.DesCountry = copyDesCountry;
                data.DesCaseType = copyDesCaseType;
                data.Systems = copySystems ?? "";
                data.Default = copyDefault;
            }

            var model = new PageViewModel
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = _dataContainer,
                Title = _localizer["New Designation Case Type"].ToString(),
                Data = data,
                PagePermission = await GetPermission(),
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        public async Task<IActionResult> Detail(string intlCode, string caseType, string desCountry, string desCaseType, string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var detail = await _repository.PatDesCaseTypes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IntlCode == intlCode && c.CaseType == caseType && c.DesCountry == desCountry && c.DesCaseType == desCaseType && c.Systems == systems);
            if (detail == null)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }
            var perm = await GetPermission();
            perm.DeleteScreenUrl = perm.CanDeleteRecord ? Url.Action("Delete", new { intlCode = intlCode, caseType = caseType, desCountry = desCountry, desCaseType = desCaseType, systems = systems }) : "";
            perm.CopyScreenUrl = perm.CanCopyRecord ? Url.Action("Add", new { fromSearch = true, copyIntlCode = intlCode, copyCaseType = caseType, copyDesCountry = desCountry, copyDesCaseType = desCaseType, copySystems = systems, copyDefault = detail.Default }) : "";
            perm.IsCopyScreenPopup = false;
            var model = new PageViewModel
            {
                Page = PageType.Detail,
                PageId = _dataContainer,
                Title = _localizer["Designation Case Type Detail"].ToString(),
                RecordId = 1,
                SingleRecord = singleRecord || !Request.IsAjax(),
                Data = detail,
                PagePermission = perm
            };
            if (Request.IsAjax() && !singleRecord && !fromSearch) model.Page = PageType.DetailContent;
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify), ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] PatDesCaseType entity)
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

                // Check for duplicate systems across records with the same (IntlCode, CaseType, DesCountry, DesCaseType)
                var allRecords = await _repository.PatDesCaseTypes.AsNoTracking()
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
                    return new JsonBadRequest($"The following systems are already assigned to another Designation Case Type record: {string.Join(", ", duplicates)}");

                await _repository.Database.ExecuteSqlRawAsync(
                    "UPDATE tblPatDesCaseType SET IntlCode=@p0, CaseType=@p1, DesCountry=@p2, DesCaseType=@p3, [Default]=@p4, Systems=@p5 WHERE IntlCode=@p6 AND CaseType=@p7 AND DesCountry=@p8 AND DesCaseType=@p9 AND Systems=@p10",
                    new object[] {
                        new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.IntlCode ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.CaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.DesCountry ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.DesCaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.Default),
                        new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.Systems),
                        new Microsoft.Data.SqlClient.SqlParameter("@p6", entity.IntlCode ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p7", entity.CaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p8", entity.DesCountry ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p9", entity.DesCaseType ?? ""),
                        new Microsoft.Data.SqlClient.SqlParameter("@p10", originalSystems ?? "")
                    });
            }
            else
            {
                // Check if a matching record exists (same IntlCode, CaseType, DesCountry, DesCaseType, Default)
                var existingRecord = await _repository.PatDesCaseTypes.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.IntlCode == entity.IntlCode && c.CaseType == entity.CaseType
                        && c.DesCountry == entity.DesCountry && c.DesCaseType == entity.DesCaseType
                        && c.Default == entity.Default);

                if (existingRecord != null)
                {
                    // Merge new systems into existing record's Systems
                    var existingSystems = (existingRecord.Systems ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var overlap = newSystems.Where(s => existingSystems.Contains(s)).ToList();
                    if (overlap.Any())
                        return new JsonBadRequest($"The following systems are already assigned: {string.Join(", ", overlap)}");

                    // Add new systems to existing
                    foreach (var s in newSystems) existingSystems.Add(s);
                    var mergedSystems = string.Join(",", existingSystems.OrderBy(s => s, StringComparer.OrdinalIgnoreCase));

                    await _repository.Database.ExecuteSqlRawAsync(
                        "UPDATE tblPatDesCaseType SET Systems=@p0 WHERE IntlCode=@p1 AND CaseType=@p2 AND DesCountry=@p3 AND DesCaseType=@p4 AND Systems=@p5",
                        mergedSystems, existingRecord.IntlCode ?? "", existingRecord.CaseType ?? "",
                        existingRecord.DesCountry ?? "", existingRecord.DesCaseType ?? "", existingRecord.Systems ?? "");

                    entity.Systems = mergedSystems;
                }
                else
                {
                    // No matching record — insert new
                    await _repository.Database.ExecuteSqlRawAsync(
                        "INSERT INTO tblPatDesCaseType (IntlCode, CaseType, DesCountry, DesCaseType, [Default], Systems) VALUES (@p0, @p1, @p2, @p3, @p4, @p5)",
                        new object[] {
                            new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.IntlCode ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.CaseType ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.DesCountry ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.DesCaseType ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.Default),
                            new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.Systems)
                        });
                }
            }

            return Json(new { id = 0, redirectUrl = Url.Action("Detail", new { intlCode = entity.IntlCode, caseType = entity.CaseType, desCountry = entity.DesCountry, desCaseType = entity.DesCaseType, systems = entity.Systems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string intlCode = "", string caseType = "", string desCountry = "", string desCaseType = "", string systems = "")
        {
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblPatDesCaseType WHERE IntlCode=@p0 AND CaseType=@p1 AND DesCountry=@p2 AND DesCaseType=@p3 AND Systems=@p4",
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
            var entity = await _repository.PatDesCaseTypes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IntlCode == intlCode && c.CaseType == caseType && c.DesCountry == desCountry && c.DesCaseType == desCaseType && c.Systems == systems);
            if (entity == null) return new RecordDoesNotExistResult();

            return RedirectToAction("Add", new { copyIntlCode = entity.IntlCode, copyCaseType = entity.CaseType, copyDesCountry = entity.DesCountry, copyDesCaseType = entity.DesCaseType, copySystems = entity.Systems, copyDefault = entity.Default });
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
            return await GetPicklistData(_repository.PatDesCaseTypes.AsQueryable(), request, property, text, filterType, requiredRelation);
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> MoveToDelete(string intlCode = "", string caseType = "", string desCountry = "", string desCaseType = "", bool defaultVal = false, string systems = "")
        {
            // Check if a matching record exists in the delete table (same key fields, any Default)
            var existingDelete = await _repository.PatDesCaseTypeDeletes.AsNoTracking()
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
                    "UPDATE tblPatDesCaseTypeDelete SET Systems=@p0 WHERE IntlCode=@p1 AND CaseType=@p2 AND DesCountry=@p3 AND DesCaseType=@p4 AND Systems=@p5",
                    mergedSystems, existingDelete.IntlCode ?? "", existingDelete.CaseType ?? "", existingDelete.DesCountry ?? "", existingDelete.DesCaseType ?? "", existingDelete.Systems ?? "");
            }
            else
            {
                // Insert new record into delete table
                await _repository.Database.ExecuteSqlRawAsync(
                    "INSERT INTO tblPatDesCaseTypeDelete (IntlCode, CaseType, DesCountry, DesCaseType, [Default], IntlCodeNew, CaseTypeNew, DesCountryNew, DesCaseTypeNew, Systems) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9)",
                    intlCode ?? "", caseType ?? "", desCountry ?? "", desCaseType ?? "", defaultVal, intlCode ?? "", caseType ?? "", desCountry ?? "", desCaseType ?? "", systems ?? "");
            }

            // Delete from source table
            await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblPatDesCaseType WHERE IntlCode=@p0 AND CaseType=@p1 AND DesCountry=@p2 AND DesCaseType=@p3 AND Systems=@p4",
                intlCode ?? "", caseType ?? "", desCountry ?? "", desCaseType ?? "", systems ?? "");

            return Ok(new { success = "Record moved to delete table successfully." });
        }
    }
}
