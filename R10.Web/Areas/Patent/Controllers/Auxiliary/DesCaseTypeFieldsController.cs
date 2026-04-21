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
            string? Bool(string name) => mainSearchFilters?.FirstOrDefault(f =>
                string.Equals(f.Property, name, StringComparison.OrdinalIgnoreCase))?.Value;
            var extFilter = Bool("IsExt");
            var inUseFilter = Bool("InUse");
            var otherFilters = mainSearchFilters?.Where(f =>
                !string.Equals(f.Property, "IsExt", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(f.Property, "InUse", StringComparison.OrdinalIgnoreCase)).ToList();

            var baseItems = new List<DesCaseTypeFieldsSearchItem>();
            // Base has no InUse column — if the user narrowed on InUse we skip base.
            if (extFilter != "true" && string.IsNullOrEmpty(inUseFilter))
            {
                var baseEntities = _repository.PatDesCaseTypeFields.AsNoTracking().AsQueryable();
                if (otherFilters != null && otherFilters.Count > 0)
                    baseEntities = baseEntities.BuildCriteria(otherFilters);
                var baseRows = await baseEntities
                    .Select(e => new { e.DesCaseType, e.Systems })
                    .Distinct()
                    .ToListAsync();
                baseItems = baseRows.Select(b => new DesCaseTypeFieldsSearchItem
                {
                    DesCaseType = b.DesCaseType,
                    InUse = null,
                    Systems = b.Systems,
                    IsExt = false
                }).ToList();
            }

            var extItems = new List<DesCaseTypeFieldsSearchItem>();
            if (extFilter != "false")
            {
                var extEntities = _repository.PatDesCaseTypeFieldsExts.AsNoTracking().AsQueryable();
                if (otherFilters != null && otherFilters.Count > 0)
                    extEntities = extEntities.BuildCriteria(otherFilters);
                extItems = await extEntities
                    .Select(e => new DesCaseTypeFieldsSearchItem
                    {
                        DesCaseType = e.DesCaseType,
                        FromField = e.FromField,
                        ToField = e.ToField,
                        InUse = e.InUse,
                        Systems = e.Systems,
                        IsExt = true
                    }).ToListAsync();
            }

            var combined = baseItems.Concat(extItems).AsQueryable();
            if (inUseFilter == "true") combined = combined.Where(x => x.InUse == true);
            else if (inUseFilter == "false") combined = combined.Where(x => x.InUse == false);

            return Json(combined.OrderBy(x => x.DesCaseType).ToList().ToDataSourceResult(request));
        }

        public async Task<IActionResult> FieldsRead([DataSourceRequest] DataSourceRequest request, string desCaseType, string systems = "")
        {
            var query = _repository.PatDesCaseTypeFields.AsNoTracking()
                .Where(f => f.DesCaseType == desCaseType);
            if (!string.IsNullOrEmpty(systems))
                query = query.Where(f => f.Systems == systems);
            var data = await query.ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> FieldsUpdate([DataSourceRequest] DataSourceRequest request,
            string desCaseType = "", string systems = "",
            [Bind(Prefix = "updated")] IList<PatDesCaseTypeFields> updated = null,
            [Bind(Prefix = "new")] IList<PatDesCaseTypeFields> added = null,
            [Bind(Prefix = "deleted")] IList<PatDesCaseTypeFields> deleted = null)
        {
            // kendoGridSave parameterMap converts keys to "models" prefix
            // Fall back: if all are empty, try "models" prefix via Request.Form
            if ((updated == null || !updated.Any()) && (added == null || !added.Any()) && (deleted == null || !deleted.Any()))
            {
                // The data comes as models[0].FromField etc - read from form directly
                var allModels = new List<PatDesCaseTypeFields>();
                int i = 0;
                while (Request.Form.ContainsKey($"models[{i}].FromField"))
                {
                    allModels.Add(new PatDesCaseTypeFields
                    {
                        DesCaseType = Request.Form[$"models[{i}].DesCaseType"].FirstOrDefault() ?? desCaseType,
                        FromField = Request.Form[$"models[{i}].FromField"].FirstOrDefault() ?? "",
                        ToField = Request.Form[$"models[{i}].ToField"].FirstOrDefault() ?? "",
                        Systems = Request.Form[$"models[{i}].Systems"].FirstOrDefault() ?? systems
                    });
                    i++;
                }
                // These are updated records (sent through the Update transport)
                updated = allModels;
            }

            updated ??= new List<PatDesCaseTypeFields>();
            added ??= new List<PatDesCaseTypeFields>();
            deleted ??= new List<PatDesCaseTypeFields>();

            foreach (var item in deleted)
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    "DELETE FROM tblPatDesCaseTypeFields WHERE DesCaseType=@p0 AND FromField=@p1 AND ToField=@p2 AND Systems=@p3",
                    item.DesCaseType ?? desCaseType, item.FromField ?? "", item.ToField ?? "", item.Systems ?? systems);
            }

            foreach (var item in updated)
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    "UPDATE tblPatDesCaseTypeFields SET ToField=@p0 WHERE DesCaseType=@p1 AND FromField=@p2 AND Systems=@p3",
                    item.ToField ?? "", item.DesCaseType ?? desCaseType, item.FromField ?? "", item.Systems ?? systems);
            }

            foreach (var item in added)
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    "INSERT INTO tblPatDesCaseTypeFields (DesCaseType, FromField, ToField, Systems) VALUES (@p0, @p1, @p2, @p3)",
                    item.DesCaseType ?? desCaseType, item.FromField ?? "", item.ToField ?? "", item.Systems ?? systems);
            }

            return Ok(new { success = "Record has been saved successfully." });
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> FieldsDestroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "deleted")] PatDesCaseTypeFields deleted)
        {
            if (deleted != null && !string.IsNullOrEmpty(deleted.FromField))
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    "DELETE FROM tblPatDesCaseTypeFields WHERE DesCaseType=@p0 AND FromField=@p1 AND ToField=@p2 AND Systems=@p3",
                    deleted.DesCaseType ?? "", deleted.FromField ?? "", deleted.ToField ?? "", deleted.Systems ?? "");
            }
            return Ok(new { success = "Record deleted successfully." });
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> FieldsReplaceAll([FromBody] FieldsReplaceAllRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.DesCaseType))
                return BadRequest("Invalid request.");

            // Delete all existing records for this group
            await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblPatDesCaseTypeFields WHERE DesCaseType=@p0 AND Systems=@p1",
                model.DesCaseType, model.Systems ?? "");

            // Re-insert all rows from the grid
            if (model.AllRows != null)
            {
                foreach (var row in model.AllRows)
                {
                    if (!string.IsNullOrEmpty(row.FromField))
                    {
                        await _repository.Database.ExecuteSqlRawAsync(
                            "INSERT INTO tblPatDesCaseTypeFields (DesCaseType, FromField, ToField, Systems) VALUES (@p0, @p1, @p2, @p3)",
                            model.DesCaseType, row.FromField ?? "", row.ToField ?? "", model.Systems ?? "");
                    }
                }
            }

            return Ok();
        }

        [Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> Add(bool fromSearch = false, string copyDesCaseType = "", string copyFromField = "", string copyToField = "", string copySystems = "", bool copyGroup = false)
        {
            var data = new PatDesCaseTypeFields { IsNewRecord = true };

            if (!string.IsNullOrEmpty(copyDesCaseType))
            {
                data.DesCaseType = copyDesCaseType;
                data.FromField = copyFromField;
                data.ToField = copyToField;
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
                Title = _localizer["New Des Case Type Fields"].ToString(),
                Data = data,
                PagePermission = await GetPermission(),
                AfterCancelledInsert = $"function() {{ window.location.href = '{Url.Action("Index")}'; }}"
            };
            ModelState.Clear();
            return Request.IsAjax() ? PartialView("Index", model) : View("Index", model);
        }

        public async Task<IActionResult> Detail(string desCaseType, string fromField = "", string toField = "", string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            // Check that at least one record exists for this DesCaseType
            var exists = await _repository.PatDesCaseTypeFields.AsNoTracking()
                .AnyAsync(c => c.DesCaseType == desCaseType);
            if (!exists)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }

            // Get the systems value from the first matching record if not provided
            if (string.IsNullOrEmpty(systems))
            {
                systems = await _repository.PatDesCaseTypeFields.AsNoTracking()
                    .Where(c => c.DesCaseType == desCaseType)
                    .Select(c => c.Systems)
                    .FirstOrDefaultAsync() ?? "";
            }

            var detail = new PatDesCaseTypeFields { DesCaseType = desCaseType, Systems = systems };

            var perm = await GetPermission();
            perm.AddScreenUrl = perm.CanAddRecord ? Url.Action("Add", new { fromSearch = true }) : "";
            perm.DeleteScreenUrl = perm.CanDeleteRecord ? Url.Action("Delete", new { desCaseType = desCaseType }) : "";
            perm.CopyScreenUrl = perm.CanCopyRecord ? Url.Action("Add", new { fromSearch = true, copyDesCaseType = desCaseType, copyGroup = true }) : "";
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
            // Group copy or detail page system update doesn't submit FromField/ToField
            if (!string.IsNullOrEmpty(entity.CopyFromSystems) || string.IsNullOrEmpty(entity.FromField))
            {
                ModelState.Remove("FromField");
                ModelState.Remove("ToField");
            }
            if (!ModelState.IsValid) return new JsonBadRequest(new { errors = ModelState.Errors() });

            entity.Systems ??= "";

            // Require at least one system
            if (string.IsNullOrWhiteSpace(entity.Systems))
                return new JsonBadRequest("At least one system must be selected.");

            // Split into individual systems - each system gets its own record
            var individualSystems = entity.Systems.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();

            var isNewRecord = entity.IsNewRecord || entity.OriginalSystems == "__NEW__" || entity.OriginalSystems == null;

            if (isNewRecord && !string.IsNullOrEmpty(entity.CopyFromSystems))
            {
                // Group copy: block if any individual system already exists for this DesCaseType
                var existingSystemStrings = await _repository.PatDesCaseTypeFields.AsNoTracking()
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
                    @"INSERT INTO tblPatDesCaseTypeFields (DesCaseType, FromField, ToField, Systems)
                      SELECT @p0, FromField, ToField, @p1
                      FROM tblPatDesCaseTypeFields
                      WHERE DesCaseType=@p2 AND Systems=@p3",
                    entity.DesCaseType ?? "", newSystems, entity.CopyFromDesCaseType ?? entity.DesCaseType ?? "", entity.CopyFromSystems);
            }
            else if (isNewRecord)
            {
                // Individual add: insert one record with the combined Systems string
                var newSystemsStr = string.Join(",", individualSystems);
                var exactExists = await _repository.PatDesCaseTypeFields.AsNoTracking()
                    .AnyAsync(c => c.DesCaseType == entity.DesCaseType && c.FromField == entity.FromField && c.ToField == entity.ToField && c.Systems == newSystemsStr);
                if (exactExists)
                    return new JsonBadRequest($"A record with Des Case Type '{entity.DesCaseType}', From Field '{entity.FromField}', To Field '{entity.ToField}', and Systems '{newSystemsStr}' already exists.");

                await _repository.Database.ExecuteSqlRawAsync(
                    "INSERT INTO tblPatDesCaseTypeFields (DesCaseType, FromField, ToField, Systems) VALUES (@p0, @p1, @p2, @p3)",
                    entity.DesCaseType ?? "", entity.FromField ?? "", entity.ToField ?? "", newSystemsStr);
            }
            else
            {
                var originalSystems = entity.OriginalSystems == "__EMPTY__" ? "" : entity.OriginalSystems;
                var newSystemsValue = string.Join(",", individualSystems);

                if (string.IsNullOrEmpty(entity.FromField))
                {
                    // Bulk systems update from detail page - update ALL records for this DesCaseType
                    if (newSystemsValue != originalSystems)
                    {
                        await _repository.Database.ExecuteSqlRawAsync(
                            "UPDATE tblPatDesCaseTypeFields SET Systems=@p0 WHERE DesCaseType=@p1 AND Systems=@p2",
                            newSystemsValue, entity.DesCaseType ?? "", originalSystems ?? "");
                    }
                }
                else
                {
                    // Single record update
                    await _repository.Database.ExecuteSqlRawAsync(
                        "UPDATE tblPatDesCaseTypeFields SET DesCaseType=@p0, FromField=@p1, ToField=@p2, Systems=@p3 WHERE DesCaseType=@p4 AND FromField=@p5 AND ToField=@p6 AND Systems=@p7",
                        new object[] {
                            new Microsoft.Data.SqlClient.SqlParameter("@p0", entity.DesCaseType ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p1", entity.FromField ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p2", entity.ToField ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p3", newSystemsValue),
                            new Microsoft.Data.SqlClient.SqlParameter("@p4", entity.DesCaseType ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p5", entity.FromField ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p6", entity.ToField ?? ""),
                            new Microsoft.Data.SqlClient.SqlParameter("@p7", originalSystems ?? "")
                        });

                    // Cascade Systems change to all sibling records in the same group
                    if (newSystemsValue != originalSystems)
                    {
                        await _repository.Database.ExecuteSqlRawAsync(
                            "UPDATE tblPatDesCaseTypeFields SET Systems=@p0 WHERE DesCaseType=@p1 AND Systems=@p2",
                            newSystemsValue, entity.DesCaseType ?? "", originalSystems ?? "");
                    }
                }
            }

            var redirectSystems = string.Join(",", individualSystems);
            return Json(new { id = 0, redirectUrl = Url.Action("Detail", new { desCaseType = entity.DesCaseType, systems = redirectSystems, singleRecord = true }) });
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> Delete(string desCaseType = "", string fromField = "", string toField = "", string systems = "")
        {
            // Delete ALL records for this (DesCaseType, Systems) group
            var count = await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblPatDesCaseTypeFields WHERE DesCaseType=@p0 AND Systems=@p1",
                new object[] {
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", desCaseType ?? ""),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", systems ?? "")
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

        public IActionResult GetSystemList()
        {
            return Json(Helpers.SystemsHelper.SystemNames);
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

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> MoveToDelete(string desCaseType = "", string systems = "")
        {
            // Check for system-level overlap in the delete table
            var newIndividualSystems = (systems ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (!newIndividualSystems.Any()) newIndividualSystems.Add("");
            var existingDeleteSystems = await _repository.PatDesCaseTypeFieldsDeletes.AsNoTracking()
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
                @"INSERT INTO tblPatDesCaseTypeFieldsDelete (DesCaseType, FromField, ToField, DesCaseTypeNew, FromFieldNew, ToFieldNew, Systems)
                  SELECT DesCaseType, FromField, ToField, DesCaseType, FromField, ToField, Systems
                  FROM tblPatDesCaseTypeFields WHERE DesCaseType=@p0 AND Systems=@p1",
                desCaseType ?? "", systems ?? "");

            // Delete from source table
            await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblPatDesCaseTypeFields WHERE DesCaseType=@p0 AND Systems=@p1",
                desCaseType ?? "", systems ?? "");

            return Ok(new { success = "Records moved to delete table successfully." });
        }

        [HttpPost, Authorize(Policy = PatentAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> MoveRowToDelete(string desCaseType = "", string fromField = "", string toField = "", string systems = "")
        {
            // Check if this exact row already exists in delete table
            var exactDupe = await _repository.PatDesCaseTypeFieldsDeletes.AsNoTracking()
                .AnyAsync(d => d.DesCaseType == desCaseType && d.FromField == fromField && d.ToField == toField && d.Systems == systems);
            if (exactDupe)
                return BadRequest("This field already exists in the delete table.");

            // Insert into delete table (New fields = regular fields)
            await _repository.Database.ExecuteSqlRawAsync(
                "INSERT INTO tblPatDesCaseTypeFieldsDelete (DesCaseType, FromField, ToField, DesCaseTypeNew, FromFieldNew, ToFieldNew, Systems) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)",
                desCaseType, fromField, toField, desCaseType, fromField, toField, systems);

            // Delete from source table
            await _repository.Database.ExecuteSqlRawAsync(
                "DELETE FROM tblPatDesCaseTypeFields WHERE DesCaseType=@p0 AND FromField=@p1 AND ToField=@p2 AND Systems=@p3",
                desCaseType, fromField, toField, systems);

            return Ok(new { success = "Field moved to delete table successfully." });
        }
    }

    public class FieldsReplaceAllRequest
    {
        public string DesCaseType { get; set; }
        public string Systems { get; set; }
        public List<FieldsReplaceAllRow> AllRows { get; set; }
    }

    public class FieldsReplaceAllRow
    {
        public string FromField { get; set; }
        public string ToField { get; set; }
    }
}
