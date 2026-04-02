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
                foreach (var filter in mainSearchFilters)
                {
                    if (filter.Property == "DesCaseType" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => a.DesCaseType == filter.Value);
                    else if (filter.Property == "SystemName" && !string.IsNullOrEmpty(filter.Value))
                        entities = entities.Where(a => a.Systems != null && EF.Functions.Like(a.Systems, "%" + filter.Value.Replace("%", "") + "%"));
                }
            }

            var data = await entities
                .Select(e => new { e.DesCaseType, e.Systems })
                .Distinct()
                .OrderBy(d => d.DesCaseType).ThenBy(d => d.Systems)
                .ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        public async Task<IActionResult> FieldsRead([DataSourceRequest] DataSourceRequest request, string desCaseType, string systems)
        {
            var data = await _repository.TmkDesCaseTypeFields.AsNoTracking()
                .Where(f => f.DesCaseType == desCaseType && f.Systems == systems)
                .ToListAsync();
            return Json(data.ToDataSourceResult(request));
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> FieldsUpdate([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")] IEnumerable<TmkDesCaseTypeFields> models)
        {
            var results = new List<TmkDesCaseTypeFields>();
            foreach (var item in models)
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    "UPDATE tblTmkDesCaseTypeFields SET FromField=@p0, ToField=@p1 WHERE DesCaseType=@p2 AND FromField=@p3 AND ToField=@p4 AND Systems=@p5",
                    item.FromField ?? "", item.ToField ?? "", item.DesCaseType ?? "", item.FromField ?? "", item.ToField ?? "", item.Systems ?? "");
                results.Add(item);
            }
            return Json(results.ToDataSourceResult(request, ModelState));
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryModify)]
        public async Task<IActionResult> FieldsCreate([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")] IEnumerable<TmkDesCaseTypeFields> models)
        {
            var results = new List<TmkDesCaseTypeFields>();
            foreach (var item in models)
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    "INSERT INTO tblTmkDesCaseTypeFields (DesCaseType, FromField, ToField, Systems) VALUES (@p0, @p1, @p2, @p3)",
                    item.DesCaseType ?? "", item.FromField ?? "", item.ToField ?? "", item.Systems ?? "");
                results.Add(item);
            }
            return Json(results.ToDataSourceResult(request, ModelState));
        }

        [HttpPost, Authorize(Policy = TrademarkAuthorizationPolicy.AuxiliaryCanDelete)]
        public async Task<IActionResult> FieldsDestroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")] IEnumerable<TmkDesCaseTypeFields> models)
        {
            foreach (var item in models)
            {
                await _repository.Database.ExecuteSqlRawAsync(
                    "DELETE FROM tblTmkDesCaseTypeFields WHERE DesCaseType=@p0 AND FromField=@p1 AND ToField=@p2 AND Systems=@p3",
                    item.DesCaseType ?? "", item.FromField ?? "", item.ToField ?? "", item.Systems ?? "");
            }
            return Json(models.ToDataSourceResult(request, ModelState));
        }

        public async Task<IActionResult> Detail(string desCaseType, string fromField = "", string toField = "", string systems = "", bool singleRecord = false, bool fromSearch = false)
        {
            var exists = await _repository.TmkDesCaseTypeFields.AsNoTracking()
                .AnyAsync(c => c.DesCaseType == desCaseType && c.Systems == systems);
            if (!exists)
            {
                if (Request.IsAjax()) return new RecordDoesNotExistResult();
                return RedirectToAction("Index");
            }

            var detail = new TmkDesCaseTypeFields { DesCaseType = desCaseType, Systems = systems };

            var permission = await GetPermission();
            permission.AddScreenUrl = permission.CanAddRecord ? Url.Action("Add", new { fromSearch = true }) : "";
            permission.DeleteScreenUrl = permission.CanDeleteRecord ? Url.Action("Delete", new { desCaseType = desCaseType, systems = systems }) : "";
            permission.CopyScreenUrl = permission.CanCopyRecord ? Url.Action("Add", new { fromSearch = true, copyDesCaseType = desCaseType, copySystems = systems, copyGroup = true }) : "";
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
            if (!Request.IsAjax())
                return RedirectToAction("Index");

            var entity = new TmkDesCaseTypeFields { IsNewRecord = true };
            if (!string.IsNullOrEmpty(copyDesCaseType))
            {
                entity.DesCaseType = copyDesCaseType;
                entity.FromField = copyFromField;
                entity.ToField = copyToField;
                entity.Systems = copySystems ?? "";
                if (copyGroup)
                    entity.CopyFromSystems = copySystems ?? "";
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
            if (!string.IsNullOrEmpty(entity.CopyFromSystems))
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
                var existingSystemValues = await _repository.TmkDesCaseTypeFields.AsNoTracking()
                    .Where(c => c.DesCaseType == entity.DesCaseType)
                    .Select(c => c.Systems)
                    .Distinct()
                    .ToListAsync();
                var overlap = individualSystems.Where(s => existingSystemValues.Contains(s, StringComparer.OrdinalIgnoreCase)).ToList();
                if (overlap.Any())
                    return new JsonBadRequest($"The following systems already exist for Des Case Type '{entity.DesCaseType}': {string.Join(", ", overlap)}");

                // Bulk-copy all From/To records from source for each new system
                foreach (var sys in individualSystems)
                {
                    await _repository.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO tblTmkDesCaseTypeFields (DesCaseType, FromField, ToField, Systems)
                          SELECT DesCaseType, FromField, ToField, @p0
                          FROM tblTmkDesCaseTypeFields
                          WHERE DesCaseType=@p1 AND Systems=@p2",
                        sys, entity.DesCaseType ?? "", entity.CopyFromSystems);
                }
            }
            else if (isNewRecord)
            {
                // Individual add: insert one record per system
                foreach (var sys in individualSystems)
                {
                    // Check if exact (DesCaseType, FromField, ToField, System) already exists
                    var exactExists = await _repository.TmkDesCaseTypeFields.AsNoTracking()
                        .AnyAsync(c => c.DesCaseType == entity.DesCaseType && c.FromField == entity.FromField && c.ToField == entity.ToField && c.Systems == sys);
                    if (exactExists)
                        return new JsonBadRequest($"A record with Des Case Type '{entity.DesCaseType}', From Field '{entity.FromField}', To Field '{entity.ToField}', and System '{sys}' already exists.");

                    await _repository.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO tblTmkDesCaseTypeFields (DesCaseType, FromField, ToField, Systems)
                          VALUES (@p0, @p1, @p2, @p3)",
                        entity.DesCaseType ?? "", entity.FromField ?? "", entity.ToField ?? "", sys);
                }
            }
            else
            {
                // Update existing record
                await _repository.Database.ExecuteSqlRawAsync(
                    @"UPDATE tblTmkDesCaseTypeFields SET DesCaseType=@p0, FromField=@p1, ToField=@p2, Systems=@p3
                      WHERE DesCaseType=@p4 AND FromField=@p5 AND ToField=@p6 AND Systems=@p7",
                    entity.DesCaseType ?? "", entity.FromField ?? "", entity.ToField ?? "", entity.Systems,
                    entity.DesCaseType ?? "", entity.FromField ?? "", entity.ToField ?? "", originalSystemsValue);

                // Cascade Systems change to all sibling records in the same group
                if (entity.Systems != originalSystemsValue)
                {
                    await _repository.Database.ExecuteSqlRawAsync(
                        "UPDATE tblTmkDesCaseTypeFields SET Systems=@p0 WHERE DesCaseType=@p1 AND Systems=@p2",
                        entity.Systems, entity.DesCaseType ?? "", originalSystemsValue);
                }
            }

            var redirectSystem = individualSystems.First();
            return Json(new { redirectUrl = Url.Action("Detail", new { desCaseType = entity.DesCaseType, systems = redirectSystem, singleRecord = true }) });
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
