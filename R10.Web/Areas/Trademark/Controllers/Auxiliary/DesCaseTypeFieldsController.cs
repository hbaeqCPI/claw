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
                entities = entities.BuildCriteria(mainSearchFilters);
            }

            var data = await entities
                .Select(e => new { e.DesCaseType, e.Systems })
                .Distinct()
                .OrderBy(d => d.DesCaseType)
                .ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        public async Task<IActionResult> FieldsRead([DataSourceRequest] DataSourceRequest request, string desCaseType, string systems = "")
        {
            var query = _repository.TmkDesCaseTypeFields.AsNoTracking()
                .Where(f => f.DesCaseType == desCaseType);
            if (!string.IsNullOrEmpty(systems))
                query = query.Where(f => f.Systems == systems);
            var data = await query.ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> FieldsUpdate([DataSourceRequest] DataSourceRequest request,
            string desCaseType = "", string systems = "",
            [Bind(Prefix = "updated")] IList<TmkDesCaseTypeFields> updated = null,
            [Bind(Prefix = "new")] IList<TmkDesCaseTypeFields> added = null,
            [Bind(Prefix = "deleted")] IList<TmkDesCaseTypeFields> deleted = null)
        {
            if ((updated == null || !updated.Any()) && (added == null || !added.Any()) && (deleted == null || !deleted.Any()))
            {
                var allModels = new List<TmkDesCaseTypeFields>();
                int i = 0;
                while (Request.Form.ContainsKey($"models[{i}].FromField"))
                {
                    allModels.Add(new TmkDesCaseTypeFields
                    {
                        DesCaseType = Request.Form[$"models[{i}].DesCaseType"].FirstOrDefault() ?? desCaseType,
                        FromField = Request.Form[$"models[{i}].FromField"].FirstOrDefault() ?? "",
                        ToField = Request.Form[$"models[{i}].ToField"].FirstOrDefault() ?? "",
                        Systems = Request.Form[$"models[{i}].Systems"].FirstOrDefault() ?? systems
                    });
                    i++;
                }
                updated = allModels;
            }

            updated ??= new List<TmkDesCaseTypeFields>();
            added ??= new List<TmkDesCaseTypeFields>();
            deleted ??= new List<TmkDesCaseTypeFields>();

            foreach (var item in deleted)
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    "DELETE FROM tblTmkDesCaseTypeFields WHERE DesCaseType=@p0 AND FromField=@p1 AND ToField=@p2 AND Systems=@p3",
                    item.DesCaseType ?? desCaseType, item.FromField ?? "", item.ToField ?? "", item.Systems ?? systems);
            }

            foreach (var item in updated)
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    "UPDATE tblTmkDesCaseTypeFields SET ToField=@p0 WHERE DesCaseType=@p1 AND FromField=@p2 AND Systems=@p3",
                    item.ToField ?? "", item.DesCaseType ?? desCaseType, item.FromField ?? "", item.Systems ?? systems);
            }

            foreach (var item in added)
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    "INSERT INTO tblTmkDesCaseTypeFields (DesCaseType, FromField, ToField, Systems) VALUES (@p0, @p1, @p2, @p3)",
                    item.DesCaseType ?? desCaseType, item.FromField ?? "", item.ToField ?? "", item.Systems ?? systems);
            }

            return Ok(new { success = "Record has been saved successfully." });
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> FieldsDestroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "deleted")] TmkDesCaseTypeFields deleted)
        {
            if (deleted != null && !string.IsNullOrEmpty(deleted.FromField))
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    "DELETE FROM tblTmkDesCaseTypeFields WHERE DesCaseType=@p0 AND FromField=@p1 AND ToField=@p2 AND Systems=@p3",
                    deleted.DesCaseType ?? "", deleted.FromField ?? "", deleted.ToField ?? "", deleted.Systems ?? "");
            }
            return Ok(new { success = "Record deleted successfully." });
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> FieldsReplaceAll([FromBody] FieldsReplaceAllRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.DesCaseType))
                return BadRequest("Invalid request.");

            await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkDesCaseTypeFields WHERE DesCaseType=@p0 AND Systems=@p1",
                model.DesCaseType, model.Systems ?? "");

            if (model.AllRows != null)
            {
                foreach (var row in model.AllRows)
                {
                    if (!string.IsNullOrEmpty(row.FromField))
                    {
                        await _repository.Database.ExecuteSqlRawAsync(
                            "INSERT INTO tblTmkDesCaseTypeFields (DesCaseType, FromField, ToField, Systems) VALUES (@p0, @p1, @p2, @p3)",
                            model.DesCaseType, row.FromField ?? "", row.ToField ?? "", model.Systems ?? "");
                    }
                }
            }

            return Ok();
        }

        public async Task<IActionResult> Detail(string desCaseType, string fromField = "", string toField = "", string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var exists = await _repository.TmkDesCaseTypeFields.AsNoTracking()
                .AnyAsync(c => c.DesCaseType == desCaseType);
            if (!exists)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(systems))
            {
                systems = await _repository.TmkDesCaseTypeFields.AsNoTracking()
                    .Where(c => c.DesCaseType == desCaseType)
                    .Select(c => c.Systems)
                    .FirstOrDefaultAsync() ?? "";
            }

            var detail = new TmkDesCaseTypeFields { DesCaseType = desCaseType, Systems = systems };

            var permission = await GetPermission();
            permission.AddScreenUrl = permission.CanAddRecord ? Url.Action("Add", new { fromSearch = true }) : "";
            permission.DeleteScreenUrl = permission.CanDeleteRecord ? Url.Action("Delete", new { desCaseType = desCaseType }) : "";
            permission.CopyScreenUrl = permission.CanCopyRecord ? Url.Action("Add", new { fromSearch = true, copyDesCaseType = desCaseType, copyGroup = true }) : "";
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
        public async Task<IActionResult> Add(bool fromSearch = false, string copyDesCaseType = "", string copyFromField = "", string copyToField = "", string copySystems = "", bool copyGroup = false)
        {
            var entity = new TmkDesCaseTypeFields { IsNewRecord = true };
            if (!string.IsNullOrEmpty(copyDesCaseType))
            {
                entity.DesCaseType = copyDesCaseType;
                entity.FromField = copyFromField;
                entity.ToField = copyToField;
                entity.Systems = copySystems ?? "";
                if (copyGroup)
                {
                    entity.CopyFromSystems = copySystems ?? "";
                    entity.CopyFromDesCaseType = copyDesCaseType ?? "";
                }
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

            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] TmkDesCaseTypeFields entity)
        {
            // Group copy doesn't submit FromField/ToField - clear their validation
            if (!string.IsNullOrEmpty(entity.CopyFromSystems) || string.IsNullOrEmpty(entity.FromField))
            {
                ModelState.Remove("FromField");
                ModelState.Remove("ToField");
            }
            if (!ModelState.IsValid)
                return new JsonBadRequest(new { errors = ModelState.Errors() });

            entity.Systems ??= "";

            // Require at least one system
            if (string.IsNullOrWhiteSpace(entity.Systems))
                return new JsonBadRequest("At least one system must be selected.");

            // Split into individual systems - each system gets its own record
            var individualSystems = entity.Systems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();

            var isNewRecord = entity.IsNewRecord || entity.OriginalSystems == "__NEW__" || entity.OriginalSystems == null;
            var originalSystemsValue = entity.OriginalSystems == "__EMPTY__" ? "" : (entity.OriginalSystems ?? "");

            if (isNewRecord && !string.IsNullOrEmpty(entity.CopyFromSystems))
            {
                // Group copy: block if any of the new systems already exist for this DesCaseType
                var existingSystemStrings = await _repository.TmkDesCaseTypeFields.AsNoTracking()
                    .Where(c => c.DesCaseType == entity.DesCaseType)
                    .Select(c => c.Systems).Distinct().ToListAsync();
                var existingIndividualSystems = existingSystemStrings
                    .SelectMany(s => (s ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var overlap = individualSystems.Where(s => existingIndividualSystems.Contains(s)).ToList();
                if (overlap.Any())
                    return new JsonBadRequest($"The following systems already exist for Des Case Type '{entity.DesCaseType}': {string.Join(", ", overlap)}");

                // Bulk-copy all From/To records from source with the combined Systems string
                var newSystems = string.Join(",", individualSystems);
                await _repository.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO tblTmkDesCaseTypeFields (DesCaseType, FromField, ToField, Systems)
                      SELECT @p0, FromField, ToField, @p1
                      FROM tblTmkDesCaseTypeFields
                      WHERE DesCaseType=@p2 AND Systems=@p3",
                    entity.DesCaseType ?? "", newSystems, entity.CopyFromDesCaseType ?? entity.DesCaseType ?? "", entity.CopyFromSystems);
            }
            else if (isNewRecord)
            {
                // Individual add: insert one record with the combined Systems string
                var newSystemsStr = string.Join(",", individualSystems);
                var exactExists = await _repository.TmkDesCaseTypeFields.AsNoTracking()
                    .AnyAsync(c => c.DesCaseType == entity.DesCaseType && c.FromField == entity.FromField && c.ToField == entity.ToField && c.Systems == newSystemsStr);
                if (exactExists)
                    return new JsonBadRequest($"A record with Des Case Type '{entity.DesCaseType}', From Field '{entity.FromField}', To Field '{entity.ToField}', and Systems '{newSystemsStr}' already exists.");

                await _repository.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO tblTmkDesCaseTypeFields (DesCaseType, FromField, ToField, Systems)
                      VALUES (@p0, @p1, @p2, @p3)",
                    entity.DesCaseType ?? "", entity.FromField ?? "", entity.ToField ?? "", newSystemsStr);
            }
            else
            {
                var newSystemsValue2 = string.Join(",", individualSystems);

                if (string.IsNullOrEmpty(entity.FromField))
                {
                    // Bulk systems update from detail page - update ALL records for this DesCaseType
                    if (newSystemsValue2 != originalSystemsValue)
                    {
                        await _repository.Database.ExecuteSqlRawAsync(
                            "UPDATE tblTmkDesCaseTypeFields SET Systems=@p0 WHERE DesCaseType=@p1 AND Systems=@p2",
                            newSystemsValue2, entity.DesCaseType ?? "", originalSystemsValue);
                    }
                }
                else
                {
                    // Single record update
                    await _repository.Database.ExecuteSqlRawAsync(
                        @"UPDATE tblTmkDesCaseTypeFields SET DesCaseType=@p0, FromField=@p1, ToField=@p2, Systems=@p3
                          WHERE DesCaseType=@p4 AND FromField=@p5 AND ToField=@p6 AND Systems=@p7",
                        entity.DesCaseType ?? "", entity.FromField ?? "", entity.ToField ?? "", newSystemsValue2,
                        entity.DesCaseType ?? "", entity.FromField ?? "", entity.ToField ?? "", originalSystemsValue);

                    // Cascade Systems change to all sibling records in the same group
                    if (newSystemsValue2 != originalSystemsValue)
                    {
                        await _repository.Database.ExecuteSqlRawAsync(
                            "UPDATE tblTmkDesCaseTypeFields SET Systems=@p0 WHERE DesCaseType=@p1 AND Systems=@p2",
                            newSystemsValue2, entity.DesCaseType ?? "", originalSystemsValue);
                    }
                }
            }

            var redirectSystems = string.Join(",", individualSystems);
            return Json(new { redirectUrl = Url.Action("Detail", new { desCaseType = entity.DesCaseType, systems = redirectSystems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string desCaseType = "", string fromField = "", string toField = "", string systems = "")
        {
            // Delete ALL records for this (DesCaseType, Systems) group
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkDesCaseTypeFields WHERE DesCaseType=@p0 AND Systems=@p1",
                desCaseType ?? "", systems ?? "");

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

        public IActionResult GetSystemList()
        {
            return Json(Helpers.SystemsHelper.SystemNames);
        }

        public async Task<IActionResult> GetPicklistData([DataSourceRequest] DataSourceRequest request, string property, string text, FilterType filterType, string requiredRelation = "")
        {
            return await GetPicklistData(_repository.TmkDesCaseTypeFields.AsQueryable(), request, property, text, filterType, requiredRelation);
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> MoveToDelete(string desCaseType = "", string systems = "")
        {
            // Check for system-level overlap in the delete table
            var newIndividualSystems = (systems ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (!newIndividualSystems.Any()) newIndividualSystems.Add("");
            var existingDeleteSystems = await _repository.TmkDesCaseTypeFieldsDeletes.AsNoTracking()
                .Where(d => d.DesCaseType == desCaseType)
                .Select(d => d.Systems).Distinct().ToListAsync();
            var existingIndividual = existingDeleteSystems
                .SelectMany(s => (s ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var overlap = newIndividualSystems.Where(s => existingIndividual.Contains(s)).ToList();
            if (overlap.Any())
                return BadRequest($"The following systems already exist in the Delete table for '{desCaseType}': {string.Join(", ", overlap)}");

            // Bulk insert into delete table
            await _repository.Database.ExecuteSqlRawAsync(
                @"INSERT INTO tblTmkDesCaseTypeFieldsDelete (DesCaseType, FromField, ToField, DesCaseTypeNew, FromFieldNew, ToFieldNew, Systems)
                  SELECT DesCaseType, FromField, ToField, DesCaseType, FromField, ToField, Systems
                  FROM tblTmkDesCaseTypeFields WHERE DesCaseType=@p0 AND Systems=@p1",
                desCaseType ?? "", systems ?? "");

            // Delete from source table
            await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkDesCaseTypeFields WHERE DesCaseType=@p0 AND Systems=@p1",
                desCaseType ?? "", systems ?? "");

            return Ok(new { success = "Records moved to delete table successfully." });
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> MoveRowToDelete(string desCaseType = "", string fromField = "", string toField = "", string systems = "")
        {
            var exactDupe = await _repository.TmkDesCaseTypeFieldsDeletes.AsNoTracking()
                .AnyAsync(d => d.DesCaseType == desCaseType && d.FromField == fromField && d.ToField == toField && d.Systems == systems);
            if (exactDupe)
                return BadRequest("This field already exists in the delete table.");

            await _repository.Database.ExecuteSqlRawAsync(
                "INSERT INTO tblTmkDesCaseTypeFieldsDelete (DesCaseType, FromField, ToField, DesCaseTypeNew, FromFieldNew, ToFieldNew, Systems) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)",
                desCaseType, fromField, toField, desCaseType, fromField, toField, systems);

            await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblTmkDesCaseTypeFields WHERE DesCaseType=@p0 AND FromField=@p1 AND ToField=@p2 AND Systems=@p3",
                desCaseType, fromField, toField, systems);

            return Ok(new { success = "Field moved to delete table successfully." });
        }
    }
}
