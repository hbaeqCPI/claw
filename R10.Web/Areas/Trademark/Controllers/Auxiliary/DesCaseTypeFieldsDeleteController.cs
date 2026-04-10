using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using R10.Web.Areas.Patent.Controllers;
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
    public class DesCaseTypeFieldsDeleteController : BaseController
    {
        private readonly IAuthorizationService _authService;
        private readonly IApplicationDbContext _repository;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly string _dataContainer = "tmkDesCaseTypeFieldsDeleteDetail";

        public DesCaseTypeFieldsDeleteController(IAuthorizationService authService, IApplicationDbContext repository, IStringLocalizer<SharedResource> localizer)
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
            var model = new PageViewModel { Page = PageType.Search, PageId = "tmkDesCaseTypeFieldsDeleteSearch", Title = _localizer["Des Case Type Fields Delete Search"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return Request.IsAjax() ? PartialView("Index", model) : View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] List<QueryFilterViewModel> mainSearchFilters)
        {
            var model = new PageViewModel { Page = PageType.SearchResults, PageId = "tmkDesCaseTypeFieldsDeleteSearchResults", Title = _localizer["Des Case Type Fields Delete Search Results"].ToString(), CanAddRecord = (await _authService.AuthorizeAsync(User, TrademarkAuthorizationPolicy.AuxiliaryModify)).Succeeded };
            return PartialView("Index", model);
        }

        [HttpGet]
        public IActionResult Search() => RedirectToAction("Index");

        public async Task<IActionResult> PageRead([DataSourceRequest] DataSourceRequest request, List<QueryFilterViewModel> mainSearchFilters)
        {
            var entities = _repository.TmkDesCaseTypeFieldsDeletes.AsNoTracking().AsQueryable();

            if (mainSearchFilters != null && mainSearchFilters.Count > 0)
            {
                entities = entities.BuildCriteria(mainSearchFilters);
            }

            // Return distinct (DesCaseType, Systems) combos for the search grid
            var data = await entities
                .Select(e => new { e.DesCaseType, e.Systems })
                .Distinct()
                .OrderBy(d => d.DesCaseType).ThenBy(d => d.Systems)
                .ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        public async Task<IActionResult> FieldsRead([DataSourceRequest] DataSourceRequest request, string desCaseType = "", string systems = "")
        {
            systems ??= "";
            var data = await _repository.TmkDesCaseTypeFieldsDeletes.AsNoTracking()
                .Where(f => f.DesCaseType == desCaseType && (f.Systems == systems || (f.Systems == null && systems == "") || (f.Systems == "" && systems == "")))
                .ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public IActionResult FieldsUpdate()
        {
            return Ok(new { success = "Record has been saved successfully." });
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> FieldsReplaceAll([FromBody] FieldsDeleteReplaceAllRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.DesCaseType))
                return BadRequest("Invalid request.");

            // Delete all existing records for this group
            await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkDesCaseTypeFieldsDelete WHERE DesCaseType=@p0 AND Systems=@p1",
                model.DesCaseType, model.Systems ?? "");

            // Re-insert all rows from the grid
            if (model.AllRows != null)
            {
                foreach (var row in model.AllRows)
                {
                    if (!string.IsNullOrEmpty(row.FromField))
                    {
                        await _repository.Database.ExecuteSqlRawAsync(
                            "INSERT INTO tblTmkDesCaseTypeFieldsDelete (DesCaseType, FromField, ToField, DesCaseTypeNew, FromFieldNew, ToFieldNew, Systems) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)",
                            model.DesCaseType, row.FromField ?? "", row.ToField ?? "", row.DesCaseTypeNew ?? "", row.FromFieldNew ?? "", row.ToFieldNew ?? "", model.Systems ?? "");
                    }
                }
            }

            return Ok();
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> FieldsDestroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "deleted")] TmkDesCaseTypeFieldsDelete deleted)
        {
            if (deleted != null && !string.IsNullOrEmpty(deleted.FromField))
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    "DELETE FROM tblTmkDesCaseTypeFieldsDelete WHERE DesCaseType=@p0 AND FromField=@p1 AND ToField=@p2 AND DesCaseTypeNew=@p3 AND FromFieldNew=@p4 AND ToFieldNew=@p5 AND Systems=@p6",
                    deleted.DesCaseType ?? "", deleted.FromField ?? "", deleted.ToField ?? "", deleted.DesCaseTypeNew ?? "", deleted.FromFieldNew ?? "", deleted.ToFieldNew ?? "", deleted.Systems ?? "");
            }
            return Ok(new { success = "Record deleted successfully." });
        }

        public async Task<IActionResult> Detail(string desCaseType, string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            systems ??= "";
            var exists = await _repository.TmkDesCaseTypeFieldsDeletes.AsNoTracking()
                .AnyAsync(c => c.DesCaseType == desCaseType && (c.Systems == systems || (c.Systems == null && systems == "") || (c.Systems == "" && systems == "")));
            if (!exists)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }

            var detail = new TmkDesCaseTypeFieldsDelete { DesCaseType = desCaseType, Systems = systems };

            var perm = await GetPermission();
            perm.AddScreenUrl = perm.CanAddRecord ? Url.Action("Add", new { fromSearch = true }) : "";
            perm.SearchScreenUrl = Url.Action("Index");
            perm.DeleteScreenUrl = perm.CanDeleteRecord ? Url.Action("Delete", new { desCaseType = desCaseType, systems = systems }) : "";
            perm.CopyScreenUrl = perm.CanCopyRecord ? Url.Action("Add", new { fromSearch = true, copyDesCaseType = desCaseType, copySystems = systems, copyGroup = true }) : "";
            perm.IsCopyScreenPopup = false;
            var model = new PageViewModel
            {
                Page = PageType.Detail,
                PageId = _dataContainer,
                Title = _localizer["Des Case Type Fields Delete Detail"].ToString(),
                RecordId = 1,
                SingleRecord = singleRecord || !Request.IsAjax(),
                Data = detail,
                PagePermission = perm
            };
            if (Request.IsAjax() && !singleRecord && !fromSearch) model.Page = PageType.DetailContent;
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string copyDesCaseType = "", string copyFromField = "", string copyToField = "", string copyDesCaseTypeNew = "", string copyFromFieldNew = "", string copyToFieldNew = "", string copySystems = "", bool copyGroup = false)
        {
            var data = new TmkDesCaseTypeFieldsDelete { IsNewRecord = true };

            if (!string.IsNullOrEmpty(copyDesCaseType))
            {
                data.DesCaseType = copyDesCaseType;
                data.FromField = copyFromField;
                data.ToField = copyToField;
                data.DesCaseTypeNew = copyDesCaseTypeNew;
                data.FromFieldNew = copyFromFieldNew;
                data.ToFieldNew = copyToFieldNew;
                data.Systems = copySystems ?? "";
                if (copyGroup)
                {
                    data.CopyFromSystems = copySystems ?? "";
                    data.CopyFromDesCaseType = copyDesCaseType ?? "";
                }
            }

            var model = new PageViewModel
            {
                Page = fromSearch ? PageType.Detail : PageType.DetailContent,
                PageId = _dataContainer,
                Title = _localizer["New Des Case Type Fields Delete"].ToString(),
                Data = data,
                PagePermission = await GetPermission(),
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify), ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkDesCaseTypeFieldsDelete entity)
        {
            // Group copy or detail page system update doesn't submit FromField/ToField
            if (!string.IsNullOrEmpty(entity.CopyFromSystems) || string.IsNullOrEmpty(entity.FromField))
            {
                ModelState.Remove("FromField");
                ModelState.Remove("ToField");
            }
            ModelState.Remove("DesCaseTypeNew");
            ModelState.Remove("FromFieldNew");
            ModelState.Remove("ToFieldNew");
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });

            entity.Systems ??= "";

            // Require at least one system
            if (string.IsNullOrWhiteSpace(entity.Systems))
                return new JsonBadRequest("At least one system must be selected.");

            // Split into individual systems
            var individualSystems = entity.Systems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();

            var isNewRecord = entity.IsNewRecord || entity.OriginalSystems == "__NEW__" || entity.OriginalSystems == null;

            if (isNewRecord && !string.IsNullOrEmpty(entity.CopyFromSystems))
            {
                // Group copy: block if any of the new systems already exist for this DesCaseType
                var existingSystemStrings = await _repository.TmkDesCaseTypeFieldsDeletes.AsNoTracking()
                    .Where(c => c.DesCaseType == entity.DesCaseType)
                    .Select(c => c.Systems)
                    .Distinct()
                    .ToListAsync();
                var existingIndividualSystems = existingSystemStrings
                    .SelectMany(s => (s ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(s => s.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                var overlap = individualSystems.Where(s => existingIndividualSystems.Contains(s)).ToList();
                if (overlap.Any())
                    return new JsonBadRequest($"The following systems already exist for Des Case Type '{entity.DesCaseType}': {string.Join(", ", overlap)}");

                var newSystems = string.Join(",", individualSystems);
                await _repository.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO tblTmkDesCaseTypeFieldsDelete (DesCaseType, FromField, ToField, DesCaseTypeNew, FromFieldNew, ToFieldNew, Systems)
                      SELECT @p0, FromField, ToField, DesCaseTypeNew, FromFieldNew, ToFieldNew, @p1
                      FROM tblTmkDesCaseTypeFieldsDelete
                      WHERE DesCaseType=@p2 AND Systems=@p3",
                    entity.DesCaseType ?? "", newSystems, entity.CopyFromDesCaseType ?? entity.DesCaseType ?? "", entity.CopyFromSystems);
            }
            else if (isNewRecord)
            {
                var newSystemsStr = string.Join(",", individualSystems);
                var existingStrs = await _repository.TmkDesCaseTypeFieldsDeletes.AsNoTracking()
                    .Where(c => c.DesCaseType == entity.DesCaseType)
                    .Select(c => c.Systems).Distinct().ToListAsync();
                var existingIndiv = existingStrs
                    .SelectMany(s => (s ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var sysOverlap = individualSystems.Where(s => existingIndiv.Contains(s)).ToList();
                if (sysOverlap.Any())
                    return new JsonBadRequest($"The following systems already exist for Des Case Type '{entity.DesCaseType}': {string.Join(", ", sysOverlap)}");

                entity.DesCaseTypeNew = entity.DesCaseType ?? "";
                entity.FromFieldNew = entity.FromField ?? "";
                entity.ToFieldNew = entity.ToField ?? "";

                await _repository.Database.ExecuteSqlRawAsync(
                    "INSERT INTO tblTmkDesCaseTypeFieldsDelete (DesCaseType, FromField, ToField, DesCaseTypeNew, FromFieldNew, ToFieldNew, Systems) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)",
                    entity.DesCaseType ?? "", entity.FromField ?? "", entity.ToField ?? "", entity.DesCaseTypeNew ?? "", entity.FromFieldNew ?? "", entity.ToFieldNew ?? "", newSystemsStr);
            }
            else
            {
                var originalSystems = entity.OriginalSystems == "__EMPTY__" ? "" : entity.OriginalSystems;
                var newSystemsValue = string.Join(",", individualSystems);

                if (string.IsNullOrEmpty(entity.FromField))
                {
                    // Bulk systems update from detail page - update ALL records for this DesCaseType+Systems group
                    if (newSystemsValue != originalSystems)
                    {
                        await _repository.Database.ExecuteSqlRawAsync(
                            "UPDATE tblTmkDesCaseTypeFieldsDelete SET Systems=@p0 WHERE DesCaseType=@p1 AND Systems=@p2",
                            newSystemsValue, entity.DesCaseType ?? "", originalSystems ?? "");
                    }
                }
                else
                {
                    // Single record update
                    await _repository.Database.ExecuteSqlRawAsync(
                        @"UPDATE tblTmkDesCaseTypeFieldsDelete SET DesCaseType=@p0, FromField=@p1, ToField=@p2,
                          DesCaseTypeNew=@p3, FromFieldNew=@p4, ToFieldNew=@p5, Systems=@p6
                          WHERE DesCaseType=@p7 AND FromField=@p8 AND ToField=@p9 AND Systems=@p10",
                        new object[] {
                            new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.DesCaseType ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.FromField ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.ToField ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p3", entity.DesCaseTypeNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.FromFieldNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.ToFieldNew ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p6", newSystemsValue),
                            new Microsoft.Data.SqlClient.SqlParameter("@p7", entity.DesCaseType ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p8", entity.FromField ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p9", entity.ToField ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p10", originalSystems ?? "")
                        });

                    // Cascade Systems change to all sibling records in the same group
                    if (newSystemsValue != originalSystems)
                    {
                        await _repository.Database.ExecuteSqlRawAsync(
                            "UPDATE tblTmkDesCaseTypeFieldsDelete SET Systems=@p0 WHERE DesCaseType=@p1 AND Systems=@p2",
                            newSystemsValue, entity.DesCaseType ?? "", originalSystems ?? "");
                    }
                }
            }

            var redirectSystems = string.Join(",", individualSystems);
            return Json(new { id = 0, redirectUrl = Url.Action("Detail", new { desCaseType = entity.DesCaseType, systems = redirectSystems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string desCaseType = "", string systems = "")
        {
            // Delete ALL records for this (DesCaseType, Systems) group
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkDesCaseTypeFieldsDelete WHERE DesCaseType=@p0 AND Systems=@p1",
                new object[] {
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", desCaseType ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", systems ?? "")
                });

            if (count == 0)
                return new RecordDoesNotExistResult();

            return Ok();
        }

        public IActionResult GetSystemList()
        {
            return Json(Helpers.SystemsHelper.SystemNames);
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text = "", FilterType filterType = FilterType.StartsWith, string requiredRelation = "")
        {
            return await GetPicklistData(_repository.TmkDesCaseTypeFieldsDeletes.AsNoTracking(), request, property, text, filterType, requiredRelation);
        }

        public IActionResult GetRecordStamps(string desCaseType = "", string systems = "")
        {
            return ViewComponent("RecordStamps", new { createdBy = "", dateCreated = (DateTime?)null, updatedBy = "", lastUpdate = (DateTime?)null });
        }

        [HttpGet]
        public IActionResult DetailLink(string desCaseType = "", string systems = "")
        {
            if (!string.IsNullOrEmpty(desCaseType))
                return RedirectToAction(nameof(Detail), new { desCaseType, systems, singleRecord = true, fromSearch = true });
            return RedirectToAction(nameof(Add), new { fromSearch = true });
        }

        [HttpPost]
        public IActionResult ExcelExportSave(string contentType, string base64, string fileName)
        {
            return File(Convert.FromBase64String(base64), contentType, fileName);
        }
    }
}
